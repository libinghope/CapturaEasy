using System.Drawing;

namespace Captura.MouseKeyHook
{
    public class MouseClickSettings : MouseOverlaySettings
    {
        // 重写 Display 默认值：点击高亮默认开启（基类 MouseOverlaySettings.Display 默认 false）
        public new bool Display
        {
            get => Get(true);
            set => Set(value);
        }

        public Color RightClickColor
        {
            get => Get(Color.FromArgb(3, 169, 244));
            set => Set(value);
        }

        public Color MiddleClickColor
        {
            get => Get(Color.FromArgb(76, 175, 80));
            set => Set(value);
        }

        public bool DisplayScroll
        {
            get => Get(true);
            set => Set(value);
        }

        public Color ScrollCircleColor
        {
            get => Get(Color.FromArgb(239, 83, 80));
            set => Set(value);
        }

        public Color ScrollArrowColor
        {
            get => Get(Color.FromArgb(33, 33, 33));
            set => Set(value);
        }
    }
}