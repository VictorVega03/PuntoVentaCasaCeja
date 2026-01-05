using Newtonsoft.Json;
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
    public partial class VerCorte : Form
    {
        Dictionary<string, string> data;
        int idsucursal, idusuario, idcorte, idcaja;
        LocaldataManager localDM;
        public VerCorte(Dictionary<string, string> Data, int idsucursal, int idusuario, int idcorte, int idcaja, LocaldataManager localdata)
        {
            InitializeComponent();
            this.data = Data;
            this.idsucursal = idsucursal;
            this.idusuario = idusuario;
            this.idcorte = idcorte;
            this.idcaja = idcaja;
            localDM = localdata;
        }

        private void accept_Click(object sender, EventArgs e)
        {
            float sobrante = 0;
            if (!(txtsobrante.Text.Equals("") || txtsobrante.Text.Equals(".")))
            {
                sobrante = float.Parse(txtsobrante.Text);
            }
            localDM.completarCorte(idcorte, idsucursal, idusuario, txtfechcorte.Text, sobrante, data);
            this.DialogResult = DialogResult.Yes; 
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void VerCorte_Load(object sender, EventArgs e)
        {
            groupBox1.Text = "CORTE Z DE CAJA " + idcaja;
            DateTime localDate = DateTime.Now;
            txtfolio.Text = data["folio_corte"];
            txtapertura.Text = data["fondo_apertura"];
            txtefectivo.Text = data["total_efectivo"];
            txtdebito.Text = data["total_tarjetas_debito"];
            txtcredito.Text = data["total_tarjetas_credito"];
            txtcheques.Text = data["total_cheques"];
            txtefeapa.Text = data["efectivo_apartados"];
            txtefecred.Text = data["efectivo_creditos"];
            txttransferencias.Text = data["total_transferencias"];
            txtfechapertura.Text = data["fecha_apertura_caja"];
            txttotapa.Text = data["total_apartados"];
            txttotcred.Text = data["total_creditos"];
            txtfechcorte.Text = localDate.ToString("yyyy-MM-dd");
            var ingresos = JsonConvert.DeserializeObject<Dictionary<string, double>>(data["ingresos"]);
            foreach (var x in ingresos)
            {
                listaingresos.Items.Add( x.Key + ": "+ x.Value);
            }
            var gastos = JsonConvert.DeserializeObject<Dictionary<string, double>>(data["gastos"]);
            foreach (var x in gastos)
            {
                listagastos.Items.Add(x.Key + ": " + x.Value);
            }        
        }

        private void txtfechcorte_TextChanged(object sender, EventArgs e)
        {

        }

        private void txttotcred_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtingresos_TextChanged(object sender, EventArgs e)
        {

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
                        cancel.PerformClick();
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
    }
}
