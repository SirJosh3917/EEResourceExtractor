using Newtonsoft.Json;

using System;
using System.Threading.Tasks;

namespace SWFResourceExtractor.Parsing
{
	[Metadata(MetadataType.ParsingConst, "addNpc(")]
	[Metadata(MetadataType.Example, @"addNpc(ItemId.NPC_FROG,""npcfrog"",_loc1_,9,[""Frog"",""Green""],6.5 / 3,-7);")]
	public class AddNpc : IParser
	{
		[Inject] private ItemIdTable _itemIdTable;

		public bool CanParse(string ln)
			=> ln.StartsWith(this.GetMetadata(MetadataType.ParsingConst));

		public Task<IResource> ParseAsync(string line)
		{
			var ln = line.AsSpan();
			var args = ln.GetFunctionParameters();

			int i = 0;

			return Task.FromResult<IResource>(new Npc {
				Id = _itemIdTable.Get(args.Get(i++)),
				PayVaultId = args.Get(i++),
				__ = args.Get(i++),
				AnimationFrames = int.Parse(args.Get(i++)),
				Tags = args.Get(i++).AsSpan().GetArray(),
			});
		}
	}

	public class Npc : IResource, IId<int>, IPayVaultId
	{
		[JsonIgnore] internal string __ { get; set; }

		public int Id { get; set; }

		public string PayVaultId { get; set; }

		[JsonProperty("frames")]
		public int AnimationFrames { get; set; }

		[JsonProperty("tags")]
		public string[] Tags { get; set; }

		public override string ToString()
			=> this.GenerateString();
	}
}