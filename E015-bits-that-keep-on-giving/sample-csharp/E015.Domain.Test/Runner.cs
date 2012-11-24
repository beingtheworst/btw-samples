using System;
using System.Reflection;
using E015.Domain.ApplicationServices.Factory;
using System.Linq;
using NUnit.Framework;

namespace E015.Domain
{
    public class Runner
    {
        public static void Main()
        {
            // you can run these tests from NUnit, but if you don't
            // have NUnit, you can run them in a console as well

            var specs = typeof(assign_employee_to_factory).Assembly.GetExportedTypes()
                   .Where(t => !t.IsAbstract)
                .Where(t => typeof(factory_application_service_spec).IsAssignableFrom(t))
                .Select(t => (factory_application_service_spec)Activator.CreateInstance(t));

            foreach (var spec in specs)
            {
                RunSpecification(spec);
            }
            Console.ReadLine();

        }

        static void RunSpecification(factory_application_service_spec specification)
        {
            Console.WriteLine(new string('=', 80));
            var cases = specification.GetType()
                .GetMethods().Where(m => m.IsDefined(typeof(TestAttribute), true));
            Print(ConsoleColor.DarkGreen, "Fixture:   {0}", specification.GetType().Name.Replace('_', ' '));
            foreach (var methodInfo in cases)
            {
                Console.WriteLine(new string('-', 80));
                var name = methodInfo.Name;
                
                Console.WriteLine();
                try
                {
                    specification.GetSpecificationName = () => name;
                    specification.SetUpSpecification();
                    
                    methodInfo.Invoke(specification, null);
                    Print(ConsoleColor.DarkGreen, "\r\nPASSED!");
                }
                catch(TargetInvocationException ex)
                {
                    Print(ConsoleColor.DarkRed, "\r\nFAIL! " + ex.InnerException.Message);
                }
                catch (Exception ex)
                {
                    Print(ConsoleColor.DarkRed, "\r\nFAIL! " + ex.Message);
                }
                finally
                {
                    specification.TeardownSpecification();
                }
            }
        }

        static void Print(ConsoleColor color, string format, params object[] args)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(format, args);
            Console.ForegroundColor = old;
        }
    }
}