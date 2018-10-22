using System;
using System.Collections.Generic;

namespace SWFResourceExtractor
{
	public class ItemIdTable
	{
		public ItemIdTable()
		{
		}

		public ItemIdTable(Dictionary<string, int> defs) : this() => _defs = defs;

		private Dictionary<string, int> _defs;

		public void Add(string stringId, int value)
			=> _defs[stringId] = value;

		public int Get(string stringId)
			=> int.TryParse(stringId ?? throw new ArgumentNullException(nameof(stringId)), out var result) ?
				result
				: _defs.ContainsKey(stringId) ?
					_defs[stringId]
					: _defs[stringId.Substring("ItemId.".Length)];
	}
}