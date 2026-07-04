using System.Windows;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace Captura
{
    /// <summary>
    /// 主题服务：替代 ModernUI AppearanceManager。
    /// ApplyTheme 通过修改 BundledTheme.BaseTheme 切换暗/亮，同时同步 CapturaBridge 调色板。
    /// ApplyAccent 直接更新 CapturaBridge 中的 Accent SolidColorBrush。
    /// </summary>
    public static class ThemeService
    {
        // 应用主题（true=暗色，false=亮色）
        public static void ApplyTheme(bool IsDark)
        {
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
            {
                if (dict is BundledTheme bt)
                {
                    bt.BaseTheme = IsDark ? BaseTheme.Dark : BaseTheme.Light;
                    break;
                }
            }

            ApplyBridgePalette(IsDark);
        }

        // 同步 CapturaBridge 调色板（DynamicResource 键）
        static void ApplyBridgePalette(bool IsDark)
        {
            var res = Application.Current.Resources;

            if (IsDark)
            {
                res["WindowBackgroundColor"] = Color.FromArgb(255, 33, 33, 33);
                res["WindowBackground"] = new SolidColorBrush(Color.FromRgb(0x21, 0x21, 0x21));
                res["WindowBackgroundAlt"] = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30));
                res["WindowText"] = new SolidColorBrush(Color.FromArgb(0xDE, 0xFF, 0xFF, 0xFF));
                res["WindowTextSecondary"] = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF));
                res["ItemBackground"] = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42));
                res["ItemBackgroundAlt"] = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42));
                res["ItemBorder"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                res["ItemText"] = new SolidColorBrush(Color.FromArgb(0xDE, 0xFF, 0xFF, 0xFF));
                res["ModernButtonText"] = new SolidColorBrush(Color.FromArgb(0xDE, 0xFF, 0xFF, 0xFF));
                res["ModernButtonTextHover"] = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                res["ModernButtonTextDisabled"] = new SolidColorBrush(Color.FromArgb(0x61, 0xFF, 0xFF, 0xFF));
                res["ModernButtonBorder"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
                res["ButtonBackgroundHover"] = new SolidColorBrush(Color.FromRgb(0x50, 0x50, 0x50));
                res["ButtonText"] = new SolidColorBrush(Color.FromArgb(0xDE, 0xFF, 0xFF, 0xFF));
                res["ButtonTextDisabled"] = new SolidColorBrush(Color.FromArgb(0x61, 0xFF, 0xFF, 0xFF));
                res["RegionBorder"] = new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55));
            }
            else
            {
                res["WindowBackgroundColor"] = Color.FromArgb(255, 255, 255, 255);
                res["WindowBackground"] = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
                res["WindowBackgroundAlt"] = new SolidColorBrush(Color.FromRgb(0xF5, 0xF5, 0xF5));
                res["WindowText"] = new SolidColorBrush(Color.FromArgb(0xDE, 0x21, 0x21, 0x21));
                res["WindowTextSecondary"] = new SolidColorBrush(Color.FromArgb(0x99, 0x75, 0x75, 0x75));
                res["ItemBackground"] = new SolidColorBrush(Color.FromRgb(0xFA, 0xFA, 0xFA));
                res["ItemBackgroundAlt"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                res["ItemBorder"] = new SolidColorBrush(Color.FromRgb(0xBD, 0xBD, 0xBD));
                res["ItemText"] = new SolidColorBrush(Color.FromArgb(0xDE, 0x21, 0x21, 0x21));
                res["ModernButtonText"] = new SolidColorBrush(Color.FromArgb(0xDE, 0x21, 0x21, 0x21));
                res["ModernButtonTextHover"] = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
                res["ModernButtonTextDisabled"] = new SolidColorBrush(Color.FromArgb(0x61, 0x75, 0x75, 0x75));
                res["ModernButtonBorder"] = new SolidColorBrush(Color.FromRgb(0xBD, 0xBD, 0xBD));
                res["ButtonBackgroundHover"] = new SolidColorBrush(Color.FromRgb(0xE0, 0xE0, 0xE0));
                res["ButtonText"] = new SolidColorBrush(Color.FromArgb(0xDE, 0x21, 0x21, 0x21));
                res["ButtonTextDisabled"] = new SolidColorBrush(Color.FromArgb(0x61, 0x75, 0x75, 0x75));
                res["RegionBorder"] = new SolidColorBrush(Color.FromRgb(0xBD, 0xBD, 0xBD));
            }
        }

        // 应用强调色：直接更新 CapturaBridge 中定义的 Accent 相关 Brush
        public static void ApplyAccent(Color AccentColor)
        {
            Application.Current.Resources["Accent"] = new SolidColorBrush(AccentColor);
            // 深一档辅助色（比主色暗约 25%）
            var alt = Color.FromArgb(AccentColor.A,
                (byte)(AccentColor.R * 0.7),
                (byte)(AccentColor.G * 0.7),
                (byte)(AccentColor.B * 0.7));
            Application.Current.Resources["AccentAlt"] = new SolidColorBrush(alt);
            Application.Current.Resources["AccentColor"] = AccentColor;
            Application.Current.Resources["LinkText"] = new SolidColorBrush(AccentColor);
            Application.Current.Resources["LinkButtonText"] = new SolidColorBrush(AccentColor);
        }
    }
}
