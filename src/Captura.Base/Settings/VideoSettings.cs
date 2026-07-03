namespace Captura.Video
{
    public class VideoSettings : PropertyStore
    {
        public string WriterKind
        {
            get => Get("");
            set => Set(value);
        }
        
        public string Writer
        {
            get => Get<string>();
            set => Set(value);
        }

        public bool PostConvert
        {
            get => Get<bool>();
            set => Set(value);
        }

        public string PostWriter
        {
            get => Get<string>();
            set => Set(value);
        }
        
        public string SourceKind
        {
            get => Get("");
            set => Set(value);
        }

        public string Source
        {
            get => Get<string>();
            set => Set(value);
        }

        public string Webcam
        {
            get => Get("");
            set => Set(value);
        }

        public int Quality
        {
            get => Get(70);
            set
            {
                Set(value);

                // 同步档位：匹配预设值则选中对应档位，否则切回自定义
                SyncPreset(value);
            }
        }

        void SyncPreset(int quality)
        {
            VideoQualityPreset preset;

            if (quality == (int)VideoQualityPreset.Smooth) preset = VideoQualityPreset.Smooth;
            else if (quality == (int)VideoQualityPreset.HD) preset = VideoQualityPreset.HD;
            else if (quality == (int)VideoQualityPreset.Original) preset = VideoQualityPreset.Original;
            else preset = VideoQualityPreset.Custom;

            Set(preset, nameof(QualityPreset));
        }

        /// <summary>
        /// 画质档位。设为非 Custom 时会自动写入对应的 Quality 值。
        /// Quality 被手动修改后会自动切回 Custom。
        /// </summary>
        public VideoQualityPreset QualityPreset
        {
            get => Get(VideoQualityPreset.HD);
            set
            {
                Set(value);

                // 非自定义时，直接写入字典中的 Quality（绕过 Quality setter 避免递归）
                if (value != VideoQualityPreset.Custom)
                {
                    Set((int)value, nameof(Quality));
                }
            }
        }

        public int FrameRate
        {
            get => Get(20);
            set => Set(value);
        }

        public bool FpsLimit
        {
            get => Get(true);
            set
            {
                Set(value);

                if (value && FrameRate > 30)
                {
                    FrameRate = 30;
                }
            }
        }

        public RecorderMode RecorderMode
        {
            get => Get(RecorderMode.Video);
            set => Set(value);
        }

        public int ReplayDuration
        {
            get => Get(20);
            set => Set(value);
        }
    }
}