using System.Windows.Forms;

namespace PuntoVentaCasaCeja
{
    partial class ListaClientes
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BinfoCliente = new System.Windows.Forms.Button();
            this.BSelCliente = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.modificarButton = new System.Windows.Forms.Button();
            this.altaButton = new System.Windows.Forms.Button();
            this.BajaButton = new System.Windows.Forms.Button();
            this.clientinfo = new System.Windows.Forms.TableLayoutPanel();
            this.tablaClientes = new System.Windows.Forms.DataGridView();
            this.BgenerarExcel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.clientinfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tablaClientes)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.BinfoCliente);
            this.groupBox1.Controls.Add(this.BSelCliente);
            this.groupBox1.Controls.Add(this.exitButton);
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1095, 667);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "LISTA DE CLIENTES";
            // 
            // BinfoCliente
            // 
            this.BinfoCliente.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BinfoCliente.Font = new System.Drawing.Font("Segoe UI Semibold", 20F, System.Drawing.FontStyle.Bold);
            this.BinfoCliente.Location = new System.Drawing.Point(282, -3);
            this.BinfoCliente.Name = "BinfoCliente";
            this.BinfoCliente.Size = new System.Drawing.Size(299, 46);
            this.BinfoCliente.TabIndex = 7;
            this.BinfoCliente.Text = "INFO. DE CLIENTE (F1)";
            this.BinfoCliente.UseVisualStyleBackColor = true;
            this.BinfoCliente.Click += new System.EventHandler(this.BinfoCliente_Click);
            // 
            // BSelCliente
            // 
            this.BSelCliente.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BSelCliente.Font = new System.Drawing.Font("Segoe UI Semibold", 20F, System.Drawing.FontStyle.Bold);
            this.BSelCliente.Location = new System.Drawing.Point(587, -3);
            this.BSelCliente.Name = "BSelCliente";
            this.BSelCliente.Size = new System.Drawing.Size(289, 46);
            this.BSelCliente.TabIndex = 6;
            this.BSelCliente.Text = "SEL. CLIENTE (ENTER)";
            this.BSelCliente.UseVisualStyleBackColor = true;
            this.BSelCliente.Click += new System.EventHandler(this.BSelCliente_Click);
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Font = new System.Drawing.Font("Segoe UI Semibold", 22F, System.Drawing.FontStyle.Bold);
            this.exitButton.Location = new System.Drawing.Point(890, -3);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(194, 46);
            this.exitButton.TabIndex = 5;
            this.exitButton.Text = "SALIR (Esc)";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.clientinfo, 0, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(7, 49);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1082, 612);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.87356F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 47.12644F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 263F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 377F));
            this.tableLayoutPanel2.Controls.Add(this.modificarButton, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.altaButton, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.BgenerarExcel, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.BajaButton, 2, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 550);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 59F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 59F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1076, 59);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // modificarButton
            // 
            this.modificarButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modificarButton.Font = new System.Drawing.Font("Segoe UI Semibold", 20F, System.Drawing.FontStyle.Bold);
            this.modificarButton.Location = new System.Drawing.Point(233, 3);
            this.modificarButton.Name = "modificarButton";
            this.modificarButton.Size = new System.Drawing.Size(199, 53);
            this.modificarButton.TabIndex = 13;
            this.modificarButton.Text = "MODIFICAR (F6)";
            this.modificarButton.UseVisualStyleBackColor = true;
            this.modificarButton.Click += new System.EventHandler(this.modificarButton_Click);
            // 
            // altaButton
            // 
            this.altaButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.altaButton.Font = new System.Drawing.Font("Segoe UI Semibold", 20F, System.Drawing.FontStyle.Bold);
            this.altaButton.Location = new System.Drawing.Point(3, 3);
            this.altaButton.Name = "altaButton";
            this.altaButton.Size = new System.Drawing.Size(224, 53);
            this.altaButton.TabIndex = 11;
            this.altaButton.Text = "ALTA DE CLIENTE (F5)";
            this.altaButton.UseVisualStyleBackColor = true;
            this.altaButton.Click += new System.EventHandler(this.altaButton_Click);
            // 
            // BajaButton
            // 
            this.BajaButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BajaButton.Font = new System.Drawing.Font("Segoe UI Semibold", 20F, System.Drawing.FontStyle.Bold);
            this.BajaButton.Location = new System.Drawing.Point(438, 3);
            this.BajaButton.Name = "BajaButton";
            this.BajaButton.Size = new System.Drawing.Size(257, 53);
            this.BajaButton.TabIndex = 12;
            this.BajaButton.Text = "DAR DE BAJA (F7)";
            this.BajaButton.UseVisualStyleBackColor = true;
            this.BajaButton.Click += new System.EventHandler(this.BajaButton_Click);
            // 
            // clientinfo
            // 
            this.clientinfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clientinfo.ColumnCount = 1;
            this.clientinfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.clientinfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.clientinfo.Controls.Add(this.tablaClientes, 0, 0);
            this.clientinfo.Location = new System.Drawing.Point(3, 3);
            this.clientinfo.Name = "clientinfo";
            this.clientinfo.RowCount = 1;
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66616F));
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66616F));
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66616F));
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66616F));
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66767F));
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.clientinfo.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66767F));
            this.clientinfo.Size = new System.Drawing.Size(1076, 541);
            this.clientinfo.TabIndex = 0;
            // 
            // tablaClientes
            // 
            this.tablaClientes.AllowUserToAddRows = false;
            this.tablaClientes.AllowUserToDeleteRows = false;
            this.tablaClientes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tablaClientes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.tablaClientes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tablaClientes.DefaultCellStyle = dataGridViewCellStyle2;
            this.tablaClientes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablaClientes.Location = new System.Drawing.Point(2, 2);
            this.tablaClientes.Margin = new System.Windows.Forms.Padding(2);
            this.tablaClientes.Name = "tablaClientes";
            this.tablaClientes.ReadOnly = true;
            this.tablaClientes.RowHeadersVisible = false;
            this.tablaClientes.RowHeadersWidth = 51;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.tablaClientes.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.tablaClientes.RowTemplate.Height = 50;
            this.tablaClientes.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.tablaClientes.Size = new System.Drawing.Size(1072, 537);
            this.tablaClientes.TabIndex = 0;
            this.tablaClientes.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.tablaClientes_CellClick);  
            this.tablaClientes.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.tablaClientes_CellClick);
            this.tablaClientes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tablaClientes_KeyDown);
            // 
            // BgenerarExcel
            // 
            this.BgenerarExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BgenerarExcel.Font = new System.Drawing.Font("Segoe UI Semibold", 20F, System.Drawing.FontStyle.Bold);
            this.BgenerarExcel.Location = new System.Drawing.Point(701, 3);
            this.BgenerarExcel.Name = "BgenerarExcel";
            this.BgenerarExcel.Size = new System.Drawing.Size(372, 53);
            this.BgenerarExcel.TabIndex = 14;
            this.BgenerarExcel.Text = "GENERAR EXCEL (F8)";
            this.BgenerarExcel.UseVisualStyleBackColor = true;
            this.BgenerarExcel.Click += new System.EventHandler(this.BgenerarExcel_Click);
            // 
            // ListaClientes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1119, 691);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ListaClientes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Clientes";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.clientinfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tablaClientes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button altaButton;
        private System.Windows.Forms.Button BajaButton;
        private System.Windows.Forms.TableLayoutPanel clientinfo;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.DataGridView tablaClientes;
        private Button modificarButton;
        private Button BSelCliente;
        private Button BinfoCliente;
        private Button BgenerarExcel;
    }
}