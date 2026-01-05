using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PuntoVentaCasaCeja
{
    public partial class HistorialCortes : Form
    {
        Action<Usuario> setUser;
        Usuario usuario;
        WebDataManager webDM;
        LocaldataManager localDM;
        CurrentData data;

        public HistorialCortes(CurrentData data)
        {
            InitializeComponent();
            this.webDM = data.webDM;
            this.data = data;
            this.localDM = webDM.localDM;
            this.tablaCortesZ.DataSource = localDM.getCortesBySucursal(data.idSucursal);
            this.tablaCortesZ.CellDoubleClick += new DataGridViewCellEventHandler(this.tablaCortesZ_CellDoubleClick);
            tablaCortesZ.ColumnHeadersDefaultCellStyle.Font = new Font(tablaCortesZ.Font.FontFamily, 16);
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None)
            {
                switch (keyData)
                {
                    case Keys.Enter:
                        BSelCorte.PerformClick();
                        break;
                    case Keys.Escape:
                        this.Close();
                        break;
                    case Keys.F6:
                        BelimHistorial.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BSelCorte_Click(object sender, EventArgs e)
        {
            if (tablaCortesZ.Rows.Count == 0)
            {
                MessageBox.Show("La tabla de cortes está vacía.", "Advertencia");
                return;
            }
            int selectedRowIndex = tablaCortesZ.CurrentCell.RowIndex;
            if (selectedRowIndex < 0)
            {
                MessageBox.Show("Seleccione un corte de la lista.", "Advertencia");
                return;
            }

            int idCorte = Convert.ToInt32(tablaCortesZ.Rows[selectedRowIndex].Cells["id"].Value);
            Dictionary<string, string> corteData = localDM.getCorte2(idCorte);
            if (corteData != null)
            {
                VerCorteHistorial verCorte = new VerCorteHistorial(corteData, data, idCorte);
                verCorte.ShowDialog();
            }
            else
            {
                MessageBox.Show("No se pudo obtener la información del corte.", "Error");
            }
        }

        private void tablaCortesZ_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            BSelCorte_Click(sender, e);
        }

        /*

        void imprimirCorte(Dictionary<string, string> corte)
        {
            double efedir = double.Parse(corte["total_efectivo"]) - double.Parse(corte["efectivo_apartados"]) - double.Parse(corte["efectivo_creditos"]);
            double tgastos = 0;
            double tingresos = 0;

            Dictionary<string, double> gastos = JsonConvert.DeserializeObject<Dictionary<string, double>>(corte["gastos"]);
            foreach (var x in gastos)
            {
                tgastos += x.Value;
            }
            Dictionary<string, double> ingresos = JsonConvert.DeserializeObject<Dictionary<string, double>>(corte["ingresos"]);
            foreach (var x in ingresos)
            {
                tingresos += x.Value;
            }

            double totalCZ = double.Parse(corte["total_efectivo"]) + double.Parse(corte["total_tarjetas_debito"]) + double.Parse(corte["total_tarjetas_credito"]) + double.Parse(corte["total_cheques"]) + double.Parse(corte["total_transferencias"]) + double.Parse(corte["sobrante"]);
            string nc = localDM.getNombreUsuario(int.Parse(corte["usuario_id"]));
            CreaTicket Ticket1 = new CreaTicket();
            Ticket1.impresora = localDM.impresora;
            Ticket1.TextoCentro("CASA CEJA");
            Ticket1.TextoCentro(" ");
            Ticket1.TextoCentro("SUCURSAL: " + localDM.getSucursalname(int.Parse(corte["sucursal_id"])).ToUpper());
            Ticket1.TextoCentro(" ");

            Ticket1.TextoCentro("CZ FOLIO:  " + corte["folio_corte"]);
            Ticket1.TextoCentro(" ");
            Ticket1.LineasGuion(); // imprime una linea de guiones
            Ticket1.TextoExtremos("FECHA DE APERTURA:", corte["fecha_apertura_caja"]);
            Ticket1.TextoExtremos("FECHA DE CORTE:", corte["fecha_corte_caja"]);
            Ticket1.LineasGuion(); // imprime una linea de guiones
            Ticket1.TextoCentro(" ");

            Ticket1.TextoExtremos("FONDO DE APERTURA:", corte["fondo_apertura"]);
            Ticket1.TextoCentro(" ");

            Ticket1.LineasGuion();
            Ticket1.TextoExtremos("EFECTIVO DE CREDITOS:", corte["efectivo_creditos"]);
            Ticket1.TextoExtremos("EFECTIVO DE APARTADOS:", corte["efectivo_apartados"]);
            Ticket1.TextoExtremos("EFECTIVO DIRECTO: ", efedir.ToString("0.00"));
            Ticket1.LineasGuion();
            Ticket1.TextoCentro(" ");
            Ticket1.TextoExtremos("EFECTIVO TOTAL: ", corte["total_efectivo"]);
            Ticket1.TextoCentro(" ");
            Ticket1.LineasGuion();
            Ticket1.TextoExtremos("TOTAL T. DEBITO", corte["total_tarjetas_debito"]);
            Ticket1.TextoExtremos("TOTAL T. CREDITO", corte["total_tarjetas_credito"]);
            Ticket1.TextoExtremos("TOTAL CHEQUES", corte["total_cheques"]);
            Ticket1.TextoExtremos("TOTAL TRANSFERENCIAS", corte["total_transferencias"]);
            Ticket1.LineasGuion();
            Ticket1.TextoCentro(" ");
            Ticket1.LineasGuion();
            Ticket1.TextoExtremos("SOBRANTE:", corte["sobrante"]);
            Ticket1.TextoExtremos("GASTOS:", tgastos.ToString("0.00"));
            Ticket1.TextoExtremos("INGRESOS:", tingresos.ToString("0.00"));
            Ticket1.LineasGuion();
            Ticket1.TextoCentro(" ");
            Ticket1.TextoExtremos("TOTAL CZ:", totalCZ.ToString("0.00"));
            Ticket1.TextoCentro(" ");
            Ticket1.TextoCentro(" ");
            Ticket1.TextoCentro(" ");
            Ticket1.LineasGuion();
            Ticket1.TextoCentro("CAJERO:" + nc.ToUpper());

            Ticket1.CortaTicket();
        }
        
        private void Bimprimir_Click(object sender, EventArgs e)
        {
            if (localDM.impresora == null)
            {
                MessageBox.Show("No se ha configurado la impresora para imprimir el corte.", "Error");
                return;
            }

            if (tablaCortesZ.Rows.Count == 0)
            {
                MessageBox.Show("La tabla de cortes está vacía.", "Advertencia");
                return;
            }

            int selectedRowIndex = tablaCortesZ.CurrentCell.RowIndex;
            if (selectedRowIndex < 0)
            {
                MessageBox.Show("Seleccione un corte de la lista.", "Advertencia");
                return;
            }

            //MessageBox.Show("Imprimiendo corte");
            Dictionary<string, string> corte = localDM.getCorte(selectedRowIndex);
            if (corte != null)
            {
                imprimirCorte(corte);
            }
            else
            {
                MessageBox.Show("No se pudo obtener la información del corte.", "Error");
            }
        }
        */

        private void BelimHistorial_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Esta seguro que desea eliminar el historial de cortes?", "Eliminar historial", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                localDM.eliminarCortes();
                MessageBox.Show("Historial de cortes eliminado");
                tablaCortesZ.DataSource = localDM.getCortesBySucursal(data.idSucursal);
            }
        }

        private void tablaCortesZ_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BSelCorte.PerformClick();
                return;
            }
            if (e.KeyCode == Keys.F6)
            {
                BelimHistorial.PerformClick();
                return;
            }
        }
    }
}
