using System.Windows;
using Captura.Audio;

namespace Captura.FFmpeg
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FFmpegViewsProvider : IFFmpegViewsProvider
    {
        readonly IAudioPlayer _audioPlayer;

        public FFmpegViewsProvider(IAudioPlayer AudioPlayer)
        {
            _audioPlayer = AudioPlayer;
        }

        public void ShowLogs()
        {
            SettingsWindow.ShowFFmpegLogs();
        }

        public void ShowUnavailable()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _audioPlayer.Play(SoundKind.Error);

                MessageBox.Show("FFmpeg was not found.",
                    "FFmpeg Unavailable",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            });
        }
    }
}
