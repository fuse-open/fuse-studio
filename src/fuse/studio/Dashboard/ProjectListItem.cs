using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Dashboard
{
	class ProjectListItem
	{
		public readonly Optional<AbsoluteFilePath> ProjectFile;
		public readonly string MenuItemName;
		public readonly string Title;
		public readonly string Description;
		public readonly string DescriptionTitle;
		public readonly string Location;
		public readonly string LocationTitle;
		public readonly Command Command;
		public readonly IControl Thumbnail;

		public ProjectListItem(
			string menuItemName,
			string title,
			string descriptionTitle,
			string description,
			string locationTitle,
			string location,
			Command command,
			IControl thumbnail,
			Optional<AbsoluteFilePath> projectFile = default(Optional<AbsoluteFilePath>))
		{
			MenuItemName = menuItemName;
			ProjectFile = projectFile;
			Title = title;
			DescriptionTitle = descriptionTitle;
			Description = description;
			LocationTitle = locationTitle;
			Location = location;
			Command = command;
			Thumbnail = thumbnail;
		}

		public override string ToString()
		{
			return "{ Title: " + Title + ", Description: " + Description + " }";
		}
	}
}
