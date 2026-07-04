using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Ink;
using System.Windows.Interop;
using System.Windows.Media;

namespace Captura.Prototype
{
    /// <summary>
    /// 录制中实时画笔标注浮层（阶段 5）。
    /// 全屏透明顶层窗口，不抢焦点，叠加在桌面之上。
    /// 由 RecordingHud 的画笔按钮触发显示/隐藏。
    /// </summary>
    public partial class DrawingOverlay : Window
    {
        // 预设颜色循环（点击颜色按钮依次切换）
        static readonly System.Windows.Media.Color[] PresetColors =
        {
            Color.FromRgb(0xFF, 0x2C, 0x2C), // 红
            Color.FromRgb(0xFF, 0xC1, 0x07), // 橙
            Color.FromRgb(0x07, 0xC1, 0x60), // 绿（微信绿）
            Color.FromRgb(0x18, 0x90, 0xFF), // 蓝
            Color.FromRgb(0xFF, 0xFF, 0xFF), // 白
            Color.FromRgb(0x21, 0x21, 0x21), // 黑
        };
        int _colorIndex;
        bool _eraserMode;
        // 默认绘画属性（颜色/线宽绑定到此对象）
        readonly DrawingAttributes _pen = new DrawingAttributes
        {
            Color = Color.FromRgb(0xFF, 0x2C, 0x2C),
            FitToCurve = true,
            Width = 3,
            Height = 3,
            IsHighlighter = false
        };

        public DrawingOverlay()
        {
            InitializeComponent();
            // 将 _pen 设为 InkCanvas 的默认绘画属性
            InkCanvas.DefaultDrawingAttributes = _pen;

            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 覆盖整个虚拟屏幕（多显示器场景也覆盖）
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            // 应用 WS_EX_NOACTIVATE，确保点击不会从 HUD 抢走焦点
            var helper = new WindowInteropHelper(this);
            var ex = GetWindowLong(helper.Handle, GWL_EXSTYLE);
            SetWindowLong(helper.Handle, GWL_EXSTYLE, (ex | WS_EX_NOACTIVATE));

            ApplyPen();
        }

        // ============ 工具切换 ============

        void Pen_Click(object sender, RoutedEventArgs e)
        {
            _eraserMode = false;
            ApplyPen();
        }

        void Eraser_Click(object sender, RoutedEventArgs e)
        {
            _eraserMode = true;
            ApplyEraser();
        }

        void Color_Click(object sender, RoutedEventArgs e)
        {
            _colorIndex = (_colorIndex + 1) % PresetColors.Length;
            var color = PresetColors[_colorIndex];
            _pen.Color = color;
            ColorBtn.Tag = new SolidColorBrush(color);
            // 切换颜色后自动回到画笔模式（用户大概率是想画不是想擦）
            if (_eraserMode)
            {
                _eraserMode = false;
                ApplyPen();
            }
        }

        void Width_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var w = WidthSlider.Value;
            _pen.Width = w;
            _pen.Height = w;
        }

        void Undo_Click(object sender, RoutedEventArgs e)
        {
            // 撤销最近一笔（InkCanvas 的 Strokes 集合）
            var strokes = InkCanvas.Strokes;
            if (strokes.Count > 0)
            {
                strokes.RemoveAt(strokes.Count - 1);
            }
        }

        void Clear_Click(object sender, RoutedEventArgs e)
        {
            InkCanvas.Strokes.Clear();
        }

        void Exit_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            Exited?.Invoke(this, EventArgs.Empty);
        }

        // ============ 模式应用 ============

        void ApplyPen()
        {
            InkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            _pen.IsHighlighter = false;
        }

        void ApplyEraser()
        {
            // StrokeEraser 按笔画整体擦除，比 PointEraser 更顺手
            InkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }

        // ============ 工具条拖拽 ============

        void Toolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 允许用户把工具条拖到任意位置（避免遮挡画面重点）
            DragMove();
        }

        // ============ 外部控制 ============

        /// <summary>用户点击"退出标注"时触发，供 RecordingHud 同步按钮状态。</summary>
        public event EventHandler Exited;

        // ============ Win32 不抢焦点 ============

        const int GWL_EXSTYLE = -20;
        const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
