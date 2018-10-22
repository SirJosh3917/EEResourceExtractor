using Newtonsoft.Json;

namespace SWFResourceExtractor
{
	public interface IPayVaultId
	{
		[JsonProperty("payvault")]
		string PayVaultId { get; }
	}
}