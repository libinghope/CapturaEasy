using System.Collections.ObjectModel;

namespace Captura.FFmpeg
{
    public class FFmpegSettings : PropertyStore
    {
        public ObservableCollection<FFmpegCodecSettings> CustomCodecs { get; } = new ObservableCollection<FFmpegCodecSettings>();

        public bool Resize
        {
            get => Get<bool>();
            set => Set(value);
        }

        public int ResizeWidth
        {
            get => Get(640);
            set => Set(value);
        }

        public int ResizeHeight
        {
            get => Get(480);
            set => Set(value);
        }

        public X264Settings X264 { get; } = new X264Settings();
    }
}
