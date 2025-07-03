using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace ReactorControl.Classes;

ref struct BinaryReader(ReadOnlySpan<byte> data)
{
    private readonly ReadOnlySpan<byte> _data = data;
    private int _position = 0;

    public T ReadUnManaged<T>() where T : unmanaged
    {
        int size = Unsafe.SizeOf<T>();
        if (_position + size > _data.Length)
            throw new InvalidOperationException("Not enough data to read unmanaged type.");

        T value = MemoryMarshal.Read<T>(_data.Slice(_position, size));
        _position += size;
        return value;
    }
    public byte ReadByte()
    {
        return ReadUnManaged<byte>();
    }

    public float ReadFloat()
    {
        return ReadUnManaged<float>();
    }
    public ushort ReadUInt16()
    {
        const int size = sizeof(ushort);
        if (_position + size > _data.Length)
            throw new InvalidOperationException("Not enough data to read UInt16.");

        ushort value = BinaryPrimitives.ReadUInt16LittleEndian(_data.Slice(_position, size));
        _position += size;
        return value;
    }
}