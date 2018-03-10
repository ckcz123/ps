using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ps
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox1.Left = (ClientSize.Width - comboBox1.Width) / 2;
            button1.Left = (ClientSize.Width - button1.Width) / 2;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            int[] selections = {32, 48};
            new Form1(selections[comboBox1.SelectedIndex]).Show();
        }
    }
}
