using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace Captura.Prototype
{
    /// <summary>
    /// 录制中悬浮球（HUD）。
    /// 行为：左键拖拽 / 单击切换展开控制条 / 悬浮球时长与 Pulse 由录制状态驱动。
    /// 窗口不抢焦点（ShowActivated=False），可悬浮在任何全屏应用之上。
    /// 由 MainWindowV2 在 Recording/Paused 时 Show，NotRecording 时 Close。
    /// </summary>
    public partial class RecordingHud : Window
    {
        Point _dragOffset;
        bool _dragging;
        bool _expanded;

        public RecordingHud()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 将隐藏 TextBinder 的文本同步显示到球面（避开 XAML 中重复绑定的麻烦）
            var dpd = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
            dpd?.AddValueChanged(TimeBinder, (s, ev) => BallTime.Text = TimeBinder.Text);

            // 初始定位到屏幕右下角
            PositionBottomRight();

            UpdatePulse();
        }

        void PositionBottomRight()
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 20;
            Top = workArea.Bottom - Height - 20;
        }

        // 拖拽：记录起始偏移
        void Ball_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragging = false;
            _dragOffset = e.GetPosition(this);
            ((UIElement)sender).CaptureMouse();
        }

        void Ball_MouseEnter(object sender, MouseEventArgs e) { }
        void Ball_MouseLeave(object sender, MouseEventArgs e) { }

        // 通过 Capture + Move 判断是拖拽还是单击
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {
                var pos = e.GetPosition(this);
                if ((pos - _dragOffset).Length > 4) _dragging = true;

                if (_dragging)
                {
                    var screen = PointToScreen(pos);
                    Left = screen.X - _dragOffset.X;
                    Top = screen.Y - _dragOffset.Y;
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            ReleaseMouseCapture();
            if (!_dragging) ToggleExpand();
            _dragging = false;
        }

        // 展开/收起控制条
        void ToggleExpand()
        {
            _expanded = !_expanded;
            ControlBar.Visibility = _expanded ? Visibility.Visible : Visibility.Collapsed;
            Width = _expanded ? 340 : 80;
        }

        // 画笔：阶段3将接入全屏 InkCanvas 透明浮层
        void Brush_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("画笔标注浮层将在阶段3实现：全屏透明 InkCanvas + 工具条。",
                "CapturaEasy 阶段 2", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 红圈脉冲：呼吸效果
        public void UpdatePulse()
        {
            var sb = new Storyboard();
            var anim = new DoubleAnimationUsingKeyFrames { RepeatBehavior = RepeatBehavior.Forever };
            anim.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            anim.KeyFrames.Add(new LinearDoubleKeyFrame(0.9, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.9))));
            Storyboard.SetTarget(anim, PulseRing);
            Storyboard.SetTargetProperty(anim, new PropertyPath(Ellipse.OpacityProperty));
            sb.Children.Add(anim);
            sb.Begin();
        }
    }
}
