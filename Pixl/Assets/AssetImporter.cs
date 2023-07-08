using System.Threading.Tasks;

namespace Pixl;

public abstract class AssetImporter
{
    protected abstract void Import(string fileName, Stream stream, Action<Resource> outputFunc);
}
