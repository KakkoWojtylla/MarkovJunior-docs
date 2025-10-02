using System;

namespace MarkovJunior.Engine;

/// <summary>
/// Provides logging hooks for the interpreter, allowing embedders to redirect
/// diagnostic output away from the console.
/// </summary>
public static class InterpreterLogging
{
    private static IInterpreterLogger _logger = new ConsoleInterpreterLogger();

    /// <summary>Gets or sets the global interpreter logger.</summary>
    public static IInterpreterLogger Logger
    {
        get => _logger;
        set => _logger = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Pushes a new logger onto the stack for the duration of the returned scope.
    /// </summary>
    public static IDisposable PushLogger(IInterpreterLogger logger)
    {
        if (logger is null) throw new ArgumentNullException(nameof(logger));
        return new LoggerScope(logger);
    }

    private sealed class LoggerScope : IDisposable
    {
        private readonly IInterpreterLogger _previous;
        private bool _disposed;

        public LoggerScope(IInterpreterLogger next)
        {
            _previous = Logger;
            Logger = next;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Logger = _previous;
            _disposed = true;
        }
    }
}

/// <summary>Defines the contract for interpreter loggers.</summary>
public interface IInterpreterLogger
{
    void Write(string message);

    void WriteLine(string message);
}

/// <summary>
/// Default logger that mirrors the interpreter output to the system console.
/// </summary>
public sealed class ConsoleInterpreterLogger : IInterpreterLogger
{
    public void Write(string message) => Console.Write(message);

    public void WriteLine(string message) => Console.WriteLine(message);
}

/// <summary>
/// Logger that suppresses all interpreter messages.
/// </summary>
public sealed class NullInterpreterLogger : IInterpreterLogger
{
    public void Write(string message)
    {
    }

    public void WriteLine(string message)
    {
    }
}
