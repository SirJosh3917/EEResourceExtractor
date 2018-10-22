using System;

namespace SWFResourceExtractor
{
	public enum MetadataType
	{
		Example,
		ParsingConst,
		AdditionalConst,
	}

	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public sealed class MetadataAttribute : Attribute
	{
		public MetadataAttribute(MetadataType metadataType, string metadataValue)
		{
			MetadataType = metadataType;
			MetadataValue = metadataValue;
		}

		public MetadataType MetadataType { get; set; }
		public string MetadataValue { get; set; }
	}
}