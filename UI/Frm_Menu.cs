using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI
{
    public partial class Frm_Menu : Form
    {
        public Frm_Menu()
        {
            InitializeComponent();
        }

        private void mLivre_Click(object sender, EventArgs e)
        {
            Frm_Livre frm = new Frm_Livre();
            frm.MdiParent = this;
            frm.Show();
        }
    }
}
