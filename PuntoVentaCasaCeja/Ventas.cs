using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using PuntoVentaCasaCeja.Properties;
using Windows.Storage;
using Firebase.Database;
using System.Reflection;
using System.Data;
using System.Threading.Tasks;

namespace PuntoVentaCasaCeja
{
    public partial class Ventas : Form
    {   
        FirebaseClient firebase;
        private bool mensajeMayoreoMostrado = false;
        bool hasTemporal;
        double totalcarrito = 0;
        double totalpagado = 0;        
        static double apertura = 0;
        static int idcorte = -1;
        static LocaldataManager localDM;
        List<ProductoVenta> carrito;
        static WebDataManager webDM;
        static string folioCorte;
        static List<Ventas> cajas = new List<Ventas>();
        static char cajaActual = '@';
        Dictionary<string, double> pagos;
        Dictionary<string, string> corte;
        List<string> list = new List<string>();
        static Usuario activador = null;
        static Usuario cajero = null;
        static bool desbloqDesc = false;
        static Usuario admin = null;
        bool reprint = false;
        string folio;
        static bool loaded = false;
        public  int idsucursal;
        static int idcaja;
        static string sucursalName;
        static string sucursalDir;
        string ticket;
        string fontName;
        int fontSize;
        int printerType;
        Dictionary<int, float[]> tabs;
        PrintPreviewDialog printPreview = new PrintPreviewDialog();
        PrintPreviewControl printPreviewControl1;
        private System.Drawing.Printing.PrintDocument docToPrint =
        new System.Drawing.Printing.PrintDocument();
        private System.Drawing.Printing.PrintDocument docZToPrint =
        new System.Drawing.Printing.PrintDocument();
        CurrentData data;
        private double totalDescuentoPrecioEspecial = 0; // Acumula descuentos por precio especial
        private double totalDescuentoCategoria = 0;
        
        public Ventas()
        {
            InitializeComponent();
            txtcodigo.GotFocus += txtcodigo_FocusChanged;
            txtcodigo.LostFocus += txtcodigo_FocusChanged;
            localDM = new LocaldataManager();
            webDM = new WebDataManager(localDM, setInt, cajero);
            carrito = new List<ProductoVenta>();
            pagos = new Dictionary<string, double>();
            cajas.Add(this);
            printPreviewControl1 = new PrintPreviewControl();
            getConfig();
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
            idsucursal = int.Parse(Settings.Default["sucursalid"].ToString());
            webDM.sucursal_id = idsucursal;
            (printPreview as Form).WindowState = FormWindowState.Maximized;
            data = new CurrentData
            {
                webDM = webDM,
                sucursalDir = sucursalDir,
                sucursalName = sucursalName,
                folioCorte = folioCorte,
                carrito = carrito,
                totalcarrito = totalcarrito,
                idCaja = idcaja,
                idSucursal = idsucursal,
                fontName = fontName,
                fontSize = fontSize,
                idCorte = idcorte,
                printerType = printerType,
                successful = false,
                usuario = null,
                desbloqDesc = desbloqDesc,
                isventa = false,
                totalDescuentoCategoria = 0,
                totalDescuentoPrecioEspecial = 0
            };
        }

        void setInt(int x)
        {

        }
        void refreshFolio()
        {
            DateTime localDate = DateTime.Now;
            int id = localDM.getLastIdVentas() + 1;
            folio = idsucursal.ToString().PadLeft(2, '0') + idcaja.ToString().PadLeft(2, '0') + localDate.Day.ToString().PadLeft(2, '0') + localDate.Month.ToString().PadLeft(2, '0') + localDate.Year + "V" + id.ToString().PadLeft(4, '0');            
            lblFolio.Text = "FOLIO: " + folio;
        }
        async void reloadData()
        {
            await SyncPendingVentas();
            await SyncPendingCortes();

            this.Enabled = false;
            LoadWindow lw = new LoadWindow();
            lw.Show(this);
            if (await webDM.GetProductos())
            {
                lw.setData(7, "Sincronizando datos desde el servidor...");
                await webDM.GetSucursales();
                lw.setData(14, "Sincronizando datos desde el servidor...");
                await webDM.GetMedidas();
                lw.setData(21, "Sincronizando datos desde el servidor...");
                await webDM.GetCategorias();
                lw.setData(28, "Sincronizando datos desde el servidor...");
                await webDM.GetUsuarios();
                lw.setData(35, "Sincronizando datos desde el servidor...");
                await webDM.GetApartados();
                lw.setData(42, "Sincronizando datos desde el servidor...");
                await webDM.GetCreditos();
                lw.setData(49, "Sincronizando datos desde el servidor...");
                await webDM.GetClientes();
                lw.setData(56, "Sincronizando datos desde el servidor...");
                await webDM.enviarApartadosTemporal();
                lw.setData(63, "Sincronizando datos desde el servidor...");
                await webDM.enviarCreditosTemporal();
                lw.setData(84, "Sincronizando datos desde el servidor...");
                await webDM.GetAbonosApartado();
                lw.setData(100, "Sincronizando datos desde el servidor...");
                await webDM.GetAbonosCredito();
                lw.setData(100, "Sincronizando datos desde el servidor...");
                await webDM.GetCortes();
            }
            else
            {
                lw.Dispose();
                MessageBox.Show("No se pudo conectar con el servidor, favor de intentar más tarde", "Advertencia");


            }
            lw.Dispose();
            this.Enabled = true;
            this.Focus();
            getConfig();
            refreshFolio();
        }
        private async void loadData()
        {
            StartTimer();

            if (loaded == false)
            {
                loaded = true;
                this.Enabled = false;
                LoadWindow lw = new LoadWindow();
                lw.Show(this);

                try
                {
                    // orden optimizado para precarga
                    if (localDM.IsCatalogPreloaded)
                    {
                        Debug.WriteLine("Base de datos precargada detectada. Sincronizando datos esenciales primero...");
                        // 1. Sincronizar datos esenciales primero
                        lw.setData(10, "Sincronizando datos básicos...");
                        await webDM.GetSucursales();
                        await webDM.GetUsuarios();

                        // 2. Actualizar catálogo (solo cambios)
                        lw.setData(30, "Actualizando catálogo...");
                        await webDM.GetProductos();

                        // *** AGREGAR ESTA LÍNEA: Sincronización forzada de categorías para descuentos ***
                        lw.setData(40, "Sincronizando descuentos de categorías...");
                        await webDM.ForzarSincronizacionCategorias();

                        // 3. El resto de sincronizaciones
                        lw.setData(50, "Sincronizando otros datos...");
                        await webDM.GetApartados();
                        await webDM.GetCreditos();
                        await webDM.GetClientes();
                        await webDM.enviarApartadosTemporal();
                        await webDM.enviarCreditosTemporal();
                        await webDM.GetAbonosApartado();
                        await webDM.GetAbonosCredito();
                        await webDM.GetCortes();
                    }
                    else
                    {
                        Debug.WriteLine("No se detectó base de datos precargada. Sincronizando datos desde el servidor...");
                        // Flujo tradicional para instalaciones sin precarga (YA INCLUYE GetCategorias)
                        if (await webDM.GetProductos())
                        {
                            lw.setData(7, "Sincronizando datos desde el servidor...");
                            await webDM.GetSucursales();
                            lw.setData(14, "Sincronizando datos desde el servidor...");
                            await webDM.GetMedidas();
                            lw.setData(21, "Sincronizando datos desde el servidor...");
                            await webDM.GetCategorias(); // ? ESTO YA ESTÁ
                            lw.setData(28, "Sincronizando datos desde el servidor...");
                            await webDM.GetUsuarios();
                            lw.setData(35, "Sincronizando datos desde el servidor...");
                            await webDM.GetApartados();
                            lw.setData(42, "Sincronizando datos desde el servidor...");
                            await webDM.GetCreditos();
                            lw.setData(49, "Sincronizando datos desde el servidor...");
                            await webDM.GetClientes();
                            lw.setData(56, "Sincronizando datos desde el servidor...");
                            await webDM.enviarApartadosTemporal();
                            lw.setData(63, "Sincronizando datos desde el servidor...");
                            await webDM.enviarCreditosTemporal();
                            lw.setData(84, "Sincronizando datos desde el servidor...");
                            await webDM.GetAbonosApartado();
                            lw.setData(100, "Sincronizando datos desde el servidor...");
                            await webDM.GetAbonosCredito();
                            lw.setData(100, "Sincronizando datos desde el servidor...");
                            await webDM.GetCortes();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en carga inicial: {ex.Message}");
                    MessageBox.Show("Error al sincronizar datos. Por favor revise su conexión.");
                }
                finally
                {
                    lw.Dispose();
                    this.Enabled = true;
                    this.Focus();
                }
            }

            getConfig();
            refreshFolio();
            cajaActual++;
            this.Text = "Cobranza " + cajaActual;
            lblcobranza.Text = "Cobranza " + cajaActual;
            txttotal.Text = "Por pagar MXN: $0.00";
            if (idcorte == -1)
            {
                idcorte = localDM.getCajaAbierta();
                if (idcorte == -1)
                {
                    if (activador == null)
                    {
                        if  (!pedirAutorizacion())
                        {
                            Application.Exit();
                            return;
                        }
                    }
                    if (pedirApertura())
                    {
                        idcorte = localDM.crearCorte(apertura, idsucursal);
                        
                    }
                    else {
                        Application.Exit();
                        return;
                    }
                    
                }
            }
            folioCorte = localDM.getFolioCorte(idcorte);
            if (cajero == null)
            {
                iniciarSesion();
            }
           
            tabla.Focus();

        }
        /*void startFirebase()
        {
            firebase = new FirebaseClient("https://papeleria-8d415-default-rtdb.firebaseio.com/");

            var observable = firebase
          .Child("variables")
          .AsObservable<string>()
          .Subscribe(d => startCCSync());

        }*/
        async void startCCSync()
        {
            var runningProcessByName = Process.GetProcessesByName("CCSync");
            if (runningProcessByName.Length == 0)
            {
                try
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), @"CCSync/CCSync.exe");
                    var process = Process.Start(path);
                    process.WaitForExit(60000);
                }
                catch (Exception e)
                {

                }
            }
        }
        void cerrarSesion()
        {
            cajero = null;
            MessageBox.Show("Sesión cerrada, esperando inicio de sesión", "Éxito");
            iniciarSesion();
        }
        void iniciarSesion()
        {
            UserLogin login = new UserLogin(localDM, setCajero, false);
            DialogResult response = login.ShowDialog();
            if (response != DialogResult.Yes)
            {
                for (int i = cajas.Count - 1; i >= 0; i--)
                {
                    cajas[i].Close();
                }
            }
            else
            {
                if (cajero.es_raiz <= 1) //aqui se configura para que los admin tengan acceso a todo
                {
                    ingresarEfectivoF3ToolStripMenuItem.Enabled = true;
                    retirarEfectivoF4ToolStripMenuItem.Enabled = true;                    
                    corteZF7ToolStripMenuItem.Enabled = true;
                    establecerImpresoraToolStripMenuItem.Enabled = true;
                    //administrarCatálogoToolStripMenuItem.Enabled = true;
                    actualizarBaseDeDatosToolStripMenuItem.Enabled = true;
                    historialDeCortesToolStripMenuItem.Enabled = true;
                    adminUsuariosToolStripMenuItem.Enabled = true;
                }
                else
                {
                    //aqui se configura para desactivar acceso a los usuarios generales
                    ingresarEfectivoF3ToolStripMenuItem.Enabled = false;
                    retirarEfectivoF4ToolStripMenuItem.Enabled = false;
                    corteZF7ToolStripMenuItem.Enabled = false;
                    establecerImpresoraToolStripMenuItem.Enabled = true;
                    //administrarCatálogoToolStripMenuItem.Enabled = false;
                    //actualizarBaseDeDatosToolStripMenuItem.Enabled = false;
                    historialDeCortesToolStripMenuItem.Enabled = false;
                    adminUsuariosToolStripMenuItem.Enabled = false;
                }
            }
            data.usuario = cajero;
        }
        System.Windows.Forms.Timer t = null;
        private void StartTimer()
        {
            t = new System.Windows.Forms.Timer();
            t.Interval = 1000;
            t.Tick += new EventHandler(t_Tick);
            t.Enabled = true;
        }

        void t_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            loadData();

        }
        void getConfig()
        {
            idsucursal = int.Parse(Settings.Default["sucursalid"].ToString());
            webDM.sucursal_id = idsucursal;
            idcaja = int.Parse(Settings.Default["posid"].ToString());
            localDM.setImpresora(Settings.Default["printername"].ToString());
            sucursalName = localDM.getSucursalname(idsucursal);
            sucursalDir = localDM.getSucursalAddr(idsucursal);
            fontName = Settings.Default["fontName"].ToString();
            fontSize = int.Parse(Settings.Default["fontSize"].ToString());
            printerType = int.Parse(Settings.Default["printertype"].ToString());
        }
        bool pedirAutorizacion()
        {
            UserLogin login = new UserLogin(localDM, setActivador, true);
            DialogResult response = login.ShowDialog();
            if (response == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                this.Dispose();
                return false;
            }
        }
        void setActivador(Usuario usuario)
        {
            activador = usuario;
        }
        void setCajero(Usuario usuario)
        {
            cajero = usuario;
            webDM.activeUser = usuario;
        }
        bool pedirApertura()
        {
            IngresarMonto im = new IngresarMonto(0, abono, setApertura, 0, data);
            DialogResult response = im.ShowDialog();
            if (response == DialogResult.No)
            {
                this.Dispose();
                return false;
            }
            else if (response != DialogResult.Yes)
            {
                MessageBox.Show("Favor de ingresar el monto de apertura antes de continuar", "Advertencia");
                return pedirApertura();
            }
            return true;
        }
        public void setApertura(double monto)
        {
            apertura = monto;
        }

        private void agregarProducto(Producto p)
        {
            int index = list.IndexOf(p.codigo);
            if (index != -1)
            {
                int cantidadAnterior = carrito[index].cantidad;
                tabla.Rows[index].Cells[1].Value = int.Parse(tabla.Rows[index].Cells[1].Value.ToString()) + 1;
                carrito[index].cantidad++;

                // RECALCULAR descuento si tiene precio especial aplicado
                if (carrito[index].es_precio_especial)
                {
                    double descuentoAnterior = carrito[index].descuento_unitario * (carrito[index].cantidad - 1);
                    double descuentoNuevo = carrito[index].descuento_unitario * carrito[index].cantidad;
                    totalDescuentoPrecioEspecial = totalDescuentoPrecioEspecial - descuentoAnterior + descuentoNuevo;
                }

                // RECALCULAR descuento de categoría
                RecalcularDescuentoCategoria(index, cantidadAnterior, carrito[index].cantidad);

                if (p.cantidad_mayoreo != 0 && carrito[index].cantidad == p.cantidad_mayoreo && p.mayoreo > 0)
                {
                    if (!mensajeMayoreoMostrado)
                    {
                        MessageBox.Show("Precio de Mayoreo");
                        mensajeMayoreoMostrado = true;
                    }
                    carrito[index].precio_venta = Math.Round(p.mayoreo, 2);
                    tabla["precio", index].Value = p.mayoreo.ToString("0.00");

                    // REAPLICAR descuento de categoría después del cambio a mayoreo
                    if (carrito[index].es_descuento_categoria)
                    {
                        // Quitar el descuento anterior
                        double descuentoAnterior = carrito[index].descuento_categoria_unitario * carrito[index].cantidad;
                        totalDescuentoCategoria -= descuentoAnterior;
                        carrito[index].es_descuento_categoria = false;

                        // Reaplicar con el nuevo precio
                        VerificarYAplicarDescuentoCategoria(index);
                    }
                }
                tabla["total", index].Value = (carrito[index].cantidad * carrito[index].precio_venta).ToString("0.00");
            }
            else
            {
                list.Insert(0, p.codigo);
                carrito.Insert(0, new ProductoVenta
                {
                    id = p.id,
                    codigo = p.codigo,
                    nombre = p.nombre,
                    cantidad = 1,
                    precio_venta = Math.Round(p.menudeo, 2),
                    precio_original = Math.Round(p.menudeo, 2),
                    es_precio_especial = false,
                    descuento_unitario = 0,
                    // INICIALIZAR NUEVOS CAMPOS DE CATEGORÍA
                    es_descuento_categoria = false,
                    descuento_categoria_unitario = 0,
                    porcentaje_descuento_categoria = 0
                });
                tabla.Rows.Insert(0, new object[]{
                p.nombre.ToUpper().ToString() + " " + p.presentacion.ToUpper().ToString(),1,
                p.menudeo.ToString("0.00"),
                p.menudeo.ToString("0.00")
                });
                tabla.Rows[0].Selected = true;

                // VERIFICAR Y APLICAR DESCUENTO DE CATEGORÍA AL NUEVO PRODUCTO
                VerificarYAplicarDescuentoCategoria(0);

                txtcodigo.Focus();
            }

            if (p.id == 0)
            {
                hasTemporal = true;
            }

            RecalcularTotales();
        }

        private void modCant(int index, int cantidad)
        {
            Producto p = localDM.GetProductByCode(list[index]);
            int cantidadAnterior = carrito[index].cantidad;

            // Si tiene precio especial, actualizar descuentos
            if (carrito[index].es_precio_especial)
            {
                double descuentoAnterior = carrito[index].descuento_unitario * carrito[index].cantidad;
                double descuentoNuevo = carrito[index].descuento_unitario * cantidad;
                totalDescuentoPrecioEspecial = totalDescuentoPrecioEspecial - descuentoAnterior + descuentoNuevo;
            }

            // RECALCULAR descuento de categoría
            RecalcularDescuentoCategoria(index, cantidadAnterior, cantidad);

            carrito[index].cantidad = cantidad;
            tabla[1, index].Value = cantidad;

            if (cantidad >= p.cantidad_mayoreo && p.cantidad_mayoreo != 0 && p.mayoreo > 0)
            {
                if (!mensajeMayoreoMostrado)
                {
                    MessageBox.Show("Precio de Mayoreo");
                    mensajeMayoreoMostrado = true;
                }
                carrito[index].precio_venta = Math.Round(p.mayoreo, 2);
                tabla[2, index].Value = p.mayoreo.ToString("0.00");

                // REAPLICAR descuento de categoría después del cambio a mayoreo
                if (carrito[index].es_descuento_categoria)
                {
                    double descuentoAnterior = carrito[index].descuento_categoria_unitario * carrito[index].cantidad;
                    totalDescuentoCategoria -= descuentoAnterior;
                    carrito[index].es_descuento_categoria = false;
                    VerificarYAplicarDescuentoCategoria(index);
                }
            }
            else
            {
                if (!carrito[index].es_precio_especial)
                {
                    carrito[index].precio_venta = Math.Round(p.menudeo, 2);
                    tabla[2, index].Value = p.menudeo.ToString("0.00");

                    // REAPLICAR descuento de categoría al volver a menudeo
                    if (carrito[index].es_descuento_categoria)
                    {
                        double descuentoAnterior = carrito[index].descuento_categoria_unitario * carrito[index].cantidad;
                        totalDescuentoCategoria -= descuentoAnterior;
                        carrito[index].es_descuento_categoria = false;
                        VerificarYAplicarDescuentoCategoria(index);
                    }
                }
                if (mensajeMayoreoMostrado && cantidad < p.cantidad_mayoreo)
                {
                    mensajeMayoreoMostrado = false;
                }
            }
            tabla["total", index].Value = (carrito[index].cantidad * carrito[index].precio_venta).ToString("0.00");

            RecalcularTotales();
        }

        private void VerificarYAplicarDescuentoCategoria(int index)
        {
            try
            {
                ProductoVenta productoVenta = carrito[index];
                Producto producto = localDM.GetProductByCode(productoVenta.codigo);

                if (producto == null) return;

                // Obtener información del descuento de la categoría
                var (tieneDescuento, porcentajeDescuento) = localDM.GetDescuentoCategoria(producto.categoria_id);

                if (tieneDescuento && porcentajeDescuento > 0)
                {
                    double porcentajeDescuentoDouble = (double)porcentajeDescuento;
                    double precioBase = productoVenta.precio_venta;
                    double descuentoUnitario = precioBase * (porcentajeDescuentoDouble / 100.0);
                    double nuevoPrecio = precioBase - descuentoUnitario;

                    // Aplicar descuento
                    productoVenta.precio_venta = Math.Round(nuevoPrecio, 2);
                    productoVenta.es_descuento_categoria = true;
                    productoVenta.descuento_categoria_unitario = Math.Round(descuentoUnitario, 2);
                    productoVenta.porcentaje_descuento_categoria = porcentajeDescuentoDouble;

                    // *** GUARDAR ESTADO ORIGINAL ***
                    productoVenta.tuvo_descuento_categoria_original = true;
                    productoVenta.descuento_categoria_original = Math.Round(descuentoUnitario, 2);
                    productoVenta.porcentaje_categoria_original = porcentajeDescuentoDouble;

                    // Actualizar total de descuentos de categoría
                    double descuentoTotal = descuentoUnitario * productoVenta.cantidad;
                    totalDescuentoCategoria += descuentoTotal;

                    Console.WriteLine($"*** CATEGORÍA APLICADA Y GUARDADA: {productoVenta.nombre}");
                    Console.WriteLine($"  Original guardado: {productoVenta.descuento_categoria_original}");

                    // Actualizar tabla visual
                    tabla[2, index].Value = productoVenta.precio_venta.ToString("0.00");
                    tabla[3, index].Value = (productoVenta.cantidad * productoVenta.precio_venta).ToString("0.00");

                    ActualizarColorProducto(index);
                    MessageBox.Show($"Descuento de categoría aplicado: {porcentajeDescuentoDouble}% - Descuento unitario: ${descuentoUnitario:0.00}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al aplicar descuento de categoría: {ex.Message}");
            }
        }

        private void RecalcularDescuentoCategoria(int index, int cantidadAnterior, int cantidadNueva)
        {
            ProductoVenta productoVenta = carrito[index];

            if (productoVenta.es_descuento_categoria)
            {
                // Restar el descuento anterior
                double descuentoAnterior = productoVenta.descuento_categoria_unitario * cantidadAnterior;
                totalDescuentoCategoria -= descuentoAnterior;

                // Sumar el nuevo descuento
                double descuentoNuevo = productoVenta.descuento_categoria_unitario * cantidadNueva;
                totalDescuentoCategoria += descuentoNuevo;

                Console.WriteLine($"Descuento de categoría recalculado: Anterior: ${descuentoAnterior:0.00}, Nuevo: ${descuentoNuevo:0.00}");
            }
        }

        double GetTotal()
        {
            double result = 0;
            foreach (ProductoVenta producto in carrito)
            {
                result += producto.precio_venta * producto.cantidad;
            }
            return result;
        }
        private void btn4_Click_1(object sender, EventArgs e)
        {
            if (carrito.Count > 0)
            {
                DialogResult res = MessageBox.Show("Aún hay productos en el carrito, ¿está seguro que desea salir?", "Advertencia", MessageBoxButtons.OKCancel);
                if (res == DialogResult.OK)
                {
                    this.Dispose();
                }
            }
            else
                this.Dispose();
        }
        private void borrar()
        {
            if (tabla.SelectedRows.Count > 0)
            {
                int index = tabla.SelectedRows[0].Cells[0].RowIndex;

                // SOLUCIÓN PROBLEMA 2: Restar descuentos antes de eliminar el producto
                ProductoVenta productoAEliminar = carrito[index];

                // Restar descuento de precio especial si lo tiene
                if (productoAEliminar.es_precio_especial)
                {
                    double descuentoEspecialTotal = productoAEliminar.descuento_unitario * productoAEliminar.cantidad;
                    totalDescuentoPrecioEspecial -= descuentoEspecialTotal;
                    Console.WriteLine($"Restando descuento precio especial: ${descuentoEspecialTotal:0.00}");
                }

                // Restar descuento de categoría si lo tiene
                if (productoAEliminar.es_descuento_categoria)
                {
                    double descuentoCategoriaTotal = productoAEliminar.descuento_categoria_unitario * productoAEliminar.cantidad;
                    totalDescuentoCategoria -= descuentoCategoriaTotal;
                    Console.WriteLine($"Restando descuento categoría: ${descuentoCategoriaTotal:0.00}");
                }

                // Eliminar producto del carrito y tabla
                list.Remove(list[index]);
                carrito.Remove(carrito[index]);
                tabla.Rows.Remove(tabla.SelectedRows[0]);

                // Recalcular totales
                RecalcularTotales();
            }
            else
            {
                MessageBox.Show("Favor de seleccionar un artículo", "Advertencia");
            }
        }

        // ===========================================
        // MODIFICACIÓN: completarVenta - INCLUIR DESCUENTOS PRECIO ESPECIAL
        // ===========================================
        // =====================================
        // SOLUCIÓN SIMPLE: Solo mover resetVenta() al final
        // MODIFICAR el método completarVenta() 
        // =====================================

        private async void completarVenta()
        {
            Console.WriteLine("=== INICIO completarVenta ===");
            Console.WriteLine($"ANTES de crear venta - totalDescuentoCategoria: {totalDescuentoCategoria}");
            Console.WriteLine($"ANTES de crear venta - totalDescuentoPrecioEspecial: {totalDescuentoPrecioEspecial}");

            sucursalName = localDM.getSucursalname(idsucursal);
            sucursalDir = localDM.getSucursalAddr(idsucursal);
            DateTime localDate = DateTime.Now;
            double cambio = (totalcarrito - data.descuento);

            string fechaVentaImpresion = localDate.ToString("dd/MM/yyyy hh:mm tt");

            Dictionary<string, string> venta = new Dictionary<string, string>();
            venta["total"] = totalcarrito.ToString("0.00");

            // CALCULAR DESCUENTO TOTAL (todos los tipos de descuento)
            double descuentoTotal = data.descuento + totalDescuentoPrecioEspecial + totalDescuentoCategoria;
            venta["descuento"] = descuentoTotal.ToString("0.00");

            venta["folio"] = folio;
            venta["folio_corte"] = folioCorte;
            venta["fecha_venta"] = localDate.ToString("yyyy-MM-dd HH:mm:ss.fff");
            venta["metodo_pago"] = JsonConvert.SerializeObject(pagos);
            venta["tipo"] = 1.ToString();
            venta["sucursal_id"] = idsucursal.ToString();
            venta["usuario_id"] = cajero.id.ToString();

            Console.WriteLine($"ANTES de CrearVenta - totalDescuentoCategoria: {totalDescuentoCategoria}");
            int id = localDM.CrearVenta(venta, carrito);
            Console.WriteLine($"DESPUÉS de CrearVenta - totalDescuentoCategoria: {totalDescuentoCategoria}");

            Console.WriteLine($"ANTES de imprimirTicketCarta - totalDescuentoCategoria: {totalDescuentoCategoria}");
            imprimirTicketCarta(fechaVentaImpresion);
            Console.WriteLine($"DESPUÉS de imprimirTicketCarta - totalDescuentoCategoria: {totalDescuentoCategoria}");

            if (localDM.impresora.Equals(""))
            {
                MessageBox.Show("No se ha establecido una impresora", "Advertencia");
            }
            else
            {
                try
                {
                    if (printerType == 1)
                    {
                        printPreviewControl1.Document.Print();
                        if (reprint)
                        {
                            printPreviewControl1.Document.Print();
                        }
                    }
                    else
                    {
                        localDM.imprimirTicket(venta, carrito, pagos, cajero.nombre, sucursalName, sucursalDir, false, cambio, data.esDescuento, data.descuento);
                    }
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    MessageBox.Show("No se guardo el PDF, ya se encuentra abierto un documento con el mismo nombre.", "Error");
                }
            }
            await send(venta, id);
            resetVenta();
            Console.WriteLine("=== FIN completarVenta ===");
        }

        void resetVenta()
        {
            reprint = false;
            totalcarrito = 0;
            data.esDescuento = false;
            Bdescuento.Enabled = true;
            data.isventa = false;
            data.descuento = 0;
            data.totalabonado = 0;
            tabla.Rows.Clear();
            list.Clear();
            totalpagado = 0;
            carrito.Clear();
            refreshFolio();
            txttotal.Text = "Por pagar MXN: $" + totalcarrito.ToString("0.00");
            pagos.Clear();

            // RESETEAR TODAS LAS VARIABLES DE DESCUENTO
            totalDescuentoPrecioEspecial = 0;
            totalDescuentoCategoria = 0;
            mensajeMayoreoMostrado = false;

            Console.WriteLine("*** Reset completado");
        }

        public async Task send(Dictionary<string, string> venta, int id)
        {   
            
            Dictionary<string, string> temp = new Dictionary<string, string>();
            foreach(var x in venta)
            {
                temp[x.Key] = x.Value;
            }

            if (await webDM.SendVentaAsync(temp, hasTemporal, id))
            {
                localDM.changeEstadoVenta(id, 2, "Enviado");
                List<ProductoVenta> productos = localDM.getCarrito(id);
                foreach(ProductoVenta p in productos)
                {
                   await webDM.restarExistencia(data.idSucursal, p.id, p.cantidad);
                   
                }
            }
        }
        public async Task SyncPendingVentas()
        {
            DataTable ventasPendientes = localDM.getVentasPendientes();

            foreach (DataRow ventaRow in ventasPendientes.Rows)
            {
                int ventaId = Convert.ToInt32(ventaRow["id"]);

                Dictionary<string, string> venta = new Dictionary<string, string>();

                foreach (DataColumn column in ventasPendientes.Columns)
                {
                    if (column.ColumnName != "id") 
                    {
                        venta[column.ColumnName] = ventaRow[column].ToString();
                    }
                }
                await send(venta, ventaId);
            }
        }
        private void btn1_Click(object sender, EventArgs e)
        {
            if (tabla.SelectedRows.Count > 0)
            {
                CambiarCantidad modcant = new CambiarCantidad(tabla.SelectedRows[0].Cells[1].Value.ToString(), tabla.SelectedRows[0].Cells[1].RowIndex, modCant);
                modcant.ShowDialog();
            }

        }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            if ((keyData & Keys.Control) == Keys.Control)
            {
               //cambiarVentana(key);
                return true;
            }
            if ((keyData & Keys.Alt) == Keys.Alt)
            {
                if (key == Keys.F4)
                {                    
                    btn4_Click_1(null, null);
                    return true;
                }
            }
            if ((keyData & Keys.Shift) == Keys.Shift)
                {
                    if (key == Keys.F5)
                    {
                        eliminarCarrito_button.PerformClick();
                        return true;
                    }
                }
            if ((keyData & Keys.Shift) == Keys.Shift)
            {
                if (key == Keys.P)
                {
                    TogglePrecioEspecial();
                    return true;
                }
            }
            else
            {
                switch (key)
                {
                    case Keys.F1:
                        txtcodigo.Focus();
                        break;
                    case Keys.F2:
                        modcant.PerformClick();
                        break;
                    case Keys.F3:
                        existencia.PerformClick();
                        tabla.Focus();
                        break;
                    case Keys.F4:
                        nuevacaja.PerformClick();
                        break;
                    case Keys.F5:
                        ingresarEfectivoF3ToolStripMenuItem.PerformClick();
                        break;
                    case Keys.F6:
                        retirarEfectivoF4ToolStripMenuItem.PerformClick();
                        break;
                    case Keys.F7:
                        corteZF7ToolStripMenuItem.PerformClick();
                        break;
                    case Keys.Down:
                        tabla.Focus();
                        SendKeys.Send("{DOWN}");
                        break;
                    case Keys.Up:
                        tabla.Focus();
                        SendKeys.Send("{UP}");
                        break;
                    case Keys.F11:
                        abonar.PerformClick();
                        break;
                    case Keys.F8:
                        reimprimirTicketF6ToolStripMenuItem.PerformClick();
                        break;
                        break;
                    case Keys.F10:
                        establecerImpresoraToolStripMenuItem.PerformClick();
                        break;
                    case Keys.F12:
                        apartados.PerformClick();
                        break;                    
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
        void cambiarVentana(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F1:
                    cajas[0].Focus();
                    break;
                case Keys.F2:
                if (cajaActual > 1) cajas[1].Focus();
                    break;
                case Keys.F3:
                    if (cajaActual > 2) cajas[2].Focus();
                    break;
                case Keys.F4:
                    if (cajaActual > 3) cajas[3].Focus();
                    break;
                case Keys.F5:
                    if (cajaActual > 4) cajas[4].Focus();
                    break;
                case Keys.F6:
                    if (cajaActual > 5) cajas[5].Focus();
                    break;
                case Keys.F7:
                    if (cajaActual > 6) cajas[6].Focus();
                    break;
                case Keys.F8:
                    if (cajaActual > 7) cajas[7].Focus();
                    break;
                case Keys.F9:
                    if (cajaActual > 8) cajas[8].Focus();
                    break;
                case Keys.F10:
                    if (cajaActual > 9) cajas[9].Focus();
                    break;
                case Keys.F11:
                    if (cajaActual > 10) cajas[10].Focus();
                    break;
                case Keys.F12:
                    if (cajaActual > 11) cajas[11].Focus();
                    break;
            }
        }
        private void busqueda(string searchparam)
        {
            BuscarExistencia buscarExistencia = new BuscarExistencia(webDM, agregarProducto, idsucursal, searchparam);
            buscarExistencia.ShowDialog();
        }

        private void tabla_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (tabla.SelectedCells.Count > 0)
                {
                    int selectedIndex = tabla.SelectedCells[0].RowIndex;

                    if (e.KeyData == Keys.Right)
                    {
                        if (selectedIndex >= 0 && selectedIndex < carrito.Count)
                        {
                            carrito[selectedIndex].cantidad++;
                            tabla.Rows[selectedIndex].Cells["Cantidad"].Value = carrito[selectedIndex].cantidad;
                            modCant(selectedIndex, carrito[selectedIndex].cantidad);
                            actualizarTotalCarrito();
                        }
                    }
                    if (e.KeyData == Keys.Left)
                    {
                        if (selectedIndex >= 0 && selectedIndex < carrito.Count)
                        {
                            if (carrito[selectedIndex].cantidad > 1)
                            {
                                carrito[selectedIndex].cantidad--;
                                tabla.Rows[selectedIndex].Cells["Cantidad"].Value = carrito[selectedIndex].cantidad;
                                modCant(selectedIndex, carrito[selectedIndex].cantidad);
                                actualizarTotalCarrito();
                            }
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // Manejo de la excepción, por ejemplo, mostrar un mensaje de error
                MessageBox.Show("Error: No se pudo acceder a la celda seleccionada. " + ex.Message);
            }

            if (e.KeyData == Keys.F2)
            {
                modcant.PerformClick();
            }
            if (e.KeyData == Keys.Delete)
            {
                borrar();
            }
            if (e.KeyData == Keys.F3)
            {
                existencia.PerformClick();
            }
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                cambiarVentana(e.KeyData);
            }
        }

        private void actualizarTotalCarrito()
        {
            // Calcular el nuevo total del carrito
            totalcarrito = carrito.Sum(p => p.cantidad * p.precio_venta);

            // Actualizar la vista para reflejar el nuevo total
            // (Reemplaza 'totalCarritoLabel' con el nombre de tu control)
            txttotal.Text = "Por Pagar MXN: " + totalcarrito.ToString("0.00");
        }

        private void existencia_Click(object sender, EventArgs e)
        {
            busqueda("");
        }

        private void txtcodigo_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Enter:
                    Producto p = localDM.GetProductByCode(txtcodigo.Text);

                    if (p != null)
                    {

                        agregarProducto(p);
                        txtcodigo.Text = "";
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                    else
                    {
                        busqueda(txtcodigo.Text);
                        txtcodigo.Text = "";
                    }
                    break;
                case Keys.Down:
                    tabla.Focus();
                    SendKeys.Send("{DOWN}");
                    break;
                case Keys.Up:
                    tabla.Focus();
                    SendKeys.Send("{UP}");
                    break;
            }
        }

        private void nuevacaja_Click(object sender, EventArgs e)
        {
            if (cajas.Count < 13)
            {
                Ventas ncaja = new Ventas();
                ncaja.Show();
            }
        }
        protected override void Dispose(bool disposing)
        {
            cajaActual--;
            cajas.Remove(this);
            if (cajas.Count == 0)
            {
                localDM.endDatabase();
                if (firebase!=null)
                firebase.Dispose();
            }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void abonar_Click(object sender, EventArgs e)
        {
            if (carrito.Count > 0)
            {
                data.isventa = true;
                MetodoPago mp = new MetodoPago(totalcarrito - totalpagado, abono, data);
                mp.ShowDialog();

                if (totalpagado >= totalcarrito)
                {
                    double cambio = totalpagado - totalcarrito;
                    txttotal.Text = "Cambio MXN: $" + cambio.ToString("0.00");
                    localDM.acumularPagos(pagos, idcorte, cambio);
                    if (cambio > 0)
                    {
                        CambioForm cf = new CambioForm(cambio);

                        cf.ShowDialog();
                    }
                    completarVenta();
                }
                else
                {
                    txttotal.Text = "Por pagar MXN: $" + (totalcarrito - totalpagado).ToString("0.00");
                    data.isventa = false;                  
                }
            }
            else
            {
                MessageBox.Show("Aún no hay productos en el carrito", "Advertencia");
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
        }

        public async Task enviarCorte(int id, Dictionary<string, string> data)
        {
            var (success, message) = await webDM.SendCorte(data);
            if (success)
            {
                localDM.changeEstadoCorte(id, 1, "Enviado");
                MessageBox.Show("Corte guardado con éxito");
            }
            else
            {
                MessageBox.Show(message, "Advertencia");
            }
        }



        private void ingreso_Click(object sender, EventArgs e)
        {
            GastosIngresos gi = new GastosIngresos(1, idcorte, localDM);
            gi.ShowDialog();
        }
        private void retiro_Click(object sender, EventArgs e)
        {
            GastosIngresos gi = new GastosIngresos(2, idcorte, localDM);
            gi.ShowDialog();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (localDM.impresora.Equals(""))
            {
                MessageBox.Show("No se ha establecido una impresora, favor de definir una desde la ventana de configuración", "Advertencia");
            }
            else
            {
                sucursalName = localDM.getSucursalname(idsucursal);
                sucursalDir = localDM.getSucursalAddr(idsucursal);
                VerOperaciones verOp = new VerOperaciones(localDM, idcaja, sucursalName, sucursalDir, cajero.es_raiz);
                verOp.ShowDialog();
            }
        }

        private async void cortep_Click(object sender, EventArgs e)
        {   
            sucursalName = localDM.getSucursalname(idsucursal);
            sucursalDir = localDM.getSucursalAddr(idsucursal);
            Dictionary<string, string> data = localDM.getCorte2(idcorte);
            VerCorte vc = new VerCorte(data, idsucursal, cajero.id, idcorte, idcaja, localDM);
            DialogResult response = vc.ShowDialog();
            if (response == DialogResult.Yes)
            {
                //corte["total_apartados"] y corte["total_creditos"] se encuentran ya en el corte obtenido
                corte = localDM.getCorte2(idcorte);
                createdocz();
                if (localDM.impresora.Equals(""))
                {
                    MessageBox.Show("No se ha establecido una impresora", "Advertencia");
                }
                else
                {
                    try
                    {
                        if (printerType == 1)
                        {
                            printPreviewControl1.Document.Print();
                        }
                        else
                        {
                            imprimirCorte(corte);
                        }
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        MessageBox.Show("No se guardo el PDF, ya se encuentra abierto un documento con el mismo nombre.", "Error");
                    }
                }
                await enviarCorte(idcorte, corte);
                idcorte = -1;
                apertura = 0;
                cajero = null;
                activador = null;
                this.Dispose();
            }
        }

        void imprimirCorte(Dictionary<string, string> corte)
        {
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

            double efedir = double.Parse(corte["total_efectivo"]) - tgastos;


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

        private void logout_Click(object sender, EventArgs e)
        {
            if (carrito.Count > 0)
            {
                MessageBox.Show("Favor de terminar la transacción antes de cerrar sesión", "Transacción en curso");
            }
            else
            {
                cerrarSesion();
            }

        }

        private void establecerImpresoraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigWindow config = new ConfigWindow(localDM,data);
            DialogResult result = config.ShowDialog();
            if (result == DialogResult.OK)
            {
                getConfig();
                refreshFolio();
            }
        }

        private void administrarCatálogoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Vercatalogo vercatalogo = new Vercatalogo(webDM);
            vercatalogo.ShowDialog();
        }

        // =====================================
        // CORRECCIÓN: Evitar doble descuento en el cálculo del total
        // MODIFICAR SOLO la parte del cálculo en imprimirTicketCarta()
        // =====================================

        private void imprimirTicketCarta(string fecha)
        {
            string piedeticket = Settings.Default["pieDeTicket"].ToString();
            ticket = "";
            string caj = cajero.nombre;
            double cambio = (totalcarrito - data.descuento);

            ticket += "CASA CEJA\n" +
                "SUCURSAL: " + sucursalName.ToUpper() + "\n" +
                "" + sucursalDir.ToUpper() + "\n" +
                "" + fecha + "\n" +
                "FOLIO: " + folio + "\n\n" +
                 "DESCRIPCION\tCANT\tP. UNIT\tP. TOTAL\n";

            // CALCULAR descuentos para el ticket
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

                // AGREGAR INDICADORES basándose en el estado original
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

                // PRECIO ORIGINAL (siempre menudeo)
                Producto productoCompleto = localDM.GetProductByCode(p.codigo);
                double precioUnitarioOriginal = productoCompleto?.menudeo ?? p.precio_venta;

                // PRECIO FINAL 
                double precioFinalConDescuentos = p.precio_venta * p.cantidad;

                // SUBTOTAL SIN DESCUENTOS
                subtotalSinDescuentos += precioUnitarioOriginal * p.cantidad;

                // *** ACUMULAR DESCUENTOS CORRECTAMENTE ***
                if (p.tuvo_descuento_categoria_original)
                {
                    descuentoCategoriaTicket += p.descuento_categoria_original * p.cantidad;
                    Console.WriteLine($"*** TICKET: {p.nombre} - Categoria original: {p.descuento_categoria_original * p.cantidad}");
                }

                if (p.es_precio_especial)
                {
                    descuentoPrecioEspecialTicket += p.descuento_unitario * p.cantidad;
                }

                ticket += n + indicadores + "\t" + p.cantidad + "\t" + precioUnitarioOriginal.ToString("0.00") + "\t" + precioFinalConDescuentos.ToString("0.00") + "\n";
            }

            Console.WriteLine($"*** TICKET TOTALES:");
            Console.WriteLine($"  Subtotal sin descuentos: {subtotalSinDescuentos}");
            Console.WriteLine($"  Descuento Categoría: {descuentoCategoriaTicket}");
            Console.WriteLine($"  Descuento Precio Especial: {descuentoPrecioEspecialTicket}");
            Console.WriteLine($"  Descuento de Venta: {data.descuento}");

            if (!fontName.Equals("Consolas"))
                ticket += "--------------------";
            ticket += "--------------------------------------------------------------\n";

            // MOSTRAR SUBTOTAL
            ticket += "SUBTOTAL $\t------>\t\t" + subtotalSinDescuentos.ToString("0.00") + "\n";

            // MOSTRAR DESCUENTOS USANDO VALORES ORIGINALES
            if (descuentoCategoriaTicket > 0)
            {
                ticket += "DESC. POR CATEGORIA\t------>\t-" + descuentoCategoriaTicket.ToString("0.00") + "\n";
            }

            if (descuentoPrecioEspecialTicket > 0)
            {
                ticket += "DESC. PRECIO ESPECIAL\t------>\t-" + descuentoPrecioEspecialTicket.ToString("0.00") + "\n";
            }

            if (data.esDescuento)
            {
                ticket += "DESCUENTO DE VENTA\t------>\t-" + data.descuento.ToString("0.00") + "\n";
            }

            // *** CORRECCIÓN: CALCULAR TOTAL USANDO EL TOTAL ACTUAL DEL CARRITO ***
            // El totalcarrito YA tiene todos los descuentos aplicados correctamente
            double totalFinalCorrecto = totalcarrito - data.descuento;

            Console.WriteLine($"*** CÁLCULO TOTAL:");
            Console.WriteLine($"  totalcarrito (con descuentos ya aplicados): {totalcarrito}");
            Console.WriteLine($"  data.descuento: {data.descuento}");
            Console.WriteLine($"  Total final correcto: {totalFinalCorrecto}");

            ticket += "TOTAL A PAGAR $\t------>\t\t" + totalFinalCorrecto.ToString("0.00") + "\n";

            if (!fontName.Equals("Consolas"))
                ticket += "--------------------";
            ticket += "--------------------------------------------------------------\n";

            // Métodos de pago (igual que antes)...
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
            ticket += "SU CAMBIO $\t------>\t\t" + (cambio * -1).ToString("0.00") + "\n";

            if (!fontName.Equals("Consolas"))
                ticket += "--------------------";
            ticket += "--------------------------------------------------------------\n\n" +
                 "LE ATENDIO: " + cajero.nombre.ToUpper() + "\n" +
                 "NO DE ARTICULOS: " + carrito.Count.ToString().PadLeft(5, '0') + "\n" +
                 "GRACIAS POR SU COMPRA\n\n";
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

        private void createdoc()
        {

            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Venta.txt");
            // Construct the PrintPreviewControl.

            //// Set location, name, and dock style for printPreviewControl1.
            //this.printPreviewControl1.Name = "printPreviewControl1";

            // Set the Document property to the PrintDocument 
            // for which the PrintPage event has been handled.
            this.printPreviewControl1.Document = docToPrint;
            this.printPreviewControl1.Zoom = 2;
            if (fontSize > 6)
                this.printPreviewControl1.Zoom = 1.5;
            if (fontSize > 10)
                this.printPreviewControl1.Zoom = 1.1;
            if (fontSize > 13)
                this.printPreviewControl1.Zoom = 1.0;
            // Set the document name. This will show be displayed when 
            // the document is loading into the control.
            this.printPreviewControl1.Document.DocumentName = path;
            this.printPreviewControl1.Document.PrinterSettings.PrinterName = localDM.impresora;

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
        private void docToPrint_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            // Insertar código para renderizar el texto del ticket.
            string text1 = ticket;

            // Fuente y formato para el texto
            FontFamily fontFamily = new FontFamily(fontName);
            Font font = new Font(
                fontFamily,
                fontSize,
                FontStyle.Regular,
                GraphicsUnit.Point);
            Rectangle rect = new Rectangle(50, 50, 750, 700);  // Mantener el área de impresión del texto
            StringFormat stringFormat = new StringFormat();
            SolidBrush solidBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0));

            stringFormat.SetTabStops(0, tabs[fontSize]);

            e.Graphics.DrawString(text1, font, solidBrush, rect, stringFormat);

           /* try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string qrPath = Path.Combine(basePath, "qr_facturacion.png");
                Image qrImage = Image.FromFile(qrPath);

                int qrWidth = 200; // Tamaño del código QR
                int qrHeight = 200;

                // Calcular la posición centrada en el ancho del área de impresión
                int qrX = (rect.Width - qrWidth) / 2 + rect.X;
                int qrY = rect.Y + rect.Height + 20; // Justo debajo del área de texto, ajustado con un margen de 20

                // Dibujar la imagen QR centrada
                e.Graphics.DrawImage(qrImage, qrX, qrY, qrWidth, qrHeight);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al cargar la imagen del QR: " + ex.Message);
            }*/
        }

        private void createdocz()
        {

            string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "Corte.txt");
            // Construct the PrintPreviewControl.

            //// Set location, name, and dock style for printPreviewControl1.
            //this.printPreviewControl1.Name = "printPreviewControl1";

            // Set the Document property to the PrintDocument 
            // for which the PrintPage event has been handled.
            printPreviewControl1.Document = docZToPrint;
            // Set the document name. This will show be displayed when 
            // the document is loading into the control.
            printPreviewControl1.Document.DocumentName = path;
            printPreviewControl1.Document.PrinterSettings.PrinterName = localDM.impresora;

            // Set the UseAntiAlias property to true so fonts are smoothed
            // by the operating system.
            printPreviewControl1.UseAntiAlias = true;
            // Add the control to the form.

            // Associate the event-handling method with the
            // document's PrintPage event.
            this.docZToPrint.PrintPage +=
                new System.Drawing.Printing.PrintPageEventHandler(
                docZToPrint_PrintPagez);
        }
        private void docZToPrint_PrintPagez(
    object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {

            // Insert code to render the page here.
            // This code will be called when the control is drawn.

            // The following code will render a simple
            // message on the document in the control.
            //StringFormat format = new StringFormat(StringFormatFlags.NoClip);
            //format.Alignment = StringAlignment.Center;
            //System.Drawing.Font printFont =
            //    new Font(fontName, fontSize, FontStyle.Regular);

            //e.Graphics.DrawString(text1, printFont,
            //    Brushes.Black, 50, 50);
            FontFamily fontFamily = new FontFamily("Calibri");
            Font tfont = new Font(
               fontFamily,
               18,
               FontStyle.Bold,
               GraphicsUnit.Point);
            Font font = new Font(
               fontFamily,
               12,
               FontStyle.Bold,
               GraphicsUnit.Point);
            Rectangle trect = new Rectangle(50, 50, 400, 30);
            Rectangle rect = new Rectangle(trect.X, trect.Y + (trect.Height*2), trect.Width, trect.Height);
            StringFormat centerMiddle = new StringFormat();
            centerMiddle.Alignment = StringAlignment.Center;
            centerMiddle.LineAlignment = StringAlignment.Center;

            StringFormat leftMiddle = new StringFormat();
            leftMiddle.Alignment = StringAlignment.Near;
            leftMiddle.LineAlignment = StringAlignment.Center;

            StringFormat rightMiddle = new StringFormat();
            rightMiddle.Alignment = StringAlignment.Far;
            rightMiddle.LineAlignment = StringAlignment.Center;
            SolidBrush solidBrush = new SolidBrush(Color.FromArgb(255, 0, 0, 0));
            SolidBrush opacityBrush = new SolidBrush(Color.FromArgb(90, 0, 0, 0));


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

            double efedir = double.Parse(corte["total_efectivo"]) - tgastos;


            double totalCZ = double.Parse(corte["total_efectivo"]) + double.Parse(corte["total_tarjetas_debito"]) + double.Parse(corte["total_tarjetas_credito"]) + double.Parse(corte["total_cheques"]) + double.Parse(corte["total_transferencias"]) + double.Parse(corte["sobrante"]);

            e.Graphics.DrawString("CASA CEJA", tfont, solidBrush, trect, centerMiddle);

            e.Graphics.DrawString("SUCURSAL: " + sucursalName.ToUpper(), font, solidBrush, rect, centerMiddle);
            e.Graphics.DrawString("CZ FOLIO: " + folioCorte, font, solidBrush, new Rectangle(rect.X, rect.Y+(rect.Height*2), rect.Width, rect.Height), centerMiddle);

            int horizontalPadding = 10;
            e.Graphics.DrawString("FECHA DE APERTURA:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 4), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString(corte["fecha_apertura_caja"], font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 4), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);
            e.Graphics.DrawString("FECHA DE CORTE:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 5), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString(corte["fecha_corte_caja"], font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 5), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("FONDO DE APERTURA:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 7), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ "+corte["fondo_apertura"], font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 7), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("TOTAL CZ:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 9), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ " + totalCZ.ToString("0.00"), font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 9), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("EFECTIVO DE CREDITOS:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 10), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ " + corte["efectivo_creditos"], font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 10), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("EFECTIVO DE APARTADOS:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 11), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ " + corte["efectivo_apartados"], font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 11), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("EFECTIVO DIRECTO:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 13), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ " + efedir.ToString("0.00"), font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 13), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);          

            e.Graphics.DrawString("TOTAL T. DEBITO:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 16), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ " + corte["total_tarjetas_debito"], font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 16), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("TOTAL T. CREDITO:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 17), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ " + corte["total_tarjetas_credito"], font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 17), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("TOTAL CHEQUES:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 18), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ " + corte["total_cheques"], font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 18), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("TOTAL TRANSFERENCIAS:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 19), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ " + corte["total_transferencias"], font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 19), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("SOBRANTE:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 21), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ " + corte["sobrante"], font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 21), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("GASTOS:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 22), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ " + tgastos.ToString("0.00"), font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 22), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("INGRESOS:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 23), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ " + tingresos.ToString("0.00"), font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 23), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("EFECTIVO TOTAL:", font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 25), rect.Width - (horizontalPadding * 2), rect.Height), leftMiddle);
            e.Graphics.DrawString("$ " + corte["total_efectivo"], font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 25), rect.Width - (horizontalPadding * 2), rect.Height), rightMiddle);

            e.Graphics.DrawString("FIRMA", font, opacityBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 28), rect.Width - (horizontalPadding * 2), rect.Height), centerMiddle);
            e.Graphics.DrawString("CAJERO: "+ cajero.nombre.ToUpper(), font, solidBrush, new Rectangle(rect.X + horizontalPadding, rect.Y + (rect.Height * 29), rect.Width - (horizontalPadding * 2), rect.Height), centerMiddle);

            Pen pen = Pens.Black;
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 4), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 5), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 7), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 9), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 10), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 11), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 13), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 16), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 17), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 18), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 19), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 21), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 22), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 23), rect.Width, rect.Height));
            e.Graphics.DrawRectangle(pen, new Rectangle(rect.X, rect.Y + (rect.Height * 25), rect.Width, rect.Height));
            e.Graphics.DrawLine(pen, new Point(rect.X, rect.Y + (rect.Height * 29)), new Point(rect.X + rect.Width, rect.Y + (rect.Height * 29)));
        }

        private void altacliente_Click(object sender, EventArgs e)
        {
            localDM.ClearDatabase();
        }

        private void apartados_Click(object sender, EventArgs e)
        {
            data.folioCorte = localDM.getFolioCorte(idcorte);
            data.totalcarrito = totalcarrito;
            data.idCorte = idcorte;
            data.usuario.nombre = cajero.nombre;

            sucursalName = localDM.getSucursalname(idsucursal);
            sucursalDir = localDM.getSucursalAddr(idsucursal);

            CredApartSel CredApar = new CredApartSel(data);

            DialogResult response = CredApar.ShowDialog();

            if (data.successful)
            {
                resetVenta();
                data.carrito.Clear();
                data.totalcarrito = 0;
                data.successful = false;
                refreshFolio();
                CredApar.Close();
            }
        }


        private void limpiarBDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            localDM.ClearDatabase();
            webDM.resetDates();
        }

        private void actualizarBaseDeDatosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reloadData();
        }

        private void eliminarCarrito_button_Click(object sender, EventArgs e)
        {
            if (carrito.Count == 0)
            {
                MessageBox.Show("El carrito ya está vacío.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                DialogResult result = MessageBox.Show("¿Está seguro de que quiere vaciar el carrito?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    resetVenta();
                }
            }
        }
        private void txtcodigo_FocusChanged(object sender, EventArgs e)
        {
            TextBox textBox = sender as TextBox;
            textBox.BackColor = textBox.Focused ? Color.FromArgb(220, 166, 64) : SystemColors.Window;
        }

        private void tabla_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void historialDeCortesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HistorialCortes hc = new HistorialCortes(data);
            hc.ShowDialog();
        }

        private void ListaClientes_Click_1(object sender, EventArgs e)
        {
            data.usuario = cajero;
            data.folioCorte = folioCorte;
            data.totalcarrito = totalcarrito;

            sucursalName = localDM.getSucursalname(idsucursal);
            sucursalDir = localDM.getSucursalAddr(idsucursal);
            ListaClientes listaCl = new ListaClientes(data);

            // Mostrar el diálogo y verificar el resultado
            DialogResult response = listaCl.ShowDialog();

            if (response == DialogResult.OK && data.successful)
            {
                resetVenta();
                data.carrito.Clear();
                data.totalcarrito = 0;
                data.successful = false;
                refreshFolio();
            }
        }

        public async Task SyncPendingCortes()
        {
            bool result = await webDM.SendPendingCortes();
            if (result)
            {
                Console.WriteLine("Cortes pendientes enviados exitosamente.");
            }
            else
            {
                Console.WriteLine("Error al enviar los cortes pendientes.");
            }
        }

        private void adminUsuariosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Usuarios usuarios = new Usuarios(data);
            usuarios.ShowDialog();
        }

        private void Bdescuento_Click(object sender, EventArgs e)
        {
            if (data.desbloqDesc == true)
            {
                aplicarDescuento();
            }
            else
            {
                UserLogin login = new UserLogin(localDM, setAdmin, true);
                DialogResult resultLogin = login.ShowDialog();
                if (resultLogin == DialogResult.Yes)
                {
                    aplicarDescuento();
                }
                else
                {
                    MessageBox.Show("Autenticación Fallida");
                }
            }
        }
        void aplicarDescuento()
        {
            if ((tabla.Rows.Count > 0) && (totalcarrito > 0))
            {
                aplicarDesc aplicDesc = new aplicarDesc(totalcarrito, data);
                aplicDesc.ShowDialog();
                if (data.esDescuento)
                {
                    totalpagado += data.descuento;
                    txttotal.Text = "Por pagar MXN: $" + (totalcarrito - totalpagado).ToString("0.00");
                    Bdescuento.Enabled = false;
                }
            }
            else
            {
                MessageBox.Show("Aún no hay productos en el carrito", "Advertencia");
            }
        }

        void setAdmin(Usuario usuario)
        {
            admin = usuario;
        }

        private void BdescTemporada_Click(object sender, EventArgs e)
        {
            UserLogin login = new UserLogin(localDM, setAdmin, true);
            DialogResult resultLogin = login.ShowDialog();
            if (resultLogin == DialogResult.Yes)
            {
                data.desbloqDesc = true;
                MessageBox.Show("Se aplicarán descuentos sin verificación de administrador hasta que se cierre la sesión.", "Aviso");
                this.BdescTemporada.Enabled = false;
            }
            else
            {
                MessageBox.Show("Autenticación Fallida");
            }
        }


        // ===========================================
        // NUEVOS MÉTODOS PARA PRECIO ESPECIAL
        // ===========================================
        private void TogglePrecioEspecial()
        {
            if (tabla.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un producto para aplicar precio especial", "Aviso",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                int index = tabla.SelectedRows[0].Index;

                if (index >= 0 && index < carrito.Count)
                {
                    AplicarOQuitarPrecioEspecial(index);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar precio especial: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarOQuitarPrecioEspecial(int index)
        {
            ProductoVenta productoVenta = carrito[index];
            Producto producto = localDM.GetProductByCode(productoVenta.codigo);

            if (producto == null)
            {
                MessageBox.Show("No se pudo obtener información del producto", "Error");
                return;
            }

            if (producto.especial <= 0)
            {
                MessageBox.Show("Este producto no tiene precio especial configurado", "Sin precio especial");
                return;
            }

            if (productoVenta.es_precio_especial)
            {
                // *** QUITAR PRECIO ESPECIAL ***
                QuitarPrecioEspecial(index, productoVenta, producto);

                // DESPUÉS de quitar precio especial, RE-APLICAR descuento de categoría
                VerificarYAplicarDescuentoCategoria(index);
            }
            else
            {
                // *** APLICAR PRECIO ESPECIAL ***
                // QUITAR descuento de categoría si lo tiene (porque precio especial lo reemplaza)
                if (productoVenta.es_descuento_categoria)
                {
                    QuitarDescuentoCategoria(index);
                }

                AplicarPrecioEspecial(index, productoVenta, producto);
            }

            ActualizarVisualizacionTabla(index);
            RecalcularTotales();
        }

        private void QuitarDescuentoCategoria(int index)
        {
            ProductoVenta productoVenta = carrito[index];

            if (productoVenta.es_descuento_categoria)
            {
                // Restar del total de descuentos de categoría
                double descuentoTotal = productoVenta.descuento_categoria_unitario * productoVenta.cantidad;
                totalDescuentoCategoria -= descuentoTotal;

                // Restaurar precio sin descuento de categoría
                productoVenta.precio_venta += productoVenta.descuento_categoria_unitario;

                // Resetear campos ACTUALES
                productoVenta.es_descuento_categoria = false;
                productoVenta.descuento_categoria_unitario = 0;
                productoVenta.porcentaje_descuento_categoria = 0;

                // *** NO TOCAR LOS CAMPOS ORIGINALES ***
                // productoVenta.tuvo_descuento_categoria_original = MANTENER
                // productoVenta.descuento_categoria_original = MANTENER
                // productoVenta.porcentaje_categoria_original = MANTENER

                Console.WriteLine($"Descuento de categoría removido de {productoVenta.nombre}");
                Console.WriteLine($"PERO se mantiene el original guardado: {productoVenta.descuento_categoria_original}");
            }
        }

        private void AplicarPrecioEspecial(int index, ProductoVenta productoVenta, Producto producto)
        {
            // Guardar precio original si no se ha guardado
            if (productoVenta.precio_original == 0)
            {
                productoVenta.precio_original = productoVenta.precio_venta;
            }

            // Calcular y aplicar descuento
            double descuentoUnitario = productoVenta.precio_venta - producto.especial;
            double descuentoTotal = descuentoUnitario * productoVenta.cantidad;

            productoVenta.precio_venta = producto.especial;
            productoVenta.es_precio_especial = true;
            productoVenta.descuento_unitario = descuentoUnitario;

            // Actualizar total de descuentos
            totalDescuentoPrecioEspecial += descuentoTotal;

            MessageBox.Show($"Precio especial aplicado: ${producto.especial:0.00}\nDescuento: ${descuentoUnitario:0.00} por unidad",
                           "Precio Especial Aplicado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void QuitarPrecioEspecial(int index, ProductoVenta productoVenta, Producto producto)
        {
            // Calcular descuento que se va a quitar
            double descuentoTotal = productoVenta.descuento_unitario * productoVenta.cantidad;

            // Restaurar precio original
            if (productoVenta.precio_original > 0)
            {
                productoVenta.precio_venta = productoVenta.precio_original;
            }
            else
            {
                productoVenta.precio_venta = producto.menudeo;
            }

            // Restar del total de descuentos
            totalDescuentoPrecioEspecial -= descuentoTotal;

            // Resetear campos
            productoVenta.es_precio_especial = false;
            productoVenta.descuento_unitario = 0;

            MessageBox.Show($"Precio especial removido. Precio actual: ${productoVenta.precio_venta:0.00}",
                           "Precio Normal Restaurado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ActualizarColorProducto(int index)
        {
            ProductoVenta producto = carrito[index];

            // Determinar qué color usar basado en los descuentos aplicados
            if (producto.es_precio_especial)
            {
                // Solo precio especial - Color verde (como ya lo tienes)
                tabla.Rows[index].DefaultCellStyle.BackColor = Color.LimeGreen;
                tabla.Rows[index].DefaultCellStyle.ForeColor = Color.Black;
            }
            else if (producto.es_descuento_categoria)
            {
                // Solo descuento de categoría - Color azul claro
                tabla.Rows[index].DefaultCellStyle.BackColor = Color.LightBlue;
                tabla.Rows[index].DefaultCellStyle.ForeColor = Color.Black;
            }
            else
            {
                // Sin descuentos - Color normal
                tabla.Rows[index].DefaultCellStyle.BackColor = SystemColors.Window;
                tabla.Rows[index].DefaultCellStyle.ForeColor = SystemColors.WindowText;
            }
        }

        private void ActualizarVisualizacionTabla(int index)
        {
            ProductoVenta producto = carrito[index];

            // Actualizar precio y total en la tabla
            tabla.Rows[index].Cells["precio"].Value = producto.precio_venta.ToString("0.00");
            tabla.Rows[index].Cells["total"].Value = (producto.cantidad * producto.precio_venta).ToString("0.00");

            ActualizarColorProducto(index);
        }

        private void ActualizarCurrentData()
        {
            data.totalcarrito = totalcarrito;
            data.totalDescuentoCategoria = totalDescuentoCategoria;
            data.totalDescuentoPrecioEspecial = totalDescuentoPrecioEspecial;
            data.carrito = carrito;
        }

        private void RecalcularTotales()
        {
            totalcarrito = GetTotal();

            // Construir texto con información de todos los descuentos
            string textoTotal = $"Por pagar MXN: ${totalcarrito:0.00}";

            List<string> descuentosInfo = new List<string>();

            if (totalDescuentoPrecioEspecial > 0)
            {
                descuentosInfo.Add($"Desc. P.Esp: ${totalDescuentoPrecioEspecial:0.00}");
            }

            if (totalDescuentoCategoria > 0)
            {
                descuentosInfo.Add($"Desc. Cat: ${totalDescuentoCategoria:0.00}");
            }

            if (descuentosInfo.Count > 0)
            {
                textoTotal += $" ({string.Join(", ", descuentosInfo)})";
            }

            txttotal.Text = textoTotal;
            ActualizarCurrentData();
            VerificarYResetearSiCarritoVacio();
        }

        private void VerificarYResetearSiCarritoVacio()
        {
            if (carrito.Count == 0)
            {
                Console.WriteLine("*** CARRITO VACÍO DETECTADO - Reseteando variables ***");

                // Resetear solo las variables de descuentos y totales
                totalDescuentoPrecioEspecial = 0;
                totalDescuentoCategoria = 0;
                totalcarrito = 0;
                totalpagado = 0;
                mensajeMayoreoMostrado = false;
                pagos.Clear();

                // Resetear campos de data
                data.esDescuento = false;
                data.descuento = 0;
                data.totalabonado = 0;
                data.isventa = false;

                // Actualizar UI
                txttotal.Text = "Por pagar MXN: $0.00";
                Bdescuento.Enabled = true;

                Console.WriteLine("*** Variables reseteadas correctamente ***");
            }
        }

    }
}