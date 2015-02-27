using Sentinel.Views.Interfaces;

namespace Sentinel.Logs.Interfaces
{
    public interface ILogFileExporter
    {
        void SaveLogViewerToFile(IWindowFrame windowFrame, string filePath);
    }
}
