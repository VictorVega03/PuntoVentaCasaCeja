using OfficeOpenXml;
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
using Windows.UI.Xaml;

namespace PuntoVentaCasaCeja
{
    public partial class ListaClientes : Form
    {
        Action<Usuario> setUser;
        Usuario usuario;
        WebDataManager webDM;
        LocaldataManager localDM;
        CurrentData data;
        AltaCliente altaCliente;
        static Usuario activador = null;
        static Usuario admin = null;
        bool baja = true;
        public ListaClientes(CurrentData data)
        {
            InitializeComponent();
            this.webDM = data.webDM;
            this.data = data;
            this.localDM = webDM.localDM;
            this.tablaClientes.DataSource = localDM.getClientes();
            this.altaCliente = new AltaCliente(data);

            tablaClientes.ColumnHeadersDefaultCellStyle.Font = new Font(tablaClientes.Font.FontFamily, 14);
            tablaClientes.Columns[4].Visible = false; // RFC
            tablaClientes.Columns[5].Visible = false; // numero_exterior
            tablaClientes.Columns[6].Visible = false; // numero_interior
            tablaClientes.Columns[7].Visible = false; // codigo_postal
            tablaClientes.Columns[8].Visible = false; // calle
            tablaClientes.Columns[9].Visible = false; // colonia
            tablaClientes.Columns[10].Visible = false; // ciudad
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
                        BinfoCliente.PerformClick();
                        break;
                    case Keys.F5:
                        altaButton.PerformClick();
                        break;
                    case Keys.F6:
                        modificarButton.PerformClick();
                        break;
                    case Keys.F7:
                        BajaButton.PerformClick();
                        break;
                    case Keys.F8:
                        BgenerarExcel.PerformClick();
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
        void setAdmin(Usuario usuario)
        {
            admin = usuario;
            webDM.activeUser = usuario;
        }
        
        private async void BajaButton_Click(object sender, EventArgs e)
        {

            if (tablaClientes.SelectedRows.Count == 0)
            {
                MessageBox.Show("No se ha seleccionado ningun cliente");
                return;
            }

            UserLogin login = new UserLogin(localDM, setAdmin, true);
            DialogResult result = login.ShowDialog();

            if (result == DialogResult.Yes)
            {
                int idCliente = Convert.ToInt32(tablaClientes.SelectedRows[0].Cells[0].Value);

                // Llamar al método para desactivar el cliente
                DialogResult deleteConfirmation = MessageBox.Show("Desea dar de baja este cliente??", "Eliminar Cliente", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (deleteConfirmation == DialogResult.Yes)
                {
                    var desactivarResult = await webDM.DesactivarClienteAsync(idCliente);

                    if (desactivarResult["status"] == "success")
                    {
                        MessageBox.Show("Cliente dado de baja correctamente");
                        localDM.eliminarCliente(idCliente);
                        localDM.eliminarClienteTemporal(idCliente);
                        tablaClientes.DataSource = localDM.getClientes();
                    }
                    else
                    {
                        MessageBox.Show($"Error al desactivar el cliente: {desactivarResult["message"]}");
                    }

                    tablaClientes.Focus();
                }
                else
                {
                    MessageBox.Show("Operación cancelada");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Autenticacion Fallida");
            }
        }


        private void seleccion(object sender, EventArgs e)
        {
            if (tablaClientes.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(tablaClientes.SelectedRows[0].Cells[0].Value); 
                Cliente cliente = localDM.getCliente(id);
                altaCliente.clienteSeleccionado(cliente);
                if (data.successful)
                {
                    this.Close();
                }
            }
        }

        private void seleccion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                seleccion(sender, e);
            }
        }

        private void tablaClientes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                seleccion_KeyDown(sender, e);
            }
        }

        private void tablaClientes_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewRow row = tablaClientes.Rows[e.RowIndex];
                tablaClientes.ClearSelection();
                row.Selected = true;
            }
        }

        private void modificarButton_Click(object sender, EventArgs e)
        {
            if (tablaClientes.SelectedRows.Count > 0)
            {
                Cliente cliente = new Cliente();
                cliente.nombre = tablaClientes.SelectedRows[0].Cells[1].Value.ToString();
                cliente.telefono = tablaClientes.SelectedRows[0].Cells[2].Value.ToString();
                cliente.correo = tablaClientes.SelectedRows[0].Cells[3].Value.ToString();
                cliente.rfc = tablaClientes.SelectedRows[0].Cells[4].Value.ToString();
                cliente.numero_exterior = tablaClientes.SelectedRows[0].Cells[5].Value.ToString();
                cliente.numero_interior = tablaClientes.SelectedRows[0].Cells[6].Value.ToString();
                cliente.codigo_postal = tablaClientes.SelectedRows[0].Cells[7].Value.ToString();
                cliente.calle = tablaClientes.SelectedRows[0].Cells[8].Value.ToString();
                cliente.colonia = tablaClientes.SelectedRows[0].Cells[9].Value.ToString();
                cliente.ciudad = tablaClientes.SelectedRows[0].Cells[10].Value.ToString();
                cliente.id = Convert.ToInt32(tablaClientes.SelectedRows[0].Cells[0].Value);
                ModificarCliente modCliente = new ModificarCliente(data, cliente);
                modCliente.ShowDialog();
            }
            else
            {
                MessageBox.Show("No se ha seleccionado ningún cliente.");
            }
            tablaClientes.DataSource = localDM.getClientes();
        }

        private void altaButton_Click(object sender, EventArgs e)
        {
            AltaCliente altaCliente = new AltaCliente(data);
            altaCliente.ShowDialog();
            tablaClientes.DataSource = localDM.getClientes();
        }

        private void BSelCliente_Click(object sender, EventArgs e)
        {
            seleccion(sender, e);
            tablaClientes.Focus();
        }


        private void BinfoCliente_Click(object sender, EventArgs e)
        {
            // Verificar que hay una fila seleccionada
            if (tablaClientes.SelectedRows.Count == 0)
            {
                MessageBox.Show("No se ha seleccionado ningún cliente.", "Selección requerida",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Obtener el ID del cliente seleccionado
                int idCliente = Convert.ToInt32(tablaClientes.SelectedRows[0].Cells[0].Value);

                // Obtener el cliente completo desde la base de datos local
                Cliente clienteSeleccionado = localDM.getCliente(idCliente);

                if (clienteSeleccionado != null)
                {
                    // Crear la vista de información y enviar el cliente
                    infoCliente infoCliente = new infoCliente(data, clienteSeleccionado);
                    infoCliente.ShowDialog();
                }
                else
                {
                    MessageBox.Show("No se pudo obtener la información del cliente.", "Error",
                                   MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener la información del cliente: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BgenerarExcel_Click(object sender, EventArgs e)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            int idSucursal = data.idSucursal;
            DataTable clientesTable = localDM.getClientes();           
            DateTime localDate = DateTime.Now;
            string fecha = localDate.ToString("dd-MM-yyyy");            

            // Especificar la carpeta principal y la subcarpeta
            string carpetaPrincipal = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CasaCejaDocs");
            string subcarpeta = Path.Combine(carpetaPrincipal, "PuntoDeVenta");

            // Utilizamos la subcarpeta para guardar el archivo
            string carpeta = subcarpeta;                    
            string nombreArchivo = "ListaClientes " + fecha + ".xlsx";
            string rutaArchivo = Path.Combine(carpeta, nombreArchivo);

            // Verificar si la carpeta principal y la subcarpeta existen, si no, crearlas
            if (!Directory.Exists(carpetaPrincipal))
            {
                Directory.CreateDirectory(carpetaPrincipal);
            }
            if (!Directory.Exists(subcarpeta))
            {
                Directory.CreateDirectory(subcarpeta);
            }

            // Verificar si el archivo ya existe
            if (File.Exists(rutaArchivo))
            {
                DialogResult dialogResult = MessageBox.Show("El archivo ya existe. ¿Deseas sobrescribirlo?", "Archivo Existente", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.No)
                {
                    return;
                }
            }

            // Generar el archivo Excel
            try
            {
                using (ExcelPackage paquete = new ExcelPackage())
                {                                  
                       // Crear la hoja para los clientes
                        ExcelWorksheet hojaClientes = paquete.Workbook.Worksheets.Add("ListadoClientes " + fecha);

                        // Agregar los encabezados de las columnas
                        for (int i = 0; i < clientesTable.Columns.Count; i++)
                        {
                            hojaClientes.Cells[1, i + 1].Value = clientesTable.Columns[i].ColumnName;
                            hojaClientes.Cells[1, i + 1].Style.Font.Bold = true;
                            hojaClientes.Cells[1, i + 1].Style.Font.Size = 14;
                        }

                        // Agregar los datos de las filas
                        for (int fila = 0; fila < clientesTable.Rows.Count; fila++)
                        {
                            for (int col = 0; col < clientesTable.Columns.Count; col++)
                            {
                                hojaClientes.Cells[fila + 2, col + 1].Value = clientesTable.Rows[fila][col].ToString();
                            }
                        }
                    

                    // Guardar el archivo en la ruta especificada
                    FileInfo archivo = new FileInfo(rutaArchivo);
                    paquete.SaveAs(archivo);

                    // Mostrar mensaje de éxito si se ha creado correctamente
                    MessageBox.Show("Lista de Clientes " + fecha + ".xlsx" + " se generó correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al generar el archivo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
    }