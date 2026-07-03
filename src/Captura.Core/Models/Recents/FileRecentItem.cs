using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Captura.FFmpeg;
using Captura.Loc;
using Captura.Video;

namespace Captura.Models
{
    public class FileRecentItem : NotifyPropertyChanged, IRecentItem
    {
        string _fileName;

        public string FileName
        {
            get => _fileName;
            private set
            {
                Set(ref _fileName, value);

                Display = Path.GetFileName(value);
            }
        }

        public RecentFileType FileType { get; }

        public FileRecentItem(string FileName, RecentFileType FileType, bool IsSaving = false)
        {
            this.FileName = FileName;
            this.FileType = FileType;
            this.IsSaving = IsSaving;

            ClickCommand = new DelegateCommand(() => ServiceProvider.LaunchFile(new ProcessStartInfo(this.FileName)));

            RemoveCommand = new DelegateCommand(() => RemoveRequested?.Invoke());

            var icons = ServiceProvider.Get<IIconSet>();
            var loc = ServiceProvider.Get<ILocalizationProvider>();
            var windowService = ServiceProvider.Get<IMainWindow>();

            Icon = GetIcon(FileType, icons);
            IconColor = GetColor(FileType);

            var list = new List<RecentAction>
            {
                new RecentAction(loc.CopyPath, icons.Clipboard, () => this.FileName.WriteToClipboard())
            };

            void AddTrimMedia()
            {
                list.Add(new RecentAction(loc.Trim, icons.Trim, () => windowService.TrimMedia(this.FileName)));
            }

            switch (FileType)
            {
                case RecentFileType.Image:
                    list.Add(new RecentAction(loc.CopyToClipboard, icons.Clipboard, OnCopyToClipboardExecute));
                    list.Add(new RecentAction(loc.UploadToImgur, icons.Upload, OnUploadToImgurExecute));
                    list.Add(new RecentAction(loc.Edit, icons.Pencil, () => windowService.EditImage(this.FileName)));
                    break;

                case RecentFileType.Audio:
                    AddTrimMedia();
                    break;

                case RecentFileType.Video:
                    AddTrimMedia();
                    list.Add(new RecentAction("Convert to GIF", icons.Gif, OnConvertToGif));
                    list.Add(new RecentAction("Compress", icons.Compress, OnCompress));
                    list.Add(new RecentAction("Upload to YouTube", icons.YouTube, () => windowService.UploadToYouTube(this.FileName)));
                    break;
            }

            list.Add(new RecentAction(loc.Delete, icons.Delete, OnDelete));

            Actions = list;

            // 缩略图：图片直接使用文件路径，视频异步抽首帧
            if (FileType == RecentFileType.Image)
            {
                ThumbnailPath = FileName;
            }
            else if (FileType == RecentFileType.Video)
            {
                GenerateVideoThumbnailAsync();
            }
        }

        async void OnUploadToImgurExecute()
        {
            if (!File.Exists(FileName))
            {
                ServiceProvider.MessageProvider.ShowError("File not Found");

                return;
            }

            var imgSystem = ServiceProvider.Get<IImagingSystem>();

            using var img = imgSystem.LoadBitmap(FileName);
            await img.UploadImage();
        }

        void OnCopyToClipboardExecute()
        {
            if (!File.Exists(FileName))
            {
                ServiceProvider.MessageProvider.ShowError("File not Found");

                return;
            }

            try
            {
                var clipboard = ServiceProvider.Get<IClipboardService>();

                var imgSystem = ServiceProvider.Get<IImagingSystem>();

                using var img = imgSystem.LoadBitmap(FileName);
                clipboard.SetImage(img);
            }
            catch (Exception e)
            {
                ServiceProvider.MessageProvider.ShowException(e, "Copy to Clipboard failed");
            }
        }

        /// <summary>
        /// 通用视频转换：通过已注册的 IVideoConverter（FFmpeg）执行。
        /// converterName 为 null 时按 Name 匹配（含关键字），否则精确匹配。
        /// </summary>
        async void ConvertVideo(Func<IVideoConverter, bool> Matcher,
            string TargetExtension,
            string DialogTitle,
            int VideoQuality = 70)
        {
            if (!File.Exists(FileName))
            {
                ServiceProvider.MessageProvider.ShowError("File not Found");
                return;
            }

            var converters = ServiceProvider.Get<IEnumerable<IVideoConverter>>();
            var converter = converters.FirstOrDefault(Matcher);

            if (converter == null)
            {
                ServiceProvider.MessageProvider.ShowError("No suitable converter available (FFmpeg required).");
                return;
            }

            // 弹保存对话框
            var dialogService = ServiceProvider.Get<IDialogService>();
            var baseName = Path.GetFileNameWithoutExtension(FileName);
            var outName = dialogService.SaveFile(
                $"{baseName}{TargetExtension}",
                TargetExtension.TrimStart('.'),
                $"*{TargetExtension}|*{TargetExtension}");

            if (string.IsNullOrEmpty(outName))
                return;

            IsSaving = true;

            try
            {
                var progress = new Progress<int>();

                await converter.StartAsync(new VideoConverterArgs
                {
                    InputFile = FileName,
                    FileName = outName,
                    VideoQuality = VideoQuality,
                    FrameRate = 15
                }, progress);

                ServiceProvider.MessageProvider.ShowInfo($"{DialogTitle} complete: {outName}");
            }
            catch (FFmpegNotFoundException)
            {
                ServiceProvider.Get<IFFmpegViewsProvider>()?.ShowUnavailable();
            }
            catch (Exception e)
            {
                ServiceProvider.MessageProvider.ShowException(e, $"{DialogTitle} failed");
            }
            finally
            {
                IsSaving = false;
            }
        }

        // 视频 → GIF
        void OnConvertToGif()
            => ConvertVideo(C => C.Extension.Equals(".gif", StringComparison.OrdinalIgnoreCase),
                ".gif", "GIF conversion");

        // 视频压缩（x264 + 较低画质 → 更小文件）
        void OnCompress()
            => ConvertVideo(C => C.Name.IndexOf("H.264", StringComparison.OrdinalIgnoreCase) >= 0,
                ".mp4", "Compression", VideoQuality: 35);

        /// <summary>
        /// 异步用 FFmpeg 抽取视频首帧作为缩略图。
        /// 失败时静默（ThumbnailPath 保持 null）。
        /// </summary>
        async void GenerateVideoThumbnailAsync()
        {
            if (!FFmpegService.FFmpegExists || !File.Exists(FileName))
                return;

            var thumbPath = Path.Combine(Path.GetTempPath(),
                $"captura_thumb_{Path.GetFileNameWithoutExtension(FileName)}_{Guid.NewGuid():N}.png");

            try
            {
                var args = new FFmpegArgsBuilder();
                var input = args.AddInputFile(FileName);
                input.AddArg("ss", "0");
                args.AddOutputFile(thumbPath)
                    .AddArg("vframes", "1")
                    .AddArg("vf", "\"scale=240:-1\"");

                var process = FFmpegService.StartFFmpeg(args.GetArgs(), thumbPath, out _);
                await Task.Run(() => process.WaitForExit());

                if (process.ExitCode == 0 && File.Exists(thumbPath))
                    ThumbnailPath = thumbPath;
                else
                    TryDelete(thumbPath);
            }
            catch
            {
                TryDelete(thumbPath);
            }
        }

        static void TryDelete(string Path)
        {
            try { File.Delete(Path); } catch { /* 忽略 */ }
        }

        void OnDelete()
        {
            if (File.Exists(FileName))
            {
                var platformServices = ServiceProvider.Get<IPlatformServices>();

                if (!platformServices.DeleteFile(FileName))
                    return;
            }

            // Remove from List
            RemoveRequested?.Invoke();
        }

        static string GetIcon(RecentFileType ItemType, IIconSet Icons)
        {
            switch (ItemType)
            {
                case RecentFileType.Audio:
                    return Icons.Music;

                case RecentFileType.Image:
                    return Icons.Image;

                case RecentFileType.Video:
                    return Icons.Video;
            }

            return null;
        }

        static string GetColor(RecentFileType ItemType)
        {
            switch (ItemType)
            {
                case RecentFileType.Audio:
                    return "DodgerBlue";

                case RecentFileType.Image:
                    return "YellowGreen";

                case RecentFileType.Video:
                    return "OrangeRed";
            }

            return null;
        }

        string _display;

        public string Display
        {
            get => _display;
            private set => Set(ref _display, value);
        }

        public string Icon { get; }
        public string IconColor { get; }

        string _thumbnailPath;
        public string ThumbnailPath
        {
            get => _thumbnailPath;
            private set => Set(ref _thumbnailPath, value);
        }

        bool _saving;

        public bool IsSaving
        {
            get => _saving;
            private set => Set(ref _saving, value);
        }

        public void Saved()
        {
            IsSaving = false;
        }

        public void Converted(string NewFileName)
        {
            FileName = NewFileName;
        }

        public event Action RemoveRequested;

        public ICommand ClickCommand { get; }
        public ICommand RemoveCommand { get; }

        public IEnumerable<RecentAction> Actions { get; }
    }
}