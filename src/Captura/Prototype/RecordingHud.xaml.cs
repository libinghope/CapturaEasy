using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Captura.Prototype
{
    /// <summary>
    /// 录制中悬浮球（HUD）。
    /// 行为：左键拖拽 / 单击切换展开控制条 / 悬浮球时长与 Pulse 由录制状态驱动。
    /// 窗口不抢焦点（ShowActivated=False），可悬浮在任何全屏应用之上。
    /// 由 MainWindow 在 Recording/Paused 时 Show，NotRecording 时 Close。
    /// </summary>
    public partial class RecordingHud : Window
    {
        Point _downPos;
        bool _moved;
        DrawingOverlay _drawingOverlay;

        public RecordingHud()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 初始定位到屏幕右下角
            PositionBottomRight();

            UpdatePulse();
        }

        protected override void OnClosed(EventArgs e)
        {
            // HUD 关闭时一并关闭画笔浮层
            _drawingOverlay?.Close();
            base.OnClosed(e);
        }

        void PositionBottomRight()
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 20;
            Top = workArea.Bottom - Height - 20;
        }

        // ============ 拖拽 + 单击展开 ============

        void Ball_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _downPos = e.GetPosition(this);
            _moved = false;
        }

        void Ball_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _moved)
                return;

            var pos = e.GetPosition(this);
            var dx = Math.Abs(pos.X - _downPos.X);
            var dy = Math.Abs(pos.Y - _downPos.Y);

            // 移动超过 4 像素才判定为拖拽
            if (dx > 4 || dy > 4)
            {
                _moved = true;
                // DragMove 是阻塞调用，自动处理 DPI 和坐标，直到鼠标释放后返回
                DragMove();
            }
        }

        void Ball_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 没有真正拖拽 → 视为单击，切换展开/收起
            if (!_moved)
                ToggleExpand();

            _moved = false;
        }

        // 展开/收起控制条
        void ToggleExpand()
        {
            ControlBar.Visibility = ControlBar.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            Width = ControlBar.Visibility == Visibility.Visible ? 340 : 80;
        }

        // 画笔标注：切换全屏 InkCanvas 浮层
        void Brush_Click(object sender, RoutedEventArgs e)
        {
            if (_drawingOverlay != null && _drawingOverlay.IsVisible)
            {
                // 已显示 → 隐藏
                _drawingOverlay.Hide();
                return;
            }

            if (_drawingOverlay == null)
            {
                _drawingOverlay = new DrawingOverlay { Owner = null };
                // 用户从浮层内点击"退出标注"时，同步状态（下次点击会重新创建/显示）
                _drawingOverlay.Exited += (s, ev) => { };
                _drawingOverlay.Closed += (s, ev) => _drawingOverlay = null;
            }

            _drawingOverlay.Show();
        }

        // 红圈脉冲：呼吸效果
        public void UpdatePulse()
        {
            var sb = new Storyboard();
            var anim = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            anim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            anim.KeyFrames.Add(new LinearDoubleKeyFrame(0.9, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.9))));
            Storyboard.SetTarget(anim, PulseRing);
            Storyboard.SetTargetProperty(anim, new PropertyPath("Opacity"));
            sb.Children.Add(anim);
            sb.Begin();
        }
    }
}
