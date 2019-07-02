using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form3_TestSelection : Form
    {
        public Form3_TestSelection()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e) //power
        {
            Form1_Power f1 = new Form1_Power();
            f1.Show();
            this.Hide();

        }

        private void button1_Click(object sender, EventArgs e) //frequency
        {
            Form4_Frequency f4 = new Form4_Frequency();
            f4.Show();
            this.Hide();
        }
    }
}
