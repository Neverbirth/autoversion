using AutoVersion.Incrementors;

using PluginCore;

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml.Linq;

namespace AutoVersion
{
    internal enum BuildAction : byte
    {
        Building,
        Testing
    }

    internal enum BuildState : byte
    {
        BuildInProgress,
        BuildDone
    }

    internal class BuildVersionIncrementor : IDisposable
    {
        private BuildAction _currentBuildAction;
        private BuildState _currentBuildState;
        private PluginMain _pluginMain;
        private ProjectItem _projectItem;
        private DateTime _buildStartDate = DateTime.MinValue;

        private IncrementorCollection _incrementors = new IncrementorCollection();
        /// <summary>
        /// Gets the incrementors.
        /// </summary>
        /// <value>The incrementors.</value>
        public IncrementorCollection Incrementors
        {
            get { return _incrementors; }
        }

        private static BuildVersionIncrementor _instance;
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static BuildVersionIncrementor Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildVersionIncrementor"/> class.
        /// </summary>
        /// <param name="pluginMain">The base instance.</param>
        public BuildVersionIncrementor(PluginMain pluginMain)
        {
            _pluginMain = pluginMain;
            _instance = this;
        }

        /// <summary>
        /// Initializes the incrementors.
        /// </summary>
        public void InitializeIncrementors()
        {
            try
            {
                _incrementors.AddFrom(Assembly.GetExecutingAssembly());

                string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.Incrementor.dll");

                foreach (string file in files)
                {
                    Assembly asm = Assembly.LoadFrom(file);

                    _incrementors.AddFrom(asm);
                }
            }
            catch (Exception ex)
            {
                throw;
                //                Logger.Write("Exception occured while initializing incrementors.\n" + ex.ToString(), LogLevel.Error);
            }
        }

        private void ExecuteIncrement()
        {
            try
            {
                _projectItem = new ProjectItem();

                if (_projectItem.IncrementSettings.UseGlobalSettings)
                    _projectItem.ApplyGlobalSettings();

                UpdateProject();
            }
            catch (Exception ex)
            {
                throw;
                //                Logger.Write("Error occured while executing build version increment.\n" + ex.ToString(), LogLevel.Error);
            }
        }

        /// <summary>
        /// Updates build version for the given project
        /// </summary>
        private void UpdateProject()
        {
            ProjectItemIncrementSettings settings = _projectItem.IncrementSettings;
            if (settings.BuildAction == BuildActionType.Both ||
                  (settings.BuildAction == BuildActionType.Build && _currentBuildAction == BuildAction.Building) ||
                  (settings.BuildAction == BuildActionType.Testing && _currentBuildAction == BuildAction.Testing))
            {
                if (_projectItem.IncrementSettings.IncrementBeforeBuild == (_currentBuildState == BuildState.BuildInProgress))
                {

                    _projectItem.Version = _projectItem.IncrementSettings.VersioningStyle.Increment(
                        _projectItem.Version,
                        _projectItem.IncrementSettings.IsUniversalTime
                            ? _buildStartDate.ToUniversalTime()
                            : _buildStartDate,
                        _projectItem.IncrementSettings.StartDate,
                        PluginBase.CurrentProject.ProjectPath);

                    _projectItem.SaveVersion();

                    if (_projectItem.IncrementSettings.UpdateAirVersion && _projectItem.IsAirProjector())
                    {
                        _projectItem.UpdateAirVersion();
                    }
                }
            }
        }

        public void OnBuildComplete()
        {
            _currentBuildState = BuildState.BuildDone;
            ExecuteIncrement();
        }

        public void OnBuilding(BuildAction action)
        {
            _currentBuildAction = action;
            _currentBuildState = BuildState.BuildInProgress;
            _buildStartDate = DateTime.Now;

            ExecuteIncrement();
        }

        public void OnProject()
        {
            _projectItem = null;
        }

        #region  IDisposable

        public void Dispose()
        {
            _projectItem = null;
            _pluginMain = null;
        }

        #endregion
    }
}
