using System;
using System.Collections.Generic;

namespace Outracks.Fuse
{
	public class HelpHeader
	{
		public readonly string Name;
		public readonly Optional<string> Description;

		public HelpHeader(string name, Optional<string> description)
		{
			Name = name;
			Description = description;
		}
	}

	public class HelpSynopsis
	{
		public readonly string Usage;
		public HelpSynopsis(string usage)
		{
			Usage = usage;
		}
	}

	public class HelpDetailedDescription
	{
		public readonly string Description;
		public HelpDetailedDescription(string description)
		{
			Description = description;
		}
	}

	public class HelpOptions
	{
		public readonly List<Table> Tables = new List<Table>();
		public HelpOptions(Table table)
		{
			Tables.Add(table);
		}

		public HelpOptions(IEnumerable<Table> tables)
		{
			Tables.AddRange(tables);
		}
	}

	public class HelpArguments
	{
		public readonly HelpHeader HelpHeader;
		public readonly HelpSynopsis Synopsis;
		public readonly HelpDetailedDescription DetailedDescription;
		public readonly Optional<HelpOptions> Options;

		public HelpArguments(HelpHeader helpHeader, HelpSynopsis usage, HelpDetailedDescription detailedDescription, Optional<HelpOptions> options)
		{
			HelpHeader = helpHeader;
			Synopsis = usage;
			DetailedDescription = detailedDescription;
			Options = options;
		}
	}

	public static class HelpWriterExtension
	{
		public static void WriteHelp(this ColoredTextWriter writer, HelpArguments arguments)
		{
			using (writer.PushColor(ConsoleColor.Yellow))
			{
				writer.WriteLine("Usage: " + arguments.Synopsis.Usage);
				arguments.HelpHeader.Description.Do(writer.WriteLine);
			}

			arguments.Options.Do(
				options =>
				{
					foreach (var table in options.Tables)
					{
						using (writer.PushColor(ConsoleColor.Green))
						{
							writer.WriteLine(table.Name.ToUpper());
						}
						writer.WriteTable(table, indent: 4, columnPadding: 4);
					}
				});

			using (writer.PushColor(ConsoleColor.Green))
				writer.WriteLine("DESCRIPTION");
			writer.WriteLine(arguments.DetailedDescription.Description);
		}
	}
}