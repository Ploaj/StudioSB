using StudioSB.IO.Models;

namespace StudioSB.IO
{
    public interface IExportableModelType
    {
        string Name { get; }
        string Extension { get; }

        void ExportIOModel(string FileName, IOModel model);
    }
}
