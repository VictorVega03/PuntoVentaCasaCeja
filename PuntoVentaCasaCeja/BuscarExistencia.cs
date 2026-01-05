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
    public partial class BuscarExistencia : Form
    {
        WebDataManager webDM;
        Dictionary<string, int> mapamedidas;
        Dictionary<string, int> mapacategorias;
        Dictionary<string, string> urls;

        int idsucursal;
        int offset;
        int currentPage;
        int maxPages;
        int rowsPerPage;
        int currentcat;
        int currentmed;
        string searchparam;
        Action <Producto> addProd;
        public BuscarExistencia(WebDataManager webDataManager, Action<Producto> addProd, int idsucursal, string searchparam)
        {
            InitializeComponent();
            this.webDM = webDataManager;
            mapacategorias = new Dictionary<string, int>();
            mapamedidas = new Dictionary<string, int>();
            offset = 0;
            currentcat = 0;
            currentmed = 0;
            currentPage = 1;
            maxPages = 1;
            rowsPerPage = 19;
            urls = new Dictionary<string, string>();
            this.addProd = addProd;
            this.idsucursal = idsucursal;
            this.searchparam = searchparam;
        }
        void loadData()
        {
            DataTable tablacatalogo;
            string arg = "";
            int rowCount;
            if (boxcategoria.SelectedIndex > 0)
            {
                arg += "AND productos.categoria_id = " + mapacategorias[boxcategoria.SelectedItem.ToString()] + " ";
            }
            if (boxmedida.SelectedIndex > 0)
            {
                arg += "AND productos.medida_id = " + mapamedidas[boxmedida.SelectedItem.ToString()] + " ";
            }
            if (txtbuscar.Text.Equals(""))
            {
                rowCount = webDM.localDM.getProductosRowCount(arg);
                calculateMaxPages(rowCount);
                tablacatalogo = webDM.localDM.getProductos(offset.ToString(), arg);

            }
            else
            {
                rowCount = webDM.localDM.getProductosRowCount(arg, txtbuscar.Text);
                calculateMaxPages(rowCount);
                tablacatalogo = webDM.localDM.getProductos(offset.ToString(), arg, txtbuscar.Text);

            }
            catalogo.DataSource = tablacatalogo;
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
        private void catalogo_KeyDown(object sender, KeyEventArgs e)
        {
            
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                    selectProd();                    
                    break;
                    case Keys.F1:
                        txtbuscar.Focus();
                        break;
                    case Keys.F2:
                        boxcategoria.DroppedDown = true;
                        boxcategoria.Focus();
                        break;
                    case Keys.F3:
                        boxmedida.DroppedDown = true;
                        boxmedida.Focus();
                        break;
                    case Keys.E:
                    if (e.Modifiers == Keys.Alt) 
                        verExistencia();
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
                }
            
            
        }
        private void selectProd()
        {
            if (catalogo.SelectedRows.Count > 0)
            {
                Producto p = webDM.localDM.GetProductByCode(catalogo.SelectedRows[0].Cells[1].Value.ToString());
                if (p != null)
                {
                    addProd(p);
                    this.Close();
                }
                
            }
            else MessageBox.Show("Favor de seleccionar un producto", "Advertencia");
        }
        private async void verExistencia()
        {
            string idprod = catalogo.SelectedRows[0].Cells[0].Value.ToString();
            string titulo = "Producto: "+ catalogo.SelectedRows[0].Cells[2].Value.ToString();
            List<ProductoExistencia> prodex = await webDM.getProductoExistencia(idprod);
            if (prodex != null)
            {
                //foreach(ProductoExistencia p in prodex)
                //{
                //    if (p.RAZON_SOCIAL.Equals(sucursal))
                //    {
                //        prodex.Remove(p);
                //        prodex.Insert(0, p);
                //        break;
                //    }
                //}
                if (prodex.Count == 0)
                {
                    MessageBox.Show("No se encontraron existencias de este producto, favor de intentar más tarde", "Advertencia");
                }
                else
                {
                    VerExistencia ve = new VerExistencia(prodex, titulo);
                    ve.ShowDialog();
                }
                
            }
            else MessageBox.Show("No se pudo conectar con el servidor, favor de intentar más tarde", "Advertencia");
        }

        private void BuscarExistencia_Load(object sender, EventArgs e)
        {
            mapacategorias = webDM.localDM.getIndicesCategorias();
            mapamedidas = webDM.localDM.getIndicesMedidas();
            boxcategoria.Items.Add("Seleccionar categoría");
            boxmedida.Items.Add("Seleccionar medida");
            boxcategoria.Items.AddRange(mapacategorias.Keys.ToArray());
            boxmedida.Items.AddRange(mapamedidas.Keys.ToArray());
            boxcategoria.SelectedIndex = currentcat;
            boxmedida.SelectedIndex = currentmed;
            txtbuscar.Text = searchparam;
            loadData();
            txtbuscar.Focus();
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
        private void boxcategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentcat = boxcategoria.SelectedIndex;
            loadData();

        }

        private void txtbuscar_TextChanged(object sender, EventArgs e)
        {
            loadData();
        }

        private void boxmedida_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentmed = boxmedida.SelectedIndex;
            loadData();
        }

        private void exit_Click(object sender, EventArgs e)
        {
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
                    case Keys.F1:
                        txtbuscar.Focus();
                        break;
                    case Keys.F2:
                        boxcategoria.DroppedDown = true;
                        boxcategoria.Focus();
                        break;
                    case Keys.F3:
                        boxmedida.DroppedDown = true;
                        boxmedida.Focus();
                        break;
                    case Keys.Down:
                        catalogo.Focus();
                        SendKeys.Send("{DOWN}");
                        break;
                    case Keys.Up:
                        catalogo.Focus();
                        SendKeys.Send("{UP}");
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void details_Click(object sender, EventArgs e)
        {
            if (catalogo.SelectedRows.Count > 0)
            {
                verExistencia();
            }
            else
            {
                MessageBox.Show("Favor de seleccionar un producto", "Advertencia");
            }
        }

        private void txtbuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Down)
            {
                catalogo.Focus();
                SendKeys.Send("{DOWN}");
            }

            if (e.KeyData == Keys.Up)
            {
                catalogo.Focus();
                SendKeys.Send("{UP}");
            }
            if (e.KeyData == Keys.Enter)
            {
                selectProd();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            if(e.Modifiers == Keys.Alt && e.KeyData == Keys.E)
            {
                verExistencia();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }
    }
}
