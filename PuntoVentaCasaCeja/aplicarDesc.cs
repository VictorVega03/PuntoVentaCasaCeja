using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Media.Core;
using Windows.UI.Xaml;

namespace PuntoVentaCasaCeja
{
    public partial class aplicarDesc : Form
    {
        private List<string> tipo = new List<string>();
        private readonly string[] rangeTipo = { "PORCENTAJE", "CANTIDAD" };
        double total;
        CurrentData data;
        bool esDescuento;
        double descuento = 0;

        public aplicarDesc(double total, CurrentData data)
        {
            InitializeComponent();
            this.data = data;
            this.total = total;
            txtDescuento.Text = "0.00";
            tipo.AddRange(rangeTipo);
            BoxTipo.DataSource = tipo;
        }

        private void Bsalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aceptar_Click(object sender, EventArgs e)
        {
            calcularDesc(esDescuento);
            this.Close();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                this.aceptar.PerformClick();
                return true;
            }
            if (keyData == Keys.Escape)
            {
                this.Bsalir.PerformClick();
                return true;
            }
            if (keyData == Keys.F1)
            {
                BoxTipo.Focus();
                BoxTipo.DroppedDown = true;
                return true;
            }
            if (keyData == Keys.F2)
            {
                txtDescuento.Focus();
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
        private void BoxTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (BoxTipo.SelectedIndex == 0)
            {
                label1.Text = "%";
            }
            else
            {
                label1.Text = "$";
            }
        }
        private void numericInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // Permitir teclas de control (como Backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
                return;
            }

            // Solo permite un punto decimal
            if ((e.KeyChar == '.') && (textBox.Text.IndexOf('.') > -1))
            {
                e.Handled = true;
                return;
            }

            // Permite solo dos decimales como máximo
            if (textBox.Text.Contains(".") && textBox.SelectionStart > textBox.Text.IndexOf('.'))
            {
                string[] partes = textBox.Text.Split('.');
                if (partes.Length > 1 && partes[1].Length >= 2 && !char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            }
        }

        private void calcularDesc(bool esDescuento)
        {
            double maxDescuento = total * 0.30;
            double.TryParse(txtDescuento.Text, out double valordescuento);
            descuento = (BoxTipo.SelectedIndex == 0) ? total * (valordescuento / 100) : valordescuento;
            if (descuento > maxDescuento)
            {
                MessageBox.Show("Se alcanzó el límite de descuento permitido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            data.esDescuento = true;
            data.descuento = descuento;
        }

        private void txtDescuento_Click(object sender, EventArgs e)
        {
            txtDescuento.SelectAll();
        }

        private void descuentoFijo(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            switch (button.Name)
            {
                case "Bdesc5":
                    BoxTipo.SelectedIndex = 0;
                    txtDescuento.Text = "5.00";
                    break;
                case "Bdesc10":
                    BoxTipo.SelectedIndex = 0;
                    txtDescuento.Text = "10.00";
                    break;
                case "Bdesc15":
                    BoxTipo.SelectedIndex = 0;
                    txtDescuento.Text = "15.00";
                    break;
                case "Bdesc20":
                    BoxTipo.SelectedIndex = 0;
                    txtDescuento.Text = "20.00";
                    break;
                default:
                    MessageBox.Show("Botón no reconocido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }
    }
}
