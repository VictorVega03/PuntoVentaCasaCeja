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
    public partial class infoCliente : Form
    {
        CurrentData data;
        Cliente clienteActual;

        // Constructor que recibe solo CurrentData (para cuando se abre sin cliente específico)
        public infoCliente(CurrentData data)
        {
            InitializeComponent();
            this.data = data;
            ConfigurarFormulario();
        }

        // Constructor que recibe CurrentData y Cliente (para mostrar información específica)
        public infoCliente(CurrentData data, Cliente cliente)
        {
            InitializeComponent();
            this.data = data;
            this.clienteActual = cliente;
            ConfigurarFormulario();
            CargarDatosCliente(cliente);
        }

        private void ConfigurarFormulario()
        {
            // Configurar el formulario como solo lectura
            HacerCamposSoloLectura();

            // Configurar eventos
            this.cancelar.Click += Cancelar_Click;
            this.KeyPreview = true; // Habilitar eventos de teclado para el formulario
            this.KeyDown += InfoCliente_KeyDown;

            // Configurar el título del formulario
            if (clienteActual != null)
            {
                this.Text = $"Información de Cliente - {clienteActual.nombre}";
            }
            else
            {
                this.Text = "Información de Cliente";
            }
        }

        private void HacerCamposSoloLectura()
        {
            // Hacer todos los TextBox de solo lectura y cambiar su apariencia
            txtnombre.ReadOnly = true;
            txttel.ReadOnly = true;
            txtcorreo.ReadOnly = true;
            txtrfc.ReadOnly = true;
            txtnoext.ReadOnly = true;
            txtnoint.ReadOnly = true;
            txtpostal.ReadOnly = true;
            txtcolonia.ReadOnly = true;
            txtciudad.ReadOnly = true;
            txtcalle.ReadOnly = true;

            // Cambiar el color de fondo para indicar que son de solo lectura
            Color colorSoloLectura = Color.FromArgb(240, 240, 240);

            txtnombre.BackColor = colorSoloLectura;
            txttel.BackColor = colorSoloLectura;
            txtcorreo.BackColor = colorSoloLectura;
            txtrfc.BackColor = colorSoloLectura;
            txtnoext.BackColor = colorSoloLectura;
            txtnoint.BackColor = colorSoloLectura;
            txtpostal.BackColor = colorSoloLectura;
            txtcolonia.BackColor = colorSoloLectura;
            txtciudad.BackColor = colorSoloLectura;
            txtcalle.BackColor = colorSoloLectura;
        }

        private void CargarDatosCliente(Cliente cliente)
        {
            if (cliente == null) return;

            try
            {
                // Cargar todos los datos del cliente en los controles
                txtnombre.Text = cliente.nombre ?? "";
                txttel.Text = cliente.telefono ?? "";
                txtcorreo.Text = cliente.correo ?? "";
                txtrfc.Text = cliente.rfc ?? "";
                txtnoext.Text = cliente.numero_exterior ?? "";
                txtnoint.Text = cliente.numero_interior ?? "";
                txtpostal.Text = cliente.codigo_postal ?? "";
                txtcolonia.Text = cliente.colonia ?? "";
                txtciudad.Text = cliente.ciudad ?? "";
                txtcalle.Text = cliente.calle ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los datos del cliente: {ex.Message}",
                               "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Cancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void InfoCliente_KeyDown(object sender, KeyEventArgs e)
        {
            // Manejar teclas de acceso rápido
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }             

        // Método para validar si hay datos del cliente
        private bool TieneDatosCliente()
        {
            return clienteActual != null && !string.IsNullOrEmpty(clienteActual.nombre);
        }
        
    }
}