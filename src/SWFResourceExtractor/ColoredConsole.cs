using System;

namespace SWFResourceExtractor
{
	public class ColoredConsole : IDisposable
	{
		public ColoredConsole(ConsoleColor fg, ConsoleColor bg)
		{
			Console.ForegroundColor = fg;
			Console.BackgroundColor = bg;
		}

		public void Dispose() => Console.ResetColor();
	}
}