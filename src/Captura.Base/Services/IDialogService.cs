namespace Captura
{
    public interface IDialogService
    {
        string PickFolder(string Current, string Description);

        string PickFile(string InitialFolder, string Description);

        // 弹出保存文件对话框。返回完整路径，取消则返回 null。
        string SaveFile(string InitialFileName, string DefaultExt, string Filter);
    }
}