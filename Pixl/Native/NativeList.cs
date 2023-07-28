using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pixl;

public unsafe struct NativeList<T> : IDisposable where T : unmanaged
{
    private nint _pointer;
    private int _length;
    private int _capacity;

    public NativeList(int capacity)
    {
        _capacity = capacity;
        _pointer = Marshal.AllocHGlobal(_capacity * sizeof(T));
    }

    public int Length { get; private set; }

    private T* Data => (T*)_pointer.ToPointer();

    public ref T this[int index]
    {
        get
        {
            if (index < 0 || index >= _length) throw new ArgumentOutOfRangeException(nameof(index));
            return ref Unsafe.AsRef<T>(Data + index);
        }
    }

    public void Add(T item) => Add(ref item);
    public void Add(ref T item)
    {
        if (_pointer == nint.Zero)
        {
            _capacity = 1;
            _pointer = Marshal.AllocHGlobal(_capacity * sizeof(T));
            _length = 0;
        }

        if (_length == _capacity)
        {
            Grow();
        }

        Data[_length++] = item;
    }

    public Span<T> AsSpan() => new(_pointer.ToPointer(), _length);

    public void Dispose()
    {
        if (_pointer == nint.Zero) return;
        Marshal.FreeHGlobal(_pointer);
        _pointer = nint.Zero;
    }

    public void RemoveAtSwapBack(int index)
    {
        if (index < 0 || index >= _length) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == --_length) return;
        Data[index] = Data[_length + 1];
    }

    public void Resize(int capacity)
    {
        if (capacity <= 0) throw new ArgumentException("Value cannot be negative", nameof(capacity));
        if (_capacity == capacity) return;

        var newPointer = Marshal.AllocHGlobal(capacity * sizeof(T));
        var dataSize = Math.Min(_length, capacity) * sizeof(T);
        Buffer.MemoryCopy(_pointer.ToPointer(), newPointer.ToPointer(), capacity * sizeof(T), dataSize);
        Marshal.FreeHGlobal(_pointer);
        _capacity = capacity;
        _pointer = newPointer;
    }

    private void Grow()
    {
        var newCapacity = _capacity * 2;
        var newPointer = Marshal.AllocHGlobal(newCapacity * sizeof(T));

        var dataSize = _length * sizeof(T);
        Buffer.MemoryCopy(_pointer.ToPointer(), newPointer.ToPointer(), newCapacity * sizeof(T), dataSize);
        Marshal.FreeHGlobal(_pointer);
        _capacity = newCapacity;
        _pointer = newPointer;
    }
}
