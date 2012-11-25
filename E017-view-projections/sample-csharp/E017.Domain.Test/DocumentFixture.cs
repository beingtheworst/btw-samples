using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using E017.Contracts;
using E017.Domain.ApplicationServices;
using E017.Domain.ApplicationServices.Factory;
using NUnit.Framework;

namespace E017.Domain
{
    /// <summary>
    /// This class generates a ".dot" text file formatted as a "digraph" that can be read and
    /// visualized by the Graphviz (http://www.graphviz.org/) application as a graph diagram.
    /// This diagram gives us yet another way to see and understand the business concerns that we are modeling.
    /// Each Command message that we have defined will be represented as "shy" teal blue eplipse (because Commands that MAY happen).
    /// The name of each Specification that we have defined will be displayed on the left of the diagram followed
    /// by a colon and the name of each Unit Test that has been defined within the specification.
    /// These Specifications:Unit Tests on the left will have an arrow that points to the Command message that they are associated with.
    /// This visually displays our Aggregate's use cases/behaviors/stories in a colored diagram.
    /// Ideally, with Aggregates with Event Sourcing (A+ES), all methods/behaviors in an Aggregate will have at least one associated Unit Test in the diagram.
    /// The Command messages (teal blue eplipses) have outgoing arrows that point to 
    /// the resulting Event message (green rectangle, things that HAVE happened) or an Exception (white eplipse) that may happen.
    /// TODO: Change Exception elipses from white to a readable pink/red to show the errors in their more traditional color.
    /// </summary>
    
    [TestFixture, Explicit]
    public sealed class DocumentFixture
    {

        static IEnumerable<Assembly> AssembliesWithSpecs()
        {
            yield return typeof(assign_employee_to_factory).Assembly;
        }

        public sealed class Sequence
        {
            public Type Command;
            public HashSet<string> EventTypes = new HashSet<string>();
            public HashSet<string> Specs = new HashSet<string>();
        }


        [Test]
        public void Test()
        {
            var types = AssembliesWithSpecs().SelectMany(t => t.GetExportedTypes());

            var runs = types
                .Where(t => typeof(IListSpecifications).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                .SelectMany(t => ((IListSpecifications)Activator.CreateInstance(t)).ListSpecifications());





            IDictionary<Type, Sequence> sequences = new Dictionary<Type, Sequence>();
            foreach (var run in runs)
            {
                var command = run.When;
                Sequence set;
                var ct = command.GetType();
                if (!sequences.TryGetValue(ct, out set))
                {
                    set = new Sequence
                        {
                            Command = ct
                        };
                    sequences[ct] = set;
                }
                var name = string.Format("{0}: {1}", run.GroupName, run.CaseName);
                set.Specs.Add(name);
                foreach (var @event in run.Then)
                {
                    var fake = @event as IAmFakeEventForTesting;

                    if (fake != null)
                    {
                        if (null != fake.FakeType)
                            set.EventTypes.Add("\"" + fake.FakeType + "\"");
                    }
                    else
                    {
                        set.EventTypes.Add(@event.GetType().Name);
                    }
                }
            }

            var list = new List<AggregateDependency>
                {
                    PickDefinitions(sequences, typeof (IFactoryState)),
                    
                };


            //Console.WriteLine(JsvFormatter.Format(s));
            var builder = new StringBuilder();
            using (var writer = new IndentedTextWriter(new StringWriter(builder)))
            {
                writer.WriteLine("digraph G {");
                writer.WriteLine("graph [fontsize=30 labelloc=\"t\"  splines=true overlap=false rankdir = \"LR\"];");
                writer.Indent++;
                int count = 0;
                foreach (var agg in list)
                {
                    writer.WriteLine("subgraph cluster_{0} {{", count++);
                    writer.Indent++;
                    var cases = new HashSet<string>();
                    var commands = new HashSet<string>();
                    foreach (var @event in agg.Events)
                    {
                        writer.WriteLine("{0} [shape=box,fillcolor=\"#CCFFCC\",style=filled,color=\"#008000\"];", @event);
                    }

                    writer.WriteLine("label=\"{0}\";", string.Join("_", agg.Aggregates));
                    foreach (var sequence in agg.Sequence)
                    {
                        foreach (var @case in sequence.Cases)
                        {
                            cases.Add(@case);
                            writer.WriteLine("\"{0}\" -> {1};", @case, sequence.WhenCommand);
                        }

                        foreach (var @event in sequence.ThenEvents)
                        {
                            commands.Add(sequence.WhenCommand);
                            writer.WriteLine("{0} -> {1};", sequence.WhenCommand, @event);
                        }
                    }
                    foreach (var @case in cases)
                    {
                        writer.WriteLine("\"{0}\" [shape=plaintext, fillcolor=\"#FFFFCC\", style=filled];", @case);
                    }
                    foreach (var command in commands)
                    {
                        writer.WriteLine(
                            "\"{0}\" [shape=ellipse,fillcolor=\"#CCFFFF\",style=filled,color=\"#006699\"];", command);
                    }
                    writer.Indent--;
                    writer.WriteLine("}");
                }
                writer.Indent--;
                writer.WriteLine("}");
            }

            // Write the Graphviz digraph file to the bin directory
            File.WriteAllText("graph.dot", builder.ToString());
            Console.WriteLine(builder.ToString());

            try
            {
                // A quick way to write the digraph file to another path location
                File.WriteAllText(@"Z:\Downloads\graph.dot", builder.ToString());
            }
            // Normally you do NOT use this "catch all exceptions" approach in production code
            catch (Exception) { }
        }

        public AggregateDependency PickDefinitions(IDictionary<Type, Sequence> sequences, params Type[] types)
        {
            var allEvents =
                types.SelectMany(t => t.GetMethods().Where(m => m.Name == "When")).Select(
                    m => m.GetParameters()[0].ParameterType).ToArray();

            var dependency = new AggregateDependency
                {
                    Aggregates = types.Select(t => t.Name.TrimStart('I').Replace("AggregateState", "")).ToArray(),
                    Events = allEvents.Select(e => e.Name).ToArray(),
                };

            var discoveredCommands = new HashSet<Type>();

            foreach (var sequence in sequences.Values)
            {
                foreach (var eventType in allEvents)
                {
                    if (sequence.EventTypes.Contains(eventType.Name))
                    {
                        // intersection
                        discoveredCommands.Add(sequence.Command);
                    }
                }
            }

            foreach (var type in discoveredCommands)
            {
                var discoveredSequence = new DiscoveredSequence
                    {
                        WhenCommand = type.Name,
                        ThenEvents = sequences[type].EventTypes.ToArray(),
                        Cases = sequences[type].Specs
                    };


                dependency.Sequence.Add(discoveredSequence);
                sequences.Remove(type);
            }
            return dependency;
        }


        public sealed class AggregateDependency
        {
            public string[] Aggregates { get; set; }
            public string[] Events { get; set; }
            public IList<DiscoveredSequence> Sequence { get; set; }

            public AggregateDependency()
            {
                Sequence = new List<DiscoveredSequence>();
            }
        }


        public sealed class DiscoveredSequence
        {
            public string WhenCommand { get; set; }
            public ICollection<string> ThenEvents { get; set; }
            public HashSet<string> Cases = new HashSet<string>();
        }
    }
}