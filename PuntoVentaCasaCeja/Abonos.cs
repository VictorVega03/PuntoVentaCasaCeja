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
using Newtonsoft.Json;
using PuntoVentaCasaCeja.Properties;
using Windows.Storage;

namespace PuntoVentaCasaCeja
{
    public partial class Abonos : Form
    {       
        int tipo;
        CurrentData data;
        string folio, ticket;
        int id;
        double total, pagado, porpagar, abonado;
        bool reprint = false;
        Action refresh;
        Dictionary<string, double> pagos;
        Dictionary<int, float[]> tabs;
        PrintPreviewControl printPreviewControl1;
        private System.Drawing.Printing.PrintDocument docToPrint =
    new System.Drawing.Printing.PrintDocument();
        public Abonos(int tipo, CurrentData data, string folio, int idOperacion, double total, double pagado, Action refresh)
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.tipo = tipo;
            this.data = data;
            this.data.esDescuento = false;

            Console.WriteLine("abono clase abonos: " + data.folioCorte);
            this.folio = folio;
            this.id = idOperacion;
            this.total = total;
            this.pagado = pagado;
            this.refresh = refresh;
            porpagar = total - pagado;
            printPreviewControl1 = new PrintPreviewControl();
            pagos = new Dictionary<string, double>();
            abonado = 0;
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

        private void Abonos_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                aceptar_Click(this, EventArgs.Empty);
            }
            if (e.KeyCode == Keys.F6)
            {
                abonar_Click(this, EventArgs.Empty);
            }
            if (e.KeyCode == Keys.Escape)
            {
                cancelar_Click(this, EventArgs.Empty);
            }
        }
        private void aceptar_Click(object sender, EventArgs e)
        {
            ProcesarPagoCompleto();
        }

        private void ProcesarPagoCompleto()
        {
            DateTime localDate = DateTime.Now;
            string folioabono;

            if (pagos.Count > 0)
            {
                AbonoCredito abonoCredito = null;
                AbonoApartado abonoApartado = null;
                string folioBase = data.idSucursal.ToString().PadLeft(2, '0') + data.idCaja.ToString().PadLeft(2, '0') + localDate.Day.ToString().PadLeft(2, '0') + localDate.Month.ToString().PadLeft(2, '0') + localDate.Year + "AA";

                if (tipo == 0)
                {
                    abonoCredito = new AbonoCredito
                    {
                        fecha = localDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        folio = folioBase,
                        folio_corte = data.folioCorte,
                        usuario_id = data.webDM.activeUser.id,
                        folio_credito = folio,
                        credito_id = id,
                        metodo_pago = JsonConvert.SerializeObject(pagos),
                        total_abonado = abonado
                    };

                    int ida = data.webDM.localDM.abonoCreditoTemporal(abonoCredito);
                    folioabono = abonoCredito.folio;
                }
                else
                {
                    abonoApartado = new AbonoApartado
                    {
                        fecha = localDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        folio = folioBase,
                        folio_corte = data.folioCorte,
                        usuario_id = data.webDM.activeUser.id,
                        folio_apartado = folio,
                        apartado_id = id,
                        metodo_pago = JsonConvert.SerializeObject(pagos),
                        total_abonado = abonado
                    };

                    int ida = data.webDM.localDM.abonoApartadoTemporal(abonoApartado);
                    folioabono = abonoApartado.folio;
                }

                data.webDM.localDM.acumularPagos(pagos, data.idCorte, 0);

                if (pagos.ContainsKey("efectivo"))
                {
                    if (tipo == 0)
                    {
                        data.webDM.localDM.acumularEfectivoCredito(pagos["efectivo"], data.idCorte);
                    }
                    else
                    {
                        data.webDM.localDM.acumularEfectivoApartado(pagos["efectivo"], data.idCorte);
                    }
                }
                else
                {
                    Console.WriteLine("La clave 'efectivo' no se encontró en el diccionario 'pagos'.");
                }

                if (id == 0)
                {
                    if (tipo == 0)
                    {
                        sendCreditoTemporal();
                    }
                    else
                    {
                        sendApartadoTemporal();
                    }
                }
                else
                {
                    if (tipo == 0)
                    {
                        sendAbonoCredito(abonoCredito);
                    }
                    else
                    {
                        sendAbonoAprattado(abonoApartado);
                    }
                }

                cargarTicketCarta(localDate.ToString("dd/MM/yyyy hh:mm tt"), folioabono);

                if (data.webDM.localDM.impresora.Equals(""))
                {
                    MessageBox.Show("No se ha establecido una impresora", "Advertencia");
                }
                else
                {
                    try
                    {
                        if (data.printerType == 1)
                        {
                            printPreviewControl1.Document.Print();
                            if (reprint)
                            {
                                printPreviewControl1.Document.Print();
                            }
                        }
                        else
                        {
                            data.webDM.localDM.imprimirAbono(tipo, pagos, data.webDM.activeUser.nombre, data.sucursalName, data.sucursalDir, localDate.ToString("dd/MM/yyyy hh:mm tt"), abonado, porpagar, folioabono, folio);
                            data.webDM.localDM.imprimirAbono(tipo, pagos, data.webDM.activeUser.nombre, data.sucursalName, data.sucursalDir, localDate.ToString("dd/MM/yyyy hh:mm tt"), abonado, porpagar, folioabono, folio);
                            if (reprint)
                            {
                                data.webDM.localDM.imprimirAbono(tipo, pagos, data.webDM.activeUser.nombre, data.sucursalName, data.sucursalDir, localDate.ToString("dd/MM/yyyy hh:mm tt"), abonado, porpagar, folioabono, folio);
                            }
                        }
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        MessageBox.Show("No se guardo el PDF, ya se encuentra abierto un documento con el mismo nombre.", "Error");
                    }
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    data.descuento = 0;
                }
            }
            else
            {
                MessageBox.Show("Favor de hacer un abono antes de continuar", "Advertencia");
            }
        }


        private async void sendAbonoCredito(AbonoCredito datos)
        {
            try
            {
                bool result = await data.webDM.enviarAbonoCredito(datos, id);
                if (result)
                {
                    MessageBox.Show("Abono registrado con éxito", "Éxito");
                    data.successful = true;
                }
                else
                {
                    MessageBox.Show("Abono registrado de manera local", "Advertencia");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar abono crédito: {ex.Message}", "Error");
            }
            refresh();
        }

        private async void sendCreditoTemporal()
        {
            try
            {
                bool result = await data.webDM.enviarCreditoTemporal(folio);
                if (result)
                {
                    MessageBox.Show("Abono registrado con éxito", "Éxito");
                    data.successful = true;
                }
                else
                {
                    MessageBox.Show("Abono registrado de manera local", "Advertencia");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar crédito temporal: {ex.Message}", "Error");
            }
            refresh();
        }

        private async void sendAbonoAprattado(AbonoApartado datos)
        {
            try
            {
                bool result = await data.webDM.enviarAbonoApartado(datos, id);
                if (result)
                {
                    MessageBox.Show("Abono registrado con éxito", "Éxito");
                    data.successful = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo conectar al servidor, favor de intentar más tarde", "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar abono apartado: {ex.Message}", "Error");
            }
            refresh();
        }

        private async void sendApartadoTemporal()
        {
            try
            {
                bool result = await data.webDM.enviarApartadoTemporal(folio);
                if (result)
                {
                    MessageBox.Show("Abono registrado con éxito", "Éxito");
                    data.successful = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo conectar al servidor, favor de intentar más tarde", "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar apartado temporal: {ex.Message}", "Error");
            }
            refresh();
        }



        private void abonar_Click(object sender, EventArgs e)
        {
            MetodoPago mp = new MetodoPago(porpagar, abono, data);
            mp.ShowDialog();

            if (total <= (pagado + abonado))
            {
                double cambio = pagado + abonado - total;
                MessageBox.Show("Cambio MXN: $" + cambio.ToString("0.00"), "Cambio");
                Dictionary<string, double> info = new Dictionary<string, double>
                {
                    ["efectivo"] = -cambio
                };

                if (pagos.ContainsKey("efectivo"))
                {
                    pagos["efectivo"] = Math.Round((pagos["efectivo"] - cambio), 2);
                }

                abonado -= cambio;              
                txtporpagar.Text = "0.00";
                data.webDM.localDM.acumularPagos(info, data.idCorte, 0);

                ProcesarPagoCompleto();
            }
        }

        private void cancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Abonos_Load(object sender, EventArgs e)
        {
            txtfolio.Text = folio;
            txttotal.Text = total.ToString("0.00");
            txtabonado.Text = abonado.ToString("0.00");
            txtporpagar.Text = (total - pagado).ToString("0.00");
        }
        void abono(int tipo, double cantidad)
        {
            switch (tipo)
            {
                case 1:
                    if (pagos.ContainsKey("efectivo"))
                    {
                        pagos["efectivo"] += cantidad;
                    }
                    else
                    {
                        pagos["efectivo"] = cantidad;
                    }
                    break;
                case 2:
                    if (pagos.ContainsKey("debito"))
                    {
                        pagos["debito"] += cantidad;
                    }
                    else
                    {
                        pagos["debito"] = cantidad;
                        reprint = true;
                    }
                    break;
                case 3:
                    if (pagos.ContainsKey("credito"))
                    {
                        pagos["credito"] += cantidad;
                    }
                    else
                    {
                        pagos["credito"] = cantidad;
                        reprint = true;
                    }
                    break;
                case 4:
                    if (pagos.ContainsKey("cheque"))
                    {
                        pagos["cheque"] += cantidad;
                    }
                    else
                    {
                        pagos["cheque"] = cantidad;
                        reprint = true;
                    }
                    break;
                case 5:
                    if (pagos.ContainsKey("transferencia"))
                    {
                        pagos["transferencia"] += cantidad;
                    }
                    else
                    {
                        pagos["transferencia"] = cantidad;
                        reprint = true;
                    }
                    break;
            }
            abonado += cantidad;
            txtabonado.Text = abonado.ToString("0.00");
            txtporpagar.Text = (total - pagado - abonado).ToString("0.00");
            porpagar = total - pagado - abonado;
        }
        private void cargarTicketCarta(string fecha, string folioAbono)
        {
            ticket = "";
            ticket += "CASA CEJA\n" +
            "SUCURSAL: " + data.sucursalName.ToUpper() + "\n" +
            "" + data.sucursalDir.ToUpper() + "\n" +
            "" + fecha + "\n" +
            "FOLIO: " + folioAbono + "\n";
            ticket += "TICKET DE ABONO\n\nCONCEPTO:\n";
            if (tipo == 0)
                ticket += "CREDITO ";
            if (tipo == 1)
                ticket += "APARTADO ";
            ticket += "CON FOLIO: " + folio + "\n";
            if (!data.fontName.Equals("Consolas"))
                ticket += "--------------------";
            ticket += "--------------------------------------------------------------\n";

            if (pagos.ContainsKey("debito"))
            {
                ticket += "PAGO T. DEBITO\t------>\t\t" + pagos["debito"].ToString("0.00") + "\n";
            }
            if (pagos.ContainsKey("credito"))
            {
                ticket += "PAGO T. CREDITO\t------>\t\t" + pagos["credito"].ToString("0.00") + "\n";
            }
            if (pagos.ContainsKey("cheque"))
            {
                ticket += "PAGO CHEQUES\t------>\t\t" + pagos["cheque"].ToString("0.00") + "\n";
            }
            if (pagos.ContainsKey("transferencia"))
            {
                ticket += "PAGO TRANSFERENCIA\t------>\t\t" + pagos["transferencia"].ToString("0.00") + "\n";
            }
            if (pagos.ContainsKey("efectivo"))
            {
                ticket += "EFECTIVO ENTREGADO\t------>\t\t" + pagos["efectivo"].ToString("0.00") + "\n";
            }
            if (!data.fontName.Equals("Consolas"))
                ticket += "--------------------";
            ticket += "--------------------------------------------------------------\n";
            ticket += "TOTAL ABONADO\t------>\t\t" + abonado.ToString("0.00") + "\n";
            if (!data.fontName.Equals("Consolas"))
                ticket += "--------------------";
            ticket += "--------------------------------------------------------------\n";         
            if (porpagar < 0)
            {     
                ticket += "SU CAMBIO\t------>\t\t" + Math.Abs(porpagar).ToString("0.00") + "\n";
            }
            else
            ticket += "POR PAGAR\t------>\t\t" + porpagar.ToString("0.00") + "\n\n"+
            "LE ATENDIO: " + data.webDM.activeUser.nombre.ToUpper() + "\n"+
            "GRACIAS POR SU PREFERENCIA\n";
            // Agregar RFC desde configuración
            string rfc = Settings.Default["rfc"].ToString();
            if (!string.IsNullOrEmpty(rfc))
            {
                ticket += "RFC: " + rfc + "\n\n";
            }
            createdoc();
        }
        private void createdoc()
        {

            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Abono.txt");
            // Construct the PrintPreviewControl.

            //// Set location, name, and dock style for printPreviewControl1.
            //this.printPreviewControl1.Name = "printPreviewControl1";

            // Set the Document property to the PrintDocument 
            // for which the PrintPage event has been handled.
            this.printPreviewControl1.Document = docToPrint;
            this.printPreviewControl1.Zoom = 2;
            if (data.fontSize > 6)
                this.printPreviewControl1.Zoom = 1.5;
            if (data.fontSize > 10)
                this.printPreviewControl1.Zoom = 1.1;
            if (data.fontSize > 13)
                this.printPreviewControl1.Zoom = 1.0;
            // Set the document name. This will show be displayed when 
            // the document is loading into the control.
            this.printPreviewControl1.Document.DocumentName = path;
            this.printPreviewControl1.Document.PrinterSettings.PrinterName = data.webDM.localDM.impresora;

            // Set the UseAntiAlias property to true so fonts are smoothed
            // by the operating system.
            this.printPreviewControl1.UseAntiAlias = true;
            // Add the control to the form.

            // Associate the event-handling method with the
            // document's PrintPage event.
            this.docToPrint.PrintPage +=
                new System.Drawing.Printing.PrintPageEventHandler(
                docToPrint_PrintPage);
        }
        private void docToPrint_PrintPage(
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
            Rectangle rect = new Rectangle(50, 50, 750, 1000);
            StringFormat stringFormat = new StringFormat();
            SolidBrush solidBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0));


            stringFormat.SetTabStops(0, tabs[data.fontSize]);

            e.Graphics.DrawString(text1, font, solidBrush, rect, stringFormat);

            //Pen pen = Pens.Black;
            //e.Graphics.DrawRectangle(pen, rect);
        }
    }
}
