using PCK.Application;
using System;
using System.Collections.Generic;

namespace PCK.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                BasicUseCase buc = new BasicUseCase();

                buc.SetDiscount(new Discount(15, DiscountRule.After));
                buc.SetUPCDiscount(12345, new Discount(7, DiscountRule.After));
                buc.SetDiscountCup(new DiscountCup(DiscountCupType.Percentage, 100, ""));
                var additionalCosts = new List<AdditionalCost>() { new AdditionalCost("Transport", AdditionalCostType.Percentage, 3M, "") };
                var result = buc.Execute(12345, 21M, additionalCosts, CombiningDiscountsMethod.Multiplicative);

                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
    }
}
