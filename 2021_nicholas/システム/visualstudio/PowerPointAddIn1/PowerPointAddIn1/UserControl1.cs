using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;

namespace PowerPointAddIn1
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(ThisAddIn.subtitledisplay == true)
            {
                ThisAddIn.subtitledisplay = false;
                Debug.WriteLine("表示：OFF");
            }
            else
            {
                ThisAddIn.subtitledisplay = true;
                Debug.WriteLine("表示：ON");
            }
        }
    }
}
