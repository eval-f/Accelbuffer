using System;
using System.IO;

namespace Accelbuffer.Helper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: accelc <c_sharp_source_file_path> [output_file_path]\n");
                return;
            }

            string sourceFilePath = args[0];
            
            if (!File.Exists(sourceFilePath))
            {
                Console.WriteLine($"file: '{sourceFilePath}' not found!\n");
                return;
            }

            string outputFilePath = args.Length > 1 ? args[1] : Path.ChangeExtension(sourceFilePath, null) + "SerializeProxy.cs";

            Console.WriteLine($"Generate successfully!\n\nFile path: '{outputFilePath}'.");
        }
    }
}
