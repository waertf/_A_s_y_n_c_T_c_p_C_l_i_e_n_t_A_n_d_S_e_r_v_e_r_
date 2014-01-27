namespace WindowsFormsApplication1AVLSServer
{
    partial class Form1
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ID = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.GPSValid = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.DateTime = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.LatLon = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.Speed = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.Direction = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.Temperature = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.Status = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.Event = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.Message = new DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToDeleteRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.AliceBlue;
            this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ID,
            this.GPSValid,
            this.DateTime,
            this.LatLon,
            this.Speed,
            this.Direction,
            this.Temperature,
            this.Status,
            this.Event,
            this.Message});
            this.dataGridView1.Location = new System.Drawing.Point(3, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(661, 203);
            this.dataGridView1.TabIndex = 0;
            // 
            // ID
            // 
            this.ID.DataPropertyName = "ID";
            this.ID.HeaderText = "ID";
            this.ID.Name = "ID";
            this.ID.ReadOnly = true;
            this.ID.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.ID.Width = 57;
            // 
            // GPSValid
            // 
            this.GPSValid.DataPropertyName = "GPSValid";
            this.GPSValid.HeaderText = "GPSValid";
            this.GPSValid.Name = "GPSValid";
            this.GPSValid.ReadOnly = true;
            this.GPSValid.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.GPSValid.Visible = false;
            this.GPSValid.Width = 75;
            // 
            // DateTime
            // 
            this.DateTime.DataPropertyName = "DateTime";
            this.DateTime.HeaderText = "DateTime";
            this.DateTime.Name = "DateTime";
            this.DateTime.ReadOnly = true;
            this.DateTime.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.DateTime.Width = 90;
            // 
            // LatLon
            // 
            this.LatLon.DataPropertyName = "Lat_Lon";
            this.LatLon.HeaderText = "Lat_Lon";
            this.LatLon.Name = "LatLon";
            this.LatLon.ReadOnly = true;
            this.LatLon.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.LatLon.Width = 85;
            // 
            // Speed
            // 
            this.Speed.DataPropertyName = "Speed";
            this.Speed.HeaderText = "Speed";
            this.Speed.Name = "Speed";
            this.Speed.ReadOnly = true;
            this.Speed.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Speed.Width = 73;
            // 
            // Direction
            // 
            this.Direction.DataPropertyName = "Direction";
            this.Direction.HeaderText = "Direction";
            this.Direction.Name = "Direction";
            this.Direction.ReadOnly = true;
            this.Direction.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Direction.Width = 88;
            // 
            // Temperature
            // 
            this.Temperature.DataPropertyName = "Temperature";
            this.Temperature.HeaderText = "Temperature";
            this.Temperature.Name = "Temperature";
            this.Temperature.ReadOnly = true;
            this.Temperature.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Temperature.Visible = false;
            this.Temperature.Width = 89;
            // 
            // Status
            // 
            this.Status.DataPropertyName = "Status";
            this.Status.HeaderText = "Status";
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            this.Status.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Status.Visible = false;
            this.Status.Width = 57;
            // 
            // Event
            // 
            this.Event.DataPropertyName = "Event";
            this.Event.HeaderText = "Event";
            this.Event.Name = "Event";
            this.Event.ReadOnly = true;
            this.Event.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Event.Width = 72;
            // 
            // Message
            // 
            this.Message.DataPropertyName = "Message";
            this.Message.HeaderText = "Message";
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            this.Message.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.Message.Width = 84;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 235);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(675, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(675, 235);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dataGridView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(667, 209);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Receive";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.textBox1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(667, 209);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Log";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(3, 3);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(661, 203);
            this.textBox1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(675, 257);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn ID;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn GPSValid;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn DateTime;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn LatLon;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn Speed;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn Direction;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn Temperature;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn Status;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn Event;
        private DataGridViewAutoFilter.DataGridViewAutoFilterTextBoxColumn Message;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox textBox1;
    }
}

