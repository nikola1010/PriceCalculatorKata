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
            var result = buc.Execute(12345, 20M);
            buc.SetDiscount(0);
            var result2 = buc.Execute(12345, 20M);

            Console.WriteLine(result);
            Console.WriteLine(result2);

            Console.ReadKey();
        }
    }
}
