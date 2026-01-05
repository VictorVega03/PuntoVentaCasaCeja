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
using Windows.Storage;
using Newtonsoft.Json;
using PuntoVentaCasaCeja.Properties;
using Windows.UI.Xaml;
using System.Drawing.Printing;

namespace PuntoVentaCasaCeja
{
    public partial class VerOperaciones : Form
    {
        int rowCount, maxPages, currentPage, offset, idCliente, rowsPerPage = 10;
        LocaldataManager localDM;
        DataTable data;
        bool firstTicket = false;
        int idcaja;
        string sucursalName, sucursalDir;
        List<ProductoVenta> productos;
        string ticket = "";
        string fontName = "";
        int fontSize;
        bool isValidTicket = false;
        string cajero;
        string fecha;
        string folio;
        string total;
        double cambio;
        double descuento;
        double totalformat;
        int printerType;
        int userlvl;
        Dictionary<string, double> pagos;
        Dictionary<int, float[]> tabs;
        BindingSource source = new BindingSource();
        private System.Drawing.Printing.PrintDocument docToPrint =
        new System.Drawing.Printing.PrintDocument();

        public VerOperaciones(LocaldataManager localdata, int idcaja, string sucursalName, string sucursalDir, int userlvl)
        {
            InitializeComponent();
            tabla.ColumnHeadersDefaultCellStyle.Font = new Font(tabla.Font.FontFamily, 18);
            tabla.RowsDefaultCellStyle.Font = new Font(tabla.Font.FontFamily, 16);
            offset = 0;
            currentPage = 1;
            this.localDM = localdata;
            this.userlvl = userlvl;
            this.idcaja = idcaja;
            this.sucursalName = sucursalName;
            this.sucursalDir = sucursalDir;
            this.fontName = Settings.Default["fontName"].ToString();
            this.fontSize = int.Parse(Settings.Default["fontSize"].ToString());
            this.printerType = int.Parse(Settings.Default["printertype"].ToString());       
            this.tabs = new Dictionary<int, float[]> () 
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
            if (userlvl > 1)
            {
                delete.Enabled = false;
            }
        }

        private void VerOperaciones_Load(object sender, EventArgs e)
        {
            loadData();
            loadTicket();
        }

        private void loadData()
        {
            string fechaSeleccionada = filtrarFecha.Value.ToString("yyyy-MM-dd");
            data = localDM.getVentasFecha(fechaSeleccionada);

            data.DefaultView.Sort = "id DESC";
            calculateMaxPages(data.Rows.Count);

            // Obtener las filas para la página actual
            var paginatedRows = data.AsEnumerable().Skip(offset).Take(rowsPerPage);

            if (paginatedRows.Any())
            {
                var paginatedData = paginatedRows.CopyToDataTable();
                // Asignar el DataSource al BindingSource para que administre las actualizaciones
                source.DataSource = paginatedData;
                tabla.DataSource = source;
            }
            else
            {
                // Manejar el caso donde no hay filas para mostrar
                source.DataSource = null;
                tabla.DataSource = source;
            }

            txtbuscar.Focus();
            // Se comentó para que no se cargue el ticket automáticamente al cargar la ventana
             loadTicket();
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

        private void txtbuscar_TextChanged(object sender, EventArgs e)
        {
            if (txtbuscar.Text.Equals(""))
            {
                string fechaSeleccionada = filtrarFecha.Value.ToString("yyyy-MM-dd");
                data = localDM.getVentasFecha(fechaSeleccionada);
            }
            else
            {
                data = localDM.getVentas(txtbuscar.Text);
                data = localDM.getVentas(txtbuscar.Text);
            }
            tabla.DataSource = data;
            
            // Se comentó para que no se cargue el ticket automáticamente cuando el texto en buscar cambie
            // loadTicket();
        }
        private void obtenerDescuento()
        {
            if (tabla.SelectedRows.Count > 0)
            {
                var selectedRow = tabla.SelectedRows[0];
                var descuentoValue = selectedRow.Cells[2].Value;
                if (double.TryParse(descuentoValue.ToString(), out double descuento))
                {
                    this.descuento = descuento;
                }
                else
                {
                    MessageBox.Show("El valor de la celda de descuento no es un número válido.", "Error de conversión");
                    this.descuento = 0;
                }
            }
        }
        private void loadTicket()
        {
            string piedeticket = Settings.Default["pieDeTicket"].ToString();
            ticket = "";
            isValidTicket = false;

            if (tabla.SelectedRows.Count > 0)
            {
                isValidTicket = true;
                cajero = tabla.SelectedRows[0].Cells[5].Value.ToString();

                DateTime fechaOriginal = DateTime.Parse(tabla.SelectedRows[0].Cells[4].Value.ToString());
                fecha = fechaOriginal.ToString("dd/MM/yyyy hh:mm tt");

                folio = tabla.SelectedRows[0].Cells[3].Value.ToString();
                total = tabla.SelectedRows[0].Cells[1].Value.ToString();
                totalformat = double.Parse(total);

                double descuentoTotalBD = double.Parse(tabla.SelectedRows[0].Cells[2].Value.ToString());

                pagos = JsonConvert.DeserializeObject<Dictionary<string, double>>(tabla.SelectedRows[0].Cells[6].Value.ToString());

                ticket += "CASA CEJA\n" +
                    "SUCURSAL: " + sucursalName.ToUpper() + "\n" +
                    "" + sucursalDir.ToUpper() + "\n" +
                    "" + fecha + "\n" +
                    "FOLIO: " + folio + "\n\n" +
                    "DESCRIPCION\tCANT\tP. UNIT\tP. TOTAL\n";

                string id = tabla.SelectedRows[0].Cells[0].Value.ToString();
                productos = localDM.getProductosVenta(id);

                double subtotalSinDescuentos = 0;
                double totalDescuentoCategoria = 0;
                double totalDescuentoPrecioEspecial = 0;

                foreach (ProductoVenta p in productos)
                {
                    Producto productoCompleto = localDM.GetProductByCode(p.codigo);
                    if (productoCompleto != null)
                    {
                        p.precio_original = productoCompleto.menudeo;

                        // INICIALIZAR campos
                        p.es_descuento_categoria = false;
                        p.es_precio_especial = false;
                        p.descuento_categoria_unitario = 0;
                        p.descuento_unitario = 0;
                        p.porcentaje_descuento_categoria = 0;

                        Console.WriteLine($"\n=== ANALIZANDO: {p.nombre} ===");
                        Console.WriteLine($"Precio BD: {p.precio_venta}");
                        Console.WriteLine($"Menudeo: {productoCompleto.menudeo}");
                        Console.WriteLine($"Especial: {productoCompleto.especial}");

                        // VERIFICAR PRECIO ESPECIAL PRIMERO (tiene prioridad)
                        if (productoCompleto.especial > 0 && Math.Abs(p.precio_venta - productoCompleto.especial) < 0.01)
                        {
                            // *** ES PRECIO ESPECIAL ***
                            p.es_precio_especial = true;
                            p.descuento_unitario = productoCompleto.menudeo - productoCompleto.especial;

                            Console.WriteLine($"✅ PRECIO ESPECIAL: {p.descuento_unitario}");
                        }
                        else
                        {
                            // VERIFICAR DESCUENTO DE CATEGORÍA
                            var (tieneDescuentoCategoria, porcentajeCategoria) = localDM.GetDescuentoCategoria(productoCompleto.categoria_id);

                            if (tieneDescuentoCategoria && porcentajeCategoria > 0)
                            {
                                double precioConCategoria = productoCompleto.menudeo * (1 - (double)porcentajeCategoria / 100.0);
                                Console.WriteLine($"Precio con categoría: {precioConCategoria}");

                                if (Math.Abs(p.precio_venta - precioConCategoria) < 0.01)
                                {
                                    // *** ES DESCUENTO DE CATEGORÍA ***
                                    p.es_descuento_categoria = true;
                                    p.porcentaje_descuento_categoria = (double)porcentajeCategoria;
                                    p.descuento_categoria_unitario = productoCompleto.menudeo - p.precio_venta;

                                    Console.WriteLine($"✅ DESCUENTO DE CATEGORÍA: {p.descuento_categoria_unitario}");
                                }
                                else
                                {
                                    Console.WriteLine($"❌ NO COINCIDE CON NINGÚN PATRÓN");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"✅ SIN DESCUENTOS");
                            }
                        }
                    }
                    else
                    {
                        p.precio_original = p.precio_venta;
                        Console.WriteLine($"❌ PRODUCTO NO ENCONTRADO EN BD");
                    }

                    // ACUMULAR totales
                    subtotalSinDescuentos += p.precio_original * p.cantidad;

                    if (p.es_descuento_categoria)
                    {
                        totalDescuentoCategoria += p.descuento_categoria_unitario * p.cantidad;
                        Console.WriteLine($"Acumulando desc categoría: +{p.descuento_categoria_unitario * p.cantidad} = {totalDescuentoCategoria}");
                    }

                    if (p.es_precio_especial)
                    {
                        totalDescuentoPrecioEspecial += p.descuento_unitario * p.cantidad;
                        Console.WriteLine($"Acumulando desc especial: +{p.descuento_unitario * p.cantidad} = {totalDescuentoPrecioEspecial}");
                    }
                }

                Console.WriteLine($"\n=== TOTALES FINALES ===");
                Console.WriteLine($"Subtotal sin descuentos: {subtotalSinDescuentos}");
                Console.WriteLine($"Total Descuento Categoría: {totalDescuentoCategoria}");
                Console.WriteLine($"Total Descuento Precio Especial: {totalDescuentoPrecioEspecial}");

                // MOSTRAR productos en el ticket
                foreach (ProductoVenta p in productos)
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

                    // AGREGAR INDICADORES (solo uno, nunca ambos)
                    string indicadores = "";
                    if (p.es_precio_especial)
                    {
                        indicadores += "*ESP";
                    }
                    else if (p.es_descuento_categoria)
                    {
                        indicadores += $"*CAT{p.porcentaje_descuento_categoria:0}%";
                    }

                    double precioUnitarioOriginal = p.precio_original;
                    double precioFinalConDescuentos = p.precio_venta * p.cantidad;

                    ticket += n + indicadores + "\t" + p.cantidad + "\t" + precioUnitarioOriginal.ToString("0.00") + "\t" + precioFinalConDescuentos.ToString("0.00") + "\n";
                }

                if (!fontName.Equals("Consolas"))
                    ticket += "--------------------";
                ticket += "--------------------------------------------------------------\n";

                ticket += "SUBTOTAL $\t------>\t\t" + subtotalSinDescuentos.ToString("0.00") + "\n";

                if (totalDescuentoCategoria > 0)
                {
                    ticket += "DESC. POR CATEGORIA\t------>\t-" + totalDescuentoCategoria.ToString("0.00") + "\n";
                }

                if (totalDescuentoPrecioEspecial > 0)
                {
                    ticket += "DESC. PRECIO ESPECIAL\t------>\t-" + totalDescuentoPrecioEspecial.ToString("0.00") + "\n";
                }

                double descuentoTotalCalculado = totalDescuentoCategoria + totalDescuentoPrecioEspecial;
                double descuentoVenta = Math.Max(0, descuentoTotalBD - descuentoTotalCalculado);

                if (descuentoVenta > 0)
                {
                    ticket += "DESCUENTO DE VENTA\t------>\t-" + descuentoVenta.ToString("0.00") + "\n";
                }

                double totalFinalConTodosLosDescuentos = subtotalSinDescuentos - totalDescuentoCategoria - totalDescuentoPrecioEspecial - descuentoVenta;
                ticket += "TOTAL A PAGAR $\t------>\t\t" + totalFinalConTodosLosDescuentos.ToString("0.00") + "\n";

                if (!fontName.Equals("Consolas"))
                    ticket += "--------------------";
                ticket += "--------------------------------------------------------------\n";

                cambio = totalFinalConTodosLosDescuentos;

                if (pagos.ContainsKey("debito"))
                {
                    ticket += "PAGO T. DEBITO\t------>\t\t" + pagos["debito"].ToString("0.00") + "\n";
                    cambio -= pagos["debito"];
                }
                if (pagos.ContainsKey("credito"))
                {
                    ticket += "PAGO T. CREDITO\t------>\t\t" + pagos["credito"].ToString("0.00") + "\n";
                    cambio -= pagos["credito"];
                }
                if (pagos.ContainsKey("cheque"))
                {
                    ticket += "PAGO CHEQUES\t------>\t\t" + pagos["cheque"].ToString("0.00") + "\n";
                    cambio -= pagos["cheque"];
                }
                if (pagos.ContainsKey("transferencia"))
                {
                    ticket += "PAGO TRANSFERENCIA\t------>\t\t" + pagos["transferencia"].ToString("0.00") + "\n";
                    cambio -= pagos["transferencia"];
                }
                if (pagos.ContainsKey("efectivo"))
                {
                    ticket += "EFECTIVO ENTREGADO\t------>\t\t" + pagos["efectivo"].ToString("0.00") + "\n";
                    cambio -= pagos["efectivo"];
                }

                ticket += "SU CAMBIO $\t------>\t\t" + Math.Abs(cambio).ToString("0.00") + "\n";

                if (!fontName.Equals("Consolas"))
                    ticket += "--------------------";
                ticket += "--------------------------------------------------------------\n\n" +
                     "LE ATENDIO: " + cajero.ToUpper() + "\n" +
                     "NO DE ARTICULOS: " + productos.Count.ToString().PadLeft(5, '0') + "\n" +
                     "GRACIAS POR SU COMPRA\n\n";
                     //"ANTONIO CEJA MARON\n" +                    
                
                // Agregar RFC desde configuración
                string rfc = Settings.Default["rfc"].ToString();
                if (!string.IsNullOrEmpty(rfc))
                {
                    ticket += "RFC: " + rfc + "\n\n";
                }

                if (piedeticket != "")
                {
                    ticket += "----------------------------------------------------------------------------------\n" +
                    piedeticket + "\n" +
                    "----------------------------------------------------------------------------------\n\n";
                }

                ticket += "SI DESEA FACTURAR ESTA COMPRA INGRESE A :\n" +
                     "https://cm-papeleria.com/public/facturacion";
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
                PaperSize ticketSize = new PaperSize("Ticket", 500, 1169);
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

            // Establecemos la ruta y configuramos el control de vista previa de impresión.
            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "reimprimirVenta.txt");
            this.printPreviewControl1.Document = docToPrint;

            // Ajustamos el Zoom del control según el tamaño de fuente (estas condiciones son de ejemplo)
            this.printPreviewControl1.Zoom = 2;
            if (fontSize > 6)
                this.printPreviewControl1.Zoom = 1.5;
            if (fontSize > 10)
                this.printPreviewControl1.Zoom = 1.1;
            if (fontSize > 13)
                this.printPreviewControl1.Zoom = 1.0;

            // Definimos el nombre del documento y seleccionamos la impresora según la configuración
            this.printPreviewControl1.Document.DocumentName = path;
            this.printPreviewControl1.Document.PrinterSettings.PrinterName = localDM.impresora;

            // Utilizamos anti alias para suavizar las fuentes
            this.printPreviewControl1.UseAntiAlias = true;

            // Asociamos el evento PrintPage para el documento
            this.docToPrint.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(docToPrint_PrintPage);
        }

        private void docToPrint_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            // Contenido del ticket (por ejemplo, variable 'ticket')
            string text1 = ticket;

            // Creamos la fuente con el nombre y tamaño configurados
            FontFamily fontFamily = new FontFamily(fontName);
            Font font = new Font(fontFamily, fontSize, FontStyle.Regular, GraphicsUnit.Point);

            // Usamos e.MarginBounds para definir el área de impresión según la configuración del tamaño de papel y márgenes
            Rectangle rect = e.MarginBounds;

            // Configuramos el formato de la cadena para gestionar tabulaciones (si aplica)
            StringFormat stringFormat = new StringFormat();
            stringFormat.SetTabStops(0, tabs[fontSize]);

            // Usamos un pincel sólido para dibujar el texto en negro
            SolidBrush solidBrush = new SolidBrush(Color.Black);

            // Dibujamos el contenido en el área definida
            e.Graphics.DrawString(text1, font, solidBrush, rect, stringFormat);
        }


        private void tabla_KeyDown(object sender, KeyEventArgs e)
        {
            //loadTicket(); // carga la vista previa en automatico al moverse con las flechas.
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    //print.PerformClick();
                    // se comento para que no se imprima automaticamente al hacer enter en la tabla de ventas.
                    loadTicket(); // carga la vista previa del ticket al hacer Enter en la tabla de ventas.
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
                case Keys.F1:
                    txtbuscar.Focus();
                    break;
            }
        }

        private void tabla_MouseClick(object sender, MouseEventArgs e)
        {           
            loadTicket(); // carga la vista previa del ticket al hacer click en la tabla de ventas.            
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
                loadTicket(); // carga la vista previa del ticket al hacer Enter en el buscador.               
                e.Handled = true;
                e.SuppressKeyPress = true;
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
                    case Keys.F1:
                        txtbuscar.Focus();
                        break;
                    case Keys.F5:
                        print.PerformClick();
                        break;
                    case Keys.F6:
                        delete.PerformClick();
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

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tabla_SelectionChanged(object sender, EventArgs e)
        {
            // esto se cambio para que no se cargue el ticket automaticamente al seleccionar una fila en la tabla de ventas
            //Asi solo muestra el primer ticket al cargar la ventana y despues solo se muestra al hacer click o enter en la tabla.
            if (!firstTicket)
            {
                loadTicket();
                firstTicket = true;
            }
        }

        private void delete_Click(object sender, EventArgs e)
        {
            DialogResult response = MessageBox.Show("¿Está seguro que desea limpiar el historial de ventas?", "Advertencia", MessageBoxButtons.YesNo);
            if(response == DialogResult.Yes)
            {
                localDM.limpiarVentas();
                loadData();
            }
        }

        private void tabla_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void filtrarFecha_ValueChanged(object sender, EventArgs e)
        {
            //Console.WriteLine(filtrarFecha.Value.ToString("dd/MM/yyyy"));   
            loadData();
        }

        private void print_Click(object sender, EventArgs e)
        {
            if (isValidTicket)
            {
                try
                {
                    if (printerType == 1)
                        printPreviewControl1.Document.Print();
                    else
                    {
                        // ENRIQUECER los productos con información completa de descuentos (IGUAL QUE EN loadTicket)
                        double subtotalSinDescuentos = 0;
                        double totalDescuentoCategoria = 0;
                        double totalDescuentoPrecioEspecial = 0;

                        foreach (ProductoVenta p in productos)
                        {
                            Producto productoCompleto = localDM.GetProductByCode(p.codigo);
                            if (productoCompleto != null)
                            {
                                // ESTABLECER precio original
                                p.precio_original = productoCompleto.menudeo;

                                // VERIFICAR si tiene descuento de categoría
                                var (tieneDescuentoCategoria, porcentajeCategoria) = localDM.GetDescuentoCategoria(productoCompleto.categoria_id);

                                if (tieneDescuentoCategoria && porcentajeCategoria > 0)
                                {
                                    p.es_descuento_categoria = true;
                                    p.porcentaje_descuento_categoria = (double)porcentajeCategoria;

                                    // Calcular precio sin descuento de categoría
                                    double precioSinDescuentoCategoria = p.precio_venta / (1 - (double)porcentajeCategoria / 100.0);
                                    p.descuento_categoria_unitario = precioSinDescuentoCategoria - p.precio_venta;

                                    // VERIFICAR precio especial sobre el precio sin descuento de categoría
                                    if (precioSinDescuentoCategoria > productoCompleto.menudeo && productoCompleto.especial > 0 &&
                                        Math.Abs(precioSinDescuentoCategoria - productoCompleto.especial) < 0.01)
                                    {
                                        // Tiene precio especial Y descuento de categoría
                                        p.es_precio_especial = true;
                                        p.descuento_unitario = productoCompleto.menudeo - productoCompleto.especial;
                                        p.precio_original = productoCompleto.menudeo;
                                    }
                                    else
                                    {
                                        // Solo descuento de categoría
                                        p.es_precio_especial = false;
                                        p.descuento_unitario = 0;
                                        p.precio_original = precioSinDescuentoCategoria;
                                    }
                                }
                                else
                                {
                                    // NO tiene descuento de categoría, verificar solo precio especial
                                    p.es_descuento_categoria = false;
                                    p.descuento_categoria_unitario = 0;
                                    p.porcentaje_descuento_categoria = 0;

                                    if (p.precio_venta < productoCompleto.menudeo)
                                    {
                                        p.es_precio_especial = true;
                                        p.descuento_unitario = productoCompleto.menudeo - p.precio_venta;
                                    }
                                    else
                                    {
                                        p.es_precio_especial = false;
                                        p.descuento_unitario = 0;
                                    }
                                }
                            }
                            else
                            {
                                // Producto no encontrado, usar valores por defecto
                                p.precio_original = p.precio_venta;
                                p.es_precio_especial = false;
                                p.descuento_unitario = 0;
                                p.es_descuento_categoria = false;
                                p.descuento_categoria_unitario = 0;
                                p.porcentaje_descuento_categoria = 0;
                            }

                            // ACUMULAR totales
                            subtotalSinDescuentos += p.precio_original * p.cantidad;

                            if (p.es_descuento_categoria)
                            {
                                totalDescuentoCategoria += p.descuento_categoria_unitario * p.cantidad;
                            }

                            if (p.es_precio_especial)
                            {
                                totalDescuentoPrecioEspecial += p.descuento_unitario * p.cantidad;
                            }
                        }

                        // CALCULAR DESCUENTO DE VENTA
                        double descuentoTotalBD = double.Parse(tabla.SelectedRows[0].Cells[2].Value.ToString());
                        double descuentoTotalCalculado = totalDescuentoCategoria + totalDescuentoPrecioEspecial;
                        double descuentoVenta = Math.Max(0, descuentoTotalBD - descuentoTotalCalculado);

                        bool esDescuento = descuentoVenta > 0;

                        Dictionary<string, string> venta = new Dictionary<string, string>();
                        venta["fecha_venta"] = fecha;
                        venta["folio"] = folio;
                        venta["total"] = total;

                        // CALCULAR cambio correcto
                        double totalFinalConTodosLosDescuentos = subtotalSinDescuentos - totalDescuentoCategoria - totalDescuentoPrecioEspecial - descuentoVenta;
                        cambio = totalFinalConTodosLosDescuentos;

                        // LLAMAR al método de impresión con toda la información
                        localDM.imprimirTicket(venta, productos, pagos, cajero, sucursalName, sucursalDir,
                            true, cambio, esDescuento, descuentoVenta);
                    }
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    MessageBox.Show("No se guardo el PDF, ya se encuentra abierto un documento con el mismo nombre.", "Error");
                }
            }
        }
        }
}
