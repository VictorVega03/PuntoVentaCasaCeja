using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PuntoVentaCasaCeja.Properties;
using System.Drawing.Text;

namespace PuntoVentaCasaCeja
{
    public partial class ConfigWindow : Form
    {
        Action<Usuario> setUser;
        Usuario usuario;
        static Usuario activador = null;
        static Usuario admin = null;
        WebDataManager webDM;
        LocaldataManager localDM;
        Dictionary<string, int> mapasucursales;
        List<string> sucursales;
        List<string> listfont;
        List<int> listSizes;
        CurrentData data;

        public ConfigWindow(LocaldataManager localdata, CurrentData data)
        {
            InitializeComponent();
            this.data = data;
            this.localDM = localdata;
            mapasucursales = localDM.getIndicesSucursales();
            sucursales = new List<string>(mapasucursales.Keys);
            boxsucursal.DataSource = sucursales;
            listfont = new List<string>();
            listSizes = new List<int> { 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

            txtid.TextChanged += new EventHandler(ValidateFields);
            tamaños.SelectedIndexChanged += new EventHandler(ValidateFields);
            fuentes.SelectedIndexChanged += new EventHandler(ValidateFields);
            boxsucursal.SelectedIndexChanged += new EventHandler(ValidateFields);
            txtprintername.TextChanged += new EventHandler(ValidateFields);
            tipo.SelectedIndexChanged += new EventHandler(ValidateFields);

            // Validar campos al inicio
            ValidateFields(null, null);
        }

        // Método para validar los campos
        private void ValidateFields(object sender, EventArgs e)
        {
            bool isValid = tamaños.SelectedIndex != -1 &&
                           !string.IsNullOrEmpty(txtid.Text) &&
                           fuentes.SelectedIndex != -1 &&
                           boxsucursal.SelectedIndex != -1 &&
                           !string.IsNullOrEmpty(txtprintername.Text) &&
                           tipo.SelectedIndex != -1;

            guardar.Enabled = isValid;
        }

        private void selectprinter_Click(object sender, EventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            DialogResult result = pd.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtprintername.Text = pd.PrinterSettings.PrinterName;
            }
        }

        private void guardar_Click(object sender, EventArgs e)
        {
            if (tamaños.SelectedIndex == -1)
            {
                MessageBox.Show("No se ha establecido el tamaño de texto", "Advertencia");
                return;
            }
            if (txtid.Text.Equals(""))
            {
                MessageBox.Show("No se ha establecido el ID de caja", "Advertencia");
                return;
            }
            string selectsedsucursal = boxsucursal.SelectedItem.ToString();
            if (mapasucursales.ContainsKey(selectsedsucursal))
            {
                Settings.Default["sucursalid"] = mapasucursales[selectsedsucursal];
                data.idSucursal = mapasucursales[selectsedsucursal];
            }
            else
            {
                Settings.Default["sucursalid"] = 0;
            }
            Settings.Default["fontName"] = fuentes.SelectedItem.ToString();
            Settings.Default["fontSize"] = int.Parse(tamaños.SelectedItem.ToString());
            Settings.Default["printername"] = txtprintername.Text;
            Settings.Default["posid"] = int.Parse(txtid.Text);
            Settings.Default["printertype"] = tipo.SelectedIndex;
            Settings.Default.Save();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ConfigWindow_Load(object sender, EventArgs e)
        {
            tamaños.DataSource = listSizes;
            string key = mapasucursales.FirstOrDefault(x => x.Value == int.Parse(Settings.Default["sucursalid"].ToString())).Key;
            boxsucursal.SelectedIndex = sucursales.IndexOf(key) == -1 ? 0 : sucursales.IndexOf(key);
            txtprintername.Text = Settings.Default["printername"].ToString();
            txtid.Text = Settings.Default["posid"].ToString();
            tamaños.SelectedIndex = listSizes.IndexOf(int.Parse(Settings.Default["fontSize"].ToString()));
            tipo.SelectedIndex = int.Parse(Settings.Default["printertype"].ToString());
            using (InstalledFontCollection col = new InstalledFontCollection())
            {
                foreach (FontFamily fa in col.Families)
                {
                    fuentes.Items.Add(fa.Name);
                    listfont.Add(fa.Name);
                }
            }
            fuentes.SelectedIndex = listfont.IndexOf(Settings.Default["fontName"].ToString());
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
                    case Keys.F5:
                        guardar.PerformClick();
                        break;
                    case Keys.F6:
                        ConfigSuc.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void ConfigSuc_Click(object sender, EventArgs e)
        {
            UserLogin login = new UserLogin(localDM, setAdmin, true);
            DialogResult resultLogin = login.ShowDialog();
            if (resultLogin == DialogResult.Yes)
            {
                ConfigSuc cs = new ConfigSuc(localDM,data);
                var result = cs.ShowDialog();
                if (result == DialogResult.OK)
                {
                    boxsucursal.SelectedItem = cs.SelectedSucursal;
                    txtid.Text = cs.IdCaja;
                }
            }
            else
            {
                MessageBox.Show("Autenticación Fallida");
            }
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
        }

    }
}