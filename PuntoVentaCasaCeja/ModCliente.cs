using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;

namespace PuntoVentaCasaCeja
{
    public partial class ModificarCliente : Form
    { 
        Action<Usuario> setUser;
        Usuario usuario;
        WebDataManager webDM;
        LocaldataManager localDM;
        CurrentData data;
        Cliente cliente;
        public ModificarCliente(CurrentData data, Cliente cliente)
        {
            InitializeComponent();
            this.webDM = data.webDM;
            this.data = data;
            this.localDM = webDM.localDM;
            this.cliente = cliente;
            txtnombre.Text = cliente.nombre;
            txtcalle.Text = cliente.calle;
            txtcolonia.Text = cliente.colonia;
            txtciudad.Text = cliente.ciudad;
            txttel.Text = cliente.telefono;
            txtpostal.Text = cliente.codigo_postal;
            txtrfc.Text = cliente.rfc;
            txtnoext.Text = cliente.numero_exterior;
            txtnoint.Text = cliente.numero_interior;
            txtcorreo.Text = cliente.correo;

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
                    case Keys.F5:
                        aceptar.PerformClick();
                        break;
                    case Keys.F6:
                        cancelar.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }
        /*private void aceptar_Click(object sender, EventArgs e)
        {
            Cliente c = new Cliente();
            c.id = cliente.id;
            c.calle = txtcalle.Text;
            c.ciudad = txtciudad.Text;
            c.colonia = txtcolonia.Text;
            c.nombre = txtnombre.Text;
            c.telefono = txttel.Text;
            c.codigo_postal = txtpostal.Text;
            c.rfc = txtrfc.Text;
            c.numero_exterior = txtnoext.Text;
            c.numero_interior = txtnoint.Text;
            c.correo = txtcorreo.Text;
            if (localDM.UpdateCliente(c))
            {
                MessageBox.Show("Cliente modificado correctamente");
            }
            else
            {
                MessageBox.Show("Error al modificar cliente");
            }
            this.Close();
        }*/
        private async void aceptar_Click(object sender, EventArgs e)
        {
            if (cliente.nombre != ""||cliente.telefono!=""||cliente.correo!="") { 
            Cliente c = new Cliente();
            c.id = cliente.id;
            c.calle = txtcalle.Text;
            c.ciudad = txtciudad.Text;
            c.colonia = txtcolonia.Text;
            c.nombre = txtnombre.Text;
            c.telefono = txttel.Text;
            c.codigo_postal = txtpostal.Text;
            c.rfc = txtrfc.Text;
            c.numero_exterior = txtnoext.Text;
            c.numero_interior = txtnoint.Text;
            c.correo = txtcorreo.Text;

            // Llamar al método asíncrono para actualizar el cliente
            var updateResult = await webDM.UpdateClienteAsync(c);

            if (updateResult["status"] == "success")
            {
                MessageBox.Show("Cliente modificado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                localDM.UpdateCliente(c);
            }
            else
            {
                MessageBox.Show($"Error al modificar cliente: {updateResult["message"]}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Close();
            }
            else
            {
                MessageBox.Show("Aun hay campos vacios", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
     }


        private void cancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
