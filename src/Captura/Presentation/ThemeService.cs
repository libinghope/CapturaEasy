using System.Windows;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace Captura
{
    /// <summary>
    /// 主题服务：替代 ModernUI AppearanceManager。
    /// ApplyTheme 通过修改 BundledTheme.BaseTheme 切换暗/亮。
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
        }

        // 应用强调色：直接更新 CapturaBridge 中定义的 Accent 相关 Brush
        public static void ApplyAccent(Color AccentColor)
        {
            Application.Current.Resources["Accent"] = new SolidColorBrush(AccentColor);
            Application.Current.Resources["AccentAlt"] = new SolidColorBrush(AccentColor);
            Application.Current.Resources["AccentColor"] = AccentColor;
            Application.Current.Resources["LinkText"] = new SolidColorBrush(AccentColor);
            Application.Current.Resources["LinkButtonText"] = new SolidColorBrush(AccentColor);
        }
    }
}
