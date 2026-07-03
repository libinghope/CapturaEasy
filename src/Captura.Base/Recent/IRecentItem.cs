using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Captura
{
    public interface IRecentItem
    {
        string Display { get; }

        string Icon { get; }

        string IconColor { get; }

        bool IsSaving { get; }

        // 缩略图路径（图片/视频有缩略图；上传/音频为 null）
        string ThumbnailPath { get; }

        event Action RemoveRequested;

        ICommand ClickCommand { get; }
        ICommand RemoveCommand { get; }

        IEnumerable<RecentAction> Actions { get; }
    }
}