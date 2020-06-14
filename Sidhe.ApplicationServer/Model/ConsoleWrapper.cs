using System;
using Sidhe.ApplicationServer.Interfaces;

namespace Sidhe.ApplicationServer.Model
{
    public class ConsoleWrapper : IConsole
    {
        public ConsoleColor ForegroundColor { get => Console.ForegroundColor; set => Console.ForegroundColor = value; }
        public string ReadLine() => Console.ReadLine();
        public void Write(string text) => Console.Write(text);
        public void WriteLine() => Console.WriteLine();
        public void WriteLine(string text) => Console.WriteLine(text);
    }
}
