using Newtonsoft.Json;

namespace SWFResourceExtractor
{
	public interface IName
	{
		[JsonProperty("name")]
		string Name { get; }
	}
}