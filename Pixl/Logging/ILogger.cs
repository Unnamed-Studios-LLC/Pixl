namespace Pixl;

internal interface ILogger
{
    void Flush();
    void Log(object @object);
}
