
namespace PuntoVentaCasaCeja
{
    partial class ApCrSel
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.credito = new System.Windows.Forms.Button();
            this.apartado = new System.Windows.Forms.Button();
            this.verCred = new System.Windows.Forms.Button();
            this.verApa = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.tableLayoutPanel1);
            this.groupBox1.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(590, 427);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "SELECCIONAR ACCIÓN A REALIZAR";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.Controls.Add(this.credito, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.apartado, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.verCred, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.verApa, 3, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 49);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(578, 372);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // credito
            // 
            this.credito.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.credito.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.credito.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.credito.FlatAppearance.BorderSize = 0;
            this.credito.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.credito.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.credito.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.credito.Location = new System.Drawing.Point(74, 26);
            this.credito.Name = "credito";
            this.credito.Size = new System.Drawing.Size(175, 145);
            this.credito.TabIndex = 4;
            this.credito.Text = "Nuevo crédito (F1)";
            this.credito.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.credito.UseVisualStyleBackColor = false;
            this.credito.Click += new System.EventHandler(this.credito_Click);
            // 
            // apartado
            // 
            this.apartado.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.apartado.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.apartado.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.apartado.FlatAppearance.BorderSize = 0;
            this.apartado.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.apartado.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.apartado.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.apartado.Location = new System.Drawing.Point(327, 26);
            this.apartado.Name = "apartado";
            this.apartado.Size = new System.Drawing.Size(175, 145);
            this.apartado.TabIndex = 5;
            this.apartado.Text = "Nuevo apartado\r\n(F2)";
            this.apartado.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.apartado.UseVisualStyleBackColor = false;
            this.apartado.Click += new System.EventHandler(this.apartado_Click);
            // 
            // verCred
            // 
            this.verCred.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.verCred.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.verCred.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.verCred.FlatAppearance.BorderSize = 0;
            this.verCred.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.verCred.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.verCred.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.verCred.Location = new System.Drawing.Point(74, 200);
            this.verCred.Name = "verCred";
            this.verCred.Size = new System.Drawing.Size(175, 145);
            this.verCred.TabIndex = 6;
            this.verCred.Text = "Mis créditos (F3)";
            this.verCred.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.verCred.UseVisualStyleBackColor = false;
            this.verCred.Click += new System.EventHandler(this.verCred_Click);
            // 
            // verApa
            // 
            this.verApa.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.verApa.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(166)))), ((int)(((byte)(64)))));
            this.verApa.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.verApa.FlatAppearance.BorderSize = 0;
            this.verApa.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.verApa.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold);
            this.verApa.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.verApa.Location = new System.Drawing.Point(327, 200);
            this.verApa.Name = "verApa";
            this.verApa.Size = new System.Drawing.Size(175, 145);
            this.verApa.TabIndex = 7;
            this.verApa.Text = "Mis apartados (F4)";
            this.verApa.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.verApa.UseVisualStyleBackColor = false;
            this.verApa.Click += new System.EventHandler(this.verApa_Click);
            // 
            // ApCrSel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 451);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximumSize = new System.Drawing.Size(630, 490);
            this.MinimumSize = new System.Drawing.Size(630, 490);
            this.Name = "ApCrSel";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Opciones de cliente";
            this.groupBox1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button credito;
        private System.Windows.Forms.Button apartado;
        private System.Windows.Forms.Button verCred;
        private System.Windows.Forms.Button verApa;
    }
}