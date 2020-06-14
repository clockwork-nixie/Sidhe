using System;

namespace Sidhe.ApplicationServer.Interfaces
{
    public interface IConsole
    {
        ConsoleColor ForegroundColor { get; set; }
        string ReadLine();
        void Write(string text);
        void WriteLine();
        void WriteLine(string text);
    }
}