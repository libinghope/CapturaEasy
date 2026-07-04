using System;
using Captura.FFmpeg;

namespace Captura.Fakes
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class FakeFFmpegViewsProvider : IFFmpegViewsProvider
    {
        public void ShowLogs()
        {
        }

        public void ShowUnavailable()
        {
            Console.Error.WriteLine("FFmpeg is not available.");
        }
    }
}
