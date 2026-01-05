using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PuntoVentaCasaCeja
{
    public partial class PrintxlsxSel : Form
    {
        private int? selectedOption = null; // Usamos nullable para manejar el valor no seleccionado

        public int? SelectedOption
        {
            get { return selectedOption; }
        }

        public PrintxlsxSel()
        {
            InitializeComponent();
            List<string> items = new List<string> { "Creditos", "Apartados", "Todo" };
            BoxOpc.DataSource = items;
        }

        private void Baceptar_Click(object sender, EventArgs e)
        {
            selectedOption = BoxOpc.SelectedIndex; // Al aceptar, guardamos la opción seleccionada
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None)
            {
                switch (keyData)
                {
                    case Keys.F1:
                        BoxOpc.Focus();
                        BoxOpc.DroppedDown = true;
                        break;
                    case Keys.Enter:
                        Baceptar.PerformClick();
                        return true;
                    case Keys.F5:
                        Baceptar.PerformClick();
                        return true;
                    case Keys.Escape:
                        this.DialogResult = DialogResult.Cancel;  // Salir sin guardar información
                        this.Close();
                        return true;
                }
            }
            return base.ProcessDialogKey(keyData);
        }

        private void Bcancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}