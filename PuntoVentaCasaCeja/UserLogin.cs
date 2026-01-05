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
    public partial class UserLogin : Form
    {
        LocaldataManager localDM;
        Action<Usuario> setUser;
        bool admin;
        public UserLogin(LocaldataManager localdataManager, Action<Usuario> setUser, bool admin)
        {
            InitializeComponent();
            this.localDM = localdataManager;
            this.setUser = setUser;
            this.admin = admin;
            automatizarLogin();
        }
        void automatizarLogin()
        {
            user.Text = "admin";
            password.Text = "admin";           
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void accept_Click(object sender, EventArgs e)
        {
            string usr = user.Text;
            string pass = password.Text;
            if (usr.Equals("") || pass.Equals(""))
            {
                MessageBox.Show("Favor de introducir el usuario y la contraseña", "Advertencia");
            }
            else
            {
                Usuario usuarioActivo = localDM.getLoginUser(usr, pass);
                if (usuarioActivo != null)
                {
                    if (admin && usuarioActivo.es_raiz > 1)
                    {
                        MessageBox.Show("Esta cuenta no tiene permisos administrativos para realizar esta acción", "Advertencia");
                    }
                    else
                    {
                        setUser(usuarioActivo);
                        this.DialogResult = DialogResult.Yes;
                        this.Close();

                    }

                }
                else
                {
                    MessageBox.Show("Usuario y/o contraseña no válidos", "Advertencia");
                }

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
                    case Keys.F1:
                        user.Focus();
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

        private void user_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                SendKeys.Send("{TAB}");
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                accept.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void UserLogin_Load(object sender, EventArgs e)
        {
            if (admin)
            {
                this.Text = "Autenticación de Administrador";
                groupBox1.Text = "AUTENTICACIÓN DE ADMINISTRADOR";
            }
            else
            {
                this.Text = "Autenticación de Cajero";
                groupBox1.Text = "AUTENTICACIÓN DE CAJERO";
            }
        }
    }
}