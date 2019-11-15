using System;

namespace blu.Common
{
    public interface IBluConsole
    {
        void Write(string message);
        void WriteLine();
        void WriteLine(string message);
        string ReadLine();
        ConsoleKeyInfo ReadKey();
    }

    public class BluConsole : IBluConsole
    {
        public void Write(string message)
        {
            Console.Write(message);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }

        public ConsoleKeyInfo ReadKey()
        {
            return Console.ReadKey();
        }
    }
}
