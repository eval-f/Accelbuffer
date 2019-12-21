using System;
using System.IO;

namespace AccelbufferCompiler
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: accelc [input files]\n");
                return;
            }

            Console.WriteLine("not supported, in progress...");
            return;

            for (int i = 0; i < args.Length; i++)
            {
                string sourceFilePath = args[i];

                if (!File.Exists(sourceFilePath))
                {
                    Console.WriteLine($"file: '{sourceFilePath}' not found!\n");
                    continue;
                }

                string outputFilePath = Path.ChangeExtension(sourceFilePath, null) + "SerializeProxy.cs";

                Console.WriteLine($"Generate at '{outputFilePath}'!");
            }
        }
    }
}
