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
                buc.SetDiscountCup(new DiscountCup(DiscountCupType.Percentage, 20, ""));
                var additionalCosts = new List<AdditionalCost>();
                var result = buc.Execute(12345, 21M, additionalCosts, CombiningDiscountsMethod.Additive);

                buc.SetDiscountCup(new DiscountCup(DiscountCupType.Absolute, 4, "USD"));
                var result2 = buc.Execute(12345, 21M, additionalCosts, CombiningDiscountsMethod.Additive);

                buc.SetDiscountCup(new DiscountCup(DiscountCupType.Percentage, 30, ""));
                var result3 = buc.Execute(12345, 21M, additionalCosts, CombiningDiscountsMethod.Additive);

                var result4 = buc.Execute(123, 21M, additionalCosts, CombiningDiscountsMethod.Additive);

                Console.WriteLine(result);
                Console.WriteLine(result2);
                Console.WriteLine(result3);
                Console.WriteLine(result4);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }
    }
}
