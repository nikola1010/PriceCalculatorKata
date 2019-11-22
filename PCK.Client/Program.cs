using PCK.Application;
using System;

namespace PCK.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                BasicUseCase buc = new BasicUseCase();

                buc.SetTax(21M);
                buc.SetDiscount(new Discount(15, DiscountRule.After));
                buc.SetUPCDiscount(12345, new Discount(7, DiscountRule.After));
                buc.SetCombiningDiscountsMethod(CombiningDiscountsMethod.Multiplicative);
                buc.AddAdditionalCost(new AdditionalCost("Transport", AdditionalCostType.Percentage, 3M, ""));

                var result = buc.Execute(12345);

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
