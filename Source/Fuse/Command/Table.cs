using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Mono.Options;

namespace Outracks.Fuse
{
	public sealed class Table
	{
		public readonly string Name;
		public readonly IImmutableList<Row> Rows;

		public Table(string name, IEnumerable<Row> rows)
			: this(name, rows.ToImmutableList())
		{ }

		public Table(string name, IImmutableList<Row> rows)
		{
			Name = name;
			Rows = rows;
		}
	}

	public sealed class Row
	{
		public readonly IImmutableList<string> Cells;

		public Row(params string[] cells)
			: this(ImmutableList.Create(cells))
		{ }

		public Row(IImmutableList<string> cells)
		{
			Cells = cells;
		}
	}

	public static class TableExtensions
	{
		public static Table ToTable(this OptionSet optionSet)
		{
			return new Table(
				"Options",
				optionSet
					.Where(o => !o.Hidden)
					.Select(o => new Row(o.GetNames()
								.Select(s => s.Length == 1 ? "-" + s : "--" + s)
								.Select(s => o.Prototype.Contains("=") ? s + "=" : s)
								.Join(", "), 
							o.Description)));
		}
	}
}
