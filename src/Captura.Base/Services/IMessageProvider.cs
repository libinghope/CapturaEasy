using System;

namespace Captura
{
    public interface IMessageProvider
    {
        void ShowError(string Message, string Header = null);

        // 普通提示（如转换完成）
        void ShowInfo(string Message, string Header = null);

        bool ShowYesNo(string Message, string Title);

        void ShowException(Exception Exception, string Message, bool Blocking = false);
    }
}
