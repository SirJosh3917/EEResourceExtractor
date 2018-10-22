using Newtonsoft.Json;

namespace SWFResourceExtractor
{
	public interface IId<T>
	{
		[JsonProperty("id")]
		T Id { get; }
	}
}