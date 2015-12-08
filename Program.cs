using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Blu.Enums;
using Blu.Extensions;
using Blu.Sources;

namespace Blu
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            //args = new string[] { "oliver", "twist", "dickens" };
            var title = string.Join(" ", args.Take(args.Length - 1));
            var author = args.Last();

            Console.WriteLine("Title: {0}", title);
            Console.WriteLine("Author: {0}", author);
            Console.WriteLine("========================================");

            var blueEngine = new BlueEngine();

            using (var catalog = new AggregateCatalog())
            {
                var assemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
                catalog.Catalogs.Add(assemblyCatalog);

                var directoryCatalog = new DirectoryCatalog("Plugins");
                catalog.Catalogs.Add(directoryCatalog);

                var container = new CompositionContainer(catalog);
                container.ComposeParts(blueEngine);
            }

            foreach (var library in blueEngine.Libraries)
            {
                try
                {
                    var t = library.GetType();

                    Console.WriteLine(t.ToString().Split('.').Last().UnCamelCase());
                    var value = LibraryResponse(library, title, author);
                    Console.WriteLine(value);
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static string LibraryResponse(ILibrary library, string title, string author)
        {
            var values = (from fmt in (Format[]) Enum.GetValues(typeof (Format))
                let entries = library.Lookup(title, author, fmt).Where(x => x.ToLower().Contains(title))
                where entries.Any()
                select GetCode(fmt)).ToList();

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
                case Format.AudiobookCD:
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