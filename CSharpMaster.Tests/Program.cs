using System;
using System.Threading.Tasks;

namespace CSharpMaster.Tests
{
    class Program
    {
        static async Task Main()
        {
            await SimpleTests.RunAllAsync();

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}