using StudioSB.IO.Models;

namespace StudioSB.IO
{

    public interface IImportableModelType
    {
        string Name { get; }
        string Extension { get; }

        IOModel ImportIOModel(string FileName);
    }
}
