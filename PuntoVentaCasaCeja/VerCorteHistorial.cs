using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PuntoVentaCasaCeja
{
    public partial class VerCorteHistorial : Form
    {
        Action<Usuario> setUser;
        Usuario usuario;
        WebDataManager webDM;
        LocaldataManager localDM;
        CurrentData data;
        Dictionary<string, string> corteData;
        int idCorte;
        public VerCorteHistorial(Dictionary<string, string> corteData, CurrentData data, int idCorte)
        {
            InitializeComponent();

            this.webDM = data.webDM;
            this.data = data;
            this.localDM = webDM.localDM;
            this.corteData = corteData;
            this.idCorte = idCorte;

            // Asignación de datos a controles con validación para evitar nulos o valores vacíos
            this.txtfolio.Text = corteData.ContainsKey("folio_corte") ? corteData["folio_corte"] : "-";
            this.txtefectivo.Text = corteData.ContainsKey("total_efectivo") ? corteData["total_efectivo"] : "0.00";
            this.txtefecred.Text = corteData.ContainsKey("efectivo_creditos") ? corteData["efectivo_creditos"] : "0.00";
            this.txtefeapa.Text = corteData.ContainsKey("efectivo_apartados") ? corteData["efectivo_apartados"] : "0.00";
            this.txttotapa.Text = corteData.ContainsKey("total_apartados") ? corteData["total_apartados"] : "0.00";
            this.txttotcred.Text = corteData.ContainsKey("total_creditos") ? corteData["total_creditos"] : "0.00";
            this.txtfechapertura.Text = corteData.ContainsKey("fecha_apertura_caja") ? corteData["fecha_apertura_caja"] : "-";
            this.txtcheques.Text = corteData.ContainsKey("total_cheques") ? corteData["total_cheques"] : "0.00";
            this.txtdebito.Text = corteData.ContainsKey("total_tarjetas_debito") ? corteData["total_tarjetas_debito"] : "0.00";
            this.txtcredito.Text = corteData.ContainsKey("total_tarjetas_credito") ? corteData["total_tarjetas_credito"] : "0.00";
            this.txtapertura.Text = corteData.ContainsKey("fondo_apertura") ? corteData["fondo_apertura"] : "0.00";
            this.txttransferencias.Text = corteData.ContainsKey("total_transferencias") ? corteData["total_transferencias"] : "0.00";
            this.txtsobrante.Text = corteData.ContainsKey("sobrante") ? corteData["sobrante"] : "0.00";
            this.txtfechcorte.Text = corteData.ContainsKey("fecha_corte_caja") ? corteData["fecha_corte_caja"] : "-";

            // Deserialización segura de gastos e ingresos
            try
            {
                var gastos = JsonConvert.DeserializeObject<Dictionary<string, double>>(corteData["gastos"]);
                this.listagastos.DataSource = gastos?.ToList() ?? new List<KeyValuePair<string, double>>();
            }
            catch (JsonException ex)
            {
                MessageBox.Show("Error al deserializar 'gastos': " + ex.Message, "Error de JSON");
                this.listagastos.DataSource = new List<KeyValuePair<string, double>>(); // Vacío en caso de error
            }

            try
            {
                var ingresos = JsonConvert.DeserializeObject<Dictionary<string, double>>(corteData["ingresos"]);
                this.listaingresos.DataSource = ingresos?.ToList() ?? new List<KeyValuePair<string, double>>();
            }
            catch (JsonException ex)
            {
                MessageBox.Show("Error al deserializar 'ingresos': " + ex.Message, "Error de JSON");
                this.listaingresos.DataSource = new List<KeyValuePair<string, double>>(); // Vacío en caso de error
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

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

            double totalCZ = double.Parse(corte["total_efectivo"]) + double.Parse(corte["total_tarjetas_debito"]) +
                             double.Parse(corte["total_tarjetas_credito"]) + double.Parse(corte["total_cheques"]) +
                             double.Parse(corte["total_transferencias"]) + double.Parse(corte["sobrante"]);
            string nc = localDM.getNombreUsuario(int.Parse(corte["usuario_id"]));
            string sucursalName = localDM.getSucursalname(int.Parse(corte["sucursal_id"]));

            CreaTicket Ticket1 = new CreaTicket();
            Ticket1.impresora = localDM.impresora;
            Ticket1.TextoCentroCorte("CASA CEJA");
            Ticket1.TextoCentroCorte(" ");
            Ticket1.TextoCentroCorte("SUCURSAL: " + sucursalName.ToUpper());
            Ticket1.TextoCentroCorte(" ");
            Ticket1.TextoCentroCorte("CZ FOLIO:  " + corte["folio_corte"]);
            Ticket1.TextoCentroCorte(" ");
            Ticket1.LineasGuion(); // imprime una linea de guiones
            Ticket1.TextoExtremosCorte("FECHA DE APERTURA:", corte["fecha_apertura_caja"]);
            Ticket1.TextoExtremosCorte("FECHA DE CORTE:", corte["fecha_corte_caja"]);
            Ticket1.LineasGuion(); // imprime una linea de guiones
            Ticket1.TextoCentroCorte(" ");
            Ticket1.TextoExtremosCorte("FONDO DE APERTURA:", corte["fondo_apertura"]);
            Ticket1.TextoCentroCorte(" ");
            Ticket1.TextoExtremosCorte("TOTAL CZ:", totalCZ.ToString("0.00"));
            Ticket1.LineasGuion();
            Ticket1.TextoExtremosCorte("EFECTIVO DE CREDITOS:", corte["efectivo_creditos"]);
            Ticket1.TextoExtremosCorte("EFECTIVO DE APARTADOS:", corte["efectivo_apartados"]);
            Ticket1.TextoExtremosCorte("EFECTIVO DIRECTO: ", efedir.ToString("0.00"));
            Ticket1.LineasGuion();
            Ticket1.TextoCentroCorte(" ");
            Ticket1.LineasGuion();
            Ticket1.TextoExtremosCorte("TOTAL T. DEBITO", corte["total_tarjetas_debito"]);
            Ticket1.TextoExtremosCorte("TOTAL T. CREDITO", corte["total_tarjetas_credito"]);
            Ticket1.TextoExtremosCorte("TOTAL CHEQUES", corte["total_cheques"]);
            Ticket1.TextoExtremosCorte("TOTAL TRANSFERENCIAS", corte["total_transferencias"]);
            Ticket1.LineasGuion();
            Ticket1.TextoCentroCorte(" ");
            Ticket1.LineasGuion();
            Ticket1.TextoExtremosCorte("SOBRANTE:", corte["sobrante"]);
            Ticket1.TextoExtremosCorte("GASTOS:", tgastos.ToString("0.00"));
            Ticket1.TextoExtremosCorte("INGRESOS:", tingresos.ToString("0.00"));
            Ticket1.TextoExtremosCorte("EFECTIVO TOTAL: ", corte["total_efectivo"]);
            Ticket1.LineasGuion();
            Ticket1.TextoCentroCorte(" ");
            Ticket1.TextoCentroCorte(" ");
            Ticket1.TextoCentroCorte(" ");
            Ticket1.TextoCentroCorte(" ");
            Ticket1.LineasGuion();
            Ticket1.TextoCentroCorte("CAJERO:" + nc.ToUpper());

            Ticket1.CortaTicket();
        }

        private void Bimprimir_Click(object sender, EventArgs e)
        {
            int selectedRowIndex = 1;
            if (selectedRowIndex < 0)
            {
                MessageBox.Show("Seleccione un corte de la lista.", "Advertencia");
                return;
            }
            Dictionary<string, string> corte = localDM.getCorte2(idCorte);
            try
            {
                if (corte != null)
                {
                    imprimirCorte(corte);
                }
                else
                {
                    MessageBox.Show("No se pudo obtener la información del corte.", "Error");
                }
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("No se guardó el PDF, ya se encuentra abierto un documento con el mismo nombre.", "Error");
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
                    case Keys.F5:
                        Bimprimir.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void listagastos_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
