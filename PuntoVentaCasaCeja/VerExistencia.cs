using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuntoVentaCasaCeja
{
    public partial class VerExistencia : Form
    {
        List<ProductoExistencia> productos;
        string titulo;
        public VerExistencia(List<ProductoExistencia> productos, string titulo)
        {
            InitializeComponent();
            this.productos = productos;
            this.titulo = titulo;
        }

        private void VerExistencia_Load(object sender, EventArgs e)
        {
            tabla.DataSource = productos;
            lblproducto.Text = this.titulo;
        }

        private void salir_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None)
            {
                switch (keyData)
                {
                    case Keys.Escape:
                        this.Close();
                        break;
                    case Keys.Down:
                        tabla.Focus();
                        SendKeys.Send("{DOWN}");
                        break;
                    case Keys.Up:
                        tabla.Focus();
                        SendKeys.Send("{UP}");
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
    }
}
