using System;
using System.Collections.Generic;
using System.Text;
using Qreed.Reflection;

namespace AutoVersion.Incrementors
{
    /// <summary>
    /// Internal base class for the default incrementers.
    /// </summary>
    /// <remarks>
    /// This is based on the old <see cref="IncrementStyle"/> enum that defined the type of increment.
    /// </remarks>
    internal abstract class BuiltInBaseIncrementor : BaseIncrementor
    {
        /// <summary>
        /// Gets the name of this incrementor.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                return IncrementStyle.ToString();
            }
        }

        /// <summary>
        /// Gets the description of this incrementor.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                return EnumHelper.GetDescription(IncrementStyle);
            }
        }

        /// <summary>
        /// Gets the increment style.
        /// </summary>
        /// <value>The increment style.</value>
        private OLD_IncrementStyle IncrementStyle
        {
            get
            {
                string name = this.GetType().Name;
                name = name.Substring(0, name.Length - "Incrementor".Length);

                return (OLD_IncrementStyle)Enum.Parse(typeof(OLD_IncrementStyle), name);
            }
        }

        /// <summary>
        /// Increments the specified value.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <param name="buildStart">The build start date/time.</param>
        /// <param name="projectStart">The project start date/time.</param>
        /// <returns>The incremented value.</returns>
        public override int Increment(int value, DateTime buildStart, DateTime projectStart, string projectFilePath)
        {
            string dayOfyear = buildStart.DayOfYear.ToString("000");
            int deltaYears = buildStart.Year - projectStart.Year;
            string yearDecade = buildStart.ToString("yy");

            if (value < 0)
                value = 0;

            switch (IncrementStyle)
            {
                case OLD_IncrementStyle.None:
                    return value;

                case OLD_IncrementStyle.Increment:
                    return value + 1;

                case OLD_IncrementStyle.TimeStamp:
                    return Int32.Parse(string.Format("{0:00}{1:00}", buildStart.Hour, buildStart.Minute));

                case OLD_IncrementStyle.YearStamp:
                    return buildStart.Year;

                case OLD_IncrementStyle.DeltaBaseDate:
                    DateSpan ds = DateSpan.GetDateDifference(buildStart, projectStart);
                    return Int32.Parse(string.Format("{0}{1:00}", (ds.Years * 12) + ds.Months, ds.Days));

                case OLD_IncrementStyle.YearDayOfYear:
                    return Int32.Parse(string.Format("{0}{1:000}", yearDecade, dayOfyear));

                case OLD_IncrementStyle.DeltaBaseYearDayOfYear:
                    return Int32.Parse(string.Format("{0}{1:000}", deltaYears, dayOfyear));

                case OLD_IncrementStyle.DeltaBaseYear:
                    return deltaYears;

                case OLD_IncrementStyle.YearDecadeStamp:
                    return Int32.Parse(yearDecade);

                case OLD_IncrementStyle.MonthStamp:
                    return buildStart.Month;

                case OLD_IncrementStyle.DayStamp:
                    return buildStart.Day;

                case OLD_IncrementStyle.MonthAndDayStamp:
                    return Int32.Parse(string.Format("{0}{1}", buildStart.Month, buildStart.Day));

                default:
                    throw (new ApplicationException("Unknown increment style: " + IncrementStyle.ToString()));
            }
        }

        internal class NoneIncrementor : BuiltInBaseIncrementor
        {
            /// <summary>
            /// Use this to reference a null incrementor.
            /// </summary>
            public static readonly NoneIncrementor Instance = new NoneIncrementor();
        }

        class DayStampIncrementor : BuiltInBaseIncrementor { }
        class DeltaBaseDateIncrementor : BuiltInBaseIncrementor { }
        class DeltaBaseYearDayOfYearIncrementor : BuiltInBaseIncrementor { }
        class DeltaBaseYearIncrementor : BuiltInBaseIncrementor { }
        class IncrementIncrementor : BuiltInBaseIncrementor { }
        class MonthStampIncrementor : BuiltInBaseIncrementor { }
        class TimeStampIncrementor : BuiltInBaseIncrementor { }
        class YearStampIncrementor : BuiltInBaseIncrementor { }
        class YearDayOfYearIncrementor : BuiltInBaseIncrementor { }
        class YearDecadeStampIncrementor : BuiltInBaseIncrementor { }
        class MonthAndDayStampIncrementor : BuiltInBaseIncrementor { }
    }
}

