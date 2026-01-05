using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Windows.Forms;

namespace PuntoVentaCasaCeja
{

    public partial class CrearProducto : Form
    {
        Dropcustom drop_categoria;
        Dropcustom drop_unidad;
        List<NuevoProducto> productos;
        BindingSource source;
        WebDataManager webDM;
        List<int> id_temp;
        Dictionary<string, int> mapamedidas;
        Dictionary<string, int> mapacategorias;
        LocaldataManager localDM;
        public CrearProducto(WebDataManager webDataManager, Dictionary<string, int> mapamedidas,  Dictionary<string, int> mapacategorias)
        {
            InitializeComponent();
            drop_categoria = new Dropcustom();
            drop_unidad = new Dropcustom();            
            drop_categoria.setLabelText("SELECCIONAR CATEGORÍA");
            drop_unidad.setLabelText("SELECCIONAR MEDIDA");
            productos = new List<NuevoProducto>();
            source = new BindingSource();
            this.mapamedidas = mapamedidas;
            this.mapacategorias = mapacategorias;
            source.DataSource = productos;
            tabla.DataSource = source;
            webDM = webDataManager;
            localDM = webDM.localDM;
            id_temp = new List<int>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtbarras.Focus();
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
            drop_unidad.setItems(unidades,  actualizarUnidad);
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
            if(e.KeyCode == Keys.Enter)
            {
                mostrar_medidas_Click(sender,e);
            }
        }

        private void txtcategoria_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if(e.KeyData == Keys.Enter)
            {
                mostrar_categorias_Click(sender,e);
            }
        }
        void limpiarCampos()
        {
            txtbarras.Clear();
            txtnombre.Clear();
            txtunidad.Clear();
            txtpresentacion.Clear();
            txtcategoria.Clear();
            txtmenudeo.Clear();
            txtmayoreo.Clear();
            txtespecial.Clear();
            txtvendedor.Clear();
            txtcantmay.Clear();
            MessageBox.Show("Producto Agregado ✔", "Éxito");

        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (txtbarras.Text.Equals("") || txtnombre.Text.Equals("") || txtunidad.Text.Equals("") || txtcategoria.Text.Equals(""))
            {
                MessageBox.Show("Debes llenar todos los campos requeridos", "Advertencia");
            }
            else
            {
                bool next = true;
                foreach (NuevoProducto prod in productos)
                {
                    if (prod.codigo.Equals(txtbarras.Text))
                    {
                        next = false;
                        MessageBox.Show("Ya se ha agregado un producto con ese código", "Adevertencia");
                    }
                }
                if (next)
                {
                    Producto p = localDM.searchProductByCode(txtbarras.Text);
                    if (p == null)
                    {
                        productos.Add(
                        new NuevoProducto
                        {
                            codigo = txtbarras.Text,
                            nombre = txtnombre.Text,
                            presentacion = txtpresentacion.Text,
                            menudeo = double.Parse(txtmenudeo.Text.Equals("") || txtmenudeo.Text.Equals(".") ? "0" : txtmenudeo.Text),
                            mayoreo = double.Parse(txtmayoreo.Text.Equals("") || txtmayoreo.Text.Equals(".") ? "0" : txtmayoreo.Text),
                            cantidad_mayoreo = int.Parse(txtcantmay.Text.Equals("") ? "0" : txtcantmay.Text),
                            especial = double.Parse(txtespecial.Text.Equals("") || txtespecial.Text.Equals(".") ? "0" : txtespecial.Text),
                            vendedor = double.Parse(txtvendedor.Text.Equals("") || txtvendedor.Text.Equals(".") ? "0" : txtvendedor.Text),
                            imagen = "...",
                            medida_id = mapamedidas[txtunidad.Text],
                            categoria_id = mapacategorias[txtcategoria.Text]
                        });
                        source.ResetBindings(false);
                        limpiarCampos();
                    }
                    else
                    {
                        if (p.activo == 1)
                        {
                            MessageBox.Show("Ya existe un producto registrado con este código de barras", "Advertencia");
                        }
                        else
                        {
                            MessageBox.Show("Ya existe un producto registrado con este código de barras pero pero fue eliminado anteriormente", "Advertencia");

                        }
                    }
                }
                

            }            
        }
        private async void enviar(List<NuevoProducto> data)
        {
            Dictionary<string, string> result = await webDM.SendProductosAsync(data);
           
            if (result["status"].Equals("success"))
            {
                webDM.localDM.quitarAltaTempóral(id_temp);
                MessageBox.Show(result["message"], "Completado");
                productos.Clear();
                source.ResetBindings(false);
            }else if (result["status"].Equals("error"))
            {
                webDM.localDM.quitarAltaTempóral(id_temp);
                MessageBox.Show(result["message"], "Error");
            }
            else
            {
                MessageBox.Show(result["message"], "Error de conexion");
                productos.Clear();
                source.ResetBindings(false);
            }
            
        }
        private void upload_Click(object sender, EventArgs e)
        {
            if (productos.Count > 0)
            {
                
                id_temp = webDM.localDM.altaTemporal(productos);
                enviar(productos);                    
            }
            else
            {
                MessageBox.Show("No se han agregado productos", "Advertencia");
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
                        if (add.Focused || upload.Focused || button1.Focused)
                            return base.ProcessDialogKey(keyData);
                        SendKeys.Send("{TAB}");
                        break;
                    case Keys.F5:
                        add.PerformClick();
                        break;
                    case Keys.F6:
                        upload.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
    }
}
