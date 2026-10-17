using System;
using System.Globalization;
using System.Globalization.KerbalCalendar;
using System.Text;

namespace StockTimeFormatter
{
    public class StockDateTimeFormatter : IDateTimeFormatter
    {
        private readonly double epoch;
        private readonly CultureInfo culture;

        public StockDateTimeFormatter()
        {
        }

        public StockDateTimeFormatter(double epoch)
        {
            this.epoch = epoch;
            culture = new CultureInfo(KSP.Localization.Localizer.CurrentLanguage);
        }

        #region IDateTimeFormatter implementation
        public string PrintTimeLong(double time)
        {
            // short-circuit if invalid time passed
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            double epoch = GetEpoch();
            double target = epoch + time;
            double span = target - epoch;
            CalendarConverter.TimeComponents t = CalendarConverter.ToTimeObject(span);
            CalendarConverter.DateComponentsSimple d = CalendarConverter.ToDateObjectSimple(span);
            return string.Format("{0}{1}, {2}{3}, {4}{5}, {6}{7}"
                , d.Day, d.Day == 1 ? "day" : "days"
                , t.Hour, t.Hour == 1 ? "hour" : "hours"
                , t.Minute, t.Minute == 1 ? "minute" : "minutes"
                , t.Second, t.Second == 1 ? "second" : "seconds"
            );
        }

        public string PrintTimeStamp(double time, bool days = false, bool years = false)
        {
            // short-circuit if invalid time passed
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            double epoch = GetEpoch();
            double target = epoch + time;
            double span = target - epoch;
            CalendarConverter.TimeComponents t = CalendarConverter.ToTimeObject(span);
            CalendarConverter.DateComponentsSimple d = CalendarConverter.ToDateObjectSimple(span);
            return string.Format("{0}{1:D2}:{2:D2}:{3:D2}"
                , days ? string.Format("Day {0} - ", d.Day+1) : ""
                , t.Hour
                , t.Minute
                , t.Second
            );
        }

        public string PrintTimeStampCompact(double time, bool days = false, bool years = false)
        {
            // short-circuit if invalid time passed
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            double epoch = GetEpoch();
            double target = epoch + time;
            double span = target - epoch;
            CalendarConverter.TimeComponents t = CalendarConverter.ToTimeObject(span);
            CalendarConverter.DateComponentsSimple d = CalendarConverter.ToDateObjectSimple(span);
            return string.Format("{0}{1}{2:D2}:{3:D2}:{4:D2}"
                , d.Year == 0 ? string.Format("{0}y, ", d.Year) : ""
                , d.Day == 0 ? string.Format("{0}d, ", d.Day) : ""
                , t.Hour
                , t.Minute
                , t.Second
            );
        }

        public string PrintTime(double time, int valuesOfInterest, bool explicitPositive)
        {
            // This is a downright strange and confusing method but as I understand it
            // what it is saying is give it the time in the following format:
            // 1y, 1d, 1h, 1m, 1s
            // But the param 'valuesOfInterest' is used to control how 'deep' it goes
            // IOW a valueofInterest of say 3 would only give you hours, minutes, and seconds
            // Why it isn't more straightforward is beyond me
            // short-circuit if invalid time passed
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            bool isNegativeTime = false;
            if (time < 0) {
                time = Math.Abs(time);
                isNegativeTime = true;
            }
            double epoch = GetEpoch();
            double target = epoch + time;
            double span = target - epoch;
            CalendarConverter.TimeComponents t = CalendarConverter.ToTimeObject(span);
            CalendarConverter.DateComponentsSimple d = CalendarConverter.ToDateObjectSimple(span);
            return string.Format("{0}{1}{2}{3}{4}"
                , isNegativeTime ? "- " : (explicitPositive ? "+ " : "")
                , (valuesOfInterest >= 3 && d.Day != 0) ? string.Format("{0}d, ", d.Day) : ""
                , (valuesOfInterest >= 2 && t.Hour != 0) ? string.Format("{0}h, ", t.Hour) : ""
                , (valuesOfInterest >= 1 && t.Minute != 0) ? string.Format("{0}m, ", t.Minute) : ""
                , valuesOfInterest >= 0 ? string.Format("{0}s", t.Second) : ""
            );
        }

        public string PrintTime(double time, int valuesOfInterest, bool explicitPositive, bool logEnglish)
        {
            return PrintTime(time, valuesOfInterest, explicitPositive);
        }

        public string PrintTimeCompact(double time, bool explicitPositive)
        {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            bool isNegativeTime = false;
            if (time < 0) {
                time = Math.Abs(time);
                isNegativeTime = true;
            }
            double epoch = GetEpoch();
            double target = epoch + time;
            double span = target - epoch;
            CalendarConverter.TimeComponents t = CalendarConverter.ToTimeObject(span);
            CalendarConverter.DateComponentsSimple d = CalendarConverter.ToDateObjectSimple(span);
            return string.Format("{0}{1}{2:D2}:{3:D2}:{4:D2}"
                , isNegativeTime ? "- " : (explicitPositive ? "+ " : "")
                , d.Day != 0 ? d.ToString() : ""
                , t.Hour
                , t.Minute
                , t.Second
            );
        }

        public string PrintDateDelta(double time, bool includeTime, bool includeSeconds, bool useAbs)
        {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            if (time < 0 && useAbs)
                time = Math.Abs(time);

            double epoch = GetEpoch();
            double target = epoch + time;
            double span = target - epoch;
            CalendarConverter.TimeComponents t = CalendarConverter.ToTimeObject(span);
            CalendarConverter.DateComponentsSimple d = CalendarConverter.ToDateObjectSimple(span);

            string delta = string.Format("{0}{1}{2}{3}"
                , d.Day > 0 ? string.Format("{0} {1} ", d.Day, d.Day == 1 ? "day" : "days") : ""
                , t.Hour > 0 && includeTime ? string.Format("{0} {1} ", t.Hour, t.Hour == 1 ? "hour" : "hours") : ""
                , t.Minute > 0 && includeTime ? string.Format("{0} {1} ", t.Minute, t.Minute == 1 ? "minute" : "minutes") : ""
                , t.Second > 0 && includeTime && includeSeconds ? string.Format("{0} {1}", t.Second, t.Second == 1 ? "second" : "seconds") : ""
            );

            if (delta.Length == 0)
                return string.Format("0 {0}", includeTime ? (includeSeconds ? "seconds" : "minutes") : "days");

            return delta;
        }

        public string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs)
        {
            return PrintDateDeltaCompact(time, includeTime, includeSeconds, useAbs, 5);
        }

        public string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs, int interestedPlaces)
        {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);
            if (time < 0 && useAbs)
                time = Math.Abs(time);
            if (time == 0d)
                return string.Format("0{0}", includeTime ? (includeSeconds ? "s" : "m") : "d");

            double epoch = GetEpoch();
            double target = epoch + time;
            double span = target - epoch;
            CalendarConverter.TimeComponents t = CalendarConverter.ToTimeObject(span);
            CalendarConverter.DateComponentsSimple d = CalendarConverter.ToDateObjectSimple(span);

            // interestedPlaces cheat sheet:
            // 5 - seconds
            // 4 - minutes
            // 3 - hours
            // 2 - days
            // 1 - years
            // Note: Printing out the difference in years isn't supported. Instead for levels 1 and 2 we will always print Days.

            StringBuilder sb = StringBuilderCache.Acquire(256);
            if (d.Day > 0 && interestedPlaces >= 1)
            {
                sb.AppendFormat("{0}d", d.Day);
            }

            if (includeTime)
            {
                if (t.Hour > 0 && interestedPlaces >= 3)
                {
                    if (sb.Length != 0)
                        sb.Append(" ");
                    sb.AppendFormat("{0}h", t.Hour);
                }

                if (t.Minute > 0 && interestedPlaces >= 4)
                {
                    if (sb.Length != 0)
                        sb.Append(" ");
                    sb.AppendFormat("{0}m", t.Minute);
                }

                if (includeSeconds && t.Second > 0 && interestedPlaces >= 5)
                {
                    if (sb.Length != 0)
                        sb.Append(" ");
                    sb.AppendFormat("{0}s", t.Second);
                }
            }

            if (sb.Length == 0)
                sb.AppendFormat("0{0}", includeTime ? (includeSeconds ? "s" : "m") : "d");

            return sb.ToStringAndRelease();
        }

        private string FormatDate(double t)
        {
            return CalendarConverter.ToDateStringLocal(CalendarConverter.ToDateObject(t));
        }

        private string FormatTime(double t)
        {
            return CalendarConverter.ToTimeString(CalendarConverter.ToTimeObject(t));
        }

        private string FormatDateTime(double t)
        {
            return FormatDate(t) + " " + FormatTime(t);
        }

        public string PrintDate(double time, bool includeTime, bool includeSeconds = false)
        {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);

            double epoch = GetEpoch();
            double target = epoch + time;
            return includeTime ? FormatDateTime(target) : FormatDate(target);
        }

        public string PrintDateNew(double time, bool includeTime)
        {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);

            double epoch = GetEpoch();
            double target = epoch + time;
            return includeTime ? FormatDateTime(target) : FormatDate(target);
        }

        // This is chiefly used by the MET display in flight view.
        public string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false)
        {
            if (IsInvalidTime(time))
                return InvalidTimeStr(time);

            double epoch = GetEpoch();
            double target = epoch + time;

            return CalendarConverter.ToDateStringISO(CalendarConverter.ToDateObject(target));
        }

        public int Minute => 60;
        public int Hour => 3600;
        public int Day => 21600;
        public int Year => 9201600;
        #endregion

        protected bool IsInvalidTime(double time)
        {
            return double.IsNaN(time) || double.IsPositiveInfinity(time) || double.IsNegativeInfinity(time);
        }

        protected string InvalidTimeStr(double time)
        {
            if (double.IsNaN(time)) {
                return "NaN";
            }
            if (double.IsPositiveInfinity(time)) {
                return "+Inf";
            }
            if (double.IsNegativeInfinity(time)) {
                return "-Inf";
            }
            return null;
        }

        protected double GetEpoch()
        {
            return epoch;
        }
    }
}
