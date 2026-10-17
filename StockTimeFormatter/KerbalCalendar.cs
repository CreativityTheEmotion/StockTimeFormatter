using System;
using System.Globalization;

namespace System.Globalization.KerbalCalendar
{
	/// <summary>
	/// Converts a universal timestamp, measured in seconds, into the components of a fictional
	/// calendar created for Kerbal Space Program's stock planetary system by CreativityTheEmotion.
	/// </summary>

	public static class CalendarConverter
	{
		/// <summary>
		/// The weekday names used by the calendar. Can be truncated to three letters, if necessary.
		/// </summary>
		public static readonly string[] WeekdayNames =
		{
			"Mohoday",
			"Eveday",
			"Minmusday",
			"Dunaday",
			"Joolday",
			"Sarnusday",
			"Sunday"
		};

		/// <summary>
		/// The month names used by the calendar. Can be truncated to three letters, if necessary.
		/// </summary>
		public static readonly string[] MonthNames =
		{
			"Rainfall",
			"Seedgrow",
			"Harvest",
			"Darkning",
			"Fastning",
			"Revival",
			"Blossom",
			"Greenary"
		};

		/// <summary>
		/// The time components returned by <see cref="CalendarConverter.ToTimeObject"/>.
		/// </summary>
		/// <param name="Hour">The hour component as an integer.</param>
		/// <param name="Minute">The minute component as a integer.</param>
		/// <param name="Second">The second component as a integer.</param>
		public class TimeComponents
		{
			public int Hour { get; set; }
			public int Minute { get; set; }
			public int Second { get; set; }
			
			public TimeComponents (int hour, int minute, int second)
			{
				Hour = hour;
				Minute = minute;
				Second = second;
			}
		};

		/// <summary>
		/// The date components returned by <see cref="CalendarConverter.ToDateObject"/>.
		/// </summary>
		/// <param name="Weekday">The zero-indexed numeric weekday value, which can be cross-referenced against a table of weekday names.</param>
		/// <param name="Month">The zero-indexed numeric month value, which can be cross-referenced against a table of month names.</param>
		/// <param name="Day">The numeric day of the month, starting at 1.</param>
		/// <param name="Year">The numeric year value, starting at 1.</param>
		public struct DateComponents
		{
			public int Weekday { get; set; }
			public int Month { get; set; }
			public int Day { get; set; }
			public int Year { get; set; }
			
			public DateComponents (int weekday, int month, int day, int year)
			{
				Weekday = weekday;
				Month = month;
				Day = day;
				Year = year;
			}
		};

		/// <summary>
		/// The date components returned by <see cref="CalendarConverter.ToDateObjectSimple"/> They match the original KSP calendar, with no months or leap years, exactly.
		/// </summary>
		/// <param name="Day">The numeric day of the month, starting at 0.</param>
		/// <param name="Year">The numeric year value, starting at 0.</param>
		public struct DateComponentsSimple
		{
			public int Day { get; set; }
			public int Year { get; set; }
			
			public DateComponentsSimple (int day, int year)
			{
				Day = day;
				Year = year;
			}
		};

		/// <summary>
		/// Converts a universal timestamp into the time components.
		/// </summary>
		/// <param name="t">
		/// The universal timestamp in seconds, supplied as a floating-point value.
		/// Fractional seconds are discarded by flooring the timestamp.
		/// </param>
		/// <returns>
		/// An object containing numeric <c>hour</c>, <c>minute</c>, and <c>second</c> values.
		/// </returns>
		public static TimeComponents ToTimeObject(double t)
		{
			int seconds = (int)Math.Floor(t);
			int minutes = seconds / 60;
			int second = seconds - minutes * 60;
			int hours = minutes / 60;
			int minute = minutes - hours * 60;
			int days = hours / 6;
			int hour = hours - days * 6;

			return new TimeComponents(hour, minute, second);
		}

		/// <summary>
		/// Converts a universal timestamp into the date components as per CreativityTheEmotion's calendar.
		/// </summary>
		/// <param name="t">The universal timestamp in seconds, supplied as a floating-point value.
		/// Fractional days (21600 seconds long) are discarded by flooring the timestamp divided by 21600.
		/// </param>
		/// <returns>
		/// An object containing numeric <c>weekday</c>, <c>month</c>, <c>day</c>, and <c>year</c> values.
		/// </returns>
		public static DateComponents ToDateObject(double t)
		{
			int days = (int)Math.Floor(t / 21600);
			int weeks = days / 13;
			int weekday = days - weeks * 13;
			weekday = weekday == 12 ? 6 : weekday % 6;

			int monthLoops = days / 107;
			int months = monthLoops * 2;
			int monthLoopDay = days - monthLoops * 107;

			if (monthLoopDay >= 53)
			{
				monthLoopDay -= 53;
				months++;
			}

			int leapYearLoops = months / 223;
			int leapYearLoopMonth = months - leapYearLoops * 223;
			int year = leapYearLoops * 28 + leapYearLoopMonth / 8;
			int month = leapYearLoopMonth % 8;

			return new DateComponents(weekday, month, monthLoopDay + 1, year + 1);
		}

		/// <summary>
		/// Converts a universal timestamp into the date components as the stock game would.
		/// </summary>
		/// <param name="t">The universal timestamp in seconds, supplied as a floating-point value.
		/// Fractional days (21600 seconds long) are discarded by flooring the timestamp divided by 21600.
		/// </param>
		/// <returns>
		/// An object containing numeric <c>day</c> and <c>year</c> values. They are zero-indexed, rather
		/// than one-indexed, as they might be needed to be used in order to declare a span of time, rather than a UT value.
		/// </returns>
		public static DateComponentsSimple ToDateObjectSimple(double t)
		{
			int days = (int)Math.Floor(t / 21600);
			int day = days % 426;
			int year = days / 426;
			return new DateComponentsSimple(day, year);
		}

		/// <summary>
		/// Converts the time components into a string.
		/// </summary>
		/// <param name="t">
		/// The time components, properly structured per the <c>TimeComponents</c> class.
		/// </param>
		/// <returns>
		/// A string where the minute and second values are padded with leading zeroes.
		/// </returns>
		public static string ToTimeString(TimeComponents t, bool includeSeconds)
		{
			return string.Format("{0:D1}", t.Hour) + ":" + string.Format("{0:D2}", t.Minute) + (includeSeconds ? (":" + string.Format("{0:D2}", t.Second)) : "");
		}

		/// <summary>
		/// Converts the date components into a long-form string.
		/// </summary>
		/// <param name="d">
		/// The date components, properly structured per the <c>DateComponents</c> class.
		/// </param>
		/// <returns>
		/// A string where the month is truncated to three letters.
		/// </returns>
		public static string ToDateStringLocal(DateComponents d)
		{
			return string.Format(WeekdayNames[d.Weekday].Substring(0, 3) + " {0:D1} ", d.Day) + MonthNames[d.Month].Substring(0, 3) + string.Format(" {0:D1}", d.Year);
		}

		/// <summary>
		/// Converts the date components into an ISO 8601-compatible string.
		/// </summary>
		/// <param name="d">
		/// The date components, properly structured per the <c>DateComponents</c> class.
		/// </param>
		/// <returns>
		/// A string where the day, month, and year values are padded with leading zeroes.
		/// </returns>
		public static string ToDateStringISO(DateComponents d)
		{
			return string.Format("{0:D4}", d.Year) + "-" + string.Format("{0:D1}", d.Month+1) + "-" + string.Format("{0:D2}", d.Day);
		}
	}
}
