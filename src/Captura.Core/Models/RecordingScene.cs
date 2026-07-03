using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;

namespace Captura.Models
{
    /// <summary>
    /// 录制场景/预设。保存一组录制配置的快照，可一键切换。
    /// 快照内容为 Settings.Video + Settings.Audio 子树 + IncludeCursor。
    /// </summary>
    public class RecordingScene
    {
        /// <summary>场景名称（用户可读）</summary>
        public string Name { get; set; }

        /// <summary>VideoSettings 子树的 JSON 快照</summary>
        public JObject Video { get; set; }

        /// <summary>AudioSettings 子树的 JSON 快照</summary>
        public JObject Audio { get; set; }

        /// <summary>是否包含鼠标光标</summary>
        public bool IncludeCursor { get; set; }
    }
}
