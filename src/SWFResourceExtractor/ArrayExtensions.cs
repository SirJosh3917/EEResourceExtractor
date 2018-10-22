namespace SWFResourceExtractor
{
	internal static class ArrayExtensions
	{
		public static T Get<T>(this T[] arr, int index)
		{
			if (index < arr.Length)
			{
				return arr[index];
			}

			return default;
		}
	}
}