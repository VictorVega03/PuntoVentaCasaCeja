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
    public partial class CambiarCantidad : Form
    {
        string cantidad;
        int index;
        Action<int, int> modCant;
        public CambiarCantidad(string cantidadActual, int index, Action<int, int>ModCant)
        {
            InitializeComponent();
            this.cantidad = cantidadActual;
            this.index = index;
            this.modCant = ModCant;
        }

        private void cancelar_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
        private void integerInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void CambiarCantidad_Load(object sender, EventArgs e)
        {
            txtcantidad.Text = cantidad;
        }

        private void aceptar_Click(object sender, EventArgs e)
        {
            if (txtcantidad.Text.Equals(""))
            {
                MessageBox.Show("Favor de ingresar la cantidad", "Advertencia");
            }
            else
            {
                if (txtcantidad.Text.Length>=9)
                {
                    MessageBox.Show("Cantidad demasiado grande, por favor no juegue con el sistema");
                }
                int cant = int.Parse(txtcantidad.Text);
                if(cant > 0)
                {
                    modCant(index, cant);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("La cantidad ingresada no es válida", "Advertencia");
                }
            }
        }

        private void txtcantidad_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyData == Keys.Enter)
            {
                aceptar.PerformClick();
            }
        }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None)
            {
                switch (keyData)
                {
                    case Keys.Escape:
                        cancelar.PerformClick();
                        break;
                    case Keys.Enter:
                        aceptar.PerformClick();
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
