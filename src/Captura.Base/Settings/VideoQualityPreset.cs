namespace Captura.Video
{
    /// <summary>
    /// 视频画质档位预设。选择档位会自动设置对应的 Quality 值；
    /// 手动调整 Quality 后会自动切回 <see cref="Custom"/>。
    /// </summary>
    public enum VideoQualityPreset
    {
        /// <summary>流畅（Quality=40，低码率，适合长录制/网课）</summary>
        Smooth = 40,

        /// <summary>高清（Quality=70，默认，平衡画质与体积）</summary>
        HD = 70,

        /// <summary>原画（Quality=95，高码率，适合游戏/演示）</summary>
        Original = 95,

        /// <summary>自定义（用户手动调整 Quality slider）</summary>
        Custom = 0
    }
}
