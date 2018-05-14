using System;
using System.IO;
using System.Linq;

namespace Outracks.Fuse
{
	public static class TableWriter
	{
		public static void WriteTable(this TextWriter writer, Table table, int indent, int columnPadding)
		{
			var columnDefinitions = table.GetColumnDefinitions(columnPadding);
			foreach (var row in table.Rows)
			{
				writer.WriteSpaces(indent);
				for (int i = 0; i < row.Cells.Count; i++)
				{
					var content = row.Cells[i];
					var padding = columnDefinitions[i] - content.Length;

					writer.Write(content);
					writer.WriteSpaces(padding);
				}
				writer.WriteLine();
			}
		}

		public static void WriteSpaces(this TextWriter writer, int spaces)
		{
			writer.Write(new string(' ', spaces));
		}

		static int[] GetColumnDefinitions(this Table table, int columnPadding)
		{
			var columnDefinitions = new int[table.Rows.Select(row => row.Cells.Count).Max()];
			foreach (var row in table.Rows)
			{
				int i = 0;
				foreach (var cell in row.Cells)
				{
					columnDefinitions[i] = Math.Max(columnDefinitions[i], cell.Length + columnPadding);
					i++;
				}
			}
			return columnDefinitions;
		}
	}

}