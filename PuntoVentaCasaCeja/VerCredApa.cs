using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PuntoVentaCasaCeja.Properties;
using Windows.Storage;
using Newtonsoft.Json;
using System.Drawing.Printing;

namespace PuntoVentaCasaCeja
{
    public partial class VerCredApa : Form
    {
        int tipo;
        WebDataManager webDM;
        LocaldataManager localDM;
        List<ProductoVenta> carrito;
        double totalcarrito;
        int rowCount, maxPages, currentPage, offset, idCliente, rowsPerPage = 10;
        List<string> estados = new List<string> ();
        bool isValidTicket = false;
        double totalpagado = 0;
        Usuario cajero;
        string ticket;
        string folio;
        int idOperacion;
        int printerType;
        Dictionary<int, float[]> tabs;
        List<AbonoCredito> listaAbonosCredito;
        List<AbonoApartado> listaAbonosApartado;
        Dictionary<string, List<Credito>> creditos;
        Dictionary<string, List<Apartado>> apartados;
        CurrentData data;
        List<Credito> currentCred;
        List<Apartado> currentApa;
        BindingSource source = new BindingSource();
        List<tabInfo> infoTabla = new List<tabInfo>();
        private System.Drawing.Printing.PrintDocument docToPrint = new System.Drawing.Printing.PrintDocument();

        public VerCredApa(int tipo, CurrentData data)
        {
            InitializeComponent();
            tabla.ColumnHeadersDefaultCellStyle.Font = new Font(tabla.Font.FontFamily, 18);
            tabla.RowsDefaultCellStyle.Font = new Font(tabla.Font.FontFamily, 16);
            this.printerType = int.Parse(Settings.Default["printertype"].ToString());
            this.KeyPreview = true;
            this.tipo = tipo;
            this.webDM = data.webDM;
            this.localDM = webDM.localDM;
            this.idCliente = data.cliente.id;         
            this.data = data;
            source.DataSource = infoTabla;
            tabla.DataSource = source;
            offset = 0;
            currentPage = 1;
            maxPages = 1;
            if (tipo == 0)
            {
                this.Text = "Mis créditos";
                groupBox1.Text = "MIS CRÉDITOS";
                creditos = localDM.getCreditosCliente(idCliente, data.idSucursal);
                string[] range = { "PENDIENTE", "EXPIRO", "CANCELADO", "PAGADO", "TODOS"};
                estados.AddRange(range);
            }
            else if (tipo == 1)
            {
                this.Text = "Mis apartados";
                groupBox1.Text = "MIS APARTADOS";
                apartados = localDM.getApartadosCliente(idCliente, data.idSucursal);
                string[] range = { "PENDIENTE", "EXPIRO", "CANCELADO", "PAGADO", "ENTREGADO", "TODOS" };
                estados.AddRange(range);
            }
            boxestado.DataSource = estados;
            boxestado.SelectedIndex = 0;
            this.cajero = webDM.activeUser;
            cargarTicketCarta();
            this.tabs = new Dictionary<int, float[]>()
            {
                {5, new float[]{ 110, 30, 50, 50 } },
                {6, new float[]{ 130, 40, 60, 60 } },
                {7, new float[]{ 145, 45, 65, 65 } },
                {8, new float[]{ 160, 50, 65, 65 } },
                {9, new float[]{ 185, 55, 70, 70 } },
                {10, new float[]{ 210, 60, 75, 75 } },
                {11, new float[]{ 225, 75, 85, 85 } },
                {12, new float[]{ 250, 75, 90, 90 } },
                {13, new float[]{ 270, 80, 100, 100 } },
                {14, new float[]{ 290, 85, 110, 110 } },
                {15, new float[]{ 310, 90, 120, 120 } }
            };

        }

        private void boxestado_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadData();            
        }

        private void boxestado_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                tabla.Focus();
            }
        }

        private void txtbuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Down)
            {
                tabla.Focus();
                SendKeys.Send("{DOWN}");
            }

            if (e.KeyData == Keys.Up)
            {
                tabla.Focus();
                SendKeys.Send("{UP}");
            }
            if (e.KeyData == Keys.Enter)
            {
                cargarTicketCarta();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void txtbuscar_TextChanged(object sender, EventArgs e)
        {
            loadData();
        }

        private void tabla_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    cargarTicketCarta();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
                case Keys.F1:
                    txtbuscar.Focus();
                    break;
                case Keys.F2:
                    boxestado.DroppedDown = true;
                    boxestado.Focus();
                    break;
                case Keys.F5:
                    abonarbtn.PerformClick();
                    break;
                case Keys.F6:
                    reprint.PerformClick();
                    break;
            }
        }

        private void tabla_MouseClick(object sender, MouseEventArgs e)
        {
            cargarTicketCarta();
        }

        private void VerCredApa_Load(object sender, EventArgs e)
        {
            loadData();
            //cargarTicketCarta();
            txtbuscar.Focus();
        }

        private void tabla_SelectionChanged(object sender, EventArgs e)
        {
            //cargarTicketCarta();
        }

        void refresh()
        {
            if (tipo == 0)
            {
                creditos = localDM.getCreditosCliente(idCliente, data.idSucursal);
            }
            if (tipo == 1)
            {
                apartados = localDM.getApartadosCliente(idCliente, data.idSucursal);
            }            
            loadData();
            cargarTicketCarta();
        }
        void loadData()
        {
            int estado = boxestado.SelectedIndex;
            if (tipo == 0)
            {
                // Limpiar la lista de información de la tabla
                infoTabla.Clear();

                if (txtbuscar.Text.Equals(""))
                {
                    // Filtrar los datos según el estado seleccionado
                    if (estado == 4)
                    {
                        currentCred = new List<Credito>();
                        currentCred.AddRange(creditos["PENDIENTE"]);
                        currentCred.AddRange(creditos["EXPIRO"]);
                        currentCred.AddRange(creditos["CANCELADO"]);
                        currentCred.AddRange(creditos["PAGADO"]);
                    }
                    else
                    {
                        currentCred = creditos[estados[estado]];
                    }
                }
                else
                {
                    // Filtrar los datos según el texto de búsqueda y el estado seleccionado
                    currentCred = new List<Credito>();
                    foreach (var entry in creditos)
                    {
                        if (estado == 4 || entry.Key == estados[estado])
                        {
                            foreach (Credito c in entry.Value)
                            {
                                if (c.folio.Contains(txtbuscar.Text))
                                {
                                    currentCred.Add(c);
                                }
                            }
                        }
                    }
                }

                // Ordenar y contar los datos filtrados
                currentCred = currentCred.OrderByDescending(x => x.fecha_de_credito).ToList();
                rowCount = currentCred.Count;
                calculateMaxPages(rowCount);

                // Agregar solo las filas correspondientes a la página actual
                for (int i = offset; i < offset + rowsPerPage && i < currentCred.Count; i++)
                {
                    tabInfo tempinfo = new tabInfo
                    {
                        ESTADO = estados[currentCred[i].estado],
                        FOLIO = currentCred[i].folio,
                        FECHA = currentCred[i].fecha_de_credito,
                        TOTAL = currentCred[i].total.ToString("0.00")
                    };
                    infoTabla.Add(tempinfo);
                }
            }
            else if (tipo == 1)
            {
                // Similar al bloque anterior, pero para los apartados
                infoTabla.Clear();

                if (txtbuscar.Text.Equals(""))
                {
                    if (estado == 5)
                    {
                        currentApa = new List<Apartado>();
                        currentApa.AddRange(apartados["PENDIENTE"]);
                        currentApa.AddRange(apartados["EXPIRO"]);
                        currentApa.AddRange(apartados["CANCELADO"]);
                        currentApa.AddRange(apartados["PAGADO"]);
                    }
                    else
                    {
                        currentApa = apartados[estados[estado]];
                    }
                }
                else
                {
                    currentApa = new List<Apartado>();
                    foreach (var entry in apartados)
                    {
                        if (estado == 5 || entry.Key == estados[estado])
                        {
                            foreach (Apartado c in entry.Value)
                            {
                                if (c.folio_corte.Contains(txtbuscar.Text))
                                {
                                    currentApa.Add(c);
                                }
                            }
                        }
                    }
                }

                currentApa = currentApa.OrderByDescending(x => x.fecha_de_apartado).ToList();
                rowCount = currentApa.Count;
                calculateMaxPages(rowCount);

                for (int i = offset; i < offset + rowsPerPage && i < currentApa.Count; i++)
                {
                    tabInfo tempinfo = new tabInfo
                    {
                        ESTADO = estados[currentApa[i].estado],
                        FOLIO = currentApa[i].folio_corte,
                        FECHA = currentApa[i].fecha_de_apartado,
                        TOTAL = currentApa[i].total.ToString("0.00")
                    };
                    infoTabla.Add(tempinfo);
                }
            }

            // Actualizar el origen de datos de la tabla
            source.ResetBindings(false);
        }


        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void abonarbtn_Click(object sender, EventArgs e)
        {
            Abonos ab = new Abonos(tipo, data, folio, idOperacion, totalcarrito, totalpagado, refresh);
            DialogResult result=ab.ShowDialog();           
        }
        private void calculateMaxPages(int rowCount)
        {
            maxPages = ((rowCount % rowsPerPage) == 0) ? rowCount / rowsPerPage : rowCount / rowsPerPage + 1;
            if (maxPages == 0)
                maxPages++;
            if (maxPages < currentPage)
            {
                currentPage = maxPages;
                offset = (currentPage - 1) * rowsPerPage;
            }
            pageLabel.Text = "Página " + currentPage + "/" + maxPages;
        }
        private void prev_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                offset -= rowsPerPage;
                currentPage--;
                loadData();
            }
        }

        private void next_Click(object sender, EventArgs e)
        {
            if (currentPage < maxPages)
            {
                offset += rowsPerPage;
                currentPage++;
                loadData();
            }
        }

        private void reprint_Click(object sender, EventArgs e)
        {
            if (isValidTicket)
            {
                try
                {
                    if (data.printerType == 1)
                        documento.Document.Print();
                    else
                    {
                        if (tipo == 0)
                        {
                            Credito selCred = currentCred[tabla.SelectedRows[0].Index + offset];
                            data.webDM.localDM.reimprimirAbonosCredito(selCred, data.sucursalName, data.sucursalDir, webDM.activeUser.nombre);
                        }
                        else
                        {
                            Apartado selApa = currentApa[tabla.SelectedRows[0].Index + offset];
                            data.webDM.localDM.reimprimirAbonosApartado(selApa, data.sucursalName, data.sucursalDir, webDM.activeUser.nombre);
                        }
                    }
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    MessageBox.Show("No se guardo el PDF, ya se encuentra abierto un documento con el mismo nombre.", "Error");
                }
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
        }

        private void cargarTicketCarta()
        {
            ticket = "";
            isValidTicket = false;
            if (tabla.SelectedRows.Count > 0)
            {
                isValidTicket = true;
                if (tipo == 0)
                {
                    Credito selCred = currentCred[tabla.SelectedRows[0].Index+offset];                    
                    if (selCred != null)
                    {
                        if (selCred.estado == 0)
                        {
                            abonarbtn.Enabled = true;
                        }
                        else
                        {
                            abonarbtn.Enabled = false;
                        }
                        listaAbonosCredito = selCred.abonos;
                        carrito = JsonConvert.DeserializeObject<List<ProductoVenta>>(selCred.productos);
                        totalcarrito = selCred.total;
                        totalpagado = selCred.total_pagado;
                        folio = selCred.folio;
                        idOperacion = selCred.id;
                        ticket += "CASA CEJA\n" +
                        "SUCURSAL: " + data.sucursalName + "\n" +
                        "" + data.sucursalDir + "\n" +
                        "" + selCred.fecha_de_credito + "\n" +
                        "FOLIO: " + folio + "\n" +
                        "TICKET DE CRÉDITO\n\n" +
                         "DESCRIPCION\tCANT\tP. UNIT\tP. TOTAL\n";
                        foreach (ProductoVenta p in carrito)
                        {
                            string n;
                            if (p.nombre.Length > 19)
                            {
                                n = p.nombre.Substring(0, 18);
                            }
                            else
                            {
                                n = p.nombre;
                            }
                            ticket += n + "\t" + p.cantidad + "\t" + p.precio_venta.ToString("0.00") + "\t" + (p.cantidad * p.precio_venta).ToString("0.00") + "\n";
                        }
                        if (!data.fontName.Equals("Consolas"))
                            ticket += "--------------------";
                        ticket += "--------------------------------------------------------------\n" +
                             "TOTAL $\t------>\t\t" + totalcarrito.ToString("0.00") + "\n";
                        if (!data.fontName.Equals("Consolas"))
                            ticket += "--------------------";
                        ticket += "--------------------------------------------------------------\n";

                        if (listaAbonosCredito != null)
                        {

                            if(listaAbonosCredito.Count > 0)
                            {
                                ticket += "HISTORIAL DE PAGOS:\n";
                            }                            
                            foreach (AbonoCredito a in listaAbonosCredito)
                            {
                                Dictionary<string, double> p = JsonConvert.DeserializeObject<Dictionary<string, double>>(a.metodo_pago);
                                ticket += "FECHA: " + a.fecha + "\t\tFOLIO DE CORTE: " + a.folio_corte + "\n";
                                if (p.ContainsKey("debito"))
                                {
                                    ticket += "PAGO T. DEBITO\t------>\t\t" + p["debito"] + "\n";
                                }
                                if (p.ContainsKey("credito"))
                                {
                                    ticket += "PAGO T. CREDITO\t------>\t\t" + p["credito"] + "\n";
                                }
                                if (p.ContainsKey("cheque"))
                                {
                                    ticket += "PAGO CHEQUES\t------>\t\t" + p["cheque"] + "\n";
                                }
                                if (p.ContainsKey("transferencia"))
                                {
                                    ticket += "PAGO TRANSFERENCIA\t------>\t\t" + p["transferencia"] + "\n";
                                }
                                if (p.ContainsKey("efectivo"))
                                {
                                    ticket += "EFECTIVO ENTREGADO\t------>\t\t" + p["efectivo"] + "\n";
                                }
                                if (!data.fontName.Equals("Consolas"))
                                    ticket += "--------------------";
                                ticket += "--------------------------------------------------------------\n";
                            }
                        }
                        ticket += "POR PAGAR $\t------>\t\t" + (totalcarrito - totalpagado).ToString("0.00") + "\n\n" + 
                        "LE ATENDIO: " + webDM.activeUser.nombre + "\n" + 
                         "NO DE ARTICULOS: " + carrito.Count.ToString().PadLeft(5, '0') + "\n";
                        // Agregar RFC desde configuración
                        string rfc = Settings.Default["rfc"].ToString();
                        if (!string.IsNullOrEmpty(rfc))
                        {
                            ticket += "RFC: " + rfc + "\n\n";
                        }
                    }
                }
                else
                {
                    Apartado selApa = currentApa[tabla.SelectedRows[0].Index + offset];
                    if (selApa != null)
                    {
                        if (selApa.estado == 0)
                        {
                            abonarbtn.Enabled = true;
                        }
                        else
                        {
                            abonarbtn.Enabled = false;
                        }
                        listaAbonosApartado = selApa.abonos;
                        carrito = JsonConvert.DeserializeObject<List<ProductoVenta>>(selApa.productos);
                        totalcarrito = selApa.total;
                        totalpagado = selApa.total_pagado;
                        folio = selApa.folio_corte;
                        idOperacion = selApa.id;
                        ticket += "CASA CEJA\n" +
                        "SUCURSAL: " + data.sucursalName + "\n" +
                        "" + data.sucursalDir + "\n" +
                        "" + selApa.fecha_de_apartado + "\n" +
                        "FOLIO: " + folio + "\n" +
                        "TICKET DE APARTADO\n\n" +
                         "DESCRIPCION\tCANT\tP. UNIT\tP. TOTAL\n";
                        foreach (ProductoVenta p in carrito)
                        {
                            string n;
                            if (p.nombre.Length > 19)
                            {
                                n = p.nombre.Substring(0, 18);
                            }
                            else
                            {
                                n = p.nombre;
                            }
                            ticket += n + "\t" + p.cantidad + "\t" + p.precio_venta.ToString("0.00") + "\t" + (p.cantidad * p.precio_venta).ToString("0.00") + "\n";
                        }
                        if (!data.fontName.Equals("Consolas"))
                            ticket += "--------------------";
                        ticket += "--------------------------------------------------------------\n" +
                             "TOTAL $\t------>\t\t" + totalcarrito.ToString("0.00") + "\n";
                        if (!data.fontName.Equals("Consolas"))
                            ticket += "--------------------";
                        ticket += "--------------------------------------------------------------\n";
                            
                        if (listaAbonosApartado != null)
                        {
                            if (listaAbonosApartado.Count > 0)
                            {
                                ticket += "HISTORIAL DE PAGOS:\n";
                            }

                            foreach (AbonoApartado a in listaAbonosApartado)
                            {
                                Dictionary<string, double> p = JsonConvert.DeserializeObject<Dictionary<string, double>>(a.metodo_pago);
                                ticket += "FECHA: " + a.fecha + "\t\tFOLIO DE CORTE: " + a.folio_corte + "\n";
                                if (p.ContainsKey("debito"))
                                {
                                    ticket += "PAGO T. DEBITO\t------>\t\t" + p["debito"] + "\n";
                                }
                                if (p.ContainsKey("credito"))
                                {
                                    ticket += "PAGO T. CREDITO\t------>\t\t" + p["credito"] + "\n";
                                }
                                if (p.ContainsKey("cheque"))
                                {
                                    ticket += "PAGO CHEQUES\t------>\t\t" + p["cheque"] + "\n";
                                }
                                if (p.ContainsKey("transferencia"))
                                {
                                    ticket += "PAGO TRANSFERENCIA\t------>\t\t" + p["transferencia"] + "\n";
                                }
                                if (p.ContainsKey("efectivo"))
                                {
                                    ticket += "EFECTIVO ENTREGADO\t------>\t\t" + p["efectivo"] + "\n";
                                }
                                if (!data.fontName.Equals("Consolas"))
                                    ticket += "--------------------";
                                ticket += "--------------------------------------------------------------\n";
                            }
                        }
                        ticket += "POR PAGAR $\t------>\t\t" + (totalcarrito - totalpagado).ToString("0.00") + "\n\n" +
                         "LE ATENDIO: " + webDM.activeUser.nombre + "\n" +
                         "NO DE ARTICULOS: " + carrito.Count.ToString().PadLeft(5, '0') + "\n";
                        // Agregar RFC desde configuración
                        string rfc = Settings.Default["rfc"].ToString();
                        if (!string.IsNullOrEmpty(rfc))
                        {
                            ticket += "RFC: " + rfc + "\n\n";
                        }
                    }     
                    
                }
                
            }
            createdoc();
        }
        private void createdoc()
        {
            // Configura el tamaño de papel según el tipo de impresora
            if (printerType == 0) // Ticket
            {
                // 78 mm ≈ 3.07 pulgadas; en PaperSize se usa hundredths of an inch, entonces 3.07*100 ≈ 307.
                // La altura se define de forma arbitraria, ajústala según la longitud de tu ticket.
                PaperSize ticketSize = new PaperSize("Ticket", 465, 1169);
                docToPrint.DefaultPageSettings.PaperSize = ticketSize;
                // Establecemos márgenes en 0 para aprovechar todo el ancho.
                docToPrint.DefaultPageSettings.Margins = new Margins(10, 10, 10, 10);
            }
            else if (printerType == 1)// Papel tamaño carta
            {
                // Por ejemplo, tamaño carta: 8.5 x 11 pulgadas.
                // En hundredths of an inch: ancho=850, alto=1100.
                PaperSize letterSize = new PaperSize("Letter", 850, 1100);
                docToPrint.DefaultPageSettings.PaperSize = letterSize;
                // Márgenes más amplios para papel carta (los puedes ajustar a tu preferencia)
                docToPrint.DefaultPageSettings.Margins = new Margins(50, 50, 50, 50);
            }
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Abono.txt");
            // Construct the PrintPreviewControl.

            //// Set location, name, and dock style for printPreviewControl1.
            //this.printPreviewControl1.Name = "printPreviewControl1";

            // Set the Document property to the PrintDocument 
            // for which the PrintPage event has been handled.
            this.documento.Document = docToPrint;
            this.documento.Zoom = 2;
            if (data.fontSize > 6)
                this.documento.Zoom = 1.5;
            if (data.fontSize > 10)
                this.documento.Zoom = 1.1;
            if (data.fontSize > 13)
                this.documento.Zoom = 1.0;
            // Set the document name. This will show be displayed when 
            // the document is loading into the control.

            this.documento.Document.DocumentName = path;
            this.documento.Document.PrinterSettings.PrinterName = localDM.impresora;


            // Set the UseAntiAlias property to true so fonts are smoothed
            // by the operating system.
            this.documento.UseAntiAlias = true;
            // Add the control to the form.

            // Associate the event-handling method with the
            // document's PrintPage event.
            this.docToPrint.PrintPage +=
                new System.Drawing.Printing.PrintPageEventHandler(
                docToPrint_PrintPage);
        }
        private void docToPrint_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            string text1 = ticket;

            // Verificar si el nombre de la fuente no está vacío
            if (string.IsNullOrEmpty(data.fontName))
            {
                throw new ArgumentException("El nombre de la fuente no puede estar vacío.");
            }

            // Verificar si la fuente está instalada en el sistema
            if (!FontFamily.Families.Any(f => f.Name.Equals(data.fontName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"La fuente '{data.fontName}' no está instalada en el sistema.");
            }

            FontFamily fontFamily = new FontFamily(data.fontName);
            Font font = new Font(
               fontFamily,
               data.fontSize,
               FontStyle.Regular,
               GraphicsUnit.Point);
            Rectangle rect = new Rectangle(10, 10, 750, 1000);
            StringFormat stringFormat = new StringFormat();
            SolidBrush solidBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            stringFormat.SetTabStops(0, tabs[data.fontSize]);

            e.Graphics.DrawString(text1, font, solidBrush, rect, stringFormat);
        }

        private void docToPrint_PrintPageOriginal(
    object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {

            // Insert code to render the page here.
            // This code will be called when the control is drawn.

            // The following code will render a simple
            // message on the document in the control.
            string text1 = ticket;
            //StringFormat format = new StringFormat(StringFormatFlags.NoClip);
            //format.Alignment = StringAlignment.Center;
            //System.Drawing.Font printFont =
            //    new Font(fontName, fontSize, FontStyle.Regular);

            //e.Graphics.DrawString(text1, printFont,
            //    Brushes.Black, 50, 50);

            FontFamily fontFamily = new FontFamily(data.fontName);
            Font font = new Font(
               fontFamily,
               data.fontSize,
               FontStyle.Regular,
               GraphicsUnit.Point);
            Rectangle rect = new Rectangle(10, 10, 750, 1000);
            StringFormat stringFormat = new StringFormat();
            SolidBrush solidBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0));


            stringFormat.SetTabStops(0, tabs[data.fontSize]);

            e.Graphics.DrawString(text1, font, solidBrush, rect, stringFormat);

            //Pen pen = Pens.Black;
            //e.Graphics.DrawRectangle(pen, rect);
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
                    case Keys.F1:
                        txtbuscar.Focus();
                        break;
                    case Keys.F2:
                        boxestado.DroppedDown = true;
                        boxestado.Focus();
                        break;
                    case Keys.F5:
                        abonarbtn.PerformClick();
                        break;
                    case Keys.F6:
                        reprint.PerformClick();
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
    public class tabInfo
    {
        public string ESTADO { get; set; }
        public string FOLIO { get; set; }
        public string FECHA { get; set; }
        public string TOTAL { get; set; }
    }
}

