using PCK.Application;
using System;

namespace PCK.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            BasicUseCase buc = new BasicUseCase();

            var result = buc.Execute(12345, 20M);
            var result2 = buc.Execute(12345, 21M);

            Console.WriteLine(result);
            Console.WriteLine(result2);

            Console.ReadKey();
        }
    }
}
