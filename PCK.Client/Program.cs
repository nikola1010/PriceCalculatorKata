using PCK.Application;
using System;

namespace PCK.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicUseCase buc = new BasicUseCase();

            buc.SetDiscount(new Discount(15, DiscountRule.After));
            buc.SetUPCDiscount(12345, new Discount(7, DiscountRule.Before));
            var result = buc.Execute(12345, 20M);

            Console.WriteLine(result);

            Console.ReadKey();
        }
    }
}
