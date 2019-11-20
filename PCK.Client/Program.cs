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

            Console.WriteLine(result);

            Console.ReadKey();
        }
    }
}
