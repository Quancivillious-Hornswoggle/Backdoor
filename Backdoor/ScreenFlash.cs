using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backdoor
{
    public partial class ScreenFlash : Form
    {
        public ScreenFlash()
        {
            InitializeComponent();
        }

        private void ScreenFlash_Load(object sender, EventArgs e)
        {
            FlashFormInAndOut();
        }

        private void FlashFormInAndOut()
        {
            

            int flashDuration = 10000;
            int flashInterval = 50;
            int flashIterations = flashDuration / (flashInterval * 2);

            for (int i = 0; i < flashIterations; i++)
            {
                this.TopMost = true;
                this.Visible = !this.Visible;
                System.Threading.Thread.Sleep(flashInterval);
            }

            this.Close();
        }
    }
}
