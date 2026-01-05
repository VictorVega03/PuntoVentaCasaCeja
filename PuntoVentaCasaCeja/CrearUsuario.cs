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
    public partial class CrearUsuario : Form
    {
        WebDataManager webDM;

        public CrearUsuario(WebDataManager webDM)
        {
            InitializeComponent();
            this.webDM = webDM;
            boxtipo.SelectedIndex = 1;
        }

        private void accept_Click(object sender, EventArgs e)
        {
            if ((txtnombre.Text.Equals("")) || txtcorreo.Text.Equals("") || txttelefono.Text.Equals("") || txtusuario.Text.Equals("") || txtclave.Text.Equals("")){
                MessageBox.Show("Favor de completar todos los campos", "Advertencia");
            }
            else
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data["nombre"] = txtnombre.Text;
                data["correo"] = txtcorreo.Text;
                data["telefono"] = txttelefono.Text;
                data["usuario"] = txtusuario.Text;
                data["clave"] = txtclave.Text;
                data["es_raiz"] = (boxtipo.SelectedIndex + 1).ToString();
                send(data);
            }
        }

        private async void send(Dictionary<string, string> usuario)
        {
            if (await webDM.SendUsuarioAsync(usuario))
            {
                txtnombre.Text = "";
                txtcorreo.Text = "";
                txttelefono.Text = "";
                txtusuario.Text = "";
                txtclave.Text = "";
                this.Close();
            }
            else
                MessageBox.Show("No se pudo conectar con el servidor, favor de intentar más tarde", "Advertencia");
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
                        accept.PerformClick();
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
        private void integerInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}
