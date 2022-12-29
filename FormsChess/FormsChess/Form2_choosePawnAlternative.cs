using System;
using System.Windows.Forms;

namespace FormsChess
{
    public partial class Form2_choosePawnAlternative : Form
    {
        bool figurkaZvolena = false;
        public Form2_choosePawnAlternative()
        {
            InitializeComponent();
        }

        private void Form2_choosePawnAlternative_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = !figurkaZvolena;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            figurkaZvolena = true;
        }
    }
}
