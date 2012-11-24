using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using E015.Contracts;

namespace E015
{
    public interface IMessage {}

    public interface ICommand : IMessage{}

    public interface IEvent : ICommand { }

    public interface IApplicationService
    {
        void Execute(object command);
    }

    public interface IFactoryEvent : IEvent
    {
        FactoryId Id { get; }
    }
    public interface IFactoryCommand : ICommand
    {
        FactoryId Id { get; }
    }

    public interface IEventStore
    {
        EventStream LoadEventStream(string id);
        void AppendEventsToStream(string id, long expectedVersion, ICollection<IEvent> events);
    }

    public sealed class EventStream
    {
        public long StreamVersion;
        public List<IEvent> Events = new List<IEvent>();
    }

    /// <summary>
    /// Special exception that is thrown by application services
    /// when something goes wrong in an expected way. This exception
    /// bears human-readable code (the name property acting as sort of a 'key') which is used to verify it
    /// in the tests.
    /// An Exception name is a hard-coded identifier that is still human readable but is not likely to change.
    /// </summary>
    [Serializable]
    public class DomainError : Exception
    {
        public DomainError(string message) : base(message) { }

        /// <summary>
        /// Creates domain error exception with a string name that is easily identifiable in the tests
        /// </summary>
        /// <param name="name">The name to be used to identify this exception in tests.</param>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        public static DomainError Named(string name, string format, params object[] args)
        {
            return new DomainError(string.Format(format, args))
            {
                Name = name
            };
        }

        public string Name { get; private set; }

        public DomainError(string message, Exception inner) : base(message, inner) { }

        protected DomainError(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) { }
    }
}
