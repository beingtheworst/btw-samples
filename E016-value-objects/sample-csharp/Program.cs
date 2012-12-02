using System;
using E016_value_objects;

namespace E016
{
    public static class Program
    {
        public static void Main()
        {
            PrintOperation("10m.Eur() + 15m.Eur()",() => 10m.Eur() + 15m.Eur());
            PrintOperation("10m.Rub() + 15m.Eur()", () => 10m.Rub() + 15m.Eur());

            PrintOperation("32m.Rub() > 30m.Rub()", () => 32m.Rub() > 30m.Rub());
            PrintOperation("10m.Eur() / 2m.Eur()", () => 10m.Eur() / 2m.Eur());

            PrintOperation("32m.Rub() > 2m.Eur()", () => 32m.Rub() > 2m.Eur());

            Console.ReadLine();


        }

        static void PrintOperation<T>(string description, Func<T> operation)
        {
            Console.WriteLine("Evaluating: {0}", description);

            try
            {
                Console.WriteLine("  Result: " + operation());
            }
            catch (Exception ex)
            {
                Console.WriteLine("  Error:  " + ex.Message);
            }
            Console.WriteLine();
        }
    }
}