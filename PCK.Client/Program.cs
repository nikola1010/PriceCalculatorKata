using PCK.Application;
using System;

namespace PCK.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicUseCase buc = new BasicUseCase();

            buc.SetDiscount(15);
            buc.SetUPCDiscount(12345, 7);
            var result = buc.Execute(12345, 20M);

            buc.ClearAllUPCDiscounts();
            buc.SetUPCDiscount(789, 7);
            var result2 = buc.Execute(12345, 21M);

            Console.WriteLine(result);
            Console.WriteLine(result2);

            Console.ReadKey();
        }
    }
}
