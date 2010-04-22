﻿using AutoVersion.Incrementors;

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace AutoVersion
{
    /// <summary>
    /// This enum defines the supported Build Versioning Styles
    /// </summary>
    /// <remarks>
    /// If you add a enum value, you need to adjust VersionIncrementer.CreateBuildVersion()
    /// </remarks>
    internal enum OLD_BuildVersioningStyleType : int
    {
        /// <summary>
        /// DeltaBaseDate 1.0.41204.2105 (base date = 10/21/1975, this was the original style)
        /// </summary>
        DeltaBaseDate = 0,
        /// <summary>
        /// YYDDD.HHMM 1.0.9021.2106 this style avoids the uint*32 issue by formating the days portion as the day of the year (1-366)
        /// </summary>
        YearDayOfYear_Timestamp = 1,
        /// <summary>
        /// 1.0.10121.2106 2008 (microsoft style) format is (current year - base year)MMDD
        /// </summary>
        DeltaBaseYear = 2,
        /// <summary>
        ///  this style just autoincrements the version field, resets to 0 if the build part is not today
        /// </summary>
        YearDayOfYear_AutoIncrement = 3,
        /// <summary>
        /// autoincrements only the build version. Leaves revision untouched.
        /// </summary>
        AutoIncrementBuildVersion = 4
    }

    /// <summary>
    /// The type of build action
    /// </summary>
    public enum BuildActionType
    {
        /// <summary>
        /// Both
        /// </summary>
        Both,
        /// <summary>
        /// Normal build
        /// </summary>
        Build,
        /// <summary>
        /// Testing
        /// </summary>
        Testing
    }

    /// <summary>
    /// The increment style type
    /// </summary>
    public enum OLD_IncrementStyle
    {
        /// <summary>
        /// No increment.
        /// </summary>
        [Description("No increment")]
        None,
        /// <summary>
        /// Day stamp (dd)
        /// </summary>
        [Description("Day stamp (dd)")]
        DayStamp,
        /// <summary>
        /// Delta base date (y * 12 + month, dayOfMonth since start date)
        /// </summary>
        [Description("Delta base date (y * 12 + month, dayOfMonth since start date)")]
        DeltaBaseDate,
        /// <summary>
        /// Delta base year including day of year (years since start date, dayOfYear)
        /// </summary>
        [Description("Delta base year including day of year (years since start date, dayOfYear)")]
        DeltaBaseYearDayOfYear,
        /// <summary>
        /// Delta base year (years since start date)
        /// </summary>
        [Description("Delta base year (years since start date)")]
        DeltaBaseYear,
        /// <summary>
        /// Simple increment
        /// </summary>
        [Description("Simple increment")]
        Increment,
        /// <summary>
        /// Month stamp (mm)
        /// </summary>
        [Description("Month stamp (mm)")]
        MonthStamp,
        /// <summary>
        /// Time stamp (hh mm)
        /// </summary>
        [Description("Time stamp (hh mm)")]
        TimeStamp,
        /// <summary>
        /// Year stamp (yyyy)
        /// </summary>
        [Description("Year stamp (yyyy)")]
        YearStamp,
        /// <summary>
        /// Year followed by the day of the year (yy, dayOfYear)
        /// </summary>
        [Description("Year followed by the day of the year (yy, dayOfYear)")]
        YearDayOfYear,
        /// <summary>
        /// Year decade stamp (yy)
        /// </summary>
        [Description("Year decade stamp (yy)")]
        YearDecadeStamp,
        /// <summary>
        /// Month and Day stamp (mmdd)
        /// </summary>        
        [Description("Month and Day stamp (mmdd)")]
        MonthAndDayStamp
    }

    /// <summary>
    /// Describes the versioning style.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal class VersioningStyle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersioningStyle"/> class.
        /// </summary>
        public VersioningStyle() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="VersioningStyle"/> class.
        /// </summary>
        /// <param name="other">Another instances to copy the values from.</param>
        public VersioningStyle(VersioningStyle other)
        {
            Major = other.Major;
            Minor = other.Minor;
            Build = other.Build;
            Revision = other.Revision;
        }

        /// <summary>
        /// The default style (None.None.None.None)
        /// </summary>
        //public static VersioningStyle Default = new VersioningStyle();
        internal string ToGlobalVariable()
        {
            /* string startDate = string.Format("{0}/{1}/{2}",
                                              ProjectStartDate.Year,
                                              ProjectStartDate.Month,
                                              ProjectStartDate.Day);

             string versionStyle = string.Format("{0}.{1}.{2}.{3}", Major, Minor, Build, Revision);

             return string.Format("{0}|{1}", versionStyle, startDate);*/

            return ToString(); // NOTE: The project start date value is saved by the solution item
        }

        /// <summary>
        /// Initializes this instances based on a global variable value.
        /// </summary>
        /// <param name="value">The value.</param>
        internal void FromGlobalVariable(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                // Assuming new enum
                string[] styles = value.Split(".".ToCharArray());

                if (styles.Length == 4)
                {
                    Major = BuildVersionIncrementor.Instance.Incrementors[styles[0]];
                    Minor = BuildVersionIncrementor.Instance.Incrementors[styles[1]];
                    Build = BuildVersionIncrementor.Instance.Incrementors[styles[2]];
                    Revision = BuildVersionIncrementor.Instance.Incrementors[styles[3]];
                }
                else
                {
                    throw (new ApplicationException("Invalid versioning style \"" + value + "\"."));
                }
            }
            else
            {
                Major = Minor = Build = Revision = null;
            }
        }

        /// <summary>
        /// Increments the specified version.
        /// </summary>
        /// <param name="currentVersion">The current version.</param>
        /// <param name="buildStartDate">The build start date.</param>
        /// <param name="projectStartDate">The project start date.</param>
        /// <returns>The incremented version.</returns>
        internal Version Increment(Version currentVersion, DateTime buildStartDate, DateTime projectStartDate)
        {
            int major = Major == null ? currentVersion.Major : Major.Increment(currentVersion.Major, buildStartDate, projectStartDate, "");
            int minor = Minor == null ? currentVersion.Minor : Minor.Increment(currentVersion.Minor, buildStartDate, projectStartDate, "");
            int build = Build == null ? currentVersion.Build : Build.Increment(currentVersion.Build, buildStartDate, projectStartDate, "");
            int revision = Revision == null ? currentVersion.Revision : Revision.Increment(currentVersion.Revision, buildStartDate, projectStartDate, "");

            return new Version(major, minor, build, revision);
        }

        internal static string GetDefaultGlobalVariable()
        {
            return new VersioningStyle().ToGlobalVariable();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}",
                                 Major.Name,
                                 Minor.Name,
                                 Build.Name,
                                 Revision.Name);
        }

        #region Ugly PropertyGrid reflection code

        private bool ShouldSerializeMajor()
        {
            return _major != BuiltInBaseIncrementor.NoneIncrementor.Instance;
        }

        private bool ShouldSerializeMinor()
        {
            return _minor != BuiltInBaseIncrementor.NoneIncrementor.Instance;
        }

        private bool ShouldSerializeBuild()
        {
            return _build != BuiltInBaseIncrementor.NoneIncrementor.Instance;
        }

        private bool ShouldSerializeRevision()
        {
            return _revision != BuiltInBaseIncrementor.NoneIncrementor.Instance;
        }

        #endregion

        private BaseIncrementor _major = BuiltInBaseIncrementor.NoneIncrementor.Instance;
        /// <summary>
        /// Gets or sets the major increment style.
        /// </summary>
        /// <value>The major increment style.</value>
        [Description("Major update style")]
        [NotifyParentProperty(true)]
        public BaseIncrementor Major
        {
            get { return this._major; }
            set
            {
                Debug.Assert(value != null);
                this._major = value;
            }
        }

        private BaseIncrementor _minor = BuiltInBaseIncrementor.NoneIncrementor.Instance;
        /// <summary>
        /// Gets or sets the minor increment style.
        /// </summary>
        /// <value>The minor increment style.</value>
        [Description("Minor update style")]
        [NotifyParentProperty(true)]
        public BaseIncrementor Minor
        {
            get { return this._minor; }
            set
            {
                Debug.Assert(value != null);
                this._minor = value;
            }
        }

        private BaseIncrementor _build = BuiltInBaseIncrementor.NoneIncrementor.Instance;
        /// <summary>
        /// Gets or sets the build increment style.
        /// </summary>
        /// <value>The build increment style.</value>
        [Description("Build update style")]
        [NotifyParentProperty(true)]
        public BaseIncrementor Build
        {
            get { return this._build; }
            set
            {
                Debug.Assert(value != null);
                this._build = value;
            }
        }

        private BaseIncrementor _revision = BuiltInBaseIncrementor.NoneIncrementor.Instance;
        /// <summary>
        /// Gets or sets the revision increment style.
        /// </summary>
        /// <value>The revision increment style.</value>
        [Description("Revision update style")]
        [NotifyParentProperty(true)]
        public BaseIncrementor Revision
        {
            get { return this._revision; }
            set
            {
                Debug.Assert(value != null);
                this._revision = value;
            }
        }
    }
}

