using System.Drawing.Design;
using AutoVersion.Extensions;
using AutoVersion.Incrementors;
using AutoVersion.Incrementors.PostProcessors;

using System;
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
    [Editor(typeof(Design.VersioningStyleEditor), typeof(UITypeEditor))]
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
            MajorIncrementActionType = other.MajorIncrementActionType;
            MinorIncrementActionType = other.MinorIncrementActionType;
            BuildIncrementActionType = other.BuildIncrementActionType;
            RevisionIncrementActionType = other.RevisionIncrementActionType;
            MajorProcessor = other.MajorProcessor;
            MinorProcessor = other.MinorProcessor;
            BuildProcessor = other.BuildProcessor;
            RevisionProcessor = other.RevisionProcessor;
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
                string[] versioningData = value.Split('|');

                // Assuming new enum
                string[] styles = value.Split(".".ToCharArray());

                if (styles.Length != 4 && styles.Length != 12)
                {
                    throw (new ApplicationException("Invalid versioning style \"" + value + "\"."));
                }
                else
                {
                    Major = BuildVersionIncrementor.Instance.Incrementors[styles[0]];
                    Minor = BuildVersionIncrementor.Instance.Incrementors[styles[1]];
                    Build = BuildVersionIncrementor.Instance.Incrementors[styles[2]];
                    Revision = BuildVersionIncrementor.Instance.Incrementors[styles[3]];

                    if (styles.Length == 12)
                    {
                        MajorIncrementActionType = (BuildActionType)(Enum.Parse(typeof(BuildActionType), styles[4]));
                        MinorIncrementActionType = (BuildActionType)(Enum.Parse(typeof(BuildActionType), styles[5]));
                        BuildIncrementActionType = (BuildActionType)(Enum.Parse(typeof(BuildActionType), styles[6]));
                        RevisionIncrementActionType = (BuildActionType)(Enum.Parse(typeof(BuildActionType), styles[7]));
                        MajorProcessor = BuildVersionIncrementor.Instance.PostProcessors[styles[8]];
                        MinorProcessor = BuildVersionIncrementor.Instance.PostProcessors[styles[9]];
                        BuildProcessor = BuildVersionIncrementor.Instance.PostProcessors[styles[10]];
                        RevisionProcessor = BuildVersionIncrementor.Instance.PostProcessors[styles[11]];

                    }
                }
            }
            else
            {
                Major = Minor = Build = Revision = null;
                MajorProcessor = MinorProcessor = BuildProcessor = RevisionProcessor = null;
            }
        }

        /// <summary>
        /// Increments the specified version.
        /// </summary>
        /// <param name="currentVersion">The current version.</param>
        /// <param name="buildStartDate">The build start date.</param>
        /// <param name="projectStartDate">The project start date.</param>
        /// <returns>The incremented version.</returns>
        internal Version Increment(Version currentVersion, DateTime buildStartDate, DateTime projectStartDate,
            string projectFilePath, BuildAction buildAction, BuildState buildState, bool traceEnabled)
        {

            int major = currentVersion.Major;
            int minor = currentVersion.Minor;
            int build = currentVersion.Build;
            int revision = currentVersion.Revision;

            if (Major != null && buildAction.EqualsType(MajorIncrementActionType))
                major = Major.Increment(currentVersion.Major, buildStartDate, projectStartDate, projectFilePath);

            if (Minor != null && buildAction.EqualsType(MinorIncrementActionType))
                minor = Minor.Increment(currentVersion.Minor, buildStartDate, projectStartDate, projectFilePath);

            if (Build != null && buildAction.EqualsType(BuildIncrementActionType))
                build = Build.Increment(currentVersion.Build, buildStartDate, projectStartDate, projectFilePath);

            if (Revision != null && buildAction.EqualsType(RevisionIncrementActionType))
                revision = Revision.Increment(currentVersion.Revision, buildStartDate, projectStartDate, projectFilePath);

            Version version = new Version(major, minor, build, revision);

            if (RevisionProcessor != null)
                version = RevisionProcessor.ProcessVersionValue(version, VersionPart.Revision, buildStartDate,
                                                             projectStartDate, projectFilePath, buildAction,
                                                             buildState, PluginCore.PluginBase.CurrentProject.TraceEnabled);

            if (BuildProcessor != null)
                version = BuildProcessor.ProcessVersionValue(version, VersionPart.Build, buildStartDate,
                                                             projectStartDate, projectFilePath, buildAction,
                                                             buildState, PluginCore.PluginBase.CurrentProject.TraceEnabled);

            if (MinorProcessor != null)
                version = MinorProcessor.ProcessVersionValue(version, VersionPart.Minor, buildStartDate,
                                                             projectStartDate, projectFilePath, buildAction,
                                                             buildState, PluginCore.PluginBase.CurrentProject.TraceEnabled);

            if (MajorProcessor != null)
                version = MajorProcessor.ProcessVersionValue(version, VersionPart.Major, buildStartDate,
                                                             projectStartDate, projectFilePath, buildAction,
                                                             buildState, PluginCore.PluginBase.CurrentProject.TraceEnabled);

            return version;
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
            return string.Format("{0}.{1}.{2}.{3}.{4}.{5}.{6}.{7}.{8}.{9}.{10}.{11}",
                                 Major.Name,
                                 Minor.Name,
                                 Build.Name,
                                 Revision.Name,
                                 MajorIncrementActionType.ToString(),
                                 MinorIncrementActionType.ToString(),
                                 BuildIncrementActionType.ToString(),
                                 RevisionIncrementActionType.ToString(),
                                 MajorProcessor.Name,
                                 MinorProcessor.Name,
                                 BuildProcessor.Name,
                                 RevisionProcessor.Name);
        }

        #region Ugly PropertyGrid reflection code

        private bool ShouldSerializeMajor()
        {
            return _major != BuiltInBaseIncrementor.NoneIncrementor.Instance;
        }

        private bool ShouldSerializeMajorProcessor()
        {
            return MajorProcessor != BuiltInBasePostProcessor.NoneProcessor.Instance;
        }

        private bool ShouldSerializeMinor()
        {
            return _minor != BuiltInBaseIncrementor.NoneIncrementor.Instance;
        }

        private bool ShouldSerializeMinorProcessor()
        {
            return MinorProcessor != BuiltInBasePostProcessor.NoneProcessor.Instance;
        }

        private bool ShouldSerializeBuild()
        {
            return _build != BuiltInBaseIncrementor.NoneIncrementor.Instance;
        }

        private bool ShouldSerializeBuildProcessor()
        {
            return BuildProcessor != BuiltInBasePostProcessor.NoneProcessor.Instance;
        }

        private bool ShouldSerializeRevision()
        {
            return _revision != BuiltInBaseIncrementor.NoneIncrementor.Instance;
        }

        private bool ShouldSerializeRevisionProcessor()
        {
            return RevisionProcessor != BuiltInBasePostProcessor.NoneProcessor.Instance;
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

        /// <summary>
        /// Gets or sets the major increment action type.
        /// </summary>
        /// <value>The build major action type.</value>
        [DisplayName("Major build type")]
        [Description("Major update build action type")]
        [NotifyParentProperty(true)]
        [DefaultValue(BuildActionType.Both)]
        public BuildActionType MajorIncrementActionType { get; set; }

        private BasePostProcessor _majorProcessor = BuiltInBasePostProcessor.NoneProcessor.Instance;

        /// <summary>
        /// Gets or sets the major post-processing style.
        /// </summary>
        /// <value>The major post-processing style.</value>
        [Description("Major post-processor style")]
        [NotifyParentProperty(true)]
        public BasePostProcessor MajorProcessor
        {
            get { return _majorProcessor; }
            set { _majorProcessor = value; }
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

        /// <summary>
        /// Gets or sets the minor increment action type.
        /// </summary>
        /// <value>The minor increment action type.</value>
        [DisplayName("Minor build type")]
        [Description("Minor update build action type")]
        [NotifyParentProperty(true)]
        [DefaultValue(BuildActionType.Both)]
        public BuildActionType MinorIncrementActionType { get; set; }

        private BasePostProcessor _minorProcessor = BuiltInBasePostProcessor.NoneProcessor.Instance;

        /// <summary>
        /// Gets or sets the minor post-processing style.
        /// </summary>
        /// <value>The minor post-processing style.</value>
        [Description("Minor post-processor style")]
        [NotifyParentProperty(true)]
        public BasePostProcessor MinorProcessor
        {
            get { return _minorProcessor; }
            set { _minorProcessor = value; }
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

        /// <summary>
        /// Gets or sets the build increment action type.
        /// </summary>
        /// <value>The build increment action type.</value>
        [DisplayName("Build build type")]
        [Description("Build update build action type")]
        [NotifyParentProperty(true)]
        [DefaultValue(BuildActionType.Both)]
        public BuildActionType BuildIncrementActionType { get; set; }

        private BasePostProcessor _buildProcessor = BuiltInBasePostProcessor.NoneProcessor.Instance;

        /// <summary>
        /// Gets or sets the build post-processing style.
        /// </summary>
        /// <value>The build post-processing style.</value>
        [Description("Build post-processor style")]
        [NotifyParentProperty(true)]
        public BasePostProcessor BuildProcessor
        {
            get { return _buildProcessor; }
            set { _buildProcessor = value; }
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

        /// <summary>
        /// Gets or sets the revision increment action type.
        /// </summary>
        /// <value>The revision increment action type.</value>
        [DisplayName("Revision build type")]
        [Description("Revision update build action type")]
        [NotifyParentProperty(true)]
        [DefaultValue(BuildActionType.Both)]
        public BuildActionType RevisionIncrementActionType { get; set; }

        private BasePostProcessor _revisionProcessor = BuiltInBasePostProcessor.NoneProcessor.Instance;

        /// <summary>
        /// Gets or sets the revision post-processing style.
        /// </summary>
        /// <value>The revision post-processing style.</value>
        [Description("Revision post-processor style")]
        [NotifyParentProperty(true)]
        public BasePostProcessor RevisionProcessor
        {
            get { return _revisionProcessor; }
            set { _revisionProcessor = value; }
        }

    }
}

