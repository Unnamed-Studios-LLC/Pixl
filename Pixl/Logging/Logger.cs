namespace Pixl;

public abstract class Logger
{
    public abstract void Flush();
    public abstract void Log(object @object);
}
