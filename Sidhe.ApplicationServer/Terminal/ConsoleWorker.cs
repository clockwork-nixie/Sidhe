using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DependencyInjection;
using JetBrains.Annotations;
using Sidhe.ApplicationServer.Interfaces;
using Sidhe.ApplicationServer.Model;
using Sidhe.Utilities;

namespace Sidhe.ApplicationServer.Terminal
{
    public class ConsoleWorker : Worker<CancellationToken>, IConsoleWorker
    {
        [NotNull] private readonly IConsole _console;
        [NotNull] private readonly IConsoleInterpreter _interpreter;


        private class ConsoleState
        {
            public int UserId { get; set; }
        }


        public delegate ApplicationResponse SessionRequestHandler([NotNull] ApplicationRequest request);


        public ConsoleWorker([NotNull] IFactory factory) : base(factory)
        {
            _console = GetInstance<IConsole>();
            _interpreter = GetInstance<IConsoleInterpreter>();
        }


        public event SessionRequestHandler OnSessionRequest;


        private void ProcessRequest(ApplicationRequest request, [NotNull] ConsoleState state)
        {
            if (request?.ErrorMessage != null)
            {
                _console.ForegroundColor = ConsoleColor.Red;
                _console.WriteLine(request.ErrorMessage);
            }
            else if (request == null || request.Command == ApplicationCommand.Unknown)
            {
                _console.ForegroundColor = ConsoleColor.Red;
                _console.WriteLine("Unknown command.");
            }
            else
            {
                var response = OnSessionRequest?.Invoke(request);

                if (response == null)
                {
                    _console.ForegroundColor = ConsoleColor.Yellow;
                    _console.WriteLine("Command was not processed.");
                }
                else if (response.ErrorMessage != null)
                {
                    _console.ForegroundColor = ConsoleColor.Red;
                    _console.WriteLine(response?.ErrorMessage ?? "No error message.");
                }
                else
                {
                    RecalculateUserId(request, state);

                    _console.ForegroundColor = ConsoleColor.Green;
                    _console.WriteLine(request.Data ?? "Command complete.");
                }
            }
            _console.ForegroundColor = ConsoleColor.White;
            _console.WriteLine();
        }


        private static void RecalculateUserId([NotNull] ApplicationRequest request, [NotNull] ConsoleState state)
        {
            switch (request.Command)
            {
                case ApplicationCommand.BeginSession:
                case ApplicationCommand.GetSession:
                    if (request.UserId > 0)
                    {
                        state.UserId = request.UserId;
                    }
                    break;

                case ApplicationCommand.EndSession:
                    if (state.UserId == request.UserId)
                    {
                        state.UserId = 0;
                    }
                    break;

                case ApplicationCommand.ResetWorld:
                    state.UserId = 0;
                    break;
            }
        }


        private void ReadConsoleNonBlocking([NotNull] ConcurrentQueue<string> queue, CancellationToken cancellation)
        {
            // The problem we have is that reading a line from the console is a blocking
            // task and worse than that: the request is not "cancellable".  The following
            // is a bit grubby but it won't cause problems.
            var task = Task.Run(() => queue.Enqueue(_console.ReadLine()), cancellation);

            try
            {
                task.Wait(cancellation);
            }
            finally
            {
                // We can only dispose of a task if it's complete, faulted or cancelled.
                // Unfortunately a cancellation via the cancellation-token isn't
                // sufficient to cancel the task itself and the console read won't let it
                // complete, so just don't dispose it: we're closing the program anyway
                // so it's not going to cause a memory leak.
                switch (task.Status)
                {
                    case TaskStatus.Canceled:
                    case TaskStatus.Faulted:
                    case TaskStatus.RanToCompletion:
                        task.Dispose();
                        break;
                }
            }
        }

        
        protected override void Work(CancellationToken cancellation)
        {
            var queue = new ConcurrentQueue<string>();
            var state = new ConsoleState();

            _console.ForegroundColor = ConsoleColor.Green;
            _console.WriteLine("Console Initialised");
            _console.ForegroundColor = ConsoleColor.White;
            _console.WriteLine();

            while (true)
            {
                _console.ForegroundColor = ConsoleColor.White;

                if (state.UserId > 0)
                {
                    _console.Write("[");
                    _console.ForegroundColor = ConsoleColor.Cyan;
                    _console.Write(state.UserId.ToString());
                    _console.ForegroundColor = ConsoleColor.White;
                    _console.Write("]");
                }
                _console.Write(">> ");

                ReadConsoleNonBlocking(queue, cancellation);

                while (queue.Count > 0 && queue.TryDequeue(out var line))
                {
                    cancellation.ThrowIfCancellationRequested();

                    try
                    {
                        var words = 
                            new string(line.Select(c => char.IsWhiteSpace(c) ? ' ' : c).ToArray())
                                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        if (words.Length > 0)
                        {
                            var command = words[0].Parse(ConsoleCommand.Unknown);
                            var arguments = words.Skip(1).ToArray();
                            var parsedInput = new ConsoleRequest(command, arguments) { UserId = state.UserId };
                            var request = _interpreter.Translate(parsedInput);
                            
                            ProcessRequest(request, state);
                        }
                    }
                    catch (Exception exception)
                    {
                        _console.ForegroundColor = ConsoleColor.DarkMagenta;
                        _console.WriteLine();
                        _console.WriteLine($"Unexpected error in {nameof(ConsoleWorker)}: {exception.Message}");
                        _console.ForegroundColor = ConsoleColor.White;
                        throw;
                    }
                }
            }
        }
    }
}
