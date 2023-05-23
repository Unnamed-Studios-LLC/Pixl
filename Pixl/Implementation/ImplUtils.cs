using System;
using System.Runtime.CompilerServices;

namespace Pixl;

internal static class ImplUtils
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
            //Debug.Log(e);
        }
    }
}
