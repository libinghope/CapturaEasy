using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Captura.Video;

namespace Captura.MouseKeyHook
{
    /// <summary>
    /// 鼠标点击涟漪高亮 overlay。
    /// 点击瞬间在点击位置生成一个扩散涟漪（半径从 0.3→2.0 倍，透明度从 0.9→0），
    /// 持续约 600ms 后消失。支持多次连击叠加（每个涟漪独立播放）。
    /// 左键黄色、右键蓝色、中键绿色。
    /// </summary>
    public class MouseClickOverlay : IOverlay
    {
        readonly MouseClickSettings _settings;

        // 涟漪队列：支持快速连击时多个涟漪同时播放
        readonly List<Ripple> _ripples = new List<Ripple>();

        const int RippleDurationMs = 600;

        public MouseClickOverlay(IMouseKeyHook Hook,
            MouseClickSettings Settings)
        {
            _settings = Settings;

            Hook.MouseDown += (S, E) =>
            {
                if (!_settings.Display)
                    return;

                // 在点击瞬间记录位置（涟漪不跟随光标移动）
                var loc = ServiceProvider.Get<IPlatformServices>().CursorPosition;

                _ripples.Add(new Ripple
                {
                    StartUtc = DateTime.UtcNow,
                    Location = loc,
                    Buttons = E.Button
                });

                // 限制队列长度，避免疯狂连击堆积
                if (_ripples.Count > 8)
                    _ripples.RemoveRange(0, _ripples.Count - 8);
            };
        }

        public void Dispose() { }

        public void Draw(IEditableFrame Editor, Func<Point, Point> PointTransform = null)
        {
            if (!_settings.Display || _ripples.Count == 0)
                return;

            var now = DateTime.UtcNow;
            var baseRadius = _settings.Radius;

            // 倒序遍历以便安全移除已完成的涟漪
            for (int i = _ripples.Count - 1; i >= 0; i--)
            {
                var r = _ripples[i];
                var elapsedMs = (now - r.StartUtc).TotalMilliseconds;

                if (elapsedMs >= RippleDurationMs)
                {
                    _ripples.RemoveAt(i);
                    continue;
                }

                var progress = elapsedMs / RippleDurationMs; // 0 → 1

                // 扩散半径：从 0.3 倍到 2.0 倍
                var radius = baseRadius * (0.3f + (float)progress * 1.7f);

                // 透明度：从 0.9 衰减到 0（前 15% 淡入，后 85% 淡出）
                float alpha;
                if (progress < 0.15)
                    alpha = (float)(progress / 0.15) * 0.9f;
                else
                    alpha = 0.9f * (1 - (float)((progress - 0.15) / 0.85));

                var loc = r.Location;
                if (PointTransform != null)
                    loc = PointTransform(loc);

                var color = GetClickCircleColor(r.Buttons);
                color = Color.FromArgb(ToByte(color.A * alpha), color);

                var d = radius * 2;
                var x = loc.X - radius;
                var y = loc.Y - radius;

                // 实心半透明圆
                Editor.FillEllipse(color, new RectangleF(x, y, d, d));

                // 圆环边框（更聚焦的视觉强调）
                var border = _settings.BorderThickness;
                if (border > 0)
                {
                    var borderColor = _settings.BorderColor;
                    borderColor = Color.FromArgb(ToByte(borderColor.A * alpha), borderColor);

                    Editor.DrawEllipse(borderColor, border, new RectangleF(x, y, d, d));
                }
            }
        }

        Color GetClickCircleColor(MouseButtons Buttons)
        {
            if (Buttons.HasFlag(MouseButtons.Right))
                return _settings.RightClickColor;

            if (Buttons.HasFlag(MouseButtons.Middle))
                return _settings.MiddleClickColor;

            return _settings.Color;
        }

        static byte ToByte(double Value)
        {
            if (Value > 255) return 255;
            if (Value < 0) return 0;
            return (byte)Value;
        }

        struct Ripple
        {
            public DateTime StartUtc;
            public Point Location;
            public MouseButtons Buttons;
        }
    }
}
