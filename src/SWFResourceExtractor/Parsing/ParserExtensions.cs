using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SWFResourceExtractor.Parsing
{
	public static class ParserExtensions
	{
		// metadata helpers

		public static string GetMetadata<T>(this T item, MetadataType metadataType)
			=> metadataType.GetFrom(item.GetType());

		public static string GetFrom<T>(this MetadataType metadataType)
			=> metadataType.GetFrom(typeof(T));

		public static string GetFrom(this MetadataType metadataType, Type type)
			=> type
				.GetCustomAttributes(false)
				.OfType<MetadataAttribute>()
				.First(x => x.MetadataType == metadataType)
				.MetadataValue;

		// arrays

		public static string TryGet(this string[] array, int index)
		{
			var val = array.TryGet<string>(index);
			return val == default ?
				""
				: val;
		}

		public static T TryGet<T>(this T[] array, int index)
		{
			if (array.Length > index &&
				index >= 0)
			{
				var val = array[index];

				return val;
			}

			return default;
		}

		// string manip.

		// this code is SO retarded can we get 1 prayer

		public static string[] GetFunctionParameters(this ReadOnlySpan<char> span)
		{
			//TODO: clean **.-.**

			var args = new List<string>();
			bool inStr = false;
			bool inArgs = false;

			var builder = new StringBuilder();

			int i = 0;

			while (span[i++] != '(') ;

			for (; i < span.Length; i++)
			{
				var c = span[i];

				if (inStr || inArgs)
				{
					builder.Append(c);
				}
				else
				{
					if (!(inStr || inArgs))
					{
						if (c == ',' || c == ')')
						{
							//TODO: extension methods
							while (builder[0] == ' ') builder.Remove(0, 1);
							while (builder[builder.Length - 1] == ' ') builder.Remove(builder.Length - 1, 1);
							if (builder[0] == '\"')
							{
								builder.Remove(0, 1);
								builder.Remove(builder.Length - 1, 1);
							}

							args.Add(builder.ToString());
							builder.Clear();

							if (c == ')') break;
						}
						else
						{
							builder.Append(c);
						}
					}
				}

				if (c == '\\')
				{
					i++;

					if (inStr || inArgs)
					{
						builder.Append(span[i]);
					}

					continue;
				}

				if (c == '\"')
				{
					inStr = !inStr;
				}

				if (c == '[' || c == ']')
				{
					inArgs = !inArgs;
				}
			}

			return args.ToArray();
		}

		public static string[] GetArray(this ReadOnlySpan<char> span)
		{
			var args = new List<string>();
			bool inStr = false;

			var builder = new StringBuilder();

			int i = 0;

			do
			{
				if (i >= span.Length) return new string[] { };
			} while (span[i++] != '[');

			for (; i < span.Length; i++)
			{
				var c = span[i];

				if (inStr)
				{
					builder.Append(c);
				}
				else
				{
					if (!inStr)
					{
						if (c == ',' || c == ']')
						{
							//TODO: extension methods
							if (builder.Length > 0)
							{
								while (builder[0] == ' ') builder.Remove(0, 1);
								while (builder[builder.Length - 1] == ' ') builder.Remove(builder.Length - 1, 1);
								if (builder[0] == '\"')
								{
									builder.Remove(0, 1);
									builder.Remove(builder.Length - 1, 1);
								}
							}

							// let's not use a span :openeyedcryinglaugh:
							args.Add(builder.ToString());
							builder.Clear();

							if (c == ']') break;
						}
						else
						{
							builder.Append(c);
						}
					}
				}

				if (c == '\\')
				{
					i++;

					if (inStr)
					{
						builder.Append(span[i]);
					}

					continue;
				}

				if (c == '\"')
				{
					inStr = !inStr;
				}
			}

			return args.ToArray();
		}
	}
}