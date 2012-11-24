using System;
using System.Collections.Generic;
using System.Linq;
using E015;
using E015.Contracts;

namespace E015.Btw.Portable
{
    public static class ConsoleActions
    {
        public static IDictionary<string,IShellAction> Actions = new Dictionary<string,IShellAction>(); 

        static ConsoleActions()
        {
            Register(new OpenFactoryAction());
            Register(new RegisterBlueprintAction());
            Register(new HireEmployeeAction());
            Register(new RecieveShipmentAction());
            Register(new UnpackShipmentsAction());
            Register(new HelpAction());
            Register(new ExitAction());
            Register(new StoryAction());
        }
        static void Register(IShellAction cmd)
        {
            Actions.Add(cmd.Usage.Split(new[]{' '},2).First(), cmd);
        }
    }

    public interface IShellAction
    {
        string Usage { get; }
        void Execute(ConsoleEnvironment env, string[] args);
    }

    public class OpenFactoryAction : IShellAction
    {
        public string Keyword { get { return "open"; } }
        public string Usage { get { return "open <factoryId> - opens new factory"; } }

        public void Execute(ConsoleEnvironment env, string[] args)
        {
            if (args.Length != 1)
                throw new ArgumentException("Expected at least 2 args");
            var id = int.Parse(args[0]);
            env.FactoryAppService.When(new OpenFactory(new FactoryId(id)));
        }
    }
    public class RegisterBlueprintAction : IShellAction
    {
        public string Keyword { get { return "reg"; } }
        public string Usage { get { return "reg <design> [<part>, <part>...]"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("Expected at least 2 args");
            }
            
            var design = args[0];
            var parts = args.Skip(1).GroupBy(s => s).Select(g => new CarPart(g.Key, g.Count())).ToArray();
            env.Blueprints.Register(design, parts);
        }
    }

    public class HireEmployeeAction : IShellAction
    {
        public string Keyword { get { return "hire"; } }
        public string Usage { get { return "hire <factoryid> <employeeName>"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("Expected at least 2 args");
            }
            var id = int.Parse(args[0]);
            var name = string.Join(" ", args.Skip(1));
            env.FactoryAppService.When(new AssignEmployeeToFactory(new FactoryId(id), name));
        }
    }

    public class RecieveShipmentAction : IShellAction
    {
        public string Keyword { get { return "ship"; } }
        public string Usage { get { return "ship <factoryId> <shipment> [<part>,<part>...]"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("Expected at least 2 args");

            var id = int.Parse(args[0]);
            var name = args[1];
            var parts = args.Skip(2).GroupBy(s => s).Select(g => new CarPart(g.Key, g.Count())).ToArray();
            env.FactoryAppService.When(new ReceiveShipmentInCargoBay(new FactoryId(id), name, parts));
        }
    }

    public class UnpackShipmentsAction : IShellAction
    {
        public string Keyword { get { return "unpack"; } }
        public string Usage { get { return "unpack <factoryId> <shipment>"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            if (args.Length < 2)
            {
                throw new ArgumentException("Expected at least 2 args");
            }
            var id = int.Parse(args[0]);
            var employee = string.Join(" ", args.Skip(1));
            env.FactoryAppService.When(new UnpackAndInventoryShipmentInCargoBay(new FactoryId(id), employee));

        }
    }

    public class HelpAction : IShellAction
    {
        public string Keyword { get { return "help"; } }
        public string Usage { get { return "help [<command>]"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            if (args.Length > 0)
            {
                IShellAction value;
                if (!env.Handlers.TryGetValue(args[0], out value))
                {
                    env.Log.Error("Can't find help for '{0}'", args[0]);
                    return;
                }
                env.Log.Info(value.Usage ?? "No Help available");
                return;
            }
            env.Log.Info("Available commands");
            foreach (var handler in env.Handlers)
            {
                env.Log.Info("  {0}", handler.Key.ToUpperInvariant());
                if (!string.IsNullOrWhiteSpace(handler.Value.Usage))
                {
                    env.Log.Info("    {0}", handler.Value.Usage);
                }
            }

        }
    }

    public class ExitAction : IShellAction
    {
        public string Keyword { get { return "exit"; } }
        public string Usage { get { return "exit"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            Environment.Exit(0);
        }
    }

    public class StoryAction : IShellAction
    {
        public string Keyword { get { return "story"; } }
        public string Usage { get { return "story [<factoryId=1>]"; } }
        public void Execute(ConsoleEnvironment env, string[] args)
        {
            int factoryId = 1;
            if (args.Length>0)
            {
                int.TryParse(args[0], out factoryId);
            }
            var id = new FactoryId(factoryId);
            var story = new List<ICommand>
                {
                    new OpenFactory(id),
                    new AssignEmployeeToFactory(id, "Luke"),
                    new AssignEmployeeToFactory(id, "Han"),
                    new ReceiveShipmentInCargoBay(id, "from uncle Ben", new[]
                        {
                            new CarPart("wheel", 10),
                            new CarPart("light saber", 2),
                            new CarPart("C3P0", 1),
                        }),
                    new ReceiveShipmentInCargoBay(id, "from Yoda", new[]
                        {
                            new CarPart("engine", 2),
                            new CarPart("chassis", 1),
                        }),
                    new UnpackAndInventoryShipmentInCargoBay(id, "Han"),
                    new ProduceACar(id, "Luke", "model-t")
                };

            foreach (var command in story)
            {
                env.FactoryAppService.Execute(command);
            }
        }

      
    }
}