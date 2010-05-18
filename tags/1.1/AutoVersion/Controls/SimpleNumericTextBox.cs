using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Controls
{
    internal class SimpleNumericTextBox : TextBox
    {

        private const int ES_NUMBER = 0x2000;
        private const int WM_PASTE = 0x302;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams @params = base.CreateParams;
                @params.Style |= ES_NUMBER;
                return @params;
            }
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == WM_PASTE)
            {
                string data = Clipboard.GetDataObject().GetData(DataFormats.Text) as string;
                if (!Regex.IsMatch(data, "^\\d+$"))
                {
                    System.Media.SystemSounds.Beep.Play();
                    return;
                }
            }
            base.WndProc(ref m);
        }

    }
}
