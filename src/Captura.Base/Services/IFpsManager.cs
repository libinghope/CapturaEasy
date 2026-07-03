namespace Captura
{
    public interface IFpsManager
    {
        void OnFrame();

        int Fps { get; }
    }
}