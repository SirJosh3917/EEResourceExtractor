using Newtonsoft.Json;

using System;
using System.Threading.Tasks;

namespace SWFResourceExtractor.Parsing
{
	[Metadata(MetadataType.ParsingConst, "addSmiley(")]
	[Metadata(MetadataType.Example, @"addSmiley(6, ""Crying"", """", smileysBMD, ""pro"");")]
	public class AddSmiley : IParser
	{
		[Inject] private ItemIdTable _itemIdTable;

		public bool CanParse(string ln)
			=> ln.StartsWith(this.GetMetadata(MetadataType.ParsingConst));

		public Task<IResource> ParseAsync(string line)
		{
			var ln = line.AsSpan();
			var args = ln.GetFunctionParameters();

			return Task.FromResult<IResource>(new Smiley {
				Id = _itemIdTable.Get(args.TryGet(0)),
				Name = args.TryGet(1),
				Description = args.TryGet(2),
				BitmapDataFileName = args.TryGet(3),
				PayVaultId = args.TryGet(4),
			});
		}
	}

	public class Smiley : IResource, IName, IPayVaultId, IId<int>, IBitmapDataFile
	{
		public int Id { get; set; }

		public string Name { get; set; }

		[JsonProperty("desc")]
		public string Description { get; set; }

		public string BitmapDataFileName { get; set; }

		public string PayVaultId { get; set; }

		public override string ToString()
			=> this.GenerateString();
	}
}