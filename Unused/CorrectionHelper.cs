using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoHIDService.Unused;

public static class CorrectionHelper
{
    public static byte[] Create(byte[] source)
    {
        byte[] result = new byte[source.Length * 3];
        for (int i = 0; i < source.Length; i++)
        {
            result[i] = source[i];
            result[i * 2] = source[i];
            result[i * 3] = source[i];
        }
        return result;
    }
    public static byte[] Decode(byte[] source)
    {
        if (source.Length % 3 != 0) throw new Exception("Length is not multiple of 3!");
        byte[] result = new byte[source.Length / 3];

        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Compare(source[i], source[i * 2], source[i * 3]);
        }
        return result;
    }
    public static byte Compare(byte a, byte b, byte c) => (byte)(a & b | a & c | b & c);
}
