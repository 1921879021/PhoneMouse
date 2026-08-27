using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhoneMouse.Desktop.Services;

/// <summary>
/// Phone Mouse 本地二维码生成器。
///
/// 当前使用 QR Code Version 5-L：
/// - 完全本地生成
/// - 不依赖 NuGet
/// - 不调用任何第三方二维码网站
/// - UTF-8 Byte Mode
/// - 最多支持 106 个 UTF-8 字节
///
/// 对 Phone Mouse 的局域网 URL 以及后续附带配对 Token 的 URL 都足够。
/// </summary>
public static class QrCodeService
{
    private const int Version = 5;
    private const int Size = 37;

    private const int DataCodewords = 108;
    private const int EccCodewords = 26;

    private const int MaxByteLength = 106;

    private const int Mask = 0;


    public static BitmapSource Create(
        string text,
        int pixelsPerModule = 5,
        int quietZone = 4)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "QR Code 内容不能为空。",
                nameof(text));
        }

        if (pixelsPerModule <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pixelsPerModule));
        }

        if (quietZone < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quietZone));
        }


        byte[] payload =
            Encoding.UTF8.GetBytes(text);


        if (payload.Length > MaxByteLength)
        {
            throw new ArgumentException(
                $"当前本地二维码最多支持 {MaxByteLength} 个 UTF-8 字节。",
                nameof(text));
        }


        bool[,] modules =
            BuildMatrix(payload);


        return Render(
            modules,
            pixelsPerModule,
            quietZone);
    }


    private static bool[,] BuildMatrix(
        byte[] payload)
    {
        bool[,] modules =
            new bool[Size, Size];

        bool[,] isFunction =
            new bool[Size, Size];


        void SetFunction(
            int x,
            int y,
            bool dark)
        {
            if (
                x < 0 ||
                y < 0 ||
                x >= Size ||
                y >= Size)
            {
                return;
            }


            modules[y, x] =
                dark;

            isFunction[y, x] =
                true;
        }


        // =============================================
        // Timing Patterns
        // =============================================

        for (int i = 0; i < Size; i++)
        {
            bool dark =
                i % 2 == 0;

            SetFunction(
                6,
                i,
                dark);

            SetFunction(
                i,
                6,
                dark);
        }


        // =============================================
        // Finder Patterns + Separator
        // =============================================

        void DrawFinder(
            int centerX,
            int centerY)
        {
            for (
                int dy = -4;
                dy <= 4;
                dy++)
            {
                for (
                    int dx = -4;
                    dx <= 4;
                    dx++)
                {
                    int x =
                        centerX + dx;

                    int y =
                        centerY + dy;


                    if (
                        x < 0 ||
                        y < 0 ||
                        x >= Size ||
                        y >= Size)
                    {
                        continue;
                    }


                    int distance =
                        Math.Max(
                            Math.Abs(dx),
                            Math.Abs(dy));


                    bool dark =
                        distance != 2 &&
                        distance != 4;


                    SetFunction(
                        x,
                        y,
                        dark);
                }
            }
        }


        DrawFinder(
            3,
            3);

        DrawFinder(
            Size - 4,
            3);

        DrawFinder(
            3,
            Size - 4);


        // =============================================
        // Alignment Pattern
        //
        // Version 5 的中心点：
        // 6, 30
        //
        // 与 Finder 重叠的跳过，只需要 (30, 30)。
        // =============================================

        void DrawAlignment(
            int centerX,
            int centerY)
        {
            for (
                int dy = -2;
                dy <= 2;
                dy++)
            {
                for (
                    int dx = -2;
                    dx <= 2;
                    dx++)
                {
                    bool dark =
                        Math.Max(
                            Math.Abs(dx),
                            Math.Abs(dy))
                        != 1;


                    SetFunction(
                        centerX + dx,
                        centerY + dy,
                        dark);
                }
            }
        }


        DrawAlignment(
            30,
            30);


        // =============================================
        // Format Information
        // =============================================

        int formatBits =
            GetFormatBits(Mask);


        for (
            int i = 0;
            i <= 5;
            i++)
        {
            SetFunction(
                8,
                i,
                GetBit(
                    formatBits,
                    i));
        }


        SetFunction(
            8,
            7,
            GetBit(
                formatBits,
                6));


        SetFunction(
            8,
            8,
            GetBit(
                formatBits,
                7));


        SetFunction(
            7,
            8,
            GetBit(
                formatBits,
                8));


        for (
            int i = 9;
            i < 15;
            i++)
        {
            SetFunction(
                14 - i,
                8,
                GetBit(
                    formatBits,
                    i));
        }


        for (
            int i = 0;
            i < 8;
            i++)
        {
            SetFunction(
                Size - 1 - i,
                8,
                GetBit(
                    formatBits,
                    i));
        }


        for (
            int i = 8;
            i < 15;
            i++)
        {
            SetFunction(
                8,
                Size - 15 + i,
                GetBit(
                    formatBits,
                    i));
        }


        // Dark Module
        SetFunction(
            8,
            Size - 8,
            true);


        // =============================================
        // 数据 + Reed-Solomon ECC
        // =============================================

        byte[] codewords =
            BuildCodewords(
                payload);


        List<bool> dataBits =
            new(
                codewords.Length * 8);


        foreach (
            byte value
            in codewords)
        {
            AppendBits(
                dataBits,
                value,
                8);
        }


        // =============================================
        // Zig-Zag Data Placement
        // =============================================

        int bitIndex = 0;

        bool upward = true;


        for (
            int right = Size - 1;
            right >= 1;
            right -= 2)
        {
            if (right == 6)
            {
                right--;
            }


            for (
                int vertical = 0;
                vertical < Size;
                vertical++)
            {
                int y =
                    upward
                        ? Size - 1 - vertical
                        : vertical;


                for (
                    int j = 0;
                    j < 2;
                    j++)
                {
                    int x =
                        right - j;


                    if (
                        isFunction[y, x])
                    {
                        continue;
                    }


                    bool bit =
                        bitIndex <
                            dataBits.Count
                            ? dataBits[bitIndex]
                            : false;


                    bitIndex++;


                    // Mask Pattern 0:
                    // (x + y) % 2 == 0
                    if (
                        (x + y) % 2 == 0)
                    {
                        bit =
                            !bit;
                    }


                    modules[y, x] =
                        bit;
                }
            }


            upward =
                !upward;
        }


        return modules;
    }


    private static byte[] BuildCodewords(
        byte[] payload)
    {
        List<bool> bits =
            new(
                DataCodewords * 8);


        // Byte Mode
        AppendBits(
            bits,
            0b0100,
            4);


        // Version 1-9 的 Byte Mode
        // Character Count = 8 bits
        AppendBits(
            bits,
            payload.Length,
            8);


        foreach (
            byte value
            in payload)
        {
            AppendBits(
                bits,
                value,
                8);
        }


        int capacityBits =
            DataCodewords * 8;


        // Terminator
        int terminatorLength =
            Math.Min(
                4,
                capacityBits -
                    bits.Count);


        AppendBits(
            bits,
            0,
            terminatorLength);


        // 补到整字节
        while (
            bits.Count % 8 != 0)
        {
            bits.Add(
                false);
        }


        // Pad Codewords
        bool useEc =
            true;


        while (
            bits.Count <
                capacityBits)
        {
            AppendBits(
                bits,
                useEc
                    ? 0xEC
                    : 0x11,
                8);


            useEc =
                !useEc;
        }


        byte[] data =
            new byte[
                DataCodewords];


        for (
            int i = 0;
            i < DataCodewords;
            i++)
        {
            int value = 0;


            for (
                int j = 0;
                j < 8;
                j++)
            {
                value =
                    (value << 1) |
                    (
                        bits[
                            i * 8 + j]
                            ? 1
                            : 0
                    );
            }


            data[i] =
                (byte)value;
        }


        byte[] divisor =
            ReedSolomonComputeDivisor(
                EccCodewords);


        byte[] remainder =
            ReedSolomonComputeRemainder(
                data,
                divisor);


        byte[] result =
            new byte[
                DataCodewords +
                EccCodewords];


        Buffer.BlockCopy(
            data,
            0,
            result,
            0,
            data.Length);


        Buffer.BlockCopy(
            remainder,
            0,
            result,
            data.Length,
            remainder.Length);


        return result;
    }


    private static byte[]
        ReedSolomonComputeDivisor(
            int degree)
    {
        byte[] result =
            new byte[degree];


        result[
            degree - 1] =
            1;


        byte root =
            1;


        for (
            int i = 0;
            i < degree;
            i++)
        {
            for (
                int j = 0;
                j < degree;
                j++)
            {
                result[j] =
                    ReedSolomonMultiply(
                        result[j],
                        root);


                if (
                    j + 1 <
                    degree)
                {
                    result[j] ^=
                        result[
                            j + 1];
                }
            }


            root =
                ReedSolomonMultiply(
                    root,
                    0x02);
        }


        return result;
    }


    private static byte[]
        ReedSolomonComputeRemainder(
            byte[] data,
            byte[] divisor)
    {
        byte[] result =
            new byte[
                divisor.Length];


        foreach (
            byte value
            in data)
        {
            byte factor =
                (byte)(
                    value ^
                    result[0]);


            for (
                int i = 0;
                i <
                    result.Length - 1;
                i++)
            {
                result[i] =
                    result[
                        i + 1];
            }


            result[
                result.Length - 1] =
                0;


            for (
                int i = 0;
                i <
                    result.Length;
                i++)
            {
                result[i] ^=
                    ReedSolomonMultiply(
                        divisor[i],
                        factor);
            }
        }


        return result;
    }


    private static byte
        ReedSolomonMultiply(
            byte x,
            byte y)
    {
        int z = 0;

        int a = x;

        int b = y;


        for (
            int i = 0;
            i < 8;
            i++)
        {
            if (
                (b & 1) != 0)
            {
                z ^=
                    a;
            }


            bool carry =
                (a & 0x80) != 0;


            a =
                (a << 1) &
                0xFF;


            if (carry)
            {
                // GF(256)
                // Primitive Polynomial 0x11D
                a ^=
                    0x1D;
            }


            b >>=
                1;
        }


        return
            (byte)z;
    }


    private static int GetFormatBits(
        int mask)
    {
        // Error Correction Level L
        // Format bits = 01
        int data =
            (1 << 3) |
            mask;


        int remainder =
            data;


        for (
            int i = 0;
            i < 10;
            i++)
        {
            remainder =
                (remainder << 1) ^
                (
                    (
                        remainder >>
                        9
                    ) & 1
                ) *
                0x537;
        }


        return
            (
                (data << 10) |
                remainder
            ) ^
            0x5412;
    }


    private static bool GetBit(
        int value,
        int index)
    {
        return
            (
                (
                    value >>
                    index
                ) & 1
            ) != 0;
    }


    private static void AppendBits(
        List<bool> bits,
        int value,
        int length)
    {
        for (
            int i =
                length - 1;
            i >= 0;
            i--)
        {
            bits.Add(
                (
                    (
                        value >>
                        i
                    ) & 1
                ) != 0);
        }
    }


    private static BitmapSource Render(
        bool[,] modules,
        int pixelsPerModule,
        int quietZone)
    {
        int qrSize =
            modules.GetLength(0);


        int totalModules =
            qrSize +
            quietZone * 2;


        int width =
            totalModules *
            pixelsPerModule;


        int stride =
            width * 4;


        byte[] pixels =
            new byte[
                stride *
                width];


        // 白底
        for (
            int i = 0;
            i < pixels.Length;
            i += 4)
        {
            pixels[i + 0] =
                255;

            pixels[i + 1] =
                255;

            pixels[i + 2] =
                255;

            pixels[i + 3] =
                255;
        }


        // 黑色模块
        for (
            int y = 0;
            y < qrSize;
            y++)
        {
            for (
                int x = 0;
                x < qrSize;
                x++)
            {
                if (
                    !modules[y, x])
                {
                    continue;
                }


                int pixelStartX =
                    (
                        x +
                        quietZone
                    ) *
                    pixelsPerModule;


                int pixelStartY =
                    (
                        y +
                        quietZone
                    ) *
                    pixelsPerModule;


                for (
                    int py = 0;
                    py <
                        pixelsPerModule;
                    py++)
                {
                    int row =
                        (
                            pixelStartY +
                            py
                        ) *
                        stride;


                    for (
                        int px = 0;
                        px <
                            pixelsPerModule;
                        px++)
                    {
                        int index =
                            row +
                            (
                                pixelStartX +
                                px
                            ) *
                            4;


                        pixels[index + 0] =
                            0;

                        pixels[index + 1] =
                            0;

                        pixels[index + 2] =
                            0;

                        pixels[index + 3] =
                            255;
                    }
                }
            }
        }


        WriteableBitmap bitmap =
            new(
                width,
                width,
                96,
                96,
                PixelFormats.Bgra32,
                null);


        bitmap.WritePixels(
            new Int32Rect(
                0,
                0,
                width,
                width),
            pixels,
            stride,
            0);


        bitmap.Freeze();


        return bitmap;
    }
}
