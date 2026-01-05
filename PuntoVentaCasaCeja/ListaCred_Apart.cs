using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PuntoVentaCasaCeja
{
    public partial class ListaCred_Apart : Form
    {
        private int rowCount = 0;
        private int maxPages = 0;
        private int currentPage = 1;
        private int offset = 0;
        private int idCliente = 0;
        private int rowsPerPage = 10;
        public int opc = -1;
        private Usuario usuario;
        private WebDataManager webDM;
        private LocaldataManager localDM;
        private CurrentData data;

        private List<string> tipo = new List<string>();
        private List<string> estados = new List<string>();

        private readonly string[] rangeTipo = {"Apartados", "Creditos" };
        private readonly string[] range = { "TODOS", "PENDIENTE", "EXPIRO", "CANCELADO", "PAGADO" };

        public ListaCred_Apart(CurrentData data)
        {
            InitializeComponent();
            this.data = data;
            this.webDM = data.webDM;
            this.localDM = webDM.localDM;

            // Inicialización de ComboBoxes
            tipo.AddRange(rangeTipo);
            BoxTipo.DataSource = tipo;
            BoxTipo.SelectedIndex = 0;

            estados.AddRange(range);
            BoxEstado.DataSource = estados;
            BoxEstado.SelectedIndex = 0;

            // Configuración inicial de la tabla
            CargarDatosTabla();
            
            tablaCreditosApartados.ColumnHeadersDefaultCellStyle.Font = new Font(tablaCreditosApartados.Font.FontFamily, 16);

        }

        private void CargarDatosTabla()
        {
            DataTable dataTable = localDM.GetApartadosDataTable(data.idSucursal);
            dataTable.DefaultView.Sort = "Fecha DESC";
            tablaCreditosApartados.DataSource = dataTable;
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
                        ShowDropDown(BoxTipo);
                        break;
                    case Keys.F2:
                        ShowDropDown(BoxEstado);
                        break;
                    case Keys.F3:
                        BcrearExcel.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void ShowDropDown(ComboBox comboBox)
        {
            comboBox.DroppedDown = true;
            comboBox.Focus();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void TablaCreditos_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
                ShowDropDown(BoxTipo);
            if (e.KeyCode == Keys.F2)
                ShowDropDown(BoxEstado);
            if (e.KeyCode == Keys.F3)
                BcrearExcel.PerformClick();
        }

        private void tablaCreditos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewRow selectedRow = tablaCreditosApartados.Rows[e.RowIndex];
                    string clienteNombre = selectedRow.Cells["Cliente"].Value.ToString();

                    if (data.cliente == null)
                        data.cliente = new Cliente();

                    int clienteId = localDM.GetCliente(clienteNombre);

                    if (clienteId != -1)
                    {
                        data.cliente.id = clienteId;
                        VerCredApa vca = new VerCredApa(BoxTipo.SelectedIndex, data);
                        vca.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("Cliente no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al procesar la acción: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BSelCliente_Click(object sender, EventArgs e)
        {
            if (tablaCreditosApartados.CurrentCell != null)
            {
                tablaCreditos_CellDoubleClick(tablaCreditosApartados, new DataGridViewCellEventArgs(tablaCreditosApartados.CurrentCell.ColumnIndex, tablaCreditosApartados.CurrentCell.RowIndex));
                tablaCreditosApartados.Focus();
            }
        }

        private void BoxTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPage = 1;
            offset = 0;
            FiltrarDatos();
        }

        private void BoxEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentPage = 1;
            offset = 0;
            FiltrarDatos();
        }

        private void FiltrarDatos()
        {
            // Obtener los datos según el tipo seleccionado
            DataTable dataTable = (BoxTipo.SelectedIndex == 0) ?
            localDM.GetApartadosDataTable(data.idSucursal) :
            localDM.GetCreditosDataTable(data.idSucursal);               

            // Aplicar filtro de estado si es necesario
            if (BoxEstado.SelectedItem != null && BoxEstado.SelectedIndex != 0)
            {
                string estado = BoxEstado.SelectedItem.ToString();
                dataTable.DefaultView.RowFilter = $"Estado = '{estado}'";
            }
            else
            {
                dataTable.DefaultView.RowFilter = string.Empty;
            }
            dataTable.DefaultView.Sort = "Fecha DESC";

            // Verificar que haya datos antes de paginar
            if (dataTable.DefaultView.Count > 0)
            {
                var paginatedRows = dataTable.DefaultView.ToTable().AsEnumerable()
                    .Skip(offset).Take(rowsPerPage);

                DataTable paginatedTable = paginatedRows.Any() ? paginatedRows.CopyToDataTable() : dataTable.Clone();

                tablaCreditosApartados.DataSource = paginatedTable;
            }
            else
            {
                tablaCreditosApartados.DataSource = dataTable.Clone(); // Vacío si no hay datos
            }

            CalculateMaxPages(dataTable.DefaultView.Count);
        }

        private void CalculateMaxPages(int rowCount)
        {
            maxPages = (rowCount + rowsPerPage - 1) / rowsPerPage;
            if (maxPages == 0) maxPages = 1;
            if (maxPages < currentPage)
            {
                currentPage = maxPages;
                offset = (currentPage - 1) * rowsPerPage;
            }
            pageLabel.Text = $"Página {currentPage}/{maxPages}";
        }

        private void prev_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                offset -= rowsPerPage;
                currentPage--;
                FiltrarDatos();
            }
        }

        private void next_Click(object sender, EventArgs e)
        {
            if (currentPage < maxPages)
            {
                offset += rowsPerPage;
                currentPage++;
                FiltrarDatos();
            }
        }

        private void Bimprimir_Click(object sender, EventArgs e)
        {
            PrintxlsxSel printxlsxSel = new PrintxlsxSel();
            DialogResult result = printxlsxSel.ShowDialog();

            if (result == DialogResult.OK && printxlsxSel.SelectedOption.HasValue)  // Solo continuar si se seleccionó algo
            {
                int opc = printxlsxSel.SelectedOption.Value;  // Obtener la opción seleccionada
                GenerarExcel(opc);        // Llamar al método que genera el Excel
            }
        }

        private void GenerarExcel(int opc)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            int idSucursal = data.idSucursal;
            DataTable creditosTable = localDM.GetCreditosDataTable(idSucursal);
            DataTable apartadosTable = localDM.GetApartadosExcelDataTable(idSucursal);
            DateTime localDate = DateTime.Now;
            string fecha = localDate.ToString("dd-MM-yyyy");

            if (creditosTable.Rows.Count == 0 && opc == 0)
            {
                MessageBox.Show("No hay creditos disponibles para la sucursal actual.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (apartadosTable.Rows.Count == 0 && opc == 1)
            {
                MessageBox.Show("No hay apartados disponibles para la sucursal actual.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            else if (creditosTable.Rows.Count == 0 && apartadosTable.Rows.Count == 0 && opc == 2)
            {
                MessageBox.Show("No hay informacion disponibles para la sucursal actual.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Especificar la carpeta principal y la subcarpeta
            string carpetaPrincipal = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CasaCejaDocs");
            string subcarpeta = Path.Combine(carpetaPrincipal, "PuntoDeVenta");

            // Utilizamos la subcarpeta para guardar el archivo
            string carpeta = subcarpeta;
            string nombre = "";
            if (opc == 0)
            {
                nombre = "ListaCreditos ";
            }
            else if (opc == 1)
            {
                nombre = "ListaApartados ";
            }
            else if (opc == 2)
            {
                nombre = "ListaCreditosyApartados ";
            }
            string nombreArchivo = nombre + fecha + ".xlsx";
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
                    if (opc == 0 || opc == 2)
                    {
                        // Crear la hoja para los créditos
                        ExcelWorksheet hojaCreditos = paquete.Workbook.Worksheets.Add("ListadoCréditos " + fecha);

                        // Agregar los encabezados de las columnas
                        for (int i = 0; i < creditosTable.Columns.Count; i++)
                        {
                            hojaCreditos.Cells[1, i + 1].Value = creditosTable.Columns[i].ColumnName;
                            hojaCreditos.Cells[1, i + 1].Style.Font.Bold = true;
                            hojaCreditos.Cells[1, i + 1].Style.Font.Size = 14;
                        }

                        // Agregar los datos de las filas
                        for (int fila = 0; fila < creditosTable.Rows.Count; fila++)
                        {
                            for (int col = 0; col < creditosTable.Columns.Count; col++)
                            {
                                hojaCreditos.Cells[fila + 2, col + 1].Value = creditosTable.Rows[fila][col].ToString();
                            }
                        }
                    }

                    if (opc == 1 || opc == 2)
                    {
                        // Crear la hoja para los apartados
                        ExcelWorksheet hojaApartados = paquete.Workbook.Worksheets.Add("ListadoApartados " + fecha);

                        // Agregar los encabezados de las columnas
                        for (int i = 0; i < apartadosTable.Columns.Count; i++)
                        {
                            hojaApartados.Cells[1, i + 1].Value = apartadosTable.Columns[i].ColumnName;
                            hojaApartados.Cells[1, i + 1].Style.Font.Bold = true;
                            hojaApartados.Cells[1, i + 1].Style.Font.Size = 14;
                        }

                        // Agregar los datos de las filas
                        for (int fila = 0; fila < apartadosTable.Rows.Count; fila++)
                        {
                            for (int col = 0; col < apartadosTable.Columns.Count; col++)
                            {
                                hojaApartados.Cells[fila + 2, col + 1].Value = apartadosTable.Rows[fila][col].ToString();
                            }
                        }
                    }

                    // Guardar el archivo en la ruta especificada
                    FileInfo archivo = new FileInfo(rutaArchivo);
                    paquete.SaveAs(archivo);

                    // Mostrar mensaje de éxito si se ha creado correctamente
                    MessageBox.Show(nombre + fecha + ".xlsx" + " se generó correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al generar el archivo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        }
    }
