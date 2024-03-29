﻿using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace AutoVersion.Incrementors
{

    /// <summary>
    /// The base increment class. Inherit from this class to implement your own custom incrementor.
    /// </summary>
    [TypeConverter(typeof(BaseIncrementorConverter))]
    public abstract class BaseIncrementor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseIncrementor"/> class.
        /// </summary>
        public BaseIncrementor()
        {
            if (string.IsNullOrEmpty(Name) || Name.Contains("."))
            {
                throw (new FormatException("The Name property of the class " + this.GetType().FullName + " is invalid."));
            }
        }

        /// <summary>
        /// Gets the name of this incrementor.
        /// </summary>
        /// <value>The name.</value>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Gets the description of this incrementor.
        /// </summary>
        /// <value>The description.</value>
        public abstract string Description
        {
            get;
        }

        /// <summary>
        /// Increments the specified value.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <param name="buildStart">The build start date/time.</param>
        /// <param name="projectStart">The project start date/time.</param>
        /// <returns>The incremented value.</returns>
        public abstract int Increment(int value, DateTime buildStart, DateTime projectStart, string projectFilePath);
    }

}
