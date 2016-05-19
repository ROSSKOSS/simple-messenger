using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Messanger1
{
    public partial class IPRequester : Form
    {
        public static string targetip;
        public static Int32 targetport;
        public IPRequester()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                targetip = textBox1.Text;
                targetport = Convert.ToInt32(textBox2.Text);
                this.Close();
            }
            catch
            {
                MessageBox.Show("Wrong IP or Port");
            }
            
        }
    }
}
