using System;
using System.Linq;
using System.Threading;
using E015;

namespace E015.Btw.Portable
{
    class Program
    {
        // This class is just a loop that wraps a simple interactive console (command-line shell) 
        // which accepts command-line input and tries to parse it through the ConsoleActions "Handlers" and execute the command.
        static void Main(string[] args)
        {
            var env = ConsoleEnvironment.BuildEnvironment();
            env.Log.Info("Starting Being The Worst interactive shell :)");
            env.Log.Info("Type 'help' to get more info");
            

            // TODO: add distance-based suggestions
            while(true)
            {
                Thread.Sleep(300);
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var split = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                IShellAction value;
                if (!env.Handlers.TryGetValue(split[0],out value))
                {
                    env.Log.Error("Unknown command '{0}'. Type 'help' for help", line);
                    continue;
                }
                try
                {
                    value.Execute(env, split.Skip(1).ToArray());
                }
                catch (DomainError ex)
                {
                    env.Log.Error("{0}: {1}", ex.Name, ex.Message);
                }
                catch(ArgumentException ex)
                {
                    env.Log.Error("Invalid usage of '{0}': {1}",split[0], ex.Message);
                    env.Log.Debug(value.Usage);
                }
                catch (Exception ex)
                {
                    env.Log.ErrorException(ex, "Failure while processing command '{0}'", split[0]);
                }
            }
        }
    }
}
