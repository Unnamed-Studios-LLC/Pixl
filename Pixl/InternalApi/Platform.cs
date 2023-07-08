namespace Pixl;

internal abstract class Platform
{
    public abstract Window CreateWindow(string title, Int2 size);
    public abstract Values CreateValues(string companyName, string productName);
}
