using Newtonsoft.Json;
using SWFResourceExtractor;
using SWFResourceExtractor.Parsing;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MinimapListMaker
{
	class Program
	{
		static void Main(string[] args)
		{
			var resources = JsonConvert.DeserializeObject<List<IResource>>(File.ReadAllText("resources.json"), new JsonSerializerSettings {
				TypeNameHandling = TypeNameHandling.Objects
			});

			var minimapColors = new Dictionary<int, uint>();

			foreach (var brick in resources.Where(x => x is Brick)
										.Select(x => x as Brick)
										.OrderBy(x => x.Id))
			{
				minimapColors[brick.Id] = brick.MapColor;
			}

			using (var fs = File.OpenWrite("minimap.txt"))
			using (var sw = new StreamWriter(fs))
			{
				foreach (var i in minimapColors) {
					sw.WriteLine($"{i.Key} {i.Value}");
				}
			}
		}
	}
}
