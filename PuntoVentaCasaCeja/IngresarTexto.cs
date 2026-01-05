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
    public partial class IngresarTexto : Form
    {
        Action<string> setText;
        string title;
        public IngresarTexto(string titulo, Action<string> setText)
        {
            InitializeComponent();
            this.title = titulo;
            this.setText = setText;
        }

        private void aceptar_Click(object sender, EventArgs e)
        {
            if (texto.Text.Equals(""))
            {
                MessageBox.Show("Favor de ingresar el dato", "Advertencia");
            }
            else
            {
                setText(texto.Text);
                this.Close();
            }
        }
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None)
            {
                switch (keyData)
                {
                    case Keys.Escape:
                        cancelar.PerformClick();
                        break;
                    case Keys.Enter:
                        aceptar.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void IngresarTexto_Load(object sender, EventArgs e)
        {
            this.Text = title;
            groupBox1.Text = title;
        }

        private void cancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }


}
