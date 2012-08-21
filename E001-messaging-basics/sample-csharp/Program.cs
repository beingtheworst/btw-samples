using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace S01E02
{
    class Program
    {
        static void Main(string[] args)
        {
            var basket = new ProductBasket();

            Comment(@"// simply adding a product as a line
            // product is added immediately");
            
            basket.AddProduct("bread", 1);
            basket.AddProduct("milk", 2);


            Comment("declare a message");
            var message = new AddProductToBasketMessage("bread",1);
            Comment("we have message declared. we can 'send' it.");
            ApplyMessage(basket, message);

            Comment("we can put messages into some queue and send them later");
            var queue = new Queue<object>();
            queue.Enqueue(new AddProductToBasketMessage("white wine", 1));
            queue.Enqueue(new AddProductToBasketMessage("shrimps", 10));

            Comment(@"
            // this is what temporal decoupling is. Our product basket
            // does not need to be available at the moment, when
            // we memorize messages

            // now, let's send them");
            while(queue.Count>0)
            {
                ApplyMessage(basket, queue.Dequeue());
            }

            Comment(@"Now let's serialize our message to binary form,
            which can travel between processes");
            var serializer = new MessageSerializer(new[] {typeof(AddProductToBasketMessage), typeof(RemoveProductMessage)});

            Comment(@"
            // serialization is a process of recording object (which exists in memory)
            // to a binary representation (which is a set of bytes).");
            using (var mem =new MemoryStream())
            {
                var msg = new AddProductToBasketMessage("rosemary", 1);
                serializer.WriteMessage(msg, msg.GetType(), mem);
                Comment("let's see how this message would look like in binary form");
                Console.WriteLine(BitConverter.ToString(mem.ToArray()));
                Comment(@"And if we tried to open it in text editor...
                Note the readable string content with some 'garbled' binary data!");
                Console.WriteLine(Encoding.ASCII.GetString(mem.ToArray()));


                Comment("now we'll save message to disk to file 'message.bin'");
                File.WriteAllBytes("message.bin", mem.ToArray());
            }

            Comment("Read message from file 'message.bin'");
            using (var stream = File.OpenRead("message.bin"))
            {
                var readMessage = serializer.ReadMessage(stream);
                Comment("Message was read: " + readMessage);
                Comment("Now let's apply it to the basket");
                ApplyMessage(basket, readMessage);
            }

            Comment(@"
            // now you've learned what a message is (Just a remote temporally
            // decoupled message call, that can be persisted and then
            // dispatched

            // you also learned how to actually serialize a message to a binary form
            // then deserialize it and dispatch");

            Comment(@"As you can see, you can use messages for passing information
            between machines, telling a story and also persisting");
        }








        static void ApplyMessage(ProductBasket basket, object message)
        {
            // this code accepts the message and actually adds product.
            ((dynamic) basket).When((dynamic)message);
        }

        static void Comment(string message)
        {
            // just printing messages nicely
            // and without spaces in the beginning
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            foreach (var line in message.Split(new[] { Environment.NewLine },StringSplitOptions.None))
            {
                Console.WriteLine(line.TrimStart());
            }
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        


        public class ProductBasket
        {
            IDictionary<string, double> _products = new Dictionary<string, double>(); 

            public void AddProduct(string name, double quantity)
            {
                double currentQuantity;
                if (!_products.TryGetValue(name,out currentQuantity))
                {
                    currentQuantity = 0;
                }
                _products[name] = quantity + currentQuantity;

                Console.WriteLine("Basket: added {0} of '{1}'", quantity, name);
            }

            public void When(AddProductToBasketMessage toBasketMessage)
            {
                Console.Write("[Message]: ");
                AddProduct(toBasketMessage.Name, toBasketMessage.Quantity);
            }
        }

        public class AddProductToBasketMessage
        {
            public readonly string Name;
            public readonly double Quantity;

            public AddProductToBasketMessage(string name, double quantity)
            {
                Name = name;
                Quantity = quantity;
            }
            public override string ToString()
            {
                return string.Format("Add {0} of {1} to basket", Quantity, Name);
            }
        }
        public class RemoveProductMessage
        {
            public readonly string Name;
            public readonly double Quantity;
            public RemoveProductMessage(string name, double quantity)
            {
                Name = name;
                Quantity = quantity;
            }

            public override string ToString()
            {
                return string.Format("Remove {0} of {1} from basket", Quantity, Name);
            }
        }





    }
}
