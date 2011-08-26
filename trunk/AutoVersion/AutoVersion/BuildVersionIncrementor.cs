using AutoVersion.Extensions;
using AutoVersion.Incrementors;
using AutoVersion.Incrementors.PostProcessors;

using PluginCore;

using System;
using System.IO;
using System.Reflection;

namespace AutoVersion
{
    public enum BuildAction : byte
    {
        Building,
        Testing
    }

    public enum BuildConfiguration : byte
    {
        Any,
        Build,
        Debug
    }

    public enum BuildState : byte
    {
        BuildInProgress,
        BuildDone
    }

    internal class BuildVersionIncrementor : IDisposable
    {
        private BuildAction _currentBuildAction;
        private BuildState _currentBuildState;
        private ProjectItem _projectItem;
        private Version _currentVersion;
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

        private PostProcessorCollection _postProcessors = new PostProcessorCollection();
        /// <summary>
        /// Gets the post processors.
        /// </summary>
        /// <value>The post processors.</value>
        public PostProcessorCollection PostProcessors
        {
            get { return _postProcessors; }
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
        public BuildVersionIncrementor()
        {
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

        /// <summary>
        /// Initializes the processors.
        /// </summary>
        public void InitializePostProcessors()
        {
            try
            {
                _postProcessors.AddFrom(Assembly.GetExecutingAssembly());

                string[] files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "*.Incrementor.dll");

                foreach (string file in files)
                {
                    Assembly asm = Assembly.LoadFrom(file);

                    _postProcessors.AddFrom(asm);
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
                _currentVersion = _projectItem.Version;

                if (GlobalIncrementSettings.GetInstance().Apply == GlobalIncrementSettings.ApplyGlobalSettings.Always || _projectItem.IncrementSettings.UseGlobalSettings)
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
            if (_currentBuildAction.EqualsType(settings.BuildAction) && settings.ConfigurationType.IsEqualToTraceValue(PluginBase.CurrentProject.TraceEnabled))
            {
                if (settings.IncrementBeforeBuild == (_currentBuildState == BuildState.BuildInProgress))
                {

                    _projectItem.Version = settings.VersioningStyle.Increment(
                        _projectItem.Version,
                        settings.IsUniversalTime
                            ? _buildStartDate.ToUniversalTime()
                            : _buildStartDate,
                        settings.StartDate,
                        PluginBase.CurrentProject.ProjectPath,
                        _currentBuildAction, _currentBuildState,
                        PluginBase.CurrentProject.TraceEnabled);

                    _projectItem.SaveVersion();

                    if (settings.UpdateAirVersion && _projectItem.IsAirProjector())
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

        public void OnBuildFailed()
        {
            if (_projectItem.IncrementSettings.IncrementBeforeBuild && _projectItem.IncrementSettings.RevertOnError)
            {
                _projectItem.Version = _currentVersion;

                _projectItem.SaveVersion();

                if (_projectItem.IncrementSettings.UpdateAirVersion && _projectItem.IsAirProjector())
                {
                    _projectItem.UpdateAirVersion();
                }
            }
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
            _currentVersion = null;
        }

        #region  IDisposable

        public void Dispose()
        {
            _projectItem = null;
            _currentVersion = null;
        }

        #endregion
    }
}
