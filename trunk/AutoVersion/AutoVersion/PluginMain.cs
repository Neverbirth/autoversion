using PluginCore;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoVersion
{
    public class PluginMain : IPlugin
    {

        #region  Constants

        private const string pluginName = "AutoVersion";
        private const string pluginGuid = "99C549A9-34B0-4ca6-9705-1D2DAB423C4B";
        private const string pluginHelp = "www.flashdevelop.org/community/";
        private const string pluginDesc = "Adds versioning capabilities to FlashDevelop.";
        private const string pluginAuth = "Héctor";

        #endregion

        #region  Fields

        private BuildVersionIncrementor _incrementor;
        private ToolStripMenuItem _versionMenuItem;

        #endregion

        #region  Properties

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
            get { return null; }
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
            if (_incrementor != null)
            {
                _incrementor.Dispose();
                _incrementor = null;
            }
        }

        public void Initialize()
        {
            _incrementor = new BuildVersionIncrementor(this);
            _incrementor.InitializeIncrementors();

            PluginCore.Managers.EventManager.AddEventHandler(this, EventType.Command);
            CreateMenuItem();
        }

        #endregion

        #region  Plugin Events

        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            switch (((DataEvent)e).Action)
            {
                case "ProjectManager.BuildingProject":
                    _incrementor.OnBuilding(BuildAction.Building);
                    break;

                case "ProjectManager.Project":
                    _incrementor.OnProject();

                    _versionMenuItem.Enabled = (PluginBase.CurrentProject != null);
                    break;

                case "ProjectManager.TestingProject":
                    _incrementor.OnBuilding(BuildAction.Testing);
                    break;
            }
        }

        #endregion

        #region  Custom Methods

        private void CreateMenuItem()
        {
            ToolStripMenuItem toolsMenu = (ToolStripMenuItem)PluginBase.MainForm.FindMenuItem("ToolsMenu");

            if (toolsMenu != null)
            {
                _versionMenuItem = new ToolStripMenuItem("Version...", null, ToolStripItemAutoVersion_Click);
                _versionMenuItem.Enabled = false;

                toolsMenu.DropDownItems.Insert(0, _versionMenuItem);
                toolsMenu.DropDownItems.Insert(1, new ToolStripSeparator());
            }
        }

        #endregion

        #endregion

    }
}
