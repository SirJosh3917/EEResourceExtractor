using System;
using System.Linq;
using System.Reflection;

namespace SWFResourceExtractor
{
	public static class ToStringHelper
	{
		public static string GenerateString<T>(this T item)
			where T : class
			=> string.Join("", typeof(T)
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Select(x => $"\t[{x.PropertyType} \"{x.Name}\": {x.GetValue(item)}]{Environment.NewLine}")
				.Prepend($"{{{Environment.NewLine}")
				.Append($"}}{Environment.NewLine}")
				.ToArray());
	}
}