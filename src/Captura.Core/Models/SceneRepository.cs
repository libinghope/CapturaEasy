using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Captura.Models
{
    /// <summary>
    /// 录制场景仓储。读写 Scenes.json。
    /// 退出时（Dispose）自动持久化。
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SceneRepository : IDisposable
    {
        public ObservableCollection<RecordingScene> Scenes { get; } = new ObservableCollection<RecordingScene>();

        static string GetFilePath() => Path.Combine(ServiceProvider.SettingsDir, "Scenes.json");

        public SceneRepository()
        {
            Load();

            // 集合变化时自动保存
            Scenes.CollectionChanged += OnScenesChanged;
        }

        void OnScenesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Save();
        }

        void Load()
        {
            try
            {
                var path = GetFilePath();

                if (!File.Exists(path))
                    return;

                var json = File.ReadAllText(path);
                var arr = JArray.Parse(json);

                foreach (var token in arr)
                {
                    var scene = token.ToObject<RecordingScene>(
                        JsonSerializer.Create(JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings()));

                    if (scene != null)
                        Scenes.Add(scene);
                }
            }
            catch
            {
                // 静默忽略损坏的配置
            }
        }

        public void Save()
        {
            try
            {
                var settings = JsonConvert.DefaultSettings?.Invoke() ?? new JsonSerializerSettings();

                var json = JsonConvert.SerializeObject(Scenes, settings);

                File.WriteAllText(GetFilePath(), json);
            }
            catch
            {
                // 静默忽略写入失败
            }
        }

        public void Dispose()
        {
            Scenes.CollectionChanged -= OnScenesChanged;
            Save();
        }
    }
}
