using System;
using System.Collections.Generic;
using System.Text;
using Qreed.Reflection;

namespace AutoVersion.Incrementors.PostProcessors
{

    internal enum PostProcessStyle : byte
    {
        None = 0,
        ReleaseReset,
        PreviousPartIncrements
    }

    /// <summary>
    /// Internal base class for the default post processors.
    /// </summary>
    internal abstract class BuiltInBasePostProcessor : BasePostProcessor
    {
        /// <summary>
        /// Gets the name of this post processor.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                return PostProcessStyle.ToString();
            }
        }

        /// <summary>
        /// Gets the description of this post processor.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                return EnumHelper.GetDescription(PostProcessStyle);
            }
        }

        /// <summary>
        /// Gets the post processor style.
        /// </summary>
        /// <value>The increment style.</value>
        private PostProcessStyle PostProcessStyle
        {
            get
            {
                string name = this.GetType().Name;
                name = name.Substring(0, name.Length - "Processor".Length);

                return (PostProcessStyle)Enum.Parse(typeof(PostProcessStyle), name);
            }
        }

        /// <summary>
        /// Processes the specified value.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <param name="buildStart">The build start date/time.</param>
        /// <param name="projectStart">The project start date/time.</param>
        /// <returns>The incremented value.</returns>
        public override Version ProcessVersionValue(Version value, VersionPart versionPart, DateTime buildStart,
            DateTime projectStart, string projectFilePath, BuildAction buildAction, BuildState buildState,
            bool traceEnabled)
        {
            switch (PostProcessStyle)
            {
                case PostProcessStyle.None:
                    return value;

                case PostProcessStyle.ReleaseReset:
                    if (buildAction == BuildAction.Building)
                    {
                        return SetVersionPartValue(value, versionPart, 0);
                    }

                    break;

                case PostProcessStyle.PreviousPartIncrements:
                    return DecrementAndIncrement(value, versionPart);

                default:
                    throw (new ApplicationException("Unknown processor style: " + PostProcessStyle.ToString()));
            }

            return value;
        }

        private Version DecrementAndIncrement(Version version, VersionPart versionPart)
        {
            switch (versionPart)
            {
                case VersionPart.Major:
                    if (version.Minor >= 100)
                        return new Version(version.Major + 1, 0, version.Build, version.Revision);

                    break;

                case VersionPart.Minor:
                    if (version.Build >= 100)
                        return new Version(version.Major, version.Minor + 1, 0, version.Revision);

                    break;

                case VersionPart.Build:
                    if (version.Minor >= 100)
                        return new Version(version.Major, version.Minor, version.Build + 1, 0);

                    break;
            }

            return version;
        }

        // Could use reflection for these methods
        private Version SetVersionPartValue(Version version, VersionPart versionPart, int value)
        {
            switch (versionPart)
            {
                case VersionPart.Major:
                    return new Version(value, version.Minor, version.Build, version.Revision);

                case VersionPart.Minor:
                    return new Version(version.Major, value, version.Build, version.Revision);

                case VersionPart.Build:
                    return new Version(version.Major, version.Minor, value, version.Revision);

                case VersionPart.Revision:
                    return new Version(version.Major, version.Minor, version.Build, value);

                default:
                    return version;
            }
        }

        internal class NoneProcessor : BuiltInBasePostProcessor
        {
            /// <summary>
            /// Use this to reference a null processor.
            /// </summary>
            public static readonly NoneProcessor Instance = new NoneProcessor();
        }

        class ReleaseResetProcessor : BuiltInBasePostProcessor { }
        class PreviousPartIncrementsProcessor : BuiltInBasePostProcessor { }
    }
}

