namespace EventSourcing.Processors
{
    public enum LoggingLevel
    {
        Verbose,
        Information,
        Warning,
        Error
    }
    public interface ILogging
    {
        void Write(LoggingLevel level, string message);
        void WriteLine(LoggingLevel level, string message);
    }
}
