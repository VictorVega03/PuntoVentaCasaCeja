using System.Windows.Forms;

namespace PuntoVentaCasaCeja
{
    partial class Ventas
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
       

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Ventas));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.lblcobranza = new System.Windows.Forms.Label();
            this.tabla = new System.Windows.Forms.DataGridView();
            this.articulos = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cantidad = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.precio = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.total = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.salir = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtcodigo = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.txttotal = new System.Windows.Forms.Label();
            this.modcant = new System.Windows.Forms.Button();
            this.abonar = new System.Windows.Forms.Button();
            this.existencia = new System.Windows.Forms.Button();
            this.nuevacaja = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblFolio = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.opcionesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ingresarEfectivoF3ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.retirarEfectivoF4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.corteZF7ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reimprimirTicketF6ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.establecerImpresoraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.historialDeCortesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adminUsuariosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.actualizarBaseDeDatosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logout = new System.Windows.Forms.Button();
            this.apartados = new System.Windows.Forms.Button();
            this.eliminarCarrito_button = new System.Windows.Forms.Button();
            this.Bdescuento = new System.Windows.Forms.Button();
            this.BdescTemporada = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tabla)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblcobranza
            // 
            resources.ApplyResources(this.lblcobranza, "lblcobranza");
            this.lblcobranza.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblcobranza.Name = "lblcobranza";
            // 
            // tabla
            // 
            this.tabla.AllowUserToAddRows = false;
            this.tabla.AllowUserToDeleteRows = false;
            this.tabla.AllowUserToResizeColumns = false;
            this.tabla.AllowUserToResizeRows = false;
            resources.ApplyResources(this.tabla, "tabla");
            this.tabla.BackgroundColor = System.Drawing.SystemColors.Window;
            this.tabla.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.tabla.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.tabla.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tabla.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.tabla.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tabla.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.articulos,
            this.cantidad,
            this.precio,
            this.total});
            this.tabla.Cursor = System.Windows.Forms.Cursors.Hand;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.DarkOrange;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tabla.DefaultCellStyle = dataGridViewCellStyle2;
            this.tabla.EnableHeadersVisualStyles = false;
            this.tabla.MultiSelect = false;
            this.tabla.Name = "tabla";
            this.tabla.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.OrangeRed;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tabla.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.tabla.RowHeadersVisible = false;
            this.tabla.RowTemplate.Height = 50;
            this.tabla.RowTemplate.ReadOnly = true;
            this.tabla.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tabla.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.tabla_CellContentClick);
            this.tabla.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tabla_KeyDown);
            // 
            // articulos
            // 
            this.articulos.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.articulos, "articulos");
            this.articulos.Name = "articulos";
            this.articulos.ReadOnly = true;
            // 
            // cantidad
            // 
            this.cantidad.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            resources.ApplyResources(this.cantidad, "cantidad");
            this.cantidad.Name = "cantidad";
            this.cantidad.ReadOnly = true;
            // 
            // precio
            // 
            this.precio.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            resources.ApplyResources(this.precio, "precio");
            this.precio.Name = "precio";
            this.precio.ReadOnly = true;
            // 
            // total
            // 
            this.total.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            resources.ApplyResources(this.total, "total");
            this.total.Name = "total";
            this.total.ReadOnly = true;
            // 
            // salir
            // 
            resources.ApplyResources(this.salir, "salir");
            this.salir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.salir.FlatAppearance.BorderSize = 0;
            this.salir.Name = "salir";
            this.salir.UseVisualStyleBackColor = false;
            this.salir.Click += new System.EventHandler(this.btn4_Click_1);
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.txtcodigo);
            this.panel1.Name = "panel1";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // txtcodigo
            // 
            resources.ApplyResources(this.txtcodigo, "txtcodigo");
            this.txtcodigo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtcodigo.Name = "txtcodigo";
            this.txtcodigo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtcodigo_KeyDown);
            // 
            // panel3
            // 
            resources.ApplyResources(this.panel3, "panel3");
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.panel3.Controls.Add(this.txttotal);
            this.panel3.Name = "panel3";
            // 
            // txttotal
            // 
            resources.ApplyResources(this.txttotal, "txttotal");
            this.txttotal.ForeColor = System.Drawing.SystemColors.Window;
            this.txttotal.Name = "txttotal";
            // 
            // modcant
            // 
            resources.ApplyResources(this.modcant, "modcant");
            this.modcant.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.modcant.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.modcant.FlatAppearance.BorderSize = 0;
            this.modcant.Name = "modcant";
            this.modcant.UseVisualStyleBackColor = false;
            this.modcant.Click += new System.EventHandler(this.btn1_Click);
            // 
            // abonar
            // 
            resources.ApplyResources(this.abonar, "abonar");
            this.abonar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.abonar.FlatAppearance.BorderSize = 0;
            this.abonar.Name = "abonar";
            this.abonar.UseVisualStyleBackColor = false;
            this.abonar.Click += new System.EventHandler(this.abonar_Click);
            // 
            // existencia
            // 
            resources.ApplyResources(this.existencia, "existencia");
            this.existencia.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.existencia.FlatAppearance.BorderSize = 0;
            this.existencia.Name = "existencia";
            this.existencia.UseVisualStyleBackColor = false;
            this.existencia.Click += new System.EventHandler(this.existencia_Click);
            // 
            // nuevacaja
            // 
            resources.ApplyResources(this.nuevacaja, "nuevacaja");
            this.nuevacaja.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.nuevacaja.FlatAppearance.BorderSize = 0;
            this.nuevacaja.Name = "nuevacaja";
            this.nuevacaja.UseVisualStyleBackColor = false;
            this.nuevacaja.Click += new System.EventHandler(this.nuevacaja_Click);
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(this.tableLayoutPanel1, "tableLayoutPanel1");
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.tableLayoutPanel1.Controls.Add(this.lblTime, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblcobranza, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblFolio, 0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // lblTime
            // 
            resources.ApplyResources(this.lblTime, "lblTime");
            this.lblTime.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblTime.Name = "lblTime";
            // 
            // lblFolio
            // 
            resources.ApplyResources(this.lblFolio, "lblFolio");
            this.lblFolio.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblFolio.Name = "lblFolio";
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.opcionesToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // opcionesToolStripMenuItem
            // 
            this.opcionesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ingresarEfectivoF3ToolStripMenuItem,
            this.retirarEfectivoF4ToolStripMenuItem,
            this.corteZF7ToolStripMenuItem,
            this.reimprimirTicketF6ToolStripMenuItem,
            this.establecerImpresoraToolStripMenuItem,
            this.historialDeCortesToolStripMenuItem,
            this.adminUsuariosToolStripMenuItem,
            this.actualizarBaseDeDatosToolStripMenuItem});
            this.opcionesToolStripMenuItem.Name = "opcionesToolStripMenuItem";
            resources.ApplyResources(this.opcionesToolStripMenuItem, "opcionesToolStripMenuItem");
            // 
            // ingresarEfectivoF3ToolStripMenuItem
            // 
            this.ingresarEfectivoF3ToolStripMenuItem.Name = "ingresarEfectivoF3ToolStripMenuItem";
            resources.ApplyResources(this.ingresarEfectivoF3ToolStripMenuItem, "ingresarEfectivoF3ToolStripMenuItem");
            this.ingresarEfectivoF3ToolStripMenuItem.Click += new System.EventHandler(this.ingreso_Click);
            // 
            // retirarEfectivoF4ToolStripMenuItem
            // 
            this.retirarEfectivoF4ToolStripMenuItem.Name = "retirarEfectivoF4ToolStripMenuItem";
            resources.ApplyResources(this.retirarEfectivoF4ToolStripMenuItem, "retirarEfectivoF4ToolStripMenuItem");
            this.retirarEfectivoF4ToolStripMenuItem.Click += new System.EventHandler(this.retiro_Click);
            // 
            // corteZF7ToolStripMenuItem
            // 
            this.corteZF7ToolStripMenuItem.Name = "corteZF7ToolStripMenuItem";
            resources.ApplyResources(this.corteZF7ToolStripMenuItem, "corteZF7ToolStripMenuItem");
            this.corteZF7ToolStripMenuItem.Click += new System.EventHandler(this.cortep_Click);
            // 
            // reimprimirTicketF6ToolStripMenuItem
            // 
            this.reimprimirTicketF6ToolStripMenuItem.Name = "reimprimirTicketF6ToolStripMenuItem";
            resources.ApplyResources(this.reimprimirTicketF6ToolStripMenuItem, "reimprimirTicketF6ToolStripMenuItem");
            this.reimprimirTicketF6ToolStripMenuItem.Click += new System.EventHandler(this.button1_Click);
            // 
            // establecerImpresoraToolStripMenuItem
            // 
            this.establecerImpresoraToolStripMenuItem.Name = "establecerImpresoraToolStripMenuItem";
            resources.ApplyResources(this.establecerImpresoraToolStripMenuItem, "establecerImpresoraToolStripMenuItem");
            this.establecerImpresoraToolStripMenuItem.Click += new System.EventHandler(this.establecerImpresoraToolStripMenuItem_Click);
            // 
            // historialDeCortesToolStripMenuItem
            // 
            this.historialDeCortesToolStripMenuItem.Name = "historialDeCortesToolStripMenuItem";
            resources.ApplyResources(this.historialDeCortesToolStripMenuItem, "historialDeCortesToolStripMenuItem");
            this.historialDeCortesToolStripMenuItem.Click += new System.EventHandler(this.historialDeCortesToolStripMenuItem_Click);
            // 
            // adminUsuariosToolStripMenuItem
            // 
            this.adminUsuariosToolStripMenuItem.Name = "adminUsuariosToolStripMenuItem";
            resources.ApplyResources(this.adminUsuariosToolStripMenuItem, "adminUsuariosToolStripMenuItem");
            this.adminUsuariosToolStripMenuItem.Click += new System.EventHandler(this.adminUsuariosToolStripMenuItem_Click);
            // 
            // actualizarBaseDeDatosToolStripMenuItem
            // 
            this.actualizarBaseDeDatosToolStripMenuItem.Name = "actualizarBaseDeDatosToolStripMenuItem";
            resources.ApplyResources(this.actualizarBaseDeDatosToolStripMenuItem, "actualizarBaseDeDatosToolStripMenuItem");
            this.actualizarBaseDeDatosToolStripMenuItem.Click += new System.EventHandler(this.actualizarBaseDeDatosToolStripMenuItem_Click);
            // 
            // logout
            // 
            resources.ApplyResources(this.logout, "logout");
            this.logout.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.logout.FlatAppearance.BorderSize = 0;
            this.logout.Name = "logout";
            this.logout.UseVisualStyleBackColor = false;
            this.logout.Click += new System.EventHandler(this.logout_Click);
            // 
            // apartados
            // 
            resources.ApplyResources(this.apartados, "apartados");
            this.apartados.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.apartados.FlatAppearance.BorderSize = 0;
            this.apartados.Name = "apartados";
            this.apartados.UseVisualStyleBackColor = false;
            this.apartados.Click += new System.EventHandler(this.apartados_Click);
            // 
            // eliminarCarrito_button
            // 
            resources.ApplyResources(this.eliminarCarrito_button, "eliminarCarrito_button");
            this.eliminarCarrito_button.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.eliminarCarrito_button.FlatAppearance.BorderSize = 0;
            this.eliminarCarrito_button.Name = "eliminarCarrito_button";
            this.eliminarCarrito_button.UseVisualStyleBackColor = false;
            this.eliminarCarrito_button.Click += new System.EventHandler(this.eliminarCarrito_button_Click);
            // 
            // Bdescuento
            // 
            resources.ApplyResources(this.Bdescuento, "Bdescuento");
            this.Bdescuento.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.Bdescuento.FlatAppearance.BorderSize = 0;
            this.Bdescuento.Name = "Bdescuento";
            this.Bdescuento.UseVisualStyleBackColor = false;
            this.Bdescuento.Click += new System.EventHandler(this.Bdescuento_Click);
            // 
            // BdescTemporada
            // 
            resources.ApplyResources(this.BdescTemporada, "BdescTemporada");
            this.BdescTemporada.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.BdescTemporada.FlatAppearance.BorderSize = 0;
            this.BdescTemporada.Name = "BdescTemporada";
            this.BdescTemporada.UseVisualStyleBackColor = false;
            this.BdescTemporada.Click += new System.EventHandler(this.BdescTemporada_Click);
            // 
            // Ventas
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.BdescTemporada);
            this.Controls.Add(this.Bdescuento);
            this.Controls.Add(this.eliminarCarrito_button);
            this.Controls.Add(this.apartados);
            this.Controls.Add(this.logout);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.nuevacaja);
            this.Controls.Add(this.existencia);
            this.Controls.Add(this.tabla);
            this.Controls.Add(this.salir);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.modcant);
            this.Controls.Add(this.abonar);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Ventas";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tabla)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DataGridView tabla;
        private Button salir;
        private Panel panel1;
        private Label label1;
        private TextBox txtcodigo;
        private Panel panel3;
        private Label txttotal;
        private Button modcant;
        private Button abonar;
        private Label lblcobranza;
        private Button existencia;
        private Button nuevacaja;
        private TableLayoutPanel tableLayoutPanel1;
        private DataGridViewTextBoxColumn articulos;
        private DataGridViewTextBoxColumn cantidad;
        private DataGridViewTextBoxColumn precio;
        private DataGridViewTextBoxColumn total;
        private Label lblTime;
        private Label lblFolio;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem opcionesToolStripMenuItem;
        private ToolStripMenuItem ingresarEfectivoF3ToolStripMenuItem;
        private ToolStripMenuItem retirarEfectivoF4ToolStripMenuItem;
        private ToolStripMenuItem reimprimirTicketF6ToolStripMenuItem;
        private ToolStripMenuItem corteZF7ToolStripMenuItem;
        private Button logout;
        private ToolStripMenuItem establecerImpresoraToolStripMenuItem;
        private Button apartados;
        private ToolStripMenuItem actualizarBaseDeDatosToolStripMenuItem;
        private Button eliminarCarrito_button;
        private ToolStripMenuItem historialDeCortesToolStripMenuItem;
        private ToolStripMenuItem adminUsuariosToolStripMenuItem;
        private Button Bdescuento;
        private Button BdescTemporada;
    }
}