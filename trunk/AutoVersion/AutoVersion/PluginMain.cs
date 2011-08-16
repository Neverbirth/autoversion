using AutoVersion.Resources;

using PluginCore;
using PluginCore.Localization;

using System;
using System.Windows.Forms;

namespace AutoVersion
{
    public class PluginMain : IPlugin
    {

        #region  Constants

        private const string pluginName = "AutoVersion";
        private const string pluginGuid = "99C549A9-34B0-4ca6-9705-1D2DAB423C4B";
        private const string pluginHelp = "www.flashdevelop.org/community/";
        private const string pluginAuth = "Héctor";

        #endregion

        #region  Fields

        private string pluginDesc = "Adds versioning capabilities to FlashDevelop.";

        private BuildVersionIncrementor _incrementor;
        private ToolStripMenuItem _versionMenuItem;

        #endregion
        
        #region  Properties

        public int Api
        {
            get { return 1; }
        }

        public string Author
        {
            get { return pluginName; }
        }

        public string Description
        {
            get { return pluginDesc; }
        }

        public string Guid
        {
            get { return pluginGuid; }
        }

        public string Help
        {
            get { return pluginHelp; }
        }

        public string Name
        {
            get { return pluginName; }
        }

        public object Settings
        {
            get { return GlobalIncrementSettings.GetInstance(); }
        }

        #endregion

        #region Constructor

        public PluginMain()
        {
            _incrementor = new BuildVersionIncrementor();
        }

        #endregion

        #region  Event Handlers

        private void ToolStripItemAutoVersion_Click(Object sender, EventArgs e)
        {
            using (VersionDialog pluginDialog = new VersionDialog())
            {
                pluginDialog.StartPosition = FormStartPosition.CenterParent;
                pluginDialog.ShowDialog();
            }
        }

        #endregion

        #region  Methods

        #region  Initialize/Dispose

        public void Dispose()
        {
            GlobalIncrementSettings.GetInstance().Save();

            if (_incrementor != null)
            {
                _incrementor.Dispose();
                _incrementor = null;
            }
        }

        public void Initialize()
        {
            _incrementor.InitializeIncrementors();
            _incrementor.InitializePostProcessors();
            
            PluginCore.Managers.EventManager.AddEventHandler(this, EventType.Command | EventType.UIStarted);

            InitLocalization();
        }

        #endregion

        #region  Plugin Events

        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            if (e.Type == EventType.UIStarted)
            {
                CreateMenuItem();
            }
            else
            {
                switch (((DataEvent)e).Action)
                {
                    case ProjectManager.ProjectManagerEvents.BuildComplete:
                        _incrementor.OnBuildComplete();
                        break;

                    case ProjectManager.ProjectManagerEvents.BuildFailed:
                        _incrementor.OnBuildFailed();
                        break;

                    case ProjectManager.ProjectManagerEvents.BuildProject:
                        _incrementor.OnBuilding(BuildAction.Building);
                        break;

                    case ProjectManager.ProjectManagerEvents.Project:
                        _incrementor.OnProject();

                        if (_versionMenuItem != null) _versionMenuItem.Enabled = (PluginBase.CurrentProject != null);
                        break;

                    case ProjectManager.ProjectManagerEvents.TestProject:
                        _incrementor.OnBuilding(BuildAction.Testing);
                        break;
                }
            }

        }

        #endregion

        #region  Custom Methods

        private void CreateMenuItem()
        {
            ToolStripMenuItem projectMenu = null;
            ToolStripItemCollection items = PluginBase.MainForm.MenuStrip.Items;

            for (int i = 0, itemCount = items.Count; i < itemCount; i++)
            {
               if (items[i] is ProjectManager.Controls.ProjectMenu)
               {
                   projectMenu = (ToolStripMenuItem)items[i];
                   break;
               }
            }

            if (projectMenu != null)
            {
                _versionMenuItem = new ToolStripMenuItem(LocaleHelper.GetString("Label.ProjectMenuItem"), null, ToolStripItemAutoVersion_Click);
                _versionMenuItem.Enabled = false;

                projectMenu.DropDownItems.Add(_versionMenuItem);
            }
        }

        /// <summary>
        /// Initializes the localization of the plugin
        /// </summary>
        public void InitLocalization()
        {
            LocaleVersion locale = PluginBase.MainForm.Settings.LocaleVersion;
            switch (locale)
            {
                /*
                case LocaleVersion.fi_FI : 
                    // We have Finnish available... or not. :)
                    LocaleHelper.Initialize(LocaleVersion.fi_FI);
                    break;
                */
                default:
                    // Plugins should default to English...
                    LocaleHelper.Initialize(LocaleVersion.en_US);
                    break;
            }
            this.pluginDesc = LocaleHelper.GetString("Info.Description");
        }

        #endregion

        #endregion

    }
}
