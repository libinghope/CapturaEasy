// Adapted from https://github.com/jasonpang/desktop-duplication-net

using SharpDX.DXGI;
using System;
using Captura.Video;
using Captura.Windows.DirectX;

namespace Captura.Windows.DesktopDuplication
{
    public class DesktopDuplicator : IDisposable
    {
        Direct2DEditorSession _editorSession;
        DxMousePointer _mousePointer;
        DuplCapture _duplCapture;

        public DesktopDuplicator(bool IncludeCursor, Output1 Output, IPreviewWindow PreviewWindow)
        {
            _duplCapture = new DuplCapture(Output);

            var bounds = Output.Description.DesktopBounds;
            var width = bounds.Right - bounds.Left;
            var height = bounds.Bottom - bounds.Top;
            
            _editorSession = new Direct2DEditorSession(width, height, PreviewWindow);

            if (IncludeCursor)
                _mousePointer = new DxMousePointer(_editorSession);
        }
        
        public IEditableFrame Capture()
        {
            // Dispose 可能与 Capture 并发执行（停止录制时的竞态）
            var dupl = _duplCapture;
            var editor = _editorSession;
            if (dupl == null || editor == null)
                return RepeatFrame.Instance;

            try
            {
                if (!dupl.Get(editor.DesktopTexture, _mousePointer))
                    return RepeatFrame.Instance;
            }
            catch
            {
                try { dupl.Init(); }
                catch
                {
                    // ignored
                }

                return RepeatFrame.Instance;
            }

            var ed = new Direct2DEditor(editor);

            _mousePointer?.Draw(ed);

            return ed;
        }

        public void Dispose()
        {
            try { _duplCapture.Dispose(); }
            catch { }
            finally { _duplCapture = null; }

            // Mouse Pointer disposed later to prevent errors.
            try { _mousePointer?.Dispose(); }
            catch { }
            finally { _mousePointer = null; }

            try { _editorSession.Dispose(); }
            catch { }
            finally { _editorSession = null; }
        }
    }
}
