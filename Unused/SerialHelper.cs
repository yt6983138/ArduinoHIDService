using System.Runtime.InteropServices;

namespace ArduinoHIDService.Unused;

public interface IOperations
{
    public void Execute();
}
[StructLayout(LayoutKind.Explicit)]
public struct MouseOperation : IOperations
{
    [FieldOffset(0)]
    public int X;
    [FieldOffset(4)]
    public int Y;
    [FieldOffset(8)]
    public int ActionFlags;
    [FieldOffset(12)]
    public int ActionData;

    public void Execute() => HIDOperationsHelper.MouseEvent((HIDOperationsHelper.MouseEventFlags)ActionFlags, X, Y, ActionData);
}
[StructLayout(LayoutKind.Explicit)]
public struct KeyboardOperation : IOperations
{
    [FieldOffset(0)]
    public byte Key;
    [FieldOffset(1)]
    public bool PressOrRelease;
    [FieldOffset(2)]
    public byte Scan;
    public KeyboardOperation() => Scan = 0;

    public void Execute() => HIDOperationsHelper.KeyboardEvent(Key, PressOrRelease, Scan);
}
public enum Identifier
{
    Mouse = 0b0,
    Keyboard = 0b1
}
public static class SerialHelper
{
    public const int BlockSizeBytes = 24;

    public static byte[] StructToBytes<T>(T structure) where T : struct
    {
        int size = Marshal.SizeOf(structure);
        byte[] output = new byte[size];

        IntPtr pointer = IntPtr.Zero;
        try
        {
            pointer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(structure, pointer, true);
            Marshal.Copy(pointer, output, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
        return output;
    }
    public static T ByteToStruct<T>(byte[] bytes) where T : struct
    {
        T str = new();

        int size = Marshal.SizeOf(str);
        IntPtr pointer = IntPtr.Zero;
        try
        {
            pointer = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, pointer, size);
            str = (T)Marshal.PtrToStructure(pointer, typeof(T))!;
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
        return str;
    }
    public static IOperations ParseByteArrayRaw(byte[] correctedBytes)
    {
        switch ((Identifier)correctedBytes[0])
        {
            case Identifier.Mouse:
                return ByteToStruct<MouseOperation>(correctedBytes[1..Marshal.SizeOf<MouseOperation>()]);
            case Identifier.Keyboard:
                return ByteToStruct<KeyboardOperation>(correctedBytes[1..Marshal.SizeOf<KeyboardOperation>()]);
            default: throw new NotSupportedException("Unknown identifier");
        }
    }
}
