namespace MaterialDrying
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;
    using YamlDotNet.Serialization.TypeResolvers;

    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("Hello!");

            if (!TryChooseConfigurationFile(out var filePath))
            {
                return;
            }

            if (!TryReadFile(filePath, out var constants))
            {
                return;
            }

            var sw = new Stopwatch();
            sw.Start();
            var exportData = new DryingCalculator(constants).Calculate();
            sw.Stop();
            Console.WriteLine($"\nTOTAL CALCULATION TIME: {sw.ElapsedMilliseconds}ms");
            
            Export(exportData, Path.GetFileNameWithoutExtension(filePath));

            Console.WriteLine("Press any key for exit");
            Console.ReadKey();
        }

        private static bool TryChooseConfigurationFile(out string filePath)
        {
            filePath = string.Empty;
            
            var files = Directory.GetFiles(AppDomain.CurrentDomain?.BaseDirectory, "*.yaml", SearchOption.TopDirectoryOnly);

            if (!files.Any())
            {
                Console.WriteLine("Not found any config-files. Please create at least one!");

                return false;
            }

            Console.WriteLine("Choose config-file:");

            var dict = files.Select((file, i) => new
                                                 {
                                                     Number = i + 1,
                                                     FileName = Path.GetFileNameWithoutExtension(file),
                                                     FilePath = file,
                                                 })
                            .ToDictionary(z => z.Number,
                                          z => new { z.FileName, z.FilePath });

            foreach (var pair in dict)
            {
                Console.WriteLine($"{pair.Key} - {pair.Value.FileName}");
            }

            var line = Console.ReadLine();

            try
            {
                var number = Convert.ToInt32(line);

                filePath = dict[number].FilePath;
            }
            catch (FormatException)
            {
                Console.WriteLine("You must enter valid number!");

                return false;
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("You must enter number from list present above!");

                return false;
            }
            
            return true;
        }

        private static bool TryReadFile(string filePath, out Constants constants)
        {
            constants = null;
            
            try
            {
                var encoding = new UTF8Encoding(true);
                string serialized;
            
                using (var file = File.OpenRead(filePath))
                {
                    serialized = encoding.GetString(file.ReadAll());
                }

                constants = new DeserializerBuilder()
                           .WithNamingConvention(NullNamingConvention.Instance)
                           .WithTypeResolver(new DynamicTypeResolver())
                           .Build()
                           .Deserialize<Constants>(serialized);

                Console.WriteLine($"\nUSED PHYSICAL PARAMETERS FOR: {Path.GetFileNameWithoutExtension(filePath)}");
                foreach (var prop in constants.GetType()
                                              .GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    Console.WriteLine($"{prop.Name}: {prop.GetValue(constants)}");
                }

                return true;
            }
            catch (YamlDotNet.Core.YamlException)
            {
                Console.WriteLine($"Config file wrong. Please check '{Path.GetFileNameWithoutExtension(filePath)}.yaml'");

                return false;
            }
        }

        private static byte[] ReadAll(this Stream stream)
        {
            var bytes = (int)stream.Length;
            var buffer = new byte[bytes];
            
            var offset = 0;
            stream.Position = offset;
            
            stream.Read(buffer, offset, bytes);

            return buffer;
        }

        private static void Export(IEnumerable<ExportData> exportData, string fileName)
        {
            try
            {
                new XlsxExporter().Export(fileName, exportData);
            }
            catch (IOException)
            {
                Console.WriteLine($"Please close file: '{fileName}.xlsx'");
            }
        }
    }
}
