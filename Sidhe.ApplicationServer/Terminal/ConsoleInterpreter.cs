using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Sidhe.ApplicationServer.Interfaces;
using Sidhe.ApplicationServer.Model;

namespace Sidhe.ApplicationServer.Terminal
{
    public class ConsoleInterpreter : IConsoleInterpreter
    {
        [NotNull] private readonly IDictionary<ConsoleCommand, Translator> _commandSet = new Dictionary<ConsoleCommand, Translator>();


        private class Translator
        {
            public Translator([NotNull] string syntax, [NotNull] Func<ConsoleRequest, ApplicationRequest> result,
                [NotNull] [ItemNotNull] params Regex[] arguments)
            {
                Arguments = arguments;
                Result = result;
                Syntax = syntax;
            }

            
            [NotNull, ItemNotNull] public Regex[] Arguments { get; }
            [NotNull] public Func<ConsoleRequest, ApplicationRequest> Result { get; }
            [NotNull] public string Syntax { get; }
            public bool UseAll { get; private set; }

            [NotNull]
            public Translator SetAll()
            {
                UseAll = true;
                return this;
            }
        }


        public ConsoleInterpreter()
        {
            _commandSet[ConsoleCommand.Connect] = 
                new Translator("connect <user-id:non-negative-integer>",
                    request => new ApplicationRequest(ApplicationCommand.BeginSession) { UserId = int.Parse(request.Arguments[0]) },
                    new Regex("^[0-9]{1,8}$"));
            
            _commandSet[ConsoleCommand.Disconnect] = 
                new Translator("disconnect", request => new ApplicationRequest(ApplicationCommand.EndSession));

            _commandSet[ConsoleCommand.Execute] =
                new Translator("execute <text:string>",
                    request => new ApplicationRequest(ApplicationCommand.Execute) { Data = string.Join(" ", request.Arguments) }).SetAll();
                
            _commandSet[ConsoleCommand.Reset] = 
                new Translator("reset", request => new ApplicationRequest(ApplicationCommand.ResetWorld));

            _commandSet[ConsoleCommand.Session] = 
                new Translator("session <user-id:non-negative-integer>",
                    request => new ApplicationRequest(ApplicationCommand.GetSession) { UserId = int.Parse(request.Arguments[0]) },
                    new Regex("^[0-9]{1,8}$"));
        }


        public ApplicationRequest Translate(ConsoleRequest input)
        {
            ApplicationRequest output = null;

            if (_commandSet.TryGetValue(input.Command, out var translator))
            {
                if (translator.UseAll)
                {
                    output = translator.Result(input);
                }
                else if (input.Arguments.Length != translator.Arguments.Length)
                {
                    output = new ApplicationRequest(ApplicationCommand.Unknown) 
                        { ErrorMessage = $"Incorrect number of arguments. Syntax: {translator.Syntax}" };
                }
                else
                {
                    var isValid = translator.Arguments
                        .Zip(input.Arguments, (validator, request) => validator.IsMatch(request))
                        .All(x => x);

                    output = isValid ? 
                        translator.Result(input) : 
                        new ApplicationRequest(ApplicationCommand.Unknown) 
                            { ErrorMessage = $"Invalid argument value. Syntax: {translator.Syntax}" };
                }

                if (output.UserId == 0)
                {
                    output.UserId = input.UserId;
                }
            }
            return output;
        }
    }
}
