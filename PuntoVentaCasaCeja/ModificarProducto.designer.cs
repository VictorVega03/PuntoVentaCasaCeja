
namespace PuntoVentaCasaCeja
{
    partial class ModificarProducto
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.cancel = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.remove = new System.Windows.Forms.Button();
            this.save = new System.Windows.Forms.Button();
            this.txtvendedor = new System.Windows.Forms.TextBox();
            this.txtespecial = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtcantmay = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtmayoreo = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtmenudeo = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txtpresentacion = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtunidad = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtnombre = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtbarras = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtcategoria = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoScroll = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 49);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1048, 582);
            this.tableLayoutPanel1.TabIndex = 24;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.cancel, 1, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 507);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1042, 72);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // cancel
            // 
            this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.cancel.Location = new System.Drawing.Point(213, 3);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(615, 65);
            this.cancel.TabIndex = 12;
            this.cancel.Text = "SALIR (Esc)";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.button1_Click);
            this.cancel.KeyDown += new System.Windows.Forms.KeyEventHandler(this.buttonEnter_KeyDown);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.remove, 1, 10);
            this.tableLayoutPanel2.Controls.Add(this.save, 0, 10);
            this.tableLayoutPanel2.Controls.Add(this.txtvendedor, 1, 9);
            this.tableLayoutPanel2.Controls.Add(this.txtespecial, 0, 9);
            this.tableLayoutPanel2.Controls.Add(this.label10, 1, 8);
            this.tableLayoutPanel2.Controls.Add(this.txtcantmay, 1, 7);
            this.tableLayoutPanel2.Controls.Add(this.label8, 0, 8);
            this.tableLayoutPanel2.Controls.Add(this.txtmayoreo, 0, 7);
            this.tableLayoutPanel2.Controls.Add(this.label9, 1, 6);
            this.tableLayoutPanel2.Controls.Add(this.txtmenudeo, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.label7, 0, 6);
            this.tableLayoutPanel2.Controls.Add(this.txtpresentacion, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.label6, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.label5, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.txtunidad, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.label4, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.txtnombre, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label3, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.txtbarras, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.txtcategoria, 0, 5);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 11;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1042, 498);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // remove
            // 
            this.remove.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.remove.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.remove.Location = new System.Drawing.Point(524, 433);
            this.remove.Name = "remove";
            this.remove.Size = new System.Drawing.Size(515, 65);
            this.remove.TabIndex = 11;
            this.remove.Text = "DAR DE BAJA (F6)";
            this.remove.UseVisualStyleBackColor = true;
            this.remove.Click += new System.EventHandler(this.upload_Click);
            this.remove.KeyDown += new System.Windows.Forms.KeyEventHandler(this.buttonEnter_KeyDown);
            // 
            // save
            // 
            this.save.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.save.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.save.Location = new System.Drawing.Point(3, 433);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(515, 65);
            this.save.TabIndex = 10;
            this.save.Text = "GUARDAR (F5)";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.add_Click);
            this.save.KeyDown += new System.Windows.Forms.KeyEventHandler(this.buttonEnter_KeyDown);
            // 
            // txtvendedor
            // 
            this.txtvendedor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtvendedor.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtvendedor.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.txtvendedor.Location = new System.Drawing.Point(524, 377);
            this.txtvendedor.Name = "txtvendedor";
            this.txtvendedor.ShortcutsEnabled = false;
            this.txtvendedor.Size = new System.Drawing.Size(515, 50);
            this.txtvendedor.TabIndex = 9;
            this.txtvendedor.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.numericInput_KeyPress);
            // 
            // txtespecial
            // 
            this.txtespecial.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtespecial.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtespecial.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.txtespecial.Location = new System.Drawing.Point(3, 377);
            this.txtespecial.Name = "txtespecial";
            this.txtespecial.ShortcutsEnabled = false;
            this.txtespecial.Size = new System.Drawing.Size(515, 50);
            this.txtespecial.TabIndex = 8;
            this.txtespecial.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.numericInput_KeyPress);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold);
            this.label10.Location = new System.Drawing.Point(524, 344);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(248, 30);
            this.label10.TabIndex = 15;
            this.label10.Text = "P. VENDEDOR (Opcional)";
            // 
            // txtcantmay
            // 
            this.txtcantmay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtcantmay.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtcantmay.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.txtcantmay.Location = new System.Drawing.Point(524, 291);
            this.txtcantmay.Name = "txtcantmay";
            this.txtcantmay.ShortcutsEnabled = false;
            this.txtcantmay.Size = new System.Drawing.Size(515, 50);
            this.txtcantmay.TabIndex = 7;
            this.txtcantmay.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.integerInput_KeyPress);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold);
            this.label8.Location = new System.Drawing.Point(3, 344);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(226, 30);
            this.label8.TabIndex = 16;
            this.label8.Text = "P. ESPECIAL (Opcional)";
            // 
            // txtmayoreo
            // 
            this.txtmayoreo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtmayoreo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtmayoreo.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.txtmayoreo.Location = new System.Drawing.Point(3, 291);
            this.txtmayoreo.Name = "txtmayoreo";
            this.txtmayoreo.ShortcutsEnabled = false;
            this.txtmayoreo.Size = new System.Drawing.Size(515, 50);
            this.txtmayoreo.TabIndex = 6;
            this.txtmayoreo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.numericInput_KeyPress);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold);
            this.label9.Location = new System.Drawing.Point(524, 258);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(223, 30);
            this.label9.TabIndex = 17;
            this.label9.Text = "CANTIDAD MAYOREO";
            // 
            // txtmenudeo
            // 
            this.txtmenudeo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtmenudeo.BackColor = System.Drawing.SystemColors.Window;
            this.txtmenudeo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtmenudeo.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.txtmenudeo.Location = new System.Drawing.Point(524, 205);
            this.txtmenudeo.Name = "txtmenudeo";
            this.txtmenudeo.ShortcutsEnabled = false;
            this.txtmenudeo.Size = new System.Drawing.Size(515, 50);
            this.txtmenudeo.TabIndex = 5;
            this.txtmenudeo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.numericInput_KeyPress);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold);
            this.label7.Location = new System.Drawing.Point(3, 258);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(132, 30);
            this.label7.TabIndex = 15;
            this.label7.Text = "P. MAYOREO";
            // 
            // txtpresentacion
            // 
            this.txtpresentacion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtpresentacion.BackColor = System.Drawing.SystemColors.Window;
            this.txtpresentacion.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtpresentacion.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.txtpresentacion.Location = new System.Drawing.Point(524, 119);
            this.txtpresentacion.MaxLength = 255;
            this.txtpresentacion.Name = "txtpresentacion";
            this.txtpresentacion.Size = new System.Drawing.Size(515, 50);
            this.txtpresentacion.TabIndex = 3;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold);
            this.label6.Location = new System.Drawing.Point(524, 172);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(136, 30);
            this.label6.TabIndex = 12;
            this.label6.Text = "P. MENUDEO";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(3, 172);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(196, 30);
            this.label5.TabIndex = 9;
            this.label5.Text = "CATEGORÍA (Enter)";
            // 
            // txtunidad
            // 
            this.txtunidad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtunidad.BackColor = System.Drawing.SystemColors.Window;
            this.txtunidad.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtunidad.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.txtunidad.Location = new System.Drawing.Point(3, 119);
            this.txtunidad.MaxLength = 255;
            this.txtunidad.Name = "txtunidad";
            this.txtunidad.ReadOnly = true;
            this.txtunidad.ShortcutsEnabled = false;
            this.txtunidad.Size = new System.Drawing.Size(515, 50);
            this.txtunidad.TabIndex = 2;
            this.txtunidad.DoubleClick += new System.EventHandler(this.mostrar_medidas_Click);
            this.txtunidad.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.txtunidad_PreviewKeyDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(524, 86);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(162, 30);
            this.label4.TabIndex = 7;
            this.label4.Text = "PRESENTACIÓN";
            // 
            // txtnombre
            // 
            this.txtnombre.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtnombre.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtnombre.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.txtnombre.Location = new System.Drawing.Point(524, 33);
            this.txtnombre.MaxLength = 255;
            this.txtnombre.Name = "txtnombre";
            this.txtnombre.Size = new System.Drawing.Size(515, 50);
            this.txtnombre.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(3, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(250, 30);
            this.label2.TabIndex = 4;
            this.label2.Text = "UNIDAD MEDIDA (Enter)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(524, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(227, 30);
            this.label3.TabIndex = 5;
            this.label3.Text = "NOMBRE PRODUCTO*";
            // 
            // txtbarras
            // 
            this.txtbarras.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtbarras.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtbarras.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.txtbarras.Location = new System.Drawing.Point(3, 33);
            this.txtbarras.MaxLength = 255;
            this.txtbarras.Name = "txtbarras";
            this.txtbarras.Size = new System.Drawing.Size(515, 50);
            this.txtbarras.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 15.75F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 30);
            this.label1.TabIndex = 2;
            this.label1.Text = "CÓDIGO BARRAS";
            // 
            // txtcategoria
            // 
            this.txtcategoria.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtcategoria.BackColor = System.Drawing.SystemColors.Window;
            this.txtcategoria.Font = new System.Drawing.Font("Segoe UI Semibold", 24F, System.Drawing.FontStyle.Bold);
            this.txtcategoria.Location = new System.Drawing.Point(3, 205);
            this.txtcategoria.MaxLength = 255;
            this.txtcategoria.Name = "txtcategoria";
            this.txtcategoria.ReadOnly = true;
            this.txtcategoria.ShortcutsEnabled = false;
            this.txtcategoria.Size = new System.Drawing.Size(515, 50);
            this.txtcategoria.TabIndex = 4;
            this.txtcategoria.DoubleClick += new System.EventHandler(this.mostrar_categorias_Click);
            this.txtcategoria.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.txtcategoria_PreviewKeyDown);
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
            this.groupBox1.Size = new System.Drawing.Size(1060, 637);
            this.groupBox1.TabIndex = 25;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "MODIFICACIÓN DE PRODUCTO";
            // 
            // ModificarProducto
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 661);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximumSize = new System.Drawing.Size(2000, 700);
            this.MinimumSize = new System.Drawing.Size(700, 450);
            this.Name = "ModificarProducto";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Modificar producto";
            this.Load += new System.EventHandler(this.ModificarProducto_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button save;
        private System.Windows.Forms.Button remove;
        private System.Windows.Forms.TextBox txtvendedor;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtmayoreo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtespecial;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtunidad;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtpresentacion;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtbarras;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtnombre;
        private System.Windows.Forms.TextBox txtcategoria;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtmenudeo;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtcantmay;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}