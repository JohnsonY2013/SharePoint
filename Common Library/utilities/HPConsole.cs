using System;

namespace hp.utilities
{
    public class HPConsole
    {
        const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static void WriteWarning(string iMessage)
        {
            var mColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(DateTime.UtcNow.ToString(TimeFormat) + "\t" + iMessage);
            Console.ForegroundColor = mColor;
        }

        public static void WriteError(string iMessage)
        {
            var mColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(DateTime.UtcNow.ToString(TimeFormat) + "\t" + iMessage);
            Console.ForegroundColor = mColor;
        }

        public static void WriteLine(string iMessage)
        {
            var mColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(DateTime.UtcNow.ToString(TimeFormat) + "\t" + iMessage);
            Console.ForegroundColor = mColor;
        }

        public static void ReadKey()
        {
            Console.ReadKey();
        }

        public static void ReadLine()
        {
            Console.ReadLine();
        }
    }
}
