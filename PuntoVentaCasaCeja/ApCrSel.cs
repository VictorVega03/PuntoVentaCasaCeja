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
    public partial class ApCrSel : Form
    {
        CurrentData data;
        Timer checkSuccessTimer;

        public ApCrSel(CurrentData data)
        {
            InitializeComponent();
            this.data = data;

            // Configuración del Timer
            checkSuccessTimer = new Timer();
            checkSuccessTimer.Interval = 500; // 500ms (medio segundo)
            checkSuccessTimer.Tick += CheckSuccess;
            checkSuccessTimer.Start();

            // Revisión inicial
            if (data.successful)
            {
                this.Close();
            }
        }

        // Método que se ejecuta periódicamente para comprobar si data.successful es true
        private void CheckSuccess(object sender, EventArgs e)
        {
            if (data.successful)
            {
                checkSuccessTimer.Stop(); // Detenemos el timer para evitar ejecuciones repetidas
                this.Close(); // Cerramos la ventana
            }
        }

        private void credito_Click(object sender, EventArgs e)
        {
            if (data.carrito.Count > 0)
            {
                RegistrarCredito rc = new RegistrarCredito(data);
                rc.ShowDialog(this); // Muestra el diálogo

                // El Timer manejará el cierre si data.successful es true
            }
            else
            {
                MessageBox.Show("Favor de agregar productos al carrito", "Advertencia");
            }
        }

        private void apartado_Click(object sender, EventArgs e)
        {
            if (data.carrito.Count > 0)
            {
                RegistrarApartado ra = new RegistrarApartado(data);
                ra.ShowDialog(this); // Muestra el diálogo

                // El Timer manejará el cierre si data.successful es true
            }
            else
            {
                MessageBox.Show("Favor de agregar productos al carrito", "Advertencia");
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
                        credito.PerformClick();
                        break;
                    case Keys.F2:
                        apartado.PerformClick();
                        break;
                    case Keys.F3:
                        verCred.PerformClick();
                        break;
                    case Keys.F4:
                        verApa.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void verCred_Click(object sender, EventArgs e)
        {
            VerCredApa vca = new VerCredApa(0, data);
            vca.ShowDialog();
            // El Timer manejará el cierre si data.successful es true
        }

        private void verApa_Click(object sender, EventArgs e)
        {
            VerCredApa vca = new VerCredApa(1, data);
            vca.ShowDialog();
            // El Timer manejará el cierre si data.successful es true
        }
    }
}
