using System.Text;

namespace Pixl;

public abstract class Shader
{
    private readonly string _filePath;

    public Shader(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    internal byte[] GetBytes()
    {
        return Encoding.UTF8.GetBytes(File.ReadAllText(_filePath));
    }
}
