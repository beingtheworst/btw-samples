using System;
using System.Diagnostics.Contracts;

namespace Platform
{
    /// <summary>
    /// Set of helper methods that are slightly more readable than
    /// plain exception throws (but are equivalent to  them)
    /// </summary>
    public static class Ensure
    {
        public static void NotNull<T>(T argument, string argumentName) where T : class
        {
            Contract.Requires(argument != null);
            if (argument == null)
                throw new ArgumentNullException(argumentName);
        }

        public static void Nonnegative(long number, string argumentName)
        {
            Contract.Requires(number >= 0);
            if (number < 0)
                throw new ArgumentOutOfRangeException(argumentName, argumentName + " should be non negative.");
        }

    }
}