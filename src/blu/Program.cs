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
using blu.Common;
using blu.Extensions;
using blu.Sources.OhioDigitalLibrary.Sources;
using Newtonsoft.Json;

namespace blu
{
    // ReSharper disable once UnusedMember.Global
    public class Program
    {
        private const string SkipFile = "skips.json";
        private static IBluConsole console;
        private static IList<Type> Libraries { get; set; }

        // ReSharper disable once UnusedMember.Global
        public static void Main(string[] args)
        {
            //args = new[] { "dune", "herbert" };
            var title = string.Join(" ", args.Take(args.Length - 1));
            var author = args.Last();

            //Libraries = new List<Type>
            //{
            //    typeof(OhioDigitalLibrary)
            //};

            console = new BluConsole();

            Run(title, author);
        }

        private static void Run(string title, string author)
        {
            var now = DateTime.Now;
            Dictionary<string, DateTime> skips;

            if (File.Exists(SkipFile))
            {
                var skipText = File.ReadAllText(SkipFile);

                skips = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(skipText);
            }
            else
            {
                skips = new Dictionary<string, DateTime>();
            }

            foreach (var skip in skips.ToList())
            {
                if (skip.Value < now)
                {
                    skips.Remove(skip.Key);
                }
            }

            console.WriteLine($"Title: {title}");
            console.WriteLine($"Author: {author}");
            console.WriteLine("========================================");

            var conventions = new ConventionBuilder();
            conventions.ForTypesDerivedFrom<ILibrary>().Export<ILibrary>().Shared();

            var assemblies = new[] { Assembly.GetEntryAssembly() };
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

                if (Libraries != null && Libraries.Any())
                {
                    plugins = plugins.Where(x => Libraries.Contains(x.GetType()));
                }

                foreach (var plugin in plugins.GroupBy(x => x.GetType()).Select(x => x.FirstOrDefault()))
                {
                    var t = plugin.GetType();
                    var name = t.ToString().Split('.').Last();

                    try
                    {
                        console.WriteLine(name.UnCamelCase());

                        if (skips.ContainsKey(name))
                        {
                            console.WriteLine("Skipping for now...");
                            continue;
                        }

                        var value = LibraryResponse(plugin, title, author);
                        console.WriteLine(value.Result);
                        console.WriteLine();
                    }
                    catch (Exception e)
                    {
                        console.WriteLine(e.Message);
                        skips.Remove(name);
                        skips.Add(name, now.AddHours(1));
                    }
                }
            }

            var skipOut = JsonConvert.SerializeObject(skips);

            File.WriteAllText(SkipFile, skipOut);
        }

        private static async Task<string> LibraryResponse(ILibrary library, string title, string author)
        {
            var values = new List<string>();
            foreach (var fmt in (Format[]) Enum.GetValues(typeof(Format)))
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