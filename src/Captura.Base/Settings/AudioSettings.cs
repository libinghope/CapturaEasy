namespace Captura.Audio
{
    public class AudioSettings : PropertyStore
    {
        public bool SeparateFilePerSource
        {
            get => Get(false);
            set => Set(value);
        }

        public bool RecordMicrophone
        {
            get => Get(false);
            set => Set(value);
        }

        public string Microphone
        {
            get => Get("");
            set => Set(value);
        }

        public bool RecordSpeaker
        {
            get => Get(false);
            set => Set(value);
        }

        public string Speaker
        {
            get => Get("");
            set => Set(value);
        }

        public int Quality
        {
            get => Get(80);
            set
            {
                Set(value);

                // 同步档位：匹配预设值则选中对应档位，否则切回自定义
                SyncPreset(value, out var preset);
                Set(preset, nameof(QualityPreset));
            }
        }

        void SyncPreset(int quality, out AudioQualityPreset preset)
        {
            if (quality == (int)AudioQualityPreset.Standard) preset = AudioQualityPreset.Standard;
            else if (quality == (int)AudioQualityPreset.HD) preset = AudioQualityPreset.HD;
            else if (quality == (int)AudioQualityPreset.Lossless) preset = AudioQualityPreset.Lossless;
            else preset = AudioQualityPreset.Custom;
        }

        /// <summary>
        /// 音质档位。设为非 Custom 时会自动写入对应的 Quality 值。
        /// Quality 被手动修改后会自动切回 Custom。
        /// </summary>
        public AudioQualityPreset QualityPreset
        {
            get => Get(AudioQualityPreset.HD);
            set
            {
                Set(value);

                // 非自定义时，直接写入字典中的 Quality（绕过 Quality setter 避免递归）
                if (value != AudioQualityPreset.Custom)
                {
                    Set((int)value, nameof(Quality));
                }
            }
        }
    }
}