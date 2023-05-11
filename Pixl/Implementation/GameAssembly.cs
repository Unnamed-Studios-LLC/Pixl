using System.Reflection;

namespace Pixl
{
    internal sealed class GameAssembly
    {
        public GameAssembly(Assembly assembly)
        {
            var entryType = typeof(IGameEntry);
            IGameEntry? entry = null;
            foreach (var type in assembly.GetTypes())
            {
                if (entry == null &&
                    entryType.IsAssignableFrom(type))
                {
                    entry = Activator.CreateInstance(type, true) as IGameEntry;
                }
            }

            Entry = entry ?? new DefaultEntry();
        }

        public IGameEntry Entry { get; }
    }
}
