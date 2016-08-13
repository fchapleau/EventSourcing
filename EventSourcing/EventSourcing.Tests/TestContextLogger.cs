using EventSourcing.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSourcing.Tests
{
    public class TestContextLogger : ILogging
    {
        private TestContext _context;
        public TestContextLogger(TestContext context)
        {
            _context = context;
        }
        public void WriteLine(LoggingLevel level, string message)
        {
            _context.WriteLine(message);
        }

        public void Write(LoggingLevel level, string msg)
        {
            _context.WriteLine(msg);
        }
    }
}
