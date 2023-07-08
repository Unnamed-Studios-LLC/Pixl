namespace Pixl.Editor;

public class EditorException : Exception
{
    public EditorException(string? message, string? details = null, Exception? innerException = null) : base(message, innerException)
    {
        Details = details;
    }

    public string? Details { get; }
}
