using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PuntoVentaCasaCeja
{
    public partial class Usuarios : Form
    {
        WebDataManager webDM;
        CurrentData data;

        public Usuarios(CurrentData data)
        {
            InitializeComponent();
            this.data = data;
            this.webDM = data.webDM;
            loadData();
        }

        private void loadData()
        {
            DataTable data = new DataTable();
            if (txtbuscar.Text.Equals(""))
            {
                data = webDM.localDM.getUsuariosCajero();
            }
            else
            {
                data = webDM.localDM.getUsuariosCajero(txtbuscar.Text);
            }
            tablaUsuarios.DataSource = data;
        }

        private void txtbuscar_TextChanged(object sender, EventArgs e)
        {
            loadData();
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
                        crear.PerformClick();
                        break;
                    case Keys.F3:
                        Bmodificar.PerformClick();
                        break;
                    case Keys.Down:
                        tablaUsuarios.Focus();
                        SendKeys.Send("{DOWN}");
                        break;
                    case Keys.Up:
                        tablaUsuarios.Focus();
                        SendKeys.Send("{UP}");
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void salir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void crear_Click(object sender, EventArgs e)
        {
            CrearUsuario crearUsuario = new CrearUsuario(webDM);
            crearUsuario.ShowDialog();
            loadData();
        }

        private void Bmodificar_Click(object sender, EventArgs e)
        {
            modify();
            loadData();
        }

        private void modify()
        {
            if (tablaUsuarios.SelectedCells.Count > 0)
            {
                int rowIndex = tablaUsuarios.SelectedCells[0].RowIndex; // se obtiene el indice de la fila seleccionada
                DataGridViewRow row = tablaUsuarios.Rows[rowIndex]; // se obtiene la fila seleccionada de la tabla
                ModifUsuario modifUsuario = new ModifUsuario(webDM);
                modifUsuario.setData(row);
                modifUsuario.ShowDialog();
            }
        }

        private void tablaUsuarios_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                crear.PerformClick();
            }
            if (e.KeyCode == Keys.F3)
            {
                Bmodificar.PerformClick();
            }
        }
    }
}
