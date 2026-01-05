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
using Windows.Storage;
using PuntoVentaCasaCeja.Properties;

namespace PuntoVentaCasaCeja
{
    public partial class RegistrarApartado : Form
    {
        WebDataManager webDM;
        LocaldataManager localDM;
        List<ProductoVenta> carrito;
        double totalcarrito;
        int idsucursal, idcaja;
        string folio, sucursalName, sucursalDir;
        DateTime localDate;
        Dictionary<string, double> pagos;
        bool reprint = false;
        double totalpagado = 0;
        int dias = 0;
        Usuario cajero;
        bool isValidTicket = false;
        string ticket;
        string fontName;
        string foliocorte;
        Cliente cliente;
        int fontSize, printerType, idcorte;
        Dictionary<int, float[]> tabs;
        PrintPreviewControl printPreviewControl1;
        private System.Drawing.Printing.PrintDocument docToPrint =
    new System.Drawing.Printing.PrintDocument();
        CurrentData data;
        public RegistrarApartado(CurrentData data)
        {
            InitializeComponent();
            this.data = data;
            this.cliente = data.cliente;
            this.webDM = data.webDM;
            this.localDM = webDM.localDM;
            this.carrito = data.carrito;
            this.totalcarrito = data.totalcarrito;
            this.sucursalName = data.sucursalName;
            this.sucursalDir = data.sucursalDir;
            this.idcorte = data.idCorte;
            idsucursal = int.Parse(Settings.Default["sucursalid"].ToString());
            idcaja = int.Parse(Settings.Default["posid"].ToString());
            this.localDate = DateTime.Now;
            this.fontName = Settings.Default["fontName"].ToString();
            this.fontSize = int.Parse(Settings.Default["fontSize"].ToString());
            printerType = int.Parse(Settings.Default["printertype"].ToString());
            pagos = new Dictionary<string, double>();
            this.cajero = webDM.activeUser;
            printPreviewControl1 = new PrintPreviewControl();
            this.folio = idsucursal.ToString().PadLeft(2, '0') + idcaja.ToString().PadLeft(2, '0') + localDate.Second.ToString().PadLeft(2, '0') + localDate.Day.ToString().PadLeft(2, '0') + localDate.Month.ToString().PadLeft(2, '0') + localDate.Year + "A";
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

        
        async void send(Apartado apartado) //se hizo para probar una excepcion, si jala dejarlo, sino regresar al comentado
        {
            Dictionary<string, string> result = await webDM.SendapartadoAsync(apartado);
            MessageBox.Show(result["message"], "Estado: " + result["status"]);

            if (result["status"] == "success")
            {
                data.successful = true;
                List<ProductoVenta> productos = new List<ProductoVenta>(carrito);

                if (productos == null || productos.Count == 0)
                {
                    MessageBox.Show("El carrito está vacío", "Error");
                    return;
                }

                foreach (ProductoVenta p in productos)
                {
                    await webDM.restarExistencia(idsucursal, p.id, p.cantidad);
                }
            }
            else
            {
                MessageBox.Show("No es posible realizar esta operacion ahora", "Error");
            }
        }

        /*
        async void send(Apartado apartado){
            Dictionary<string, string> result = await webDM.SendapartadoAsync(apartado);
            MessageBox.Show(result["message"], "Estado: " + result["status"]);

            if (result["status"] == "success")
            {   
                data.successful = true;
                List<ProductoVenta> productos = carrito;

                if (productos == null || productos.Count == 0)
                {
                    MessageBox.Show("El carrito está vacío", "Error");
                    return;
                }

                foreach (ProductoVenta p in productos)
                {
                    //Console.WriteLine($"Restando existencia para producto ID: {p.id}, cantidad: {p.cantidad}");
                    await webDM.restarExistencia(idsucursal, p.id, p.cantidad);
                }
            }
            else
            {
                MessageBox.Show("No es posible realizar esta operacion ahora", "Error");
            }
        }
        */

        private void RegistrarApartado_Load(object sender, EventArgs e)
        {
            
            folio = idsucursal.ToString().PadLeft(2, '0') + idcaja.ToString().PadLeft(2, '0') + localDate.Second.ToString().PadLeft(2, '0') + localDate.Day.ToString().PadLeft(2, '0') + localDate.Month.ToString().PadLeft(2, '0') + localDate.Year + "A";
            txtfolio.Text = folio;
            txtnombre.Text = cliente.nombre;
            txttel.Text = cliente.telefono;
            txtcorreo.Text = cliente.correo;
            txttotal.Text = totalcarrito.ToString("0.00");
            txtabonado.Text = "0.00";
            txtpagoentrega.Text = totalcarrito.ToString("0.00");
        }

        private void txtdias_TextChanged(object sender, EventArgs e)
        {
            dias = txtdias.Text.Equals("")?0: int.Parse(txtdias.Text);
            DateTime d = localDate.AddDays(dias);
            txtfecha.Text = d.Day.ToString().PadLeft(2, '0') + "/" + d.Month.ToString().PadLeft(2, '0') + "/" + d.Year;
        }

        private void integerInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
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
            totalpagado += cantidad;            
            txtabonado.Text = totalpagado.ToString("0.00");
            txtpagoentrega.Text = (totalcarrito - totalpagado).ToString("0.00");
        }

        private void abonar_Click(object sender, EventArgs e)
        {
            if (this.txtPorcentajeDesc.Text != "" && this.txtPorcentajeDesc.Text != null && this.txtPorcentajeDesc.Text != "0") {
                if (data.porcentajeDesc != -1)
                {
                    double porcentajeDescuento = double.Parse(this.txtPorcentajeDesc.Text);

                    double descuentoPorcentaje = (porcentajeDescuento * this.totalcarrito) / 100;

                    data.porcentajeDesc = descuentoPorcentaje;
                } 
            }
            MetodoPago mp = new MetodoPago(totalcarrito-totalpagado, abono, data);
            mp.ShowDialog();
            this.txtPorcentajeDesc.Text = "0";
            data.porcentajeDesc = 0;
            //aceptar.PerformClick();
        }

        private void txtfolio_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtdias_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txtdias.Text.Equals(""))
                {
                    MessageBox.Show("Favor de completar los campos requeridos", "Advertencia");
                }
                else
                {
                    abonar.PerformClick();
                }
            }
        }

        private void cancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
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
                    case Keys.Enter:
                        if (aceptar.Focused || cancelar.Focused || abonar.Focused)
                            return base.ProcessDialogKey(keyData);
                        SendKeys.Send("{TAB}");
                        break;
                    case Keys.F5:
                        aceptar.PerformClick();
                        break;
                    case Keys.F6:
                        abonar.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        // =====================================
        // TICKET DE APARTADO CON LÓGICA DE DESCUENTOS
        // Aplicar la misma lógica que el ticket de ventas
        // =====================================

        private void imprimirTicketCarta(string fecha)
        {
            string piedeticket = Settings.Default["pieDeTicket"].ToString();
            ticket = "";
            string caj = data.usuario.nombre;

            ticket += "CASA CEJA\n" +
                "SUCURSAL: " + sucursalName.ToUpper() + "\n" +
                "" + sucursalDir.ToUpper() + "\n" +
                "" + fecha + "\n" +
                "FOLIO: " + folio + "\n" +
                "TICKET DE APARTADO\n\n" +
                 "DESCRIPCION\tCANT\tP. UNIT\tP. TOTAL\n";

            // CALCULAR descuentos para el ticket - IGUAL QUE EN VENTAS
            double subtotalSinDescuentos = 0;
            double descuentoCategoriaTicket = 0;
            double descuentoPrecioEspecialTicket = 0;

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

                // AGREGAR INDICADORES basándose en el estado original - IGUAL QUE EN VENTAS
                string indicadores = "";

                if (p.es_precio_especial)
                {
                    indicadores += "*ESP";
                }

                // MOSTRAR INDICADOR DE CATEGORÍA SI LO TUVO ORIGINALMENTE
                if (p.tuvo_descuento_categoria_original)
                {
                    if (indicadores.Length > 0) indicadores += " ";
                    indicadores += $"*CAT{p.porcentaje_categoria_original:0}%";
                }

                // PRECIO ORIGINAL (siempre menudeo) - IGUAL QUE EN VENTAS
                Producto productoCompleto = localDM.GetProductByCode(p.codigo);
                double precioUnitarioOriginal = productoCompleto?.menudeo ?? p.precio_venta;

                // Si no tiene precio_original establecido, usar menudeo
                if (p.precio_original <= 0 && productoCompleto != null)
                {
                    p.precio_original = productoCompleto.menudeo;
                }

                if (p.precio_original > 0)
                {
                    precioUnitarioOriginal = p.precio_original;
                }

                // PRECIO FINAL 
                double precioFinalConDescuentos = p.precio_venta * p.cantidad;

                // SUBTOTAL SIN DESCUENTOS
                subtotalSinDescuentos += precioUnitarioOriginal * p.cantidad;

                // *** ACUMULAR DESCUENTOS USANDO VALORES ORIGINALES - IGUAL QUE EN VENTAS ***
                if (p.tuvo_descuento_categoria_original)
                {
                    descuentoCategoriaTicket += p.descuento_categoria_original * p.cantidad;
                    Console.WriteLine($"*** APARTADO: {p.nombre} - Categoria original: {p.descuento_categoria_original * p.cantidad}");
                }

                if (p.es_precio_especial)
                {
                    descuentoPrecioEspecialTicket += p.descuento_unitario * p.cantidad;
                }

                ticket += n + indicadores + "\t" + p.cantidad + "\t" + precioUnitarioOriginal.ToString("0.00") + "\t" + precioFinalConDescuentos.ToString("0.00") + "\n";
            }

            Console.WriteLine($"*** APARTADO TOTALES:");
            Console.WriteLine($"  Subtotal sin descuentos: {subtotalSinDescuentos}");
            Console.WriteLine($"  Descuento Categoría: {descuentoCategoriaTicket}");
            Console.WriteLine($"  Descuento Precio Especial: {descuentoPrecioEspecialTicket}");
            Console.WriteLine($"  Descuento adicional (porcentaje): {data.porcentajeDesc}");

            if (!fontName.Equals("Consolas"))
                ticket += "--------------------";
            ticket += "--------------------------------------------------------------\n";

            // MOSTRAR SUBTOTAL - IGUAL QUE EN VENTAS
            ticket += "SUBTOTAL $\t------>\t\t" + subtotalSinDescuentos.ToString("0.00") + "\n";

            // *** MOSTRAR DESCUENTOS USANDO VALORES ORIGINALES - IGUAL QUE EN VENTAS ***
            if (descuentoCategoriaTicket > 0)
            {
                ticket += "DESC. POR CATEGORIA\t------>\t-" + descuentoCategoriaTicket.ToString("0.00") + "\n";
            }

            if (descuentoPrecioEspecialTicket > 0)
            {
                ticket += "DESC. PRECIO ESPECIAL\t------>\t-" + descuentoPrecioEspecialTicket.ToString("0.00") + "\n";
            }

            // DESCUENTO ADICIONAL DE APARTADO (si existe)
            if (data.porcentajeDesc > 0)
            {
                ticket += "DESCUENTO APARTADO\t------>\t-" + data.porcentajeDesc.ToString("0.00") + "\n";
            }

            // *** CALCULAR TOTAL USANDO EL TOTAL ACTUAL DEL CARRITO - IGUAL QUE EN VENTAS ***
            // El totalcarrito YA tiene todos los descuentos aplicados correctamente
            double totalFinalCorrecto = totalcarrito - data.porcentajeDesc;

            Console.WriteLine($"*** APARTADO CÁLCULO TOTAL:");
            Console.WriteLine($"  totalcarrito (con descuentos ya aplicados): {totalcarrito}");
            Console.WriteLine($"  data.porcentajeDesc: {data.porcentajeDesc}");
            Console.WriteLine($"  Total final correcto: {totalFinalCorrecto}");

            ticket += "TOTAL $\t------>\t\t" + totalFinalCorrecto.ToString("0.00") + "\n";

            // MÉTODOS DE PAGO - IGUAL QUE ANTES
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

            if (!fontName.Equals("Consolas"))
                ticket += "--------------------";
            ticket += "--------------------------------------------------------------\n" +
                "POR PAGAR $\t------>\t\t" + (totalFinalCorrecto - totalpagado).ToString("0.00") + "\n\n" +
                 "LE ATENDIO: " + data.usuario.nombre.ToUpper() + "\n" +
                 "NO DE ARTICULOS: " + carrito.Count.ToString().PadLeft(5, '0') + "\n" +
                 "FECHA DE VENCIMIENTO:\n" + txtfecha.Text + "\n" +
                 "CLIENTE:\n" + cliente.nombre + "\n" +
                 "NUMERO DE CELULAR:\n" + cliente.telefono + "\n\n";
                // "ANTONIO CEJA MARON\n" +
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

            createdoc();
        }

        // =====================================
        // ASEGURAR que los productos del carrito tengan la información de descuentos
        // Agregar este método para verificar/establecer descuentos antes de imprimir
        // =====================================

        private void PrepararDescuentosParaTicket()
        {
            Console.WriteLine("=== PREPARANDO DESCUENTOS PARA TICKET DE APARTADO ===");

            foreach (ProductoVenta p in carrito)
            {
                Producto productoCompleto = localDM.GetProductByCode(p.codigo);
                if (productoCompleto != null)
                {
                    // ESTABLECER precio original si no está
                    if (p.precio_original <= 0)
                    {
                        p.precio_original = productoCompleto.menudeo;
                    }

                    // VERIFICAR si tiene descuento de categoría aplicado pero no marcado
                    if (!p.tuvo_descuento_categoria_original)
                    {
                        var (tieneDescuentoCategoria, porcentajeCategoria) = localDM.GetDescuentoCategoria(productoCompleto.categoria_id);

                        if (tieneDescuentoCategoria && porcentajeCategoria > 0)
                        {
                            double precioConCategoria = productoCompleto.menudeo * (1 - (double)porcentajeCategoria / 100.0);

                            // Si el precio actual coincide con precio con categoría, marcarlo
                            if (Math.Abs(p.precio_venta - precioConCategoria) < 0.01)
                            {
                                p.tuvo_descuento_categoria_original = true;
                                p.descuento_categoria_original = productoCompleto.menudeo - p.precio_venta;
                                p.porcentaje_categoria_original = (double)porcentajeCategoria;

                                Console.WriteLine($"*** APARTADO: Detectado descuento categoría en {p.nombre}: {p.descuento_categoria_original}");
                            }
                        }
                    }

                    // VERIFICAR si tiene precio especial aplicado pero no marcado
                    if (!p.es_precio_especial && productoCompleto.especial > 0)
                    {
                        if (Math.Abs(p.precio_venta - productoCompleto.especial) < 0.01)
                        {
                            p.es_precio_especial = true;
                            p.descuento_unitario = productoCompleto.menudeo - productoCompleto.especial;

                            Console.WriteLine($"*** APARTADO: Detectado precio especial en {p.nombre}: {p.descuento_unitario}");
                        }
                    }
                }
            }
        }

        // =====================================
        // MODIFICAR el método aceptar_Click para preparar descuentos antes de imprimir
        // =====================================

        private void aceptar_Click(object sender, EventArgs e)
        {
            if (txtnombre.Text.Equals("") || txtcorreo.Text.Equals("") || txttel.Text.Equals("") || txtdias.Text.Equals(""))
            {
                MessageBox.Show("Favor de completar los campos requeridos", "Advertencia");
            }
            else
            {
                // Verificar si el apartado ya existe antes de intentar guardarlo
                if (!localDM.ApartadoExiste(folio))
                {
                    Apartado na = new Apartado()
                    {
                        productos = JsonConvert.SerializeObject(carrito),
                        total = data.totalcarrito,
                        total_pagado = totalpagado,
                        folio_corte = data.folioCorte,
                        fecha_de_apartado = localDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        estado = 0,
                        cliente_creditos_id = cliente.id,
                        temporal = cliente.activo == -1 ? 1 : 0,
                        id_cajero_registro = webDM.activeUser.id,
                        fecha_entrega = null,
                        id_cajero_entrega = null,
                        sucursal_id = idsucursal,
                        observaciones = txtobservaciones.Text,
                    };
                    na.abonos = new List<AbonoApartado>();

                    if (pagos.Count > 0)
                    {
                        Console.WriteLine("RegistarApartado: " + data.folioCorte);
                        Console.Write(data.totalcarrito);
                        AbonoApartado abono = new AbonoApartado
                        {
                            fecha = localDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            folio = idsucursal.ToString().PadLeft(2, '0') + idcaja.ToString().PadLeft(2, '0') + localDate.Second.ToString().PadLeft(2, '0') + localDate.Day.ToString().PadLeft(2, '0') + localDate.Month.ToString().PadLeft(2, '0') + localDate.Year + "A",
                            folio_corte = data.folioCorte,
                            apartado_id = 0,
                            usuario_id = webDM.activeUser.id,
                            folio_apartado = folio,
                            metodo_pago = JsonConvert.SerializeObject(pagos),
                            total_abonado = totalpagado,
                        };

                        int ida = localDM.abonoApartadoTemporal(abono);
                        na.abonos.Add(abono);

                        localDM.acumularPagos(pagos, idcorte, 0);
                        if (pagos.ContainsKey("efectivo"))
                        {
                            localDM.acumularEfectivoApartado(pagos["efectivo"], idcorte);
                        }
                    }

                    decimal totalCarritoDecimal;
                    if (decimal.TryParse(data.totalcarrito.ToString(), out totalCarritoDecimal))
                    {
                        bool actualizado = localDM.actualizarTotalesCorte(idcorte, totalCarritoDecimal, true);

                        if (actualizado)
                        {
                            Console.WriteLine($"Total de apartados del corte {idcorte} actualizado correctamente.");
                        }
                        else
                        {
                            Console.WriteLine($"Error al actualizar el total de apartados del corte {idcorte}. Revisar logs.");
                            MessageBox.Show("Error interno: No se pudo procesar el total del carrito. Contacte a soporte técnico.", "Error");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error al convertir data.totalcarrito a decimal. Valor: {data.totalcarrito}");
                        MessageBox.Show("Error interno: No se pudo procesar el total del carrito. Contacte a soporte técnico.", "Error");
                        return;
                    }

                    // *** PREPARAR DESCUENTOS ANTES DE IMPRIMIR ***
                    PrepararDescuentosParaTicket();

                    if (printerType == 1)
                    {
                        // Impresora de TAMAÑO CARTA
                        imprimirTicketCarta(localDate.ToString("dd/MM/yyyy hh:mm tt"));
                        imprimirTicketCarta(localDate.ToString("dd/MM/yyyy hh:mm tt"));
                    }
                    else
                    {
                        // Impresora TÉRMICA
                        if (!localDM.impresora.Equals(""))
                        {
                            try
                            {
                                localDM.imprimirApartado(na, carrito, pagos, cajero.nombre, sucursalName, sucursalDir, txtfecha.Text, cliente.nombre, cliente.telefono);
                                localDM.imprimirApartado(na, carrito, pagos, cajero.nombre, sucursalName, sucursalDir, txtfecha.Text, cliente.nombre, cliente.telefono);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error al imprimir en impresora térmica: " + ex.Message);
                            }
                        }
                        else
                        {
                            MessageBox.Show("No se ha configurado una impresora térmica", "Advertencia");
                        }
                    }

                    send(na);
                    this.DialogResult = DialogResult.Yes;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("El apartado ya existe en la base de datos.", "Advertencia");
                }
            }
        }

        private void createdoc() // El codigo anterior esta en VerCredApa.
        {
            // Ruta para guardar el archivo de texto con el ticket
            string path = Path.Combine(Application.StartupPath, "Apartado.txt");

            // Guardar el contenido del ticket en el archivo
            try
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    sw.WriteLine(ticket);  // Escribe el contenido del ticket en el archivo
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el archivo: " + ex.Message);
                return; // Salir del método si ocurre un error al guardar el archivo
            }

            // Configuración de PrintPreviewControl (opcional)
            this.printPreviewControl1.Document = docToPrint;
            this.printPreviewControl1.Zoom = 2;
            if (fontSize > 6) this.printPreviewControl1.Zoom = 1.5;
            if (fontSize > 10) this.printPreviewControl1.Zoom = 1.1;
            if (fontSize > 13) this.printPreviewControl1.Zoom = 1.0;

            // Asignar el nombre del documento y configurar la impresora
            this.docToPrint.DocumentName = path;
            this.docToPrint.PrinterSettings.PrinterName = localDM.impresora;

            // Verificar si el nombre de la impresora es válido
            if (string.IsNullOrEmpty(this.docToPrint.PrinterSettings.PrinterName))
            {
                MessageBox.Show("No se ha configurado una impresora válida.");
                return;
            }

            // Habilitar anti-aliasing en la vista previa
            this.printPreviewControl1.UseAntiAlias = true;

            // Asociar el evento de impresión con el controlador de impresión
            this.docToPrint.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(docToPrint_PrintPage);

            // Intentar imprimir el documento
            try
            {
                this.docToPrint.Print();  // Enviar el documento a la impresora
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar imprimir el documento: " + ex.Message);
            }
        }
        
        private void docToPrint_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            string text1 = ticket;

            // Fuente y formato para el texto
            FontFamily fontFamily = new FontFamily(fontName);
            int fontSize = 8;
            Font font = new Font(
                fontFamily,
                fontSize,
                FontStyle.Regular,
                GraphicsUnit.Point);

            // Ajusta el rectángulo de impresión para una impresora térmica de 80 mm
            Rectangle rect = new Rectangle(10, 10, 350, 1200); // El ancho de 350 es adecuado para 80 mm
            StringFormat stringFormat = new StringFormat();
            SolidBrush solidBrush = new SolidBrush(Color.Black);

            stringFormat.SetTabStops(0, tabs[fontSize]);

            // Dibuja el texto dentro del área ajustada
            e.Graphics.DrawString(text1, font, solidBrush, rect, stringFormat);

            // Asegúrate de que el tamaño de la fuente no exceda el ancho disponible
            if (fontSize > 12)
            {
                e.Graphics.DrawString("Ajuste el tamaño de fuente", font, solidBrush, rect, stringFormat);
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

            // Verificar si el valor ingresado es mayor a 100
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.KeyChar.ToString());
            if (decimal.TryParse(newText, out decimal value) && value > 100)
            {
                e.Handled = true;
                return;
            }
        }

    }
}
