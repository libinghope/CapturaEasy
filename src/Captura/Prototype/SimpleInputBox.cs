using System.Windows;
using System.Windows.Controls;

namespace Captura.Prototype
{
    /// <summary>
    /// 极简 WPF 输入对话框（用于场景命名等单次输入场景）。
    /// </summary>
    public static class SimpleInputBox
    {
        public static string Show(Window Owner, string Title, string Prompt, string DefaultValue)
        {
            var textBox = new TextBox
            {
                Text = DefaultValue,
                Margin = new Thickness(0, 4, 0, 12)
            };

            textBox.SelectAll();
            textBox.Focus();

            var okBtn = new Button
            {
                Content = "确定",
                Width = 72,
                Height = 28,
                Margin = new Thickness(0, 0, 8, 0),
                IsDefault = true
            };

            var cancelBtn = new Button
            {
                Content = "取消",
                Width = 72,
                Height = 28,
                IsCancel = true
            };

            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            btnPanel.Children.Add(okBtn);
            btnPanel.Children.Add(cancelBtn);

            var stack = new StackPanel
            {
                Margin = new Thickness(16)
            };

            var promptText = new TextBlock { Text = Prompt };
            stack.Children.Add(promptText);
            stack.Children.Add(textBox);
            stack.Children.Add(btnPanel);

            var window = new Window
            {
                Title = Title,
                Width = 360,
                SizeToContent = SizeToContent.Height,
                Owner = Owner,
                Content = stack,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false
            };

            string result = null;
            okBtn.Click += (s, e) =>
            {
                result = textBox.Text;
                window.DialogResult = true;
                window.Close();
            };

            return window.ShowDialog() == true ? result : null;
        }
    }
}
