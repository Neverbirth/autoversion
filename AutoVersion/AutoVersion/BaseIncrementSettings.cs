using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace AutoVersion
{
    /// <summary>
    /// Abtract class containing all common increment settings (global/solutionitem).
    /// </summary>
    /// <seealso cref="ProjectItemIncrementSettings"/>
    /// <seealso cref="GlobalIncrementSettings"/>
    [DefaultProperty("VersioningStyle")]
    internal abstract class BaseIncrementSettings
    {

        #region  Properties 

        /// <summary>
        /// Gets or sets a value indicating whether to auto update the file version.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to auto update the file version; otherwise, <c>false</c>.
        /// </value>
        [Category("Increment Settings"), Description("Auto update the version data."), DisplayName("Update version data"), DefaultValue(false)]
        public bool AutoUpdateVersionData { get; set; }

        /// <summary>
        /// Gets or sets a value indicating wether to only update version data, and not overwrite the whole file.
        /// </summary>
        /// <value><c>true</c> to enable Smart Update; otherwise, <c>false</c>.</value>
        [Browsable(true)]
        [Category("Increment Settings")]
        [DisplayName("Smart Update")]
        [Description("Set this option when the version file is prone to be modified and do not want to be overwritten each time. It may affect compilation time somewhat.")]
        [DefaultValue(false)]
        public bool SmartUpdate { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>The start date.</value>
        [Category("Increment Settings"), Description("The start date to use."), DisplayName("Start Date"), DefaultValue(typeof(DateTime), "1975/10/21")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using UTC.
        /// </summary>
        /// <value><c>true</c> if this instance is using UTC; otherwise, <c>false</c>.</value>
        [Category("Increment Settings"), Description("Indicates wheter to use Coordinated Universal Time (UTC) time stamps."), DisplayName("Use Coordinated Universal Time"), DefaultValue(false)]
        public bool IsUniversalTime { get; set; }

        private VersioningStyle _versioningStyle = new VersioningStyle();
        /// <summary>
        /// Gets or sets the increment settings.
        /// </summary>
        /// <value>The increment settings.</value>
        [Browsable(true)]
        [Category("Increment Settings")]
        [DisplayName("Versioning Style")]
        [Description("The version increment style settings.")]
        public VersioningStyle VersioningStyle
        {
            get { return this._versioningStyle; }
            set { this._versioningStyle = value; }
        }

        private bool _incrementBeforeBuild = true;
        /// <summary>
        /// Gets or set if the increment should happen before or after the current build.
        /// </summary>
        /// <remarks>WorkItem 3589 from PeteBSC</remarks>
        /// <value>The new value for this property.</value>
        [Category("Condition")]
        [Description("If the increment should be executed before the build. Incrementing after build is complete doesn't work with Flash IDE projects.")]
        [DisplayName("Increment Before Build")]
        [DefaultValue(true)]
        public bool IncrementBeforeBuild
        {
            get { return _incrementBeforeBuild; }
            set { _incrementBeforeBuild = value; }
        }

        /// <summary>
        /// Gets or sets the build action
        /// </summary>
        /// <value>The build action on which the auto update should occur.</value>
        [Category("Condition"), DefaultValue(BuildActionType.Both), DisplayName("Build Action"), Description("Set this to the desired build action when the auto update should occur.")]
        public BuildActionType BuildAction { get; set; }

        #endregion

        #region  Methods

        /// <summary>
        /// Copies settings from another instance.
        /// </summary>
        /// <param name="source">The source to copy the settings from.</param>
        public virtual void CopyFrom(BaseIncrementSettings source)
        {
            VersioningStyle = new VersioningStyle(source.VersioningStyle);
            AutoUpdateVersionData = source.AutoUpdateVersionData;
            BuildAction = source.BuildAction;
            StartDate = source.StartDate;
            SmartUpdate = source.SmartUpdate;
            IsUniversalTime = source.IsUniversalTime;
            IncrementBeforeBuild = source.IncrementBeforeBuild;
        }

        /// <summary>
        /// Loads the settings into this instance.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Saves the settings of this instance.
        /// </summary>
        public abstract void Save();

        private bool ShouldSerializeVersioningStyle()
        {
            return _versioningStyle.ToString() != "None.None.None.None";
        }

        #endregion

    }
}
