namespace Captura.Audio
{
    /// <summary>
    /// 音频音质档位预设。选择档位会自动设置对应的 Quality 值；
    /// 手动调整 Quality 后会自动切回 <see cref="Custom"/>。
    /// </summary>
    public enum AudioQualityPreset
    {
        /// <summary>标准（Quality=60，语音为主，体积小）</summary>
        Standard = 60,

        /// <summary>高清（Quality=80，默认，兼顾语音与音乐）</summary>
        HD = 80,

        /// <summary>无损（Quality=100，最高码率）</summary>
        Lossless = 100,

        /// <summary>自定义（用户手动调整 Quality slider）</summary>
        Custom = 0
    }
}
