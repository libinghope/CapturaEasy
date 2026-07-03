using System.Windows;
using System.Windows.Media;
using Captura.ViewModels;

namespace Captura
{
    public partial class InterfacePage
    {
        void SelectedAccentColorChanged(object Sender, RoutedPropertyChangedEventArgs<Color?> E)
        {
            if (E.NewValue != null && DataContext is ViewModelBase vm)
            {
                ThemeService.ApplyAccent(E.NewValue.Value);

                vm.Settings.UI.AccentColor = E.NewValue.Value.ToString();
            }
        }

        void DarkThemeClick(object Sender, RoutedEventArgs E)
        {
            if (DataContext is ViewModelBase vm)
            {
                ThemeService.ApplyTheme(vm.Settings.UI.DarkTheme);
            }
        }
    }
}
