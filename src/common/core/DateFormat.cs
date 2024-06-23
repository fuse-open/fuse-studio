using System;

namespace Outracks
{
	public static class DateFormat
	{
		public static string PeriodBetween(DateTime time, DateTime pastTime)
		{
			var span = time.Subtract(pastTime);
			// Fallback in case pastTime is in the future
			if (span.Ticks < 0)
			{
				if (span.TotalDays >= -1)
				{
					var hours = Math.Abs(span.Hours);
					var minutes = Math.Abs(span.Minutes);
					return "Today in " + hours + (hours == 1 ? " hour " : " hours")
						+ (minutes > 0 ? " " + minutes + (minutes == 1 ? " minute" : " minutes") : "");
				}
				return "In " + Math.Floor(Math.Abs(span.TotalDays)) + " days";
			}

			if (span.TotalDays >= 365)
			{
				var years = time.Year - pastTime.Year;
				return "About " + (years == 1 ? "a year" : years + " years") + " ago";
			}
			if (span.TotalDays >= 28)
			{
				var months = time.Month - pastTime.Month;
				return "About " + (months <= 1 ? "a month" : months + " months") + " ago";
			}

			if (span.TotalDays < 1 && pastTime.Day == time.Day)
				return "Today at " + pastTime.ToShortTimeString();
			if (span.TotalDays < 2 && time.Day - pastTime.Day == 1)
				return "Yesterday at " + pastTime.ToShortTimeString();
			if (span.TotalDays < 7)
				return Math.Ceiling(span.TotalDays) + " days ago";
			if (span.TotalDays < 28)
			{
				var weeks = Convert.ToInt32(span.TotalDays / 7);
				return weeks + " " + (weeks == 1 ? "week" : "weeks") + " ago";
			}

			return "N/A";
		}
	}
}