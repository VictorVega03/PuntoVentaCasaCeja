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
    public partial class AltaCliente : Form
    {
        Action<Usuario> setUser;
        Usuario usuario;

        WebDataManager webDM;
        LocaldataManager localDM;
        bool alta = true;
        bool temporal = true;
        Cliente cliente;
        CurrentData data;
        public AltaCliente(CurrentData data)
        {
            InitializeComponent();
            this.webDM = data.webDM;
            this.data = data;
            this.localDM = webDM.localDM;
            setUser = SetUser;
        }
        private void integerInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
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
                    case Keys.Enter:
                        if (aceptar.Focused || cancelar.Focused)
                            return base.ProcessDialogKey(keyData);
                        SendKeys.Send("{TAB}");
                        break;
                    case Keys.F5:
                        aceptar.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void cancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void aceptar_Click(object sender, EventArgs e)
        {
            if (alta)
            {
                if (txtnombre.Text.Equals("") || txttel.Text.Equals("") || txtcorreo.Text.Equals(""))
                {
                    MessageBox.Show("Favor de completar los campos obligatorios (*)", "Advertencia");
                }
                else
                {
                    int id = localDM.ExisteCliente(txtnombre.Text, txtcorreo.Text, txttel.Text);
                    if (id == -1)
                    {

                        NuevoCliente cl = new NuevoCliente()
                        {
                            nombre = txtnombre.Text,
                            rfc = txtrfc.Text,
                            calle = txtcalle.Text,
                            numero_exterior = txtnoext.Text,
                            numero_interior = txtnoint.Text,
                            colonia = txtcolonia.Text,
                            codigo_postal = txtpostal.Text,
                            ciudad = txtciudad.Text,
                            telefono = txttel.Text,
                            correo = txtcorreo.Text,
                        };
                        id = localDM.clienteTemporal(cl);
                        cliente = new Cliente
                        {
                            id = id,
                            nombre = txtnombre.Text,
                            rfc = txtrfc.Text,
                            calle = txtcalle.Text,
                            numero_exterior = txtnoext.Text,
                            numero_interior = txtnoint.Text,
                            colonia = txtcolonia.Text,
                            codigo_postal = txtpostal.Text,
                            ciudad = txtciudad.Text,
                            telefono = txttel.Text,
                            correo = txtcorreo.Text,
                            activo = -1
                        };
                        send(cl);
                        if (!temporal)
                        {
                            ClearAllText(this);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ya existe un cliente con estos datos", "Advertencia");
                    }
                }
            }
            if (cliente != null && !temporal)
            {
                data.cliente = cliente;
                    this.Close();
            }
            if (data.successful)
            {
                this.Close();
            }
        }
        public void clienteSeleccionado(Cliente cliente)
        {
            data.cliente = cliente;
            ApCrSel sel = new ApCrSel(data);
            if (sel.ShowDialog() == DialogResult.OK)
            {
                this.Close(); 
            }
            if (data.successful)
            {
                this.Close();
            }
        }

        async void send(NuevoCliente cliente)
        {
            Dictionary<string, string> result = await webDM.SendClienteAsync(cliente);
            //MessageBox.Show(result["message"], "Estado: " + result["status"]);
            temporal = true;
            if (result["status"].Equals("error"))
            {
                MessageBox.Show("Numero de Telefono ya registrado");
                temporal = false;
            }
            else
            {
                MessageBox.Show("Cliente Registrado con exito");
                this.Close();
            }
        }
        void ClearAllText(Control con)
        {
            foreach (Control c in con.Controls)
            {
                if (c is TextBox)
                    ((TextBox)c).Clear();
                else
                    ClearAllText(c);
            }
        }

        public void SetUser(Usuario user)
        {
            usuario = user;
        }
    }
}