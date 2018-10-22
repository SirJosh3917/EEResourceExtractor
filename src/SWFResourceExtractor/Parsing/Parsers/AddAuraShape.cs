using Newtonsoft.Json;

using System;
using System.Threading.Tasks;

namespace SWFResourceExtractor.Parsing
{
	[Metadata(MetadataType.ParsingConst, "addAuraShape(")]
	[Metadata(MetadataType.Example, @"addAuraShape(10,""Bubble"",aurasBubbleBMD,""aurabubble"",8,0.1,false,false);")]
	public class AddAuraShape : IParser
	{
		[Inject] private ItemIdTable _itemIdTable;

		public bool CanParse(string ln)
			=> ln.StartsWith(this.GetMetadata(MetadataType.ParsingConst));

		public Task<IResource> ParseAsync(string line)
		{
			var ln = line.AsSpan();
			var args = ln.GetFunctionParameters();

			int i = 0;

			return Task.FromResult<IResource>(new AuraShape {
				Id = _itemIdTable.Get(args.TryGet(i++)),
				Name = args.TryGet(i++),
				BitmapDataFileName = args.TryGet(i++),
				PayVaultId = args.TryGet(i++),
				TotalFrames = int.Parse(args.TryGet<string>(i++) ?? 0.ToString()),
				SecondsPerFrame = decimal.Parse(args.TryGet<string>(i++) ?? 0.2f.ToString()),
				Rotates = bool.Parse(args.TryGet<string>(i++) ?? false.ToString()),
				Generated = bool.Parse(args.TryGet<string>(i++) ?? true.ToString()),
			});
		}
	}

	public class AuraShape : IResource, IName, IPayVaultId, IBitmapDataFile, IId<int>
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string BitmapDataFileName { get; set; }

		public string PayVaultId { get; set; }

		[JsonProperty("frames")]
		public int TotalFrames { get; set; }

		[JsonProperty("spf")]
		public decimal SecondsPerFrame { get; set; }

		[JsonProperty("rotates")]
		public bool Rotates { get; set; }

		[JsonProperty("gen")]
		public bool Generated { get; set; }

		public override string ToString()
			=> this.GenerateString();
	}
}