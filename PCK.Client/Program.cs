using PCK.Application;
using System;
using System.Collections.Generic;

namespace PCK.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicUseCase buc = new BasicUseCase();

            buc.SetDiscount(new Discount(15, DiscountRule.After));
            buc.SetUPCDiscount(12345, new Discount(7, DiscountRule.After));
            var additionalCosts = new List<AdditionalCost>() { new AdditionalCost("Packaging", AdditionalCostType.Percentage, 1), new AdditionalCost("Transport", AdditionalCostType.Absolute, 2.2M) };
            var result = buc.Execute(12345, 21M, additionalCosts);

            buc.SetDiscount(null);
            buc.ClearAllUPCDiscounts();
            var result2 = buc.Execute(12345, 21M, new List<AdditionalCost>());

            Console.WriteLine(result);
            Console.WriteLine(result2);

            Console.ReadKey();
        }
    }
}
