using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;

namespace Captura.FFmpeg
{
    public static class FFmpegService
    {
        const string FFmpegExeName = "ffmpeg.exe";

        public static bool FFmpegExists => File.Exists(FFmpegExePath);

        public static string FFmpegExePath
        {
            get
            {
                // 内置 FFmpeg：优先查找 Codecs 子目录（构建时由 Captura.csproj 复制）
                return new[] { ServiceProvider.AppDir, ServiceProvider.LibDir }
                           .Where(M => M != null)
                           .Select(M => Path.Combine(M, "Codecs", FFmpegExeName))
                           .FirstOrDefault(File.Exists)
                       ?? FFmpegExeName;
            }
        }

        public static Process StartFFmpeg(string Arguments, string FileName, out IFFmpegLogEntry FFmpegLog)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = FFmpegExePath,
                    Arguments = Arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                },
                EnableRaisingEvents = true
            };

            var log = ServiceProvider.Get<IFFmpegLogRepository>();

            var logItem = log.CreateNew(Path.GetFileName(FileName), Arguments);
            FFmpegLog = logItem;

            process.ErrorDataReceived += (S, E) => logItem.Write(E.Data);

            process.Start();

            process.BeginErrorReadLine();

            return process;
        }

        public static bool WaitForConnection(this NamedPipeServerStream ServerStream, int Timeout)
        {
            var asyncResult = ServerStream.BeginWaitForConnection(Ar => {}, null);

            if (asyncResult.AsyncWaitHandle.WaitOne(Timeout))
            {
                ServerStream.EndWaitForConnection(asyncResult);

                return ServerStream.IsConnected;
            }

            return false;
        }
    }
}
