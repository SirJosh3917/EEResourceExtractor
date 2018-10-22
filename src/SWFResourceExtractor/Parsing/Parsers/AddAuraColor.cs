using System;
using System.Threading.Tasks;

namespace SWFResourceExtractor.Parsing
{
	[Metadata(MetadataType.ParsingConst, "addAuraColor(")]
	[Metadata(MetadataType.Example, @"addAuraColor(1,""Red"",""aurared"");")]
	public class AddAuraColor : IParser
	{
		[Inject] private ItemIdTable _itemIdTable;

		public bool CanParse(string ln)
			=> ln.StartsWith(this.GetMetadata(MetadataType.ParsingConst));

		public Task<IResource> ParseAsync(string line)
		{
			var ln = line.AsSpan();
			var args = ln.GetFunctionParameters();

			int i = 0;

			return Task.FromResult<IResource>(new AuraColor {
				Id = _itemIdTable.Get(args.TryGet(i++)),
				Name = args.TryGet(i++),
				PayVaultId = args.TryGet(i++),
			});
		}
	}

	public class AuraColor : IResource, IName, IPayVaultId, IId<int>
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string PayVaultId { get; set; }

		public override string ToString()
			=> this.GenerateString();
	}
}