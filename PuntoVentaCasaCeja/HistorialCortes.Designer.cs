namespace PuntoVentaCasaCeja
{
    partial class HistorialCortes
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
            this.exitButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.BSelCorte = new System.Windows.Forms.Button();
            this.BelimHistorial = new System.Windows.Forms.Button();
            this.clientinfo = new System.Windows.Forms.TableLayoutPanel();
            this.tablaCortesZ = new System.Windows.Forms.DataGridView();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.clientinfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tablaCortesZ)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.exitButton);
            this.groupBox1.Controls.Add(this.tableLayoutPanel3);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(16, 15);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(1460, 821);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "HISTORIAL DE CORTES";
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Font = new System.Drawing.Font("Segoe UI Semibold", 22F, System.Drawing.FontStyle.Bold);
            this.exitButton.Location = new System.Drawing.Point(1187, -4);
            this.exitButton.Margin = new System.Windows.Forms.Padding(4);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(259, 57);
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
            this.tableLayoutPanel3.Location = new System.Drawing.Point(9, 60);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1443, 753);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 491F));
            this.tableLayoutPanel2.Controls.Add(this.BSelCorte, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.BelimHistorial, 2, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 657);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1435, 72);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // BSelCorte
            // 
            this.BSelCorte.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BSelCorte.Font = new System.Drawing.Font("Segoe UI Semibold", 20F, System.Drawing.FontStyle.Bold);
            this.BSelCorte.Location = new System.Drawing.Point(4, 4);
            this.BSelCorte.Margin = new System.Windows.Forms.Padding(4);
            this.BSelCorte.Name = "BSelCorte";
            this.BSelCorte.Size = new System.Drawing.Size(709, 64);
            this.BSelCorte.TabIndex = 7;
            this.BSelCorte.Text = "SEL. CORTE (ENTER)";
            this.BSelCorte.UseVisualStyleBackColor = true;
            this.BSelCorte.Click += new System.EventHandler(this.BSelCorte_Click);
            // 
            // BelimHistorial
            // 
            this.BelimHistorial.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BelimHistorial.Font = new System.Drawing.Font("Segoe UI Semibold", 20F, System.Drawing.FontStyle.Bold);
            this.BelimHistorial.Location = new System.Drawing.Point(721, 4);
            this.BelimHistorial.Margin = new System.Windows.Forms.Padding(4);
            this.BelimHistorial.Name = "BelimHistorial";
            this.BelimHistorial.Size = new System.Drawing.Size(710, 64);
            this.BelimHistorial.TabIndex = 9;
            this.BelimHistorial.Text = "ELIM. HISTORIAL (F6)";
            this.BelimHistorial.UseVisualStyleBackColor = true;
            this.BelimHistorial.Click += new System.EventHandler(this.BelimHistorial_Click);
            // 
            // clientinfo
            // 
            this.clientinfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.clientinfo.ColumnCount = 1;
            this.clientinfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.clientinfo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.clientinfo.Controls.Add(this.tablaCortesZ, 0, 0);
            this.clientinfo.Location = new System.Drawing.Point(4, 4);
            this.clientinfo.Margin = new System.Windows.Forms.Padding(4);
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
            this.clientinfo.Size = new System.Drawing.Size(1435, 645);
            this.clientinfo.TabIndex = 0;
            // 
            // tablaCortesZ
            // 
            this.tablaCortesZ.AllowUserToAddRows = false;
            this.tablaCortesZ.AllowUserToDeleteRows = false;
            this.tablaCortesZ.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.tablaCortesZ.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.tablaCortesZ.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.tablaCortesZ.DefaultCellStyle = dataGridViewCellStyle2;
            this.tablaCortesZ.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tablaCortesZ.Location = new System.Drawing.Point(3, 3);
            this.tablaCortesZ.Name = "tablaCortesZ";
            this.tablaCortesZ.ReadOnly = true;
            this.tablaCortesZ.RowHeadersVisible = false;
            this.tablaCortesZ.RowHeadersWidth = 51;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold);
            this.tablaCortesZ.RowsDefaultCellStyle = dataGridViewCellStyle3;
            this.tablaCortesZ.RowTemplate.Height = 24;
            this.tablaCortesZ.RowTemplate.ReadOnly = true;
            this.tablaCortesZ.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.tablaCortesZ.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.tablaCortesZ.Size = new System.Drawing.Size(1429, 639);
            this.tablaCortesZ.TabIndex = 0;
            this.tablaCortesZ.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tablaCortesZ_KeyDown);
            // 
            // HistorialCortes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1492, 850);
            this.Controls.Add(this.groupBox1);
            this.Name = "HistorialCortes";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Historial de Cortes";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.clientinfo.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tablaCortesZ)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BSelCorte;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel clientinfo;
        private System.Windows.Forms.DataGridView tablaCortesZ;
        private System.Windows.Forms.Button BelimHistorial;
    }
}