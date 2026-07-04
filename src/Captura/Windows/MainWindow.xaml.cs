using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Captura.Bootstrap;
using Captura.Models;
using Captura.Prototype;
using Captura.Video;
using Captura.ViewModels;

namespace Captura
{
    /// <summary>
    /// CapturaEasy 主窗口（A3 转正版）。
    /// 合并自：
    ///   - 原 V1 MainWindow：单例 / 初始化 / 退出流程 / 单实例唤醒 / 托盘交互 / 预览宿主
    ///   - V2 卡片化原型：顶栏拖拽 / 场景保存 / 截图下拉 / 最近列表 / HUD 自动弹出
    /// 注意：保留 x:Class="Captura.MainWindow" 与 public static Instance，
    ///       PreviewWindowService 等 7 处对 MainWindow.Instance 的引用零修改。
    /// </summary>
    public partial class MainWindow
    {
        public static MainWindow Instance { get; private set; }

        readonly MainWindowHelper _helper;

        // 录制中悬浮球（HUD）：Recording/Paused 时自动弹出，NotRecording 时关闭
        RecordingHud _hud;
        IDisposable _stateSub;
        // 记住录制开始时区域选择器是否可见，停止后恢复
        bool _regionWasVisible;

        public MainWindow()
        {
            Instance = this;

            InitializeComponent();

            _helper = ServiceProvider.Get<MainWindowHelper>();

            _helper.MainViewModel.Init(!App.CmdOptions.NoPersist, !App.CmdOptions.Reset);

            _helper.HotkeySetup.Setup();

            _helper.TimerModel.Init();

            Loaded += OnLoaded;
            Closing += OnClosing;
            // Closed 仅在真正关闭时触发（Closing 未取消），用于清理 HUD
            Closed += OnClosed;

            // --tray 或设置项要求启动即最小化到托盘
            if (App.CmdOptions.Tray || _helper.Settings.Tray.MinToTrayOnStartup)
                Hide();

            // 单实例：再次启动时唤醒已运行实例到前台
            SingleInstanceManager.StartListening(WakeApp);
        }

        void OnLoaded(object Sender, RoutedEventArgs Args)
        {
            RepositionWindowIfOutside();

            ServiceProvider.Get<WebcamPage>().SetupPreview();

            _helper.HotkeySetup.ShowUnregistered();

            // 订阅录制状态：进入 Recording/Paused 显示悬浮球，NotRecording 隐藏
            _stateSub = _helper.RecordingViewModel.RecorderState.Subscribe(OnRecorderStateChanged);
        }

        void OnClosing(object Sender, System.ComponentModel.CancelEventArgs Args)
        {
            // 录制中拒绝退出（CanExit 会弹确认/提示）
            if (!TryExit())
                Args.Cancel = true;
        }

        void OnClosed(object Sender, EventArgs Args)
        {
            _stateSub?.Dispose();
            CloseHud();
        }

        // ============ HUD 悬浮球 ============

        void OnRecorderStateChanged(RecorderState State)
        {
            // 跨线程安全
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OnRecorderStateChanged(State));
                return;
            }

            // 录制期间隐藏区域选择器（它是 Topmost 透明窗口，会挡住主窗口按钮）
            var regionProvider = ServiceProvider.Get<IRegionProvider>();

            switch (State)
            {
                case RecorderState.Recording:
                case RecorderState.Paused:
                    // 记住区域选择器原状态，录制结束后恢复
                    _regionWasVisible = regionProvider.SelectorVisible;
                    regionProvider.SelectorVisible = false;
                    ShowHud();
                    break;
                case RecorderState.NotRecording:
                    CloseHud();
                    // 恢复区域选择器可见性
                    regionProvider.SelectorVisible = _regionWasVisible;
                    break;
            }
        }

        void ShowHud()
        {
            if (_hud == null)
            {
                _hud = new RecordingHud { Owner = this };
                _hud.Closed += (S, E) => _hud = null;
                _hud.Show();
            }
        }

        void CloseHud()
        {
            if (_hud != null)
            {
                _hud.Close();
                _hud = null;
            }
        }

        // ============ 单实例唤醒 ============

        void WakeApp()
        {
            Dispatcher.Invoke(() =>
            {
                if (WindowState == WindowState.Minimized)
                    WindowState = WindowState.Normal;

                Activate();
            });
        }

        // ============ 窗口位置恢复 ============

        void RepositionWindowIfOutside()
        {
            // 窗口尺寸考虑 DPI
            var rect = new RectangleF((float) Left,
                (float) Top,
                (float) ActualWidth,
                (float) ActualHeight).ApplyDpi();

            if (!Screen.AllScreens.Any(M => M.Bounds.Contains(rect)))
            {
                Left = 50;
                Top = 50;
            }
        }

        // ============ 顶栏 / 窗口控制 ============

        // 顶栏空白区拖拽窗口（冒泡事件：按钮会自行吞掉 MouseLeftButtonDown，DragMove 只在空白区触发）
        void TopBar_PreviewMouseLeftButtonDown(object Sender, MouseButtonEventArgs Args)
        {
            if (Args.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        void MinButton_Click(object Sender, RoutedEventArgs Args) => SystemCommands.MinimizeWindow(this);

        void CloseButton_Click(object Sender, RoutedEventArgs Args)
        {
            // 关闭按钮：依设置决定最小化到托盘还是真正退出
            if (_helper.Settings.Tray.MinToTrayOnClose)
                Hide();
            else Close();
        }

        // ============ 托盘交互 ============

        void SystemTray_TrayMouseDoubleClick(object Sender, RoutedEventArgs Args)
        {
            if (Visibility == Visibility.Visible)
                Hide();
            else this.ShowAndFocus();
        }

        // 托盘菜单：显示主窗口
        void ShowMainWindow(object Sender, RoutedEventArgs E) => this.ShowAndFocus();

        // 托盘菜单：退出
        void MenuExit_Click(object Sender, RoutedEventArgs Args) => Close();

        // ============ 退出流程 ============

        bool TryExit()
        {
            // 录制中不允许直接退出
            if (!_helper.RecordingViewModel.CanExit())
                return false;

            ServiceProvider.Dispose();

            return true;
        }

        // ============ 顶栏：设置 / 最近 ============

        void OpenSettings(object Sender, RoutedEventArgs Args) => SettingsWindow.ShowInstance();

        void ToggleRecent(object Sender, RoutedEventArgs Args) => SettingsWindow.ShowRecent();

        // ============ 场景保存 ============

        void SaveScene_Click(object Sender, RoutedEventArgs Args)
        {
            var sceneVm = ServiceProvider.Get<SceneViewModel>();

            var name = SimpleInputBox.Show(this, "保存场景",
                "请输入场景名称：",
                $"场景 {sceneVm.Scenes.Count + 1}");

            if (!string.IsNullOrWhiteSpace(name))
                sceneVm.SaveCurrentAsCommand.Execute(name);
        }

        // ============ 截图下拉 ============

        void ToggleShotFlyout(object Sender, RoutedEventArgs Args) => ShotFlyout.IsOpen = !ShotFlyout.IsOpen;

        // 点击变体后关闭 Popup
        void ShotItem_Click(object Sender, MouseButtonEventArgs Args) => ShotFlyout.IsOpen = false;
    }
}
