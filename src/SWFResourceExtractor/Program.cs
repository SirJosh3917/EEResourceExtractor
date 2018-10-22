using Newtonsoft.Json;

using SWFResourceExtractor.Parsing;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SWFResourceExtractor
{
	internal class Program
	{
		public const string FreegameSwf = "Resources/freegame781.swf";
		public const string DecompLocation = "decomp/";
		public const int BadInt = -1337;

		private static void Main()
			=> MainAsync().GetAwaiter().GetResult();

		private static async Task MainAsync()
		{
			if (!Directory.Exists(DecompLocation))
			{
				Console.WriteLine($"Extracting scripts (will take a minute)...");
				ExtractSource("script");
				Console.WriteLine($"Extracting images...");
				ExtractSource("image");
			}

			Console.WriteLine("Loading Item ID Definitions");
			var defs = ItemIdDefinition();

			Console.WriteLine("Extracting Resources");
			var resources = await ExtractResourcesAsync(defs);

			Console.WriteLine($"Resources Extracted");

			var settings = new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.Objects,
				TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
			};

			var indented = JsonConvert.SerializeObject(resources, Formatting.Indented, settings);
			var none = JsonConvert.SerializeObject(resources, Formatting.None, settings);

			File.WriteAllText("resources.json", none);
			File.WriteAllText("resources_indented.json", indented);

			Console.ReadLine();
		}

		private static Dictionary<string, int> ItemIdDefinition()
		{
			const string ItemId = DecompLocation + "scripts/items/ItemId.as";
			const string Identifier = "public static const ";
			var IdentifierLen = Identifier.Length;

			var defs = new Dictionary<string, int>();

			using (var fs = File.OpenRead(ItemId))
			using (var sr = new StreamReader(fs))
				while (!sr.EndOfStream)
				{
					var ln = sr.ReadLine().Trim();

					if (ln.StartsWith(Identifier))
					{
						var extract = ExtractValue(ln);

						if (extract != BadInt)
						{
							defs[ln.Substring(IdentifierLen, ln.IndexOf(':') - IdentifierLen)] = extract;
						}
					}
				}

			return defs;
		}

		private static int ExtractValue(string ln)
		{
			var open = ln.IndexOf('=') + 1;
			var close = ln.IndexOf(';');

			var val = ln.Substring(open, close - open).Trim();

			if (int.TryParse(val, out var res))
				return res;

			using (var _ = new ColoredConsole(ConsoleColor.Red, ConsoleColor.Black))
				Console.WriteLine($"Unable to extract integer from '{ln}' ({val})");
			return BadInt;
		}

		private static async Task<IResource[]> ExtractResourcesAsync(Dictionary<string, int> defs)
		{
			const string ItemManager = DecompLocation + "scripts/items/ItemManager.as";

			Console.WriteLine($"Loading {nameof(ItemIdTable)}");
			var itemIdTable = new ItemIdTable(defs);

			var disposableParsers = new List<IDisposable>(1);

			Console.WriteLine("Discovering parsers...");
			var parsers = Assembly
							.GetExecutingAssembly()
							.GetTypes()
							.Where(x => !x.IsInterface && x.GetInterfaces().Contains(typeof(IParser)))
							.Select(x => {
								var parser = (IParser)InjectionUtils.CreateNew(x, itemIdTable);

								if (x.GetInterfaces().Contains(typeof(IDisposable)))
								{
									disposableParsers.Add((IDisposable)parser);
								}

								return parser;
							})
							.ToArray();

			Console.WriteLine($"{parsers.Length} Parsers discovered");
			Console.WriteLine("Running Parser");

			var results = new List<Task<IResource>>();

			using (var fs = File.OpenRead(ItemManager))
			using (var sr = new StreamReader(fs))
				while (!sr.EndOfStream)
				{
					var rawLn = sr.ReadLine().Trim();

					foreach (var i in parsers)
					{
						if (i.CanParse(rawLn))
						{
							Console.WriteLine($"{i.GetType()} Parsing {rawLn}");
							results.Add(i.ParseAsync(rawLn));
							break;
						}
					}

					Console.WriteLine($"No parser found for {rawLn}");
				}

			foreach (var i in disposableParsers)
			{
				Console.WriteLine($"Disposing {i}");
				i.Dispose();
			}

			return (await Task.WhenAll(results))
					.Where(x => !(x is null))
					.ToArray();
		}

		private static void ExtractSource(string resKnd)
		{
			// https://github.com/Decagon/Termite/blob/master/Termite/Program.cs#L208

			using (var process = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = "java.exe",
					Arguments = $"-jar Resources/ffdec.jar -export {resKnd} {DecompLocation} {FreegameSwf}",
					RedirectStandardOutput = true,
					UseShellExecute = false,
				}
			})
			{
				process.Start();

				// Synchronously read the standard output of the spawned process.
				var reader = process.StandardOutput;
				var output = reader.ReadToEnd();

				// Write the redirected output to this application's window.
				Console.WriteLine(output);

				process.WaitForExit();
				process.Close();
			}
		}
	}
}