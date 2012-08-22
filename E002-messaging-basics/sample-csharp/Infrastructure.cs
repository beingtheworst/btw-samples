using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace E002
{
    // [rinat abdullin]: during the podcast recording we had more complex
    // message serializer in this file (taken from Lokad.CQRS). However,
    // for the first episode it was an overkill...
    public class SimpleNetSerializer 
    {
        readonly BinaryFormatter _formatter = new BinaryFormatter();
        public void WriteMessage(object message, Type type, Stream stream)
        {
            _formatter.Serialize(stream, message);
        }

        public object ReadMessage(Stream stream)
        {
            return _formatter.Deserialize(stream);
        }
    }
}