using System.Windows;
using Captura.Audio;
using Captura.Loc;
using Captura.Views;

namespace Captura.FFmpeg
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FFmpegViewsProvider : IFFmpegViewsProvider
    {
        readonly ILocalizationProvider _loc;
        readonly IAudioPlayer _audioPlayer;
        readonly FFmpegSettings _settings;
        readonly IDialogService _dialogService;

        public FFmpegViewsProvider(ILocalizationProvider Loc,
            IAudioPlayer AudioPlayer,
            FFmpegSettings Settings,
            IDialogService DialogService)
        {
            _loc = Loc;
            _audioPlayer = AudioPlayer;
            _settings = Settings;
            _dialogService = DialogService;
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

                var message = "FFmpeg was not found on your system.\n\n" +
                              $"Yes = {_loc.SelectFFmpegFolder}\n" +
                              $"No  = {_loc.DownloadFFmpeg}\n" +
                              "Cancel = Dismiss";

                var result = MessageBox.Show(message, "FFmpeg Unavailable",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                    PickFolder();
                else if (result == MessageBoxResult.No)
                    ShowDownloader();
            });
        }

        public void ShowDownloader()
        {
            FFmpegDownloaderWindow.ShowInstance();
        }

        public void PickFolder()
        {
            var folder = _dialogService.PickFolder(_settings.GetFolderPath(), _loc.SelectFFmpegFolder);

            if (!string.IsNullOrWhiteSpace(folder))
                _settings.FolderPath = folder;
        }
    }
}
