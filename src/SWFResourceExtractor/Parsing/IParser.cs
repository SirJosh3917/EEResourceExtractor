using System.Threading.Tasks;

namespace SWFResourceExtractor.Parsing
{
	public interface IParser
	{
		bool CanParse(string ln);

		Task<IResource> ParseAsync(string ln);
	}
}