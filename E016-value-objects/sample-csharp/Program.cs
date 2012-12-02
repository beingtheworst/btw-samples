using System;
using E016_value_objects;

namespace E016
{
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine(10m.Eur() + 15m.Eur());

            Console.WriteLine(32m.Rub() > 30m.Rub());

            Console.Read();



        }
    }
}