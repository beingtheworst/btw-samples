using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace E011_specs_of_the_living_dead

{
    /// <summary>
    /// <para>
    /// /// This is simplified version of Greg Young's Simple Testing,
    /// adjusted for specification testing of A+ES entities, which is
    /// friendly with Resharper
    /// </para>
    /// <para>Check Lokad.CQRS and Simple Testing for more deep implementations</para>
    /// </summary>
    [TestFixture]
    public abstract class factory_specs
    {
        // this is our syntax for tests
        /// <summary> Events that consistute aggregate history</summary>
        public IList<IEvent> Given;

        /// <summary> aggregate method that we call </summary>
        public Expression<Action<FactoryAggregate>> When;

        /// <summary> Assign here events that we expect to be published </summary>
        public IList<IEvent> Then
        {
            set { AssertCustomerGWT(Given, When, value); }
        }


        public Expression<Predicate<Exception>> ThenException
        {
            set
            {
                try
                {
                    Execute(Given, When);
                    Assert.Fail("Expected exception: " + value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Then expect exception: " + value);
                    if (!value.Compile()(ex))
                        throw;
                }
            }
        }

        [SetUp]
        public void Setup()
        {
            // just reset the specification
            Given = NoEvents;
            When = null;
        }

        public readonly IEvent[] NoEvents = new IEvent[0];

        static void AssertCustomerGWT(ICollection<IEvent> given, Expression<Action<FactoryAggregate>> when,
            ICollection<IEvent> then)
        {
            var changes = Execute(given, when);

            if (then.Count == 0) Console.WriteLine("Expect no events");
            else
                foreach (var @event in then)
                {
                    Console.WriteLine("Then: " + @event);
                }

            AssertEquality(then.ToArray(), changes.ToArray());
        }


        static IEnumerable<IEvent> Execute(ICollection<IEvent> given, Expression<Action<FactoryAggregate>> when)
        {
            if (given.Count == 0) Console.WriteLine("Given no events");
            foreach (var @event in given)
            {
                Console.WriteLine("Given: " + @event);
            }

            var customer = new FactoryAggregate(new FactoryState(given));

            PrintWhen(when);
            when.Compile()(customer);
            return customer.EventsThatHappened;
        }

        static void PrintWhen(Expression<Action<FactoryAggregate>> when)
        {
            // this output can be made prettier, if we 
            // either use expression helpers (see Greg Young's Simple testing for that)
            // or use commands at the application level (see tests in Lokad.CQRS for that)
            Console.WriteLine();
            Console.WriteLine("When: " + when);
            Console.WriteLine();
        }


        static void AssertEquality(IEvent[] expected, IEvent[] actual)
        {
            // in this method we assert full equality between events by serializing
            // and comparing data
            var actualBytes = SerializeEventsToBytes(actual);
            var expectedBytes = SerializeEventsToBytes(expected);
            bool areEqual = actualBytes.SequenceEqual(expectedBytes);
            if (areEqual) return;
            // however if events differ, and this can be seen in human-readable version,
            // then we display human-readable version (derived from ToString())
            CollectionAssert.AreEqual(
                expected.Select(s => s.ToString()).ToArray(),
                actual.Select(s => s.ToString()).ToArray());

            CollectionAssert.AreEqual(expectedBytes, actualBytes,
                "Expected events differ from actual, but differences are not represented in ToString()");
        }

        static byte[] SerializeEventsToBytes(IEvent[] actual)
        {
            // this helper class transforms events to their binary representation
            BinaryFormatter formatter = new BinaryFormatter();
            using (var mem = new MemoryStream())
            {
                formatter.Serialize(mem, actual);
                return mem.ToArray();
            }
        }
    }
}