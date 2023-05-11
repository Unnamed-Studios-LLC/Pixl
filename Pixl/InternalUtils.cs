using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Pixl.Demo")]
[assembly: InternalsVisibleTo("Pixl.Player.Win")]

namespace Pixl
{
    internal static class InternalUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CallUserMethod(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                // TODO log error
            }
        }
    }
}
