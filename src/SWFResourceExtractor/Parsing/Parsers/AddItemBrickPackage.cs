using System;
using System.Threading.Tasks;

namespace SWFResourceExtractor.Parsing
{
	[Metadata(MetadataType.ParsingConst, ":ItemBrickPackage = new ItemBrickPackage(")]
	[Metadata(MetadataType.Example, @"var _loc2_:ItemBrickPackage = new ItemBrickPackage(""basic"",""Basic Blocks"",[""Primary"",""Simple"",""Standard"",""Default""]);")]
	public class AddItemBrickPackage : IParser
	{
		[Inject] private readonly ItemIdTable _itemIdTable;

		public bool CanParse(string ln)
			=> ln.ToString().Contains(this.GetMetadata(MetadataType.ParsingConst));

		public Task<IResource> ParseAsync(string line)
		{
			var ln = line.AsSpan();
			var args = ln.GetFunctionParameters();
			var tags = ln.GetArray(); // could use args[4] in the future

			int i = 0;

			return Task.FromResult<IResource>(new ItemBrickPackage {
				PayVaultId = args.Get(i++),
				Name = args.Get(i++),
				Tags = tags,
			});
		}
	}

	public class ItemBrickPackage : IResource, IName, IPayVaultId
	{
		public string PayVaultId { get; set; }

		public string Name { get; set; }

		public string[] Tags { get; set; }

		public override string ToString()
			=> this.GenerateString();
	}
}