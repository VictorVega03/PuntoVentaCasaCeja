using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.UI.Xaml.Controls;

namespace PuntoVentaCasaCeja
{
    public partial class CambioForm : Form
    {
        public CambioForm(double cambio)
        {
            InitializeComponent();
            lblCambio.Text = "MXN: " + cambio.ToString("C2");
        }

        private void aceptar_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {            
            if (keyData == Keys.Enter)
            {
                aceptar_Click(this, new EventArgs());
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
    }
}
