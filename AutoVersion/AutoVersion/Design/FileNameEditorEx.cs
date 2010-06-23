using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.Design;

namespace AutoVersion.Design
{
    class FileNameEditorEx: FileNameEditor
    {

        protected override void InitializeDialog(System.Windows.Forms.OpenFileDialog openFileDialog)
        {
            base.InitializeDialog(openFileDialog);

            openFileDialog.CheckFileExists = false;
        }

    }
}
