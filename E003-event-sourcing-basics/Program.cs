using System;
using System.Collections.Generic;

namespace E003_event_sourcing_basics
{
    class Program
    {
        static void Main(string[] args)
        {



        }
    }

    
    public sealed class ProductBasketState
    {
        public ProductBasketState(IEnumerable<object> events)
        {
            Products = new Dictionary<string, double>();
            foreach (var e in events)
            {
                Mutate(e);
            }
        }

        public void Mutate(object e)
        {
            ((dynamic) this).When((dynamic) e);
        }

        public IDictionary<string, double> Products { get; private set; } 

        public void When(ProductAddedToBasket e)
        {
            if (!Products.ContainsKey(e.Name))
            {
                Products[e.Name] = 0;
            }
            Products[e.Name] += e.Quantity;
        }
        //public void When(BonusCardAddedToBasket e)
        //{
            
        //}
    }

    [Serializable]
    public class AddProductToBasket
    {
        public string Name;
        public double Quantity;
    }
    [Serializable]
    public class ProductAddedToBasket
    {
        public string Name;
        public double Quantity;
    }
    [Serializable]
    public class ApplyDiscountCardToBasket
    {
        public DiscountType Discount;
    }
    public enum DiscountType
    {
        Standard, Premium, LoyalShopper
    }

    //public class DiscountAddedToProduct
    //{
    //    public string Name;
    //    public double
    //}
}
