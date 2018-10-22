using Newtonsoft.Json;

namespace SWFResourceExtractor
{
	public interface IBitmapDataFile
	{
		[JsonProperty("BMD")]
		string BitmapDataFileName { get; }
	}
}