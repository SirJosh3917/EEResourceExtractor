using Newtonsoft.Json;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

using System;
using System.Collections.Concurrent;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace SWFResourceExtractor.Parsing
{
	[Metadata(MetadataType.ParsingConst, ".addBrick(createBrick(")]
	[Metadata(MetadataType.Example, @"_loc3_.addBrick(createBrick(1089,ItemLayer.FORGROUND,blocksBMD,""pro"","""",ItemTab.BLOCK,false,true,261,4293256677,[""White"",""Light""]));")]
	public class AddBrick : IParser, IDisposable
	{
		// https://rextester.com/GKOY91638
		// for evaluating string expressions

		public AddBrick()
		{
			_mathExec = new DataTable();
			_blocksBM = new ConcurrentDictionary<string, Image<Rgba32>>();

			foreach (var i in Directory.GetFiles(Program.DecompLocation, "*locksBM.png"))
			{
				// 1234_items.ItemManager_myBlocksBM.png
				//                        ^^^^^^^^^^

				string fileName = i.Split('_')[2].Split('.')[0] + "D";

				_blocksBM.TryAdd(fileName, Image.Load(i));
			}
		}

		private ConcurrentDictionary<string, Image<Rgba32>> _blocksBM;

		private DataTable _mathExec;

		// "_loc3_.addBrick(createBrick".Length
		public const int SpliceLocation = 27;

		[Inject] private ItemIdTable _itemIdTable;

		public bool CanParse(string ln)
			=> ln.ToString().Contains(this.GetMetadata(MetadataType.ParsingConst));

		public Task<IResource> ParseAsync(string line)
		{
			var ln = line.AsSpan();
			// probably a "hack", but we can be sure it'll work 100% of the time
			var args = ln.Slice(SpliceLocation).GetFunctionParameters();

			int i = 0;

			var itemId = args.Get(i++);

			// if the first charecter isn't a number or 'I', ignore it.
			// it will only allow block Ids or ItemId.s in, and prevent _locX_ and paramX

			var firstChar = itemId.Substring(0, 1)[0];

			if (!((firstChar >= '0' && firstChar <= '9') || firstChar == 'I'))
			{
				return Task.FromResult<IResource>(null);
			}

			var brick = new Brick {
				Id = _itemIdTable.Get(itemId),
				ItemLayer = ParseItemLayer(args.Get(i++)),
				BitmapDataFileName = args.Get(i++),
				PayVaultId = args.Get(i++),
				Description = args.Get(i++),
				ItemTab = ParseItemTab(args.Get(i++)),
				OwnerOnly = bool.Parse(args.Get(i++)),
				HasShadow = bool.Parse(args.Get(i++)),
				PositionInFile = int.Parse(_mathExec.Compute(args.Get(i++), null).ToString()),
				MapColor = (uint)i++,
				Tags = (args.Get(i++) ?? "").AsSpan().GetArray(),
				AdminOnly = bool.Parse(args.Get(i++) ?? false.ToString()),
				NeedsPurchase = bool.Parse(args.Get(i++) ?? false.ToString()),
				IsContestSlot = bool.Parse(args.Get(i++) ?? false.ToString()),
			};

			// position 8 is where we have to check if it's -1, and then average the colors

			const int mapColPos = 9;
			const int avgSign = -1;

			if (args[mapColPos] == (avgSign).ToString())
			{
				brick.MapColor = AverageMapColor(brick)
									.GetAwaiter()
									.GetResult();
			}
			else
			{
				brick.MapColor = uint.Parse(args[mapColPos]);
			}

			brick.PngImage = GetImageStream(brick)
								.GetAwaiter()
								.GetResult();

			return Task.FromResult<IResource>(brick);
		}

		public void Dispose()
		{
			foreach (var i in _blocksBM.Values)
			{
				i.Dispose();
			}
		}

		private async Task<byte[]> GetImageStream(Brick brick)
		{
			var img = _blocksBM[brick.BitmapDataFileName];

			int x = brick.PositionInFile * 16,
				initX = x;

			using (var genImg = new Image<Rgba32>(16, 16))
			{
				for (; x < initX + 16; x++)
					for (int y = 0; y < 16; y++)
					{
						genImg[x - initX, y] = img[x, y];
					}

				using (var ms = new MemoryStream())
				{
					genImg.SaveAsPng(ms);
					return ms.ToArray();
				}
			}
		}

		private async Task<uint> AverageMapColor(Brick brick)
		{
			var img = _blocksBM[brick.BitmapDataFileName];

			int x = brick.PositionInFile * 16,
				initX = x;

			long r = 0, g = 0, b = 0, a = 0;

			for (; x < initX + 16; x++)
				for (int y = 0; y < 16; y++)
				{
					var p = img[x, y];
					r += p.R;
					g += p.G;
					b += p.B;
					a += p.A;
				}

			const long totalPxls = 16 * 16;

			uint avgR = (uint)(r / totalPxls),
				avgG = (uint)(g / totalPxls),
				avgB = (uint)(b / totalPxls),
				avgA = (uint)(a / totalPxls);

			return 0xFF000000 | avgR << 16 | avgG << 8 | avgB << 0;
		}

		private static ItemLayer ParseItemLayer(string layer)
		{
			//TODO: get something better?

			switch (layer.Substring("ItemLayer.".Length))
			{
				case "FORGROUND": return ItemLayer.Foreground;
				case "BACKGROUND": return ItemLayer.Background;
				case "DECORATION": return ItemLayer.Decoration;
				case "ABOVE": return ItemLayer.Above;

				default: throw new Exception($"//TODO: name exception, but parsing went wrong {nameof(ItemLayer)}");
			}
		}

		private static ItemTab ParseItemTab(string tab)
		{
			//TODO: better?

			switch (tab.Substring("ItemTab.".Length))
			{
				case "BLOCK": return ItemTab.Block;
				case "ACTION": return ItemTab.Action;
				case "DECORATIVE": return ItemTab.Decorative;
				case "BACKGROUND": return ItemTab.Background;

				default: throw new Exception($"//TODO: name exception, but parsing went wrong {nameof(ItemTab)}");
			}
		}

		private static int Increment(ref int current, int amt = 1)
		{
			current += amt;
			return current;
		}
	}

	public class Brick : IResource, IBitmapDataFile, IPayVaultId, IId<int>
	{
		public int Id { get; set; }

		[JsonProperty("layer")]
		public ItemLayer ItemLayer { get; set; }

		public string BitmapDataFileName { get; set; }

		public string PayVaultId { get; set; }

		[JsonProperty("desc")]
		public string Description { get; set; }

		[JsonProperty("tab")]
		public ItemTab ItemTab { get; set; }

		[JsonProperty("owner")]
		public bool OwnerOnly { get; set; }

		[JsonProperty("admin")]
		public bool AdminOnly { get; set; }

		[JsonProperty("shadow")]
		public bool HasShadow { get; set; }

		[JsonProperty("pos")]
		public int PositionInFile { get; set; }

		/// <summary>You should check if the ItemLayer is a Decoration. If so, ignore the map color and represent it as '0'.</summary>
		[JsonProperty("mapCol")]
		public uint MapColor { get; set; }

		[JsonProperty("tags")]
		public string[] Tags { get; set; }

		///<summary>Make what you will, but this value is set to true for diamond blocks & all the contest trophies.
		///The name certainly doesn't fit, but I'm not spending much time on it.</summary>
		[JsonProperty("buy")]
		public bool NeedsPurchase { get; set; }

		/// <summary>Yet another vague value, but it's set to true for all the customly drawn contest blocks, if that makes sense.</summary>
		[JsonProperty("contestslot")]
		public bool IsContestSlot { get; set; }

		[JsonProperty("img")]
		public byte[] PngImage { get; set; }

		public override string ToString()
			=> this.GenerateString();
	}

	public enum ItemLayer : int
	{
		Foreground = 0,
		Background = 1,
		Decoration = 2,
		Above = 3,
	}

	public enum ItemTab : int
	{
		Block = 0,
		Action = 1,
		Decorative = 2,
		Background = 3,
	}
}