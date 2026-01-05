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
    public partial class MetodoPago : Form
    {
        double total;
        Action<int, double> abonar;
        double abonado;       
        CurrentData data;        

        public MetodoPago(double total, Action <int, double> Abonar, CurrentData data)
        {
            InitializeComponent();  
            //this.abonado = data.totalabonado;
            this.total = total;
            this.abonar = Abonar;           
            this.data = data;                         
        }
        private void MetodoPago_Load(object sender, EventArgs e)
        {          
            lbltotal.Text = total.ToString("0.00");
            lblabonado.Text = abonado.ToString("0.00");
            lblfaltante.Text = total.ToString("0.00");
            if (data.isventa)
            {
                efectivo_Click(sender, e);
            }
        }

        private void efectivo_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Efectivo mp "+data.porcentajeDesc);
            IngresarMonto im = new IngresarMonto(1, abonar, setTotal, total, data);            
            im.ShowDialog();
            if (total <= 0)
            {
                this.Close();
            }
        }
        void setTotal(double cant)
        {
            total -= cant;
            abonado += cant;
            data.totalabonado = abonado;            
            lblfaltante.Text = total.ToString("0.00");
            lblabonado.Text = abonado.ToString("0.00");
        }

        private void debito_Click(object sender, EventArgs e)
        {

            Console.WriteLine("debito mp " + data.porcentajeDesc);
            IngresarMonto im = new IngresarMonto(2, abonar, setTotal, total, data);
            im.ShowDialog();
            if (total <= 0)
            {
                this.Close();
            }
        }

        private void credito_Click(object sender, EventArgs e)
        {
            Console.WriteLine("credito mp " + data.porcentajeDesc);
            IngresarMonto im = new IngresarMonto(3, abonar, setTotal, total, data);
            im.ShowDialog();
            if (total <= 0)
            {
                this.Close();
            }
        }

        private void cheque_Click(object sender, EventArgs e)
        {
            Console.WriteLine("cheque mp " + data.porcentajeDesc);
            IngresarMonto im = new IngresarMonto(4, abonar, setTotal, total, data);
            im.ShowDialog();
            if (total <= 0)
            {
                this.Close();
            }
        }

        private void transferencia_Click(object sender, EventArgs e)
        {
            Console.WriteLine("transferencia mp " + data.porcentajeDesc);
            IngresarMonto im = new IngresarMonto(5, abonar, setTotal, total, data);
            im.ShowDialog();
            if (total <= 0)
            {
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
                        exit.PerformClick();
                        break;
                    case Keys.F1:
                        efectivo.PerformClick();
                        break;
                    case Keys.F2:
                        debito.PerformClick();
                        break;
                    case Keys.F3:
                        credito.PerformClick();
                        break;
                    case Keys.F4:
                        cheque.PerformClick();
                        break;
                    case Keys.F5:
                        transferencia.PerformClick();
                        break;
                    default:
                        return base.ProcessDialogKey(keyData);
                }
                return true;
            }
            return base.ProcessDialogKey(keyData);
        }

        private void exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }      
    }
}
