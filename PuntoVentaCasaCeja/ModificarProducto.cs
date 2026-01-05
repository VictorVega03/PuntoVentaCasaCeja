using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace PuntoVentaCasaCeja
{
    public partial class ModificarProducto : Form
    {
        Dropcustom drop_categoria;
        Dropcustom drop_unidad;
        Dictionary<string, int> mapamedidas;
        Dictionary<string, int> mapacategorias;
        WebDataManager webDM;
        string idProducto;
        string currentCode;
        public ModificarProducto( WebDataManager webDataManager, Dictionary<string, int> DiccionarioMedidas,
        Dictionary<string, int> DiccionarioCategorias)
        {
            InitializeComponent();
            this.webDM = webDataManager;
            drop_categoria = new Dropcustom();
            drop_unidad = new Dropcustom();
            drop_categoria.setLabelText("SELECCIONAR CATEGORÍA");
            drop_unidad.setLabelText("SELECCIONAR MEDIDA");
            mapacategorias = DiccionarioCategorias;
            mapamedidas = DiccionarioMedidas;
            idProducto = "";
        }
        private void mostrar_categorias_Click(object sender, EventArgs e)
        {
            if (drop_categoria.IsDisposed)
            {
                drop_categoria = new Dropcustom();
                drop_categoria.setLabelText("SELECCIONAR CATEGORÍA");
            }
            string[] categorias = new List<string>(mapacategorias.Keys).ToArray();


            drop_categoria.setItems(categorias, actualizarCategoria);
            drop_categoria.Show();
            drop_categoria.Focus();
            drop_categoria.WindowState = FormWindowState.Normal;
        }

        private void mostrar_medidas_Click(object sender, EventArgs e)
        {
            if (drop_unidad.IsDisposed)
            {
                drop_unidad = new Dropcustom();
                drop_unidad.setLabelText("SELECCIONAR MEDIDA");
            }
            string[] unidades = new List<string>(mapamedidas.Keys).ToArray();
            drop_unidad.setItems(unidades, actualizarUnidad);
            drop_unidad.Show();
            drop_unidad.Focus();
            drop_unidad.WindowState = FormWindowState.Normal;
        }
        public void actualizarCategoria(string value)
        {
            txtcategoria.Text = value;
            txtmenudeo.Focus();
        }

        public void actualizarUnidad(string value)
        {
            txtunidad.Text = value;
            txtpresentacion.Focus();
        }

        private void txtunidad_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                mostrar_medidas_Click(sender, e);
            }
        }

        private void txtcategoria_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                mostrar_categorias_Click(sender, e);
            }
        }
        private void numericInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
        }
        public void setData(DataGridViewRow row)
        {
            idProducto = row.Cells[0].Value.ToString();
            txtbarras.Text= row.Cells[1].Value.ToString();
            currentCode = row.Cells[1].Value.ToString();
            txtnombre.Text= row.Cells[2].Value.ToString();
            txtcategoria.Text = row.Cells[3].Value.ToString();
            txtunidad.Text = row.Cells[4].Value.ToString();
            txtpresentacion.Text= row.Cells[5].Value.ToString();
            txtmenudeo.Text = row.Cells[7].Value.ToString();
            txtmayoreo.Text = row.Cells[8].Value.ToString();
            txtcantmay.Text = row.Cells[9].Value.ToString();
            txtespecial.Text = row.Cells[10].Value.ToString();
            txtvendedor.Text = row.Cells[11].Value.ToString();

        }

        private void add_Click(object sender, EventArgs e)
        {
            if(txtbarras.Text.Equals("") || txtnombre.Text.Equals("") || txtunidad.Text.Equals("") || txtcategoria.Text.Equals(""))
            {
                MessageBox.Show("Debes llenar todos los campos requeridos", "Advertencia");
            }
            else
            {
                Dictionary<string, string> content = new Dictionary<string, string>();
                content["_method"] = "PATCH";
                if (!txtbarras.Text.Equals(currentCode))
                {
                    
                    content["codigo"] = txtbarras.Text;
                }                
                content["nombre"] = txtnombre.Text;
                content["presentacion"] = txtpresentacion.Text;
                content["menudeo"] = txtmenudeo.Text.Equals("") || txtmenudeo.Text.Equals(".") ? "0" : double.Parse(txtmenudeo.Text).ToString("0.00");
                content["mayoreo"] = txtmayoreo.Text.Equals("") || txtmayoreo.Text.Equals(".") ? "0" : double.Parse(txtmayoreo.Text).ToString("0.00");
                content["cantidad_mayoreo"] = txtcantmay.Text.Equals("") ? "0" : txtcantmay.Text;
                content["especial"] = txtespecial.Text.Equals("") || txtespecial.Text.Equals(".") ? "0" : double.Parse(txtespecial.Text).ToString("0.00");
                content["vendedor"] = txtvendedor.Text.Equals("") || txtvendedor.Text.Equals(".") ? "0" : double.Parse(txtvendedor.Text).ToString("0.00");
                if (mapamedidas.ContainsKey(txtunidad.Text))
                content["medida_id"] = mapamedidas[txtunidad.Text].ToString();
                if (mapacategorias.ContainsKey(txtcategoria.Text))
                content["categoria_id"] = mapacategorias[txtcategoria.Text].ToString();
                modify(content);
                
            }
        }
        private async void modify(Dictionary<string, string> producto)
        {
            if (await webDM.ModifyProductoAsync(idProducto, producto))
            {
                this.DialogResult = DialogResult.Yes;
                this.Close();
            }
                
            else MessageBox.Show("No se pudo conectar con el servidor, favor de intentar más tarde", "Advertencia");
        }

        private void upload_Click(object sender, EventArgs e)
        {
            DialogResult result= MessageBox.Show("¿Desea dar de baja este producto? (Ya no figurará en el catalogo)", "Advertencia", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                disable();
            }
        }
        public async void disable()
        {
            if(await webDM.DisableProductoAsync(idProducto))
            this.Dispose();
            else MessageBox.Show("No se pudo conectar con el servidor, favor de intentar más tarde", "Advertencia");
        }

        private void integerInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
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
                    case Keys.Enter:
                        if (cancel.Focused || save.Focused || remove.Focused)
                            return base.ProcessDialogKey(keyData);
                        SendKeys.Send("{TAB}");
                        break;
                    case Keys.F5:
                        save.PerformClick();
                        break;
                    case Keys.F6:
                        remove.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void ModificarProducto_Load(object sender, EventArgs e)
        {
            txtbarras.Focus();
        }
        private void buttonEnter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                (sender as Button).PerformClick();
            }
        }
    }
}