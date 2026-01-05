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
    public partial class GastosIngresos : Form
    {
        int tipo;
        int idcorte;
        LocaldataManager localDM;
        string spanlabel = "";
        public GastosIngresos(int tipo, int Idcorte, LocaldataManager localdata)
        {
            InitializeComponent();
            this.tipo = tipo;
            this.idcorte = Idcorte;
            this.localDM = localdata;
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }
        private void numericInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
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
                    case Keys.Enter:
                        if (accept.Focused || cancel.Focused)
                            return base.ProcessDialogKey(keyData);
                        SendKeys.Send("{TAB}");
                        break;
                    case Keys.F1:
                        txtconcepto.Focus();
                        break;
                    case Keys.F5:
                        accept.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void accept_Click(object sender, EventArgs e)
        {
            bool res = true;
            if (string.IsNullOrWhiteSpace(txtconcepto.Text) || string.IsNullOrWhiteSpace(txtmonto.Text) || txtmonto.Text.Equals("."))
            {
                MessageBox.Show("Favor de completar todos los datos", "Advertencia");
            }
            else
            {
                if (double.TryParse(txtmonto.Text, out double m))
                {
                    string c = txtconcepto.Text;
                    if (tipo == 1)
                    {
                        localDM.registrarIngreso(c, m, idcorte);
                        MessageBox.Show(spanlabel + " registrado", "Éxito");
                        this.Close();
                    }
                    else if (tipo == 2)
                    {
                        double total = localDM.getEfectivoCaja(idcorte);
                        if (m > total)
                        {
                            MessageBox.Show("No hay suficiente efectivo", "Advertencia");
                            res = false;
                        }
                        else
                        {
                            localDM.registrarGasto(c, m, idcorte);
                            MessageBox.Show(spanlabel + " registrado", "Éxito");
                            this.Close();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Monto no válido", "Error");
                }
            }
        }

        private void GastosIngresos_Load(object sender, EventArgs e)
        {
            if (tipo == 1)
                spanlabel = "Ingreso";
            else if (tipo == 2)
                spanlabel = "Retiro";
            groupBox1.Text = spanlabel.ToUpper() + " DE EFECTIVO";
        }
    }
}
