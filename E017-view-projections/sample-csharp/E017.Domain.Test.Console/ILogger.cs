using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Platform
{
    public interface ILogger
    {
        void Fatal(string text);

        void Error(string text);

        void Info(string text);

        void Debug(string text);

        void Trace(string text);

        void Fatal(string format, params object[] args);

        void Error(string format, params object[] args);

        void Info(string format, params object[] args);

        void Debug(string format, params object[] args);

        void Trace(string format, params object[] args);

        void FatalException(Exception exc, string text);

        void ErrorException(Exception exc, string text);

        void InfoException(Exception exc, string text);

        void DebugException(Exception exc, string text);

        void TraceException(Exception exc, string text);

        void FatalException(Exception exc, string format, params object[] args);

        void ErrorException(Exception exc, string format, params object[] args);

        void InfoException(Exception exc, string format, params object[] args);

        void DebugException(Exception exc, string format, params object[] args);

        void TraceException(Exception exc, string format, params object[] args);
    }

    /// <summary>
    /// Static class that is responsible for wiring in
    /// proper logger infrastructure (NLOG in this case)
    /// </summary>
    public static class LogManager
    {
        private static bool _initialized;

        //public static string LogsDirectory
        //{
        //    get
        //    {
        //        EnsureInitialized();
        //        return Environment.GetEnvironmentVariable("EVENTSTORE_LOGSDIR");
        //    }
        //}

        public static ILogger GetLoggerFor<T>()
        {
            return GetLogger(typeof(T).Name);
        }

        public static ILogger GetLogger(string logName)
        {
            return _logFactory(logName);
        }


        static Func<string, ILogger> _logFactory = s => ConsoleLoggerFactory.GetLogFor(s);
        static Action _finalizer;



        


        public static void Init(Func<string,ILogger> logger, Action dispose)
        {
            //Ensure.NotNull(componentName, "componentName");
            if (_initialized)
                throw new InvalidOperationException("Cannot initialize twice");

            _initialized = true;
            _logFactory = logger;
            _finalizer = dispose;
            //SetLogsDirectoryIfNeeded(logsDirectory);
            //SetComponentName(componentName);
            RegisterGlobalExceptionHandler();
        }

        public static void Finish()
        {
            _finalizer();
            //NLog.LogManager.Configuration = null;
        }

        private static void EnsureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException("Init method must be called");
        }

        //private static void SetLogsDirectoryIfNeeded(string logsDirectory)
        //{
        //    const string logsDirEnvVar = "EVENTSTORE_LOGSDIR";
        //    var directory = Environment.GetEnvironmentVariable(logsDirEnvVar);
        //    if (directory == null)
        //    {
        //        directory = logsDirectory;
        //        Environment.SetEnvironmentVariable(logsDirEnvVar, directory, EnvironmentVariableTarget.Process);
        //    }
        //}

        //private static void SetComponentName(string componentName)
        //{
        //    Environment.SetEnvironmentVariable("EVENTSTORE_INT-COMPONENT-NAME", componentName, EnvironmentVariableTarget.Process);
        //}

        private static void RegisterGlobalExceptionHandler()
        {
            var globalLogger = GetLogger("GLOBAL-LOGGER");
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var exc = e.ExceptionObject as Exception;
                if (exc != null)
                {
                    globalLogger.FatalException(exc, "Global Unhandled Exception occured.");
                }
                else
                {
                    globalLogger.Fatal("Global Unhandled Exception object: {0}.", e.ExceptionObject);
                }
            };
        }
    }

    class LazyLogger : ILogger
    {
        private readonly Lazy<ILogger> _logger;

        public LazyLogger(Func<ILogger> factory)
        {
            //Ensure.NotNull(factory, "factory");
            _logger = new Lazy<ILogger>(factory);
        }

        public void Fatal(string text)
        {
            _logger.Value.Fatal(text);
        }

        public void Error(string text)
        {
            _logger.Value.Error(text);
        }

        public void Info(string text)
        {
            _logger.Value.Info(text);
        }

        public void Debug(string text)
        {
            _logger.Value.Debug(text);
        }

        public void Trace(string text)
        {
            _logger.Value.Trace(text);
        }

        public void Fatal(string format, params object[] args)
        {
            _logger.Value.Fatal(format, args);
        }

        public void Error(string format, params object[] args)
        {
            _logger.Value.Error(format, args);
        }

        public void Info(string format, params object[] args)
        {
            _logger.Value.Info(format, args);
        }

        public void Debug(string format, params object[] args)
        {
            _logger.Value.Debug(format, args);
        }

        public void Trace(string format, params object[] args)
        {
            _logger.Value.Trace(format, args);
        }

        public void FatalException(Exception exc, string format)
        {
            _logger.Value.FatalException(exc, format);
        }

        public void ErrorException(Exception exc, string format)
        {
            _logger.Value.ErrorException(exc, format);
        }

        public void InfoException(Exception exc, string format)
        {
            _logger.Value.InfoException(exc, format);
        }

        public void DebugException(Exception exc, string format)
        {
            _logger.Value.DebugException(exc, format);
        }

        public void TraceException(Exception exc, string format)
        {
            _logger.Value.TraceException(exc, format);
        }

        public void FatalException(Exception exc, string format, params object[] args)
        {
            _logger.Value.FatalException(exc, format, args);
        }

        public void ErrorException(Exception exc, string format, params object[] args)
        {
            _logger.Value.ErrorException(exc, format, args);
        }

        public void InfoException(Exception exc, string format, params object[] args)
        {
            _logger.Value.InfoException(exc, format, args);
        }

        public void DebugException(Exception exc, string format, params object[] args)
        {
            _logger.Value.DebugException(exc, format, args);
        }

        public void TraceException(Exception exc, string format, params object[] args)
        {
            _logger.Value.TraceException(exc, format, args);
        }
    }

    public static class ConsoleLoggerFactory
    {
        static BlockingCollection<Tuple<ConsoleColor, string>> _queue;
        static Thread _thread;
        static  readonly object _lock = new object();

        public static ILogger GetLogFor(string name)
        {
            lock (_lock)
            {
                if (_thread == null)
                {
                     _queue = new BlockingCollection<Tuple<ConsoleColor, string>>();
                    _thread = new Thread(PerformLogging)
                    {
                        IsBackground = true,
                        Name = "Console Logger"
                    };
                    _thread.Start();
                }
            }
            return new ConsoleLogger(_queue);
        }
        static void PerformLogging()
        {
            foreach (var tuple in _queue.GetConsumingEnumerable())
            {
                var old = Console.ForegroundColor;
                Console.ForegroundColor = tuple.Item1;
                Console.WriteLine(tuple.Item2);
                Console.ForegroundColor = old;
            }
        }
    }

    /// <summary>
    /// This should be used as singleton, which polls log messages from multiple sources.
    /// Replace with NLog in production
    /// </summary>
    public sealed class ConsoleLogger : ILogger
    {
        // TODO: reuse object pool, if needed.
        // TODO: better queue for Mono

        readonly BlockingCollection<Tuple<ConsoleColor, string>> _queue; 
 
        public ConsoleLogger(BlockingCollection<Tuple<ConsoleColor, string>> queue)
        {
            _queue = queue;
        }


        static readonly ConsoleColor FatalColor = ConsoleColor.Red;
        static readonly ConsoleColor ErrorColor = ConsoleColor.DarkRed;
        static readonly ConsoleColor InfoColor = ConsoleColor.Green;
        static readonly ConsoleColor DebugColor = ConsoleColor.Gray;
        static readonly ConsoleColor TraceColor = ConsoleColor.DarkGray;
        public void Fatal(string text)
        {
            _queue.Add(Tuple.Create(FatalColor, text));
        }

        public void Error(string text)
        {
            _queue.Add(Tuple.Create(ErrorColor, text));
        }

        public void Info(string text)
        {
            _queue.Add(Tuple.Create(InfoColor, text));
        }

        public void Debug(string text)
        {
            _queue.Add(Tuple.Create(DebugColor, text));
        }

        public void Trace(string text)
        {
            _queue.Add(Tuple.Create(TraceColor, text));
        }

        public void Fatal(string format, params object[] args)
        {
            Fatal(string.Format(format, args));
        }

        public void Error(string format, params object[] args)
        {
            Error(string.Format(format, args));
        }

        public void Info(string format, params object[] args)
        {
            Info(string.Format(format, args));
        }

        public void Debug(string format, params object[] args)
        {
            Debug(string.Format(format, args));
        }

        public void Trace(string format, params object[] args)
        {
            Trace(string.Format(format, args));
        }

        public void FatalException(Exception exc, string text)
        {
            Fatal(text);
            Fatal(exc.ToString());
        }

        public void ErrorException(Exception exc, string text)
        {
            Error(text);
            Error(exc.ToString());
        }

        public void InfoException(Exception exc, string text)
        {
            Info(text);
            Info(exc.ToString());
        }

        public void DebugException(Exception exc, string text)
        {
            Debug(text);
            Debug(exc.ToString());
        }

        public void TraceException(Exception exc, string text)
        {
            Trace(text);
            Trace(exc.ToString());
        }

        public void FatalException(Exception exc, string format, params object[] args)
        {
            FatalException(exc, string.Format(format, args));
        }

        public void ErrorException(Exception exc, string format, params object[] args)
        {
            ErrorException(exc, string.Format(format, args));
        }

        public void InfoException(Exception exc, string format, params object[] args)
        {
            InfoException(exc, string.Format(format, args));
        }

        public void DebugException(Exception exc, string format, params object[] args)
        {
            DebugException(exc, string.Format(format, args));
        }

        public void TraceException(Exception exc, string format, params object[] args)
        {
            TraceException(exc, string.Format(format, args));
        }
    }

}