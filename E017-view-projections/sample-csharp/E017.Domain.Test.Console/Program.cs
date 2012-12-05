using System;
using System.Linq;
using System.Threading;
using E017;

namespace E017.Domain.Test.Console
{
    class Program
    {
        // This class is just a loop that wraps a simple interactive console (command-line shell) 
        // which accepts command-line input and tries to parse it through the ConsoleActions "ActionHandlers" and execute the console command.
        static void Main(string[] args)
        {
            // setup and wire our console environment
            var env = ConsoleEnvironment.BuildEnvironment();
            env.Log.Info("Starting Being The Worst interactive shell :)");
            env.Log.Info("Type 'help' to get more info");
            

            // TODO: add distance-based suggestions
            // TODO: Rinat, was the comment above left over from something else or intended to be in here?

            while(true)
            {
                Thread.Sleep(300);
                System.Console.Write("> ");
                var line = System.Console.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var split = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                IShellAction value;
                if (!env.ActionHandlers.TryGetValue(split[0],out value))
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
