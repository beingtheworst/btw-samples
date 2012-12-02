using System;

namespace E016_value_objects
{
    /// <summary>
    /// 	Extensions around
    /// 	<see cref="CurrencyAmount" />
    /// </summary>
    public static class CurrencyAmountExtendsDecimal
    {
        /// <summary>
        /// 	Converts the specified deimal to a currency.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="currency">The currency.</param>
        /// <returns>
        /// 	new instance of the currency amount
        /// </returns>
        public static CurrencyAmount In(this decimal value, CurrencyType currency)
        {
            return new CurrencyAmount(currency, value);
        }

        public static CurrencyAmount Eur(this decimal value)
        {
            return new CurrencyAmount(CurrencyType.Eur, value);
        }

        public static CurrencyAmount Rub(this decimal value)
        {
            return new CurrencyAmount(CurrencyType.Rub, value);
        }

        public static CurrencyAmount In(this double value, CurrencyType currency)
        {
            return new CurrencyAmount(currency, Convert.ToDecimal(value));
        }
    }
}