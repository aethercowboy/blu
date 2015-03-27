using blu.Enums;
using blu.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using blu.Extensions;

namespace blu
{
    class Program
    {
        static void Main(string[] args)
        {
            string title = String.Join(" ", args.Take(args.Length - 1));
            string author = args.Last();

            Console.WriteLine("Title: {0}", title);
            Console.WriteLine("Author: {0}", author);
            Console.WriteLine("========================================");

            List<Type> libraryTypes = new List<Type>() {
                typeof(Gutenberg),
                typeof(LibriVox),
                typeof(GeaugaCountyPublicLibrary),
                typeof(Clevnet),
                typeof(HarrisCountyPublicLibrary),
                typeof(Hoopla)
            };

            foreach (Type t in libraryTypes)
            {
                Console.WriteLine(t.ToString().Split('.').Last().UnCamelCase());
                var value = LibraryResponse(t, title, author);
                Console.WriteLine(value);
                Console.WriteLine();
            }
        }

        private static string LibraryResponse<T>(string title, string author)
            where T : ILibrary, new()
        {
            ILibrary lib = (ILibrary)Activator.CreateInstance<T>();

            List<string> values = new List<string>();

            foreach (Format fmt in (Format[])Enum.GetValues(typeof(Format)))
            {
                var entries = lib.Lookup(title, author, fmt)
                    .Where(x => x.ToLower().Contains(title));

                if (entries.Any())
                {
                    values.Add(GetCode(fmt));
                }
            }

            return String.Join("/", values);
        }

        private static string LibraryResponse(Type t, string title, string author)
        {
            ILibrary lib = (ILibrary) Activator.CreateInstance(t);

            List<string> values = new List<string>();

            foreach(Format fmt in (Format[]) Enum.GetValues(typeof(Format))) {
                var entries = lib.Lookup(title, author, fmt)
                    .Where(x => x.ToLower().Contains(title));

                if (entries.Any())
                {
                    values.Add(GetCode(fmt));
                }
            }

            return String.Join("/", values);
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
                default:
                    return "?";
            }
        }
    }
}
