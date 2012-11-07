using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
// ReSharper disable InconsistentNaming
namespace E014.Domain.ApplicationServices
{
    
    /// <summary>
    /// Base class for testing application services that host aggregates with
    /// event sourcing
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class application_service_spec<T> : IListSpecifications where T : IIdentity
    {
        readonly List<IEvent<T>> _given = new List<IEvent<T>>();

        ICommand<T> _when;
        readonly List<IEvent<T>> _then = new List<IEvent<T>>();
        readonly List<IEvent<T>> _givenEvents = new List<IEvent<T>>();
        bool _thenWasCalled;

        protected static DateTime Date(int year, int month = 1, int day = 1, int hour = 0)
        {
            return new DateTime(year, month, day, hour, 0, 0, DateTimeKind.Unspecified);
        }

        protected static DateTime Time(int hour, int minute = 0, int second = 0)
        {
            return new DateTime(2011, 1, 1, hour, minute, second, DateTimeKind.Unspecified);
        }

        protected abstract void SetupServices();

        protected class ExceptionThrown : IEvent<T>, IAmFakeEventForTesting
        {
            public T Id { get; set; }
            public string Name { get; set; }

            public ExceptionThrown(string name)
            {
                Name = name;
            }

            public override string ToString()
            {
                return string.Format("Domain error '{0}'", Name);
            }
        }

        public void Given(params IEvent<T>[] g)
        {
            _given.AddRange(g);
            foreach (var @event in g)
            {
                var setup = @event as SpecSetupEvent<T>;
                if (setup != null)
                {
                    setup.Apply();
                }
                else _givenEvents.Add(@event);
            }
        }

        public void When(ICommand<T> command)
        {
            _when = command;
        }

        [SetUp]
        public void SetUpSpecification()
        {
            _when = null;
            _given.Clear();
            _then.Clear();
            _thenWasCalled = false;
            _givenEvents.Clear();
            SetupServices();
        }

        public Func<string> GetSpecificationName = () => TestContext.CurrentContext.Test.Name;

        [TearDown]
        public void TeardownSpecification()
        {
            if (!_thenWasCalled)
                Assert.Fail("THEN was not called from the unit test");
        }

        protected void PrintSpecification()
        {
            Console.WriteLine("Fixture:       {0}", GetType().Name.Replace("_", " "));
            Console.WriteLine("Specification: {0}", GetSpecificationName().Replace("_", " "));

            Console.WriteLine();
            if (_given.Any())
            {
                Console.WriteLine("GIVEN:");

                for (var i = 0; i < _given.Count; i++)
                {
                    PrintAdjusted("  " + (i + 1) + ". ", _given[i].ToString().Trim());
                }
            }
            else
            {
                Console.WriteLine("GIVEN no events");
            }

            if (_when != null)
            {
                Console.WriteLine();
                Console.WriteLine("WHEN:");
                PrintAdjusted("  ", _when.ToString().Trim());
            }

            Console.WriteLine();

            if (_then.Any())
            {
                Console.WriteLine("THEN:");
                for (int i = 0; i < _then.Count; i++)
                {
                    PrintAdjusted("  " + (i + 1) + ". ", _then[i].ToString().Trim());
                }
            }
            else
            {
                Console.WriteLine("THEN nothing.");
            }
        }

        protected void PrintResults(ICollection<ExpectResult> exs)
        {
            var results = exs.ToArray();
            var failures = results.Where(f => f.Failure != null).ToArray();
            if (!failures.Any())
            {
                Console.WriteLine();
                Console.WriteLine("Results: [Passed]");
                return;
            }
            Console.WriteLine();
            Console.WriteLine("Results: [Failed]");

            for (int i = 0; i < results.Length; i++)
            {
                PrintAdjusted("  " + (i + 1) + ". ", results[i].Expectation);
                PrintAdjusted("     ", results[i].Failure ?? "PASS");
            }
        }

        protected abstract void ExecuteCommand(IEventStore store, ICommand<T> cmd);

        public void Expect(string error)
        {
            Expect(new ExceptionThrown(error));
        }

        bool _dontExecuteOnExpect;

        public void Expect(params IEvent<T>[] g)
        {
            _thenWasCalled = true;
            _then.AddRange(g);

            IEnumerable<IEvent<T>> actual;
            var givenEvents = _givenEvents.Cast<IEvent<IIdentity>>().ToArray();

            if (_dontExecuteOnExpect) return;
            var store = new InMemoryStore(givenEvents);
            try
            {
                ExecuteCommand(store, _when);
                actual = store.Store.Skip(_givenEvents.Count).Cast<IEvent<T>>().ToArray();
            }
            catch (DomainError e)
            {
                actual = new IEvent<T>[] { new ExceptionThrown(e.Name) };
            }

            var results = CompareAssert(_then.ToArray(), actual.ToArray()).ToArray();

            PrintSpecification();
            PrintResults(results);

            if (results.Any(r => r.Failure != null))
                Assert.Fail("Specification failed");
        }

        public static string GetAdjusted(string adj, string text)
        {
            var first = true;
            var builder = new StringBuilder();
            foreach (var s in text.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
            {
                builder.Append(first ? adj : new string(' ', adj.Length));
                builder.AppendLine(s);
                first = false;
            }
            return builder.ToString();
        }

        static void PrintAdjusted(string adj, string text)
        {
            Console.Write(GetAdjusted(adj, text));
        }

        protected static IEnumerable<ExpectResult> CompareAssert(
            IEvent<T>[] expected,
            IEvent<T>[] actual)
        {
            var max = Math.Max(expected.Length, actual.Length);

            for (int i = 0; i < max; i++)
            {
                var ex = expected.Skip(i).FirstOrDefault();
                var ac = actual.Skip(i).FirstOrDefault();

                var expectedString = ex == null ? "No event expected" : ex.ToString();
                var actualString = ac == null ? "No event actually" : ac.ToString();

                var result = new ExpectResult { Expectation = expectedString };

                var realDiff = CompareObjects.FindDifferences(ex, ac);
                if (!string.IsNullOrEmpty(realDiff))
                {
                    var stringRepresentationsDiffer = expectedString != actualString;

                    result.Failure = stringRepresentationsDiffer ?
                        GetAdjusted("Was:  ", actualString) :
                        GetAdjusted("Diff: ", realDiff);
                }

                yield return result;
            }
        }

        public class ExpectResult
        {
            public string Failure;
            public string Expectation;
        }

        sealed class InMemoryStore : IEventStore
        {
            public readonly List<IEvent> Store = new List<IEvent>();

            public InMemoryStore(IEnumerable<IEvent<IIdentity>> given)
            {
                Store.AddRange(given);
            }

            EventStream IEventStore.LoadEventStream(IIdentity id)
            {
                var events = Store.OfType<IEvent<IIdentity>>().Where(i => id.Equals(i.Id)).Cast<IEvent>().ToList();
                return new EventStream
                {
                    Events = events,
                    StreamVersion = events.Count
                };
            }

            void IEventStore.AppendEventsToStream(IIdentity id, long expectedVersion, ICollection<IEvent> events)
            {
                foreach (var @event in events)
                {
                    Store.Add(@event);
                }
            }
        }

        public IEnumerable<SpecificationInfo> ListSpecifications()
        {
            var type = GetType();

            if (type.IsAbstract)
                yield break;
            _dontExecuteOnExpect = true;

            var myMethods = GetType().GetMethods().Where(m => m.IsDefined(typeof(TestAttribute), true)).ToArray();
            foreach (var method in myMethods)
            {
                SetUpSpecification();
                method.Invoke(this, null);
                yield return new SpecificationInfo
                    {
                    CaseName = method.Name.Replace("_", " "),
                    GroupName = type.Name.Replace("_", " "),
                    Given = _givenEvents.Cast<IEvent>().ToArray(),
                    When = _when,
                    Then = _then.Cast<IEvent>().ToArray()
                };
            }
        }
    }

    /// <summary>
    /// Helper event, which allows us to turn test domain services into light-weight
    /// event-sourced classes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class SpecSetupEvent<T> : IEvent<T>, IAmFakeEventForTesting where T : IIdentity
    {
        public T Id { get; set; }

        readonly string _describe;
        public readonly Action Apply;

        public SpecSetupEvent(Action apply, string describe, params object[] args)
        {
            Apply = apply;
            _describe = string.Format(describe, args);
        }
        public override string ToString()
        {
            return _describe;
        }
    }

    public interface IListSpecifications
    {
        IEnumerable<SpecificationInfo> ListSpecifications();
    }

    public class SpecificationInfo
    {
        public string GroupName;
        public string CaseName;
        public IEvent[] Given;
        public ICommand When;
        public IEvent[] Then;
    }

    public interface IAmFakeEventForTesting { }
}
