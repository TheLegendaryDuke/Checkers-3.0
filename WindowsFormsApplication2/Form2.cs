using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form2 : Form
    {
        Form1 f;
        Button easyB, medium, hardB;
        Label promptL;
        public Form2()
        {
            InitializeComponent();
            hardB = new Button();
            promptL = new Label();
            easyB = new Button();
            medium = new Button();
            promptL.Text = "Please select a difficulty";
            promptL.AutoSize = true;
            promptL.Location = new Point(71, 9);
            medium.Location = new Point(94, 90);
            medium.Text = "Medium";
            medium.Click += medium_Click;
            easyB.Location = new Point(94, 55);
            easyB.Text = "Easy";
            easyB.Click += easy_Click;
            hardB.Location = new Point(94, 125);
            hardB.Text = "Hell";
            hardB.Click += hard_Click;
            this.Controls.Add(easyB);
            this.Controls.Add(promptL);
            this.Controls.Add(medium);
            this.Controls.Add(hardB);
            easyB.Hide();
            medium.Hide();
            hardB.Hide();
            promptL.Hide();
            hardB.Enabled = false;
        }

        private void easy_Click(object sender, EventArgs e)
        {
            f = new Form1(true, 1, this);
            f.Show();
            this.Hide();
            easyB.Hide();
            medium.Hide();
            hardB.Hide();
            promptL.Hide();
            button1.Show();
            button2.Show();
            label1.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            f = new Form1(false, 0, this);
            f.Show();
            this.Hide();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            label1.Hide();
            button1.Hide();
            button2.Hide();
            easyB.Show();
            medium.Show();
            hardB.Show();
            promptL.Show();
        }

        void medium_Click(object sender, EventArgs e)
        {
            f = new Form1(true, 2, this);
            f.Show();
            this.Hide();
            easyB.Hide();
            medium.Hide();
            hardB.Hide();
            button1.Show();
            button2.Show();
            label1.Show();
            promptL.Hide();
        }
        void hard_Click(object sender, EventArgs e)
        {
            f = new Form1(true, 3, this);
            f.Show();
            this.Hide();
            easyB.Hide();
            medium.Hide();
            hardB.Hide();
            button1.Show();
            button2.Show();
            label1.Show();
            promptL.Hide();
        }
    }


}

