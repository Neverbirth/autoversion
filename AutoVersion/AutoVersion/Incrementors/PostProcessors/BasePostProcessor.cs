using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace AutoVersion.Incrementors.PostProcessors
{
    /// <summary>
    /// The part of the version number being processed
    /// </summary>
    public enum VersionPart : byte
    {
        Major = 0,
        Minor,
        Build,
        Revision
    }


    /// <summary>
    /// The base post processor class. Inherit from this class to implement your own custom incrementor.
    /// </summary>
    [TypeConverter(typeof(BasePostProcessorConverter))]
    public abstract class BasePostProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasePostProcessor"/> class.
        /// </summary>
        public BasePostProcessor()
        {
            if (string.IsNullOrEmpty(Name) || Name.Contains("."))
            {
                throw (new FormatException("The Name property of the class " + this.GetType().FullName + " is invalid."));
            }
        }

        /// <summary>
        /// Gets the name of this processor.
        /// </summary>
        /// <value>The name.</value>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Gets the description of this processor.
        /// </summary>
        /// <value>The description.</value>
        public abstract string Description
        {
            get;
        }

        /// <summary>
        /// Processes the specified value.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <param name="buildStart">The build start date/time.</param>
        /// <param name="projectStart">The project start date/time.</param>
        /// <returns>The incremented value.</returns>
        public abstract Version ProcessVersionValue(Version value, VersionPart versionPart, DateTime buildStart,
            DateTime projectStart, string projectFilePath, BuildAction buildAction, BuildState buildState,
            bool traceEnabled);
    }
}
