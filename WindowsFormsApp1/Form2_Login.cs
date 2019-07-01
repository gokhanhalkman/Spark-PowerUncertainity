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
    public partial class Form2_Login : Form
    {
        string warning = "";
        new Dictionary<string, string> userDatabase = new Dictionary<string, string>();

        public Form2_Login()
        {
            InitializeComponent();
            UpdateUserDataBase();
        }

        void UpdateUserDataBase()
        {
            //userDatabase.Add("username", "password");
            userDatabase.Add("aydin", "123456");
            userDatabase.Add("mahdi", "456789");
            userDatabase.Add("atakan", "789123");
            userDatabase.Add("gokhan", "123456");
        }
        public static Form3_TestSelection f3;
        private void button1_Click(object sender, EventArgs e)
        {
            if (userDatabase.ContainsKey(textBox1.Text))
            {
                if (userDatabase[textBox1.Text].Equals(textBox2.Text))
                {
                    f3 = new Form3_TestSelection();
                    f3.Show();
                    this.Hide();
                }
                else
                    warning = "Wrong password";                
            }
            else
                warning = "No matching username";

            label3.Text = warning;
        }
    }
}
