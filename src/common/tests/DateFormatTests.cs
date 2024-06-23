using System;
using NUnit.Framework;

namespace Outracks.Tests
{
	[TestFixture]
	public class DateFormatTests
	{
		[Test]
		public void SameDateReturnsTodayString()
		{
			var time = DateTime.Parse("2017-06-30T14:00:00");
			var pastTime = DateTime.Parse("2017-06-30T13:37:00");

			Assert.AreEqual("Today at " + pastTime.ToShortTimeString(), DateFormat.PeriodBetween(time, pastTime));
		}

		[Test]
		public void OneDayBeforeReturnsYesterdayString()
		{
			var time = DateTime.Parse("2017-06-30");
			var oneDayEarlier = DateTime.Parse("2017-06-29T13:37:00");
			Assert.AreEqual(
				"Yesterday at " + oneDayEarlier.ToShortTimeString(),
				DateFormat.PeriodBetween(time, oneDayEarlier));
		}

		[Test]
		public void TheDayBeforeYesterdayIsNotYesterday()
		{
			var time = DateTime.Parse("2017-06-30T15:00:00");
			var notYesterday = DateTime.Parse("2017-06-28T17:00:00");
			Assert.AreEqual(
				"2 days ago",
				DateFormat.PeriodBetween(time, notYesterday));
		}

		[TestCase("2017-06-30", "2017-06-27", "3 days ago")]
		[TestCase("2017-06-30", "2017-06-24", "6 days ago")]
		[TestCase("2017-06-30", "2017-06-23", "1 week ago")]
		[TestCase("2017-06-30", "2017-06-16", "2 weeks ago")]
		[TestCase("2017-06-30", "2017-06-09", "3 weeks ago")]
		[TestCase("2017-06-30", "2017-06-02", "About a month ago")]
		[TestCase("2017-06-30", "2017-04-30", "About 2 months ago")]
		[TestCase("2017-06-30", "2016-06-30", "About a year ago")]
		[TestCase("2017-06-30", "2012-06-30", "About 5 years ago")]
		[Test]
		public void TimeSinceTodaySinceStringTest(string timeStr, string pastTimeStr, string expected)
		{
			var time = DateTime.Parse(timeStr);
			var pastTime = DateTime.Parse(pastTimeStr);
			Assert.AreEqual(expected, DateFormat.PeriodBetween(time, pastTime));
		}

		[Test]
		public void TimePeriodBetweenFutureTime()
		{
			var time = DateTime.Parse("2017-07-27");
			var future = DateTime.Parse("2017-10-10");
			Assert.AreEqual("In 75 days", DateFormat.PeriodBetween(time, future));
		}

		[Test]
		public void TimePeriod2Hours15MinutesLaterTheSameDay()
		{
			var time = DateTime.Parse("2017-07-27T12:00:00");
			var later = DateTime.Parse("2017-07-27T14:15:00");

			Assert.AreEqual("Today in 2 hours 15 minutes", DateFormat.PeriodBetween(time, later));
		}


		[Test]
		public void TimePeriod3HoursLaterTheSameDay()
		{
			var time = DateTime.Parse("2017-07-27T12:00:00");
			var later = DateTime.Parse("2017-07-27T15:00:00");

			Assert.AreEqual("Today in 3 hours", DateFormat.PeriodBetween(time, later));
		}
	}
}