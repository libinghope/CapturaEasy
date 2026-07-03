using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using Captura.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;

namespace Captura.ViewModels
{
    /// <summary>
    /// 录制场景/预设管理。支持把当前录制配置保存为命名场景，
    /// 并一键切换（写回 Settings）。
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SceneViewModel
    {
        readonly SceneRepository _repo;
        readonly Settings _settings;

        public SceneViewModel(SceneRepository Repo, Settings Settings)
        {
            _repo = Repo;
            _settings = Settings;
            Scenes = _repo.Scenes;

            SelectedScene = new ReactiveProperty<RecordingScene>();
            // 选中场景变化时自动切换配置
            SelectedScene.Where(S => S != null).Subscribe(S => OnApply(S));

            SaveCurrentAsCommand = new ReactiveCommand<string>()
                .WithSubscribe(OnSaveCurrentAs);

            DeleteCommand = new ReactiveCommand<RecordingScene>()
                .WithSubscribe(OnDelete);
        }

        public ObservableCollection<RecordingScene> Scenes { get; }

        /// <summary>当前选中场景。变化时自动应用配置。</summary>
        public ReactiveProperty<RecordingScene> SelectedScene { get; }

        public ICommand SaveCurrentAsCommand { get; }

        public ICommand DeleteCommand { get; }

        void OnSaveCurrentAs(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return;

            var scene = new RecordingScene
            {
                Name = Name,
                Video = JObject.FromObject(_settings.Video),
                Audio = JObject.FromObject(_settings.Audio),
                IncludeCursor = _settings.IncludeCursor
            };

            _repo.Scenes.Add(scene);
        }

        void OnApply(RecordingScene Scene)
        {
            if (Scene == null)
                return;

            if (Scene.Video != null)
                JsonConvert.PopulateObject(Scene.Video.ToString(), _settings.Video);

            if (Scene.Audio != null)
                JsonConvert.PopulateObject(Scene.Audio.ToString(), _settings.Audio);

            _settings.IncludeCursor = Scene.IncludeCursor;
        }

        void OnDelete(RecordingScene Scene)
        {
            if (Scene != null)
                _repo.Scenes.Remove(Scene);
        }
    }
}
