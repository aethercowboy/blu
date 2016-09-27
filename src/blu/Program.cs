using System;
using System.Collections.Generic;
using System.Composition.Convention;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using blu.Common.Enums;
using blu.Common.Sources;
using System.Reflection;
using blu.Extensions;
using Newtonsoft.Json;

namespace blu
{
    public class Program
    {
        private static string skipFile = "skips.json";

        public static void Main(string[] args)
        {
            //args = new[] { "oliver", "twist", "dickens" };
            var title = string.Join(" ", args.Take(args.Length - 1));
            var author = args.Last();
            var now = DateTime.Now;
            Dictionary<string, DateTime> skips;

            if (File.Exists(skipFile))
            {
                var skipText = File.ReadAllText(skipFile);

                skips = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(skipText);
            }
            else
            {
                skips = new Dictionary<string, DateTime>();
            }

            foreach (var skip in skips)
            {
                if (skip.Value < now)
                {
                    skips.Remove(skip.Key);
                }
            }

            Console.WriteLine($"Title: {title}");
            Console.WriteLine($"Author: {author}");
            Console.WriteLine("========================================");

            var conventions = new ConventionBuilder();
            conventions.ForTypesDerivedFrom<ILibrary>().Export<ILibrary>().Shared();

            var assemblies = new[] {Assembly.GetEntryAssembly()};
            var currentDirectory = Directory.GetCurrentDirectory();
            var assembliesFromDirectory =
                Directory.GetFiles(currentDirectory, "blu.Sources.*.dll", SearchOption.AllDirectories)
                    .Select(AssemblyLoadContext.GetAssemblyName)
                    .Select(Assembly.Load)
                    .ToList();

            var configuration =
                new ContainerConfiguration().WithAssemblies(assemblies, conventions)
                    .WithAssemblies(assembliesFromDirectory, conventions);

            using (var container = configuration.CreateContainer())
            {
                var plugins = container.GetExports<ILibrary>();

                foreach (var plugin in plugins.GroupBy(x => x.GetType()).Select(x => x.FirstOrDefault()))
                {
                    var t = plugin.GetType();
                    var name = t.ToString().Split('.').Last();

                    if (skips.ContainsKey(name))
                    {
                        Console.WriteLine("Skipping for now...");
                        continue;
                    }

                    try
                    {
                        Console.WriteLine(name.UnCamelCase());
                        var value = LibraryResponse(plugin, title, author);
                        Console.WriteLine(value.Result);
                        Console.WriteLine();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        skips.Remove(name);
                        skips.Add(name, now.AddHours(1));
                    }
                }
            }

            var skipOut = JsonConvert.SerializeObject(skips);

            File.WriteAllText(skipFile, skipOut);
        }

        private static async Task<string> LibraryResponse(ILibrary library, string title, string author)
        {
            var values = new List<string>();
            foreach (var fmt in (Format[])Enum.GetValues(typeof(Format)))
            {
                var entries = (await library.Lookup(title, author, fmt)).Where(x => x.ToLower().Contains(title));
                if (entries.Any()) values.Add(GetCode(fmt));
            }

            return string.Join("/", values);
        }

        private static string GetCode(Format fmt)
        {
            switch (fmt)
            {
                case Format.DownloadableAudiobook:
                    return "A";
                case Format.EBook:
                    return "E";
                case Format.AudiobookCd:
                    return "C";
                case Format.Print:
                    return "P";
                case Format.EComic:
                    return "B";
                case Format.EMusic:
                    return "W";
                default:
                    return "?";
            }
        }
    }
}
