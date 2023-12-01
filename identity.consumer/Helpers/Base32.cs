namespace Identity.Consumer.Helpers;

public static class Base32
{
    public static byte[] Decode(string input)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentNullException(nameof(input));
        
        input = input.TrimEnd('=');

        var length = input.Length * 5 / 8;
        var bytes = new byte[length];
        var index = 0;
        
        byte num1 = 0;
        byte num2 = 8;
        
        foreach (var num3 in input.Select(CharToValue))
        {
            if (num2 > 5)
            {
                var num4 = num3 << num2 - 5;
                num1 |= (byte)num4;
                num2 -= 5;
            }
            else
            {
                var num5 = num3 >> 5 - num2;
                var num6 = (byte)(num1 | (uint)num5);
                
                bytes[index++] = num6;
                num1 = (byte)(num3 << 3 + num2);
                num2 += 3;
            }
        }

        if (index != length)
            bytes[index] = num1;
        
        return bytes;
    }

    public static string Encode(byte[] input)
    {
        if (input == null || input.Length == 0)
            throw new ArgumentNullException(nameof(input));

        var length = (int)Math.Ceiling(input.Length / 5.0) * 8;
        var chArray1 = new char[length];
        byte b1 = 0;
        byte num1 = 5;
        int num2 = 0;
        
        foreach (byte num3 in input)
        {
            byte b2 = (byte)(b1 | (uint)num3 >> 8 - num1);
            chArray1[num2++] = ValueToChar(b2);
            if (num1 < 4)
            {
                byte b3 = (byte)(num3 >> 3 - num1 & 31);
                chArray1[num2++] = ValueToChar(b3);
                num1 += 5;
            }

            num1 -= 3;
            b1 = (byte)(num3 << num1 & 31);
        }

        if (num2 != length)
        {
            var chArray2 = chArray1;
            var num4 = num2 + 1;
            var num5 = (int)ValueToChar(b1);
            
            chArray2[num2] = (char)num5;
            while (num4 != length)
                chArray1[num4++] = '=';
        }

        return new string(chArray1);
    }

    private static int CharToValue(char c)
    {
        int num = c;
        return num switch
        {
            < 91 and > 64 => num - 65,
            < 56 and > 49 => num - 24,
            < 123 and > 96 => num - 97,
            _ => throw new ArgumentException("Character is not a Base32 character.", nameof(c))
        };
    }

    private static char ValueToChar(byte b)
    {
        return b switch
        {
            < 26 => (char)(b + 65U),
            < 32 => (char)(b + 24U),
            _ => throw new ArgumentException("Byte is not a Base32 value.", nameof(b))
        };
    }
}