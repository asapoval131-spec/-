using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;

class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern SafeFileHandle GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern bool WriteConsoleOutputW(
        SafeFileHandle hConsoleOutput,
        CharInfo[] lpBuffer,
        Coord dwBufferSize,
        Coord dwBufferCoord,
        ref SmallRect lpWriteRegion);

    [StructLayout(LayoutKind.Sequential)]
    public struct Coord
    {
        public short X;
        public short Y;
        public Coord(short x, short y) { X = x; Y = y; }
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct CharInfo
    {
        [FieldOffset(0)] public char UnicodeChar;
        [FieldOffset(2)] public ushort Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SmallRect
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }

    const int STD_OUTPUT_HANDLE = -11;

    static void Main()
    {
        Console.Clear();
        Console.Title = "Вертикальная заливка треугольника снизу вверх";

        SafeFileHandle hOutput = GetStdHandle(STD_OUTPUT_HANDLE);

        short width = 20;
        short height = 10;
        CharInfo[] buffer = new CharInfo[width * height];

        ushort bgGreen = 0x0020;     // зелёный фон
        ushort fillCharColor = 0x000A; // ярко-зелёный текст

        // Изначально весь буфер пустой (точки на чёрном фоне)
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i].UnicodeChar = ' ';
            buffer[i].Attributes = 0;
        }

        SmallRect screenRect = new SmallRect
        {
            Left = 5,
            Top = 2,
            Right = (short)(5 + width - 1),
            Bottom = (short)(2 + height - 1)
        };

        // Заливка снизу вверх: y = 9 -> 0
        for (short y = (short)(height - 1); y >= 0; y--)
        {
            // Ширина строки пирамиды: на y=9 -> 20, на y=0 -> 2
            // Формула: halfWidth = (y + 1)  (от 1 до 10)
            // Заполняем от centerX-halfWidth+... до centerX+halfWidth-1
            // Проще: filledWidth = 2 * (y + 1) ... но 2*(9+1)=20 ✓, 2*(0+1)=2 ✓
            // Однако для y=0 нужно 2 символа в центре — ширина 2 даёт два центральных столбца.
            short filledWidth = (short)(2 * (y + 1));
            short startX = (short)((width - filledWidth) / 2);

            for (short x = startX; x < startX + filledWidth; x++)
            {
                int index = y * width + x;
                buffer[index].UnicodeChar = '█';
                buffer[index].Attributes = (ushort)(bgGreen | fillCharColor);
            }

            WriteConsoleOutputW(hOutput, buffer, new Coord(width, height), new Coord(0, 0), ref screenRect);

            Thread.Sleep(200);
        }

        Console.SetCursorPosition(0, 14);
        Console.WriteLine("Заливка треугольника завершена. Нажмите любую клавишу...");
        Console.ReadKey();
    }
}