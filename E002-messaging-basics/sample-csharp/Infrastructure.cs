using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf.Meta;

namespace S01E02
{



    public  class MessageSerializer 
    {
        protected ICollection<Type> KnownTypes { get; private set; }
        readonly IDictionary<Type, Formatter> _formattersByType = new Dictionary<Type, Formatter>();
        readonly IDictionary<string, Formatter> _formattersByContract = new Dictionary<string, Formatter>();

        
        protected  Formatter PrepareFormatter(Type type)
        {
            //var name = ContractEvil.GetContractReference(type);
            var name = type.Name;
            var formatter = RuntimeTypeModel.Default.CreateFormatter(type);
            return new Formatter(name, type, formatter.Deserialize, (o, stream) => formatter.Serialize(stream, o));
        }

        protected sealed class Formatter
        {
            public readonly string ContractName;
            public readonly Type FormatterType;
            public readonly Func<Stream, object> DeserializerDelegate;
            public readonly Action<object, Stream> SerializeDelegate;

            public Formatter(string contractName, Type formatterType, Func<Stream, object> deserializerDelegate, Action<object, Stream> serializeDelegate)
            {
                ContractName = contractName;
                DeserializerDelegate = deserializerDelegate;
                SerializeDelegate = serializeDelegate;
                FormatterType = formatterType;
            }
        }

        public MessageSerializer(ICollection<Type> knownTypes)
        {
            KnownTypes = knownTypes;
            Build();
        }

        void Build()
        {
            foreach (var type in KnownTypes)
            {
                var formatter = PrepareFormatter(type);
                try
                {
                    _formattersByContract.Add(formatter.ContractName, formatter);
                }
                catch (ArgumentException ex)
                {
                    var msg = string.Format("Duplicate contract '{0}' being added to {1}", formatter.ContractName, GetType().Name);
                    throw new InvalidOperationException(msg, ex);
                }
                try
                {
                    _formattersByType.Add(type, formatter);
                }
                catch (ArgumentException e)
                {
                    var msg = string.Format("Duplicate type '{0}' being added to {1}", type, GetType().Name);
                    throw new InvalidOperationException(msg, e);
                }
            }
        }

        public void WriteMessage(object message, Type type, Stream stream)
        {
            Formatter formatter;
            if (!_formattersByType.TryGetValue(type, out formatter))
            {
                var s =
                    string.Format(
                        "Can't find serializer for unknown object type '{0}'. Have you passed all known types to the constructor?",
                        message.GetType());
                throw new InvalidOperationException(s);
            }
            using (var bin = new BinaryWriter(stream))
            {
                bin.Write(formatter.ContractName);
                using (var inner = new MemoryStream())
                {
                    formatter.SerializeDelegate(message, inner);
                    bin.Write((int)inner.Position);
                    bin.Write(inner.ToArray());
                }
            }
        }

        public object ReadMessage(Stream stream)
        {
            using (var bin = new BinaryReader(stream))
            {
                var contract = bin.ReadString();
                Formatter formatter;
                if (!_formattersByContract.TryGetValue(contract, out formatter))
                    throw new InvalidOperationException(string.Format("Couldn't find contract type for name '{0}'", contract));
                var length = bin.ReadInt32();
                var data = bin.ReadBytes(length);
                using (var inner = new MemoryStream(data, 0, length))
                {
                    return formatter.DeserializerDelegate(inner);
                }
            }
        }

        static readonly MessageAttribute[] Empty = new MessageAttribute[0];
        public MessageAttribute[] ReadAttributes(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                var attributeCount = reader.ReadInt32();
                if (attributeCount == 0) return Empty;

                var attributes = new MessageAttribute[attributeCount];

                for (var i = 0; i < attributeCount; i++)
                {
                    var key = reader.ReadString();
                    var value = reader.ReadString();
                    attributes[i] = new MessageAttribute(key, value);
                }
                return attributes;
            }
        }

        public void WriteAttributes(ICollection<MessageAttribute> attributes, Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(attributes.Count);
                foreach (var attribute in attributes)
                {
                    writer.Write(attribute.Key ?? "");
                    writer.Write(attribute.Value ?? "");
                }
            }
        }
    }


    public struct MessageAttribute 
    {
        public readonly string Key;
        public readonly string Value;

        public static readonly MessageAttribute[] Empty = new MessageAttribute[0];

        public MessageAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

}