using EventSourcing.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Tests.Server
{
    public class ConsoleLogger : ILogging
    {
        public void WriteLine(LoggingLevel level, string message)
        {
            switch (level)
            {
                case LoggingLevel.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                case LoggingLevel.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case LoggingLevel.Information: Console.ForegroundColor = ConsoleColor.White; break;
                case LoggingLevel.Verbose: Console.ForegroundColor = ConsoleColor.Gray; break;
            }
            Console.WriteLine(message);
        }

        public void Write(LoggingLevel level, string msg)
        {
            switch (level)
            {
                case LoggingLevel.Error: Console.ForegroundColor = ConsoleColor.Red; break;
                case LoggingLevel.Warning: Console.ForegroundColor = ConsoleColor.Yellow; break;
                case LoggingLevel.Information: Console.ForegroundColor = ConsoleColor.White; break;
                case LoggingLevel.Verbose: Console.ForegroundColor = ConsoleColor.Gray; break;
            }
            Console.Write(msg);
        }
    }
}
