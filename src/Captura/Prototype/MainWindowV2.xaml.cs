using System;
using System.Windows;
using System.Windows.Input;
using Captura.Models;
using Captura.ViewModels;

namespace Captura.Prototype
{
    /// <summary>
    /// 卡片化主窗口（阶段 2）。
    /// 接入要点：
    ///   - 订阅 RecordingViewModel.RecorderState，录制中自动弹出 RecordingHud 悬浮球
    ///   - 顶栏可拖拽窗口
    ///   - 关闭时清理悬浮球
    /// </summary>
    public partial class MainWindowV2 : Window
    {
        RecordingHud _hud;
        IDisposable _stateSub;

        public MainWindowV2()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 订阅录制状态：进入 Recording/Paused 显示 HUD，NotRecording 隐藏
            var recorder = ServiceProvider.Get<RecordingViewModel>();
            _stateSub = recorder.RecorderState.Subscribe(OnRecorderStateChanged);
        }

        void OnClosed(object sender, EventArgs e)
        {
            _stateSub?.Dispose();
            CloseHud();
        }

        void OnRecorderStateChanged(RecorderState state)
        {
            // 跨线程安全
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OnRecorderStateChanged(state));
                return;
            }

            switch (state)
            {
                case RecorderState.Recording:
                case RecorderState.Paused:
                    ShowHud();
                    break;
                case RecorderState.NotRecording:
                    CloseHud();
                    break;
            }
        }

        void ShowHud()
        {
            if (_hud == null)
            {
                _hud = new RecordingHud { Owner = this };
                _hud.Closed += (s, e) => _hud = null;
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

        // 顶栏空白区拖拽窗口
        void TopBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        void MinButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

        void OpenSettings(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("设置抽屉将在阶段3实现。", "CapturaEasy 原型");
        }

        void ToggleShotFlyout(object sender, RoutedEventArgs e)
        {
            ShotFlyout.IsOpen = !ShotFlyout.IsOpen;
        }

        // 点击变体后关闭 Popup
        void ShotItem_Click(object sender, MouseButtonEventArgs e)
        {
            ShotFlyout.IsOpen = false;
        }

        void ToggleRecent(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("最近列表将在阶段3升级为抽屉/弹出面板。", "CapturaEasy 原型");
        }
    }
}
