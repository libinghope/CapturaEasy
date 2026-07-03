using System;
using System.Windows;
using Captura.Audio;
using Captura.Loc;
using Captura.Views;

namespace Captura.Bootstrap
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MessageProvider : IMessageProvider
    {
        readonly IAudioPlayer _audioPlayer;
        readonly ILocalizationProvider _loc;

        public MessageProvider(IAudioPlayer AudioPlayer, ILocalizationProvider Loc)
        {
            _audioPlayer = AudioPlayer;
            _loc = Loc;
        }

        public void ShowError(string Message, string Header = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _audioPlayer.Play(SoundKind.Error);

                var fullMessage = string.IsNullOrEmpty(Header)
                    ? Message
                    : $"{Header}\n\n{Message}";

                MessageBox.Show(fullMessage, _loc.ErrorOccurred, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public void ShowInfo(string Message, string Header = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(Message, Header ?? "Captura", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public void ShowException(Exception Exception, string Message, bool Blocking = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var win = new ErrorWindow(Exception, Message);

                _audioPlayer.Play(SoundKind.Error);

                if (Blocking)
                {
                    win.ShowDialog();
                }
                else win.ShowAndFocus();
            });
        }

        public bool ShowYesNo(string Message, string Title)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show(Message, Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            });
        }
    }
}
