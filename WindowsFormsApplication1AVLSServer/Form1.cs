using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DataGridViewAutoFilter;

namespace WindowsFormsApplication1AVLSServer
{
    
    public partial class Form1 : Form
    {
        private ToolStripStatusLabel filterStatusLabel, showAllLabel;
        private Thread t1;
        private List<Record> records = new List<Record>();
        private DataTable table;
        private BindingSource dataSource;
        public Form1()
        {
            InitializeComponent();
            //dataGridView1.AutoGenerateColumns = false;

            records.Add(new Record()
            {
                DateTime = "11",
                Direction = "2",
                Event = "3",
                GPSValid = "4",
                ID = "5",
                Lat_Lon = "6",
                Message = "7",
                Speed = "8",
                Status = "9",
                Temperature = "10"
            });
            records.Add(new Record()
            {
                DateTime = "1",
                Direction = "2",
                Event = "3",
                GPSValid = "4",
                ID = "5",
                Lat_Lon = "6",
                Message = "7",
                Speed = "8",
                Status = "9",
                Temperature = "10"
            });
            table = ConvertToDatatable(records);

            dataSource = new BindingSource(table, null);
            dataGridView1.DataSource = dataSource;

            dataGridView1.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(dataGridView1_DataBindingComplete);
            this.Shown += new EventHandler(Form1_Shown);
            dataGridView1.AutoSize = true;
            dataGridView1.BindingContextChanged += new EventHandler(dataGridView1_BindingContextChanged);

            filterStatusLabel = new ToolStripStatusLabel();
            showAllLabel = new ToolStripStatusLabel();
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            filterStatusLabel,showAllLabel});
            filterStatusLabel.Text = "";
            filterStatusLabel.Visible = false;
            showAllLabel.Text = "Show &All";
            showAllLabel.Visible = false;
            showAllLabel.IsLink = true;
            showAllLabel.LinkBehavior = LinkBehavior.HoverUnderline;

            showAllLabel.Click += new EventHandler(showAllLabel_Click);
            

            #region test
             t1 = new System.Threading.Thread
              (delegate()
              {
                  Random rand = new Random();
                  while (true)
                  {
                      string test = dataGridView1.RowCount.ToString();
                      this.UIThread(() => this.Text = "Total number : "+test);
                      Thread.Sleep(300);
                  }
              });
            t1.Start();
            #endregion test

        }

        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            t1.Abort();
            base.OnFormClosing(e);
        }
        private void showAllLabel_Click(object sender, EventArgs e)
        {
            DataGridViewAutoFilterTextBoxColumn.RemoveFilter(dataGridView1);
        }

        void dataGridView1_BindingContextChanged(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource == null) return;

            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.HeaderCell = new
                    DataGridViewAutoFilterColumnHeaderCell(col.HeaderCell);
            }
            dataGridView1.AutoResizeColumns();
        }

        void Form1_Shown(object sender, EventArgs e)
        {
            Size size = new Size(dataGridView1.Width, dataGridView1.Height);
            this.Size = size;
        }

        

        void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Put each of the columns into programmatic sort mode.
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
            }
            String filterStatus = DataGridViewAutoFilterColumnHeaderCell
          .GetFilterStatus(dataGridView1);
            if (String.IsNullOrEmpty(filterStatus))
            {
                showAllLabel.Visible = false;
                filterStatusLabel.Visible = false;
            }
            else
            {
                showAllLabel.Visible = true;
                filterStatusLabel.Visible = true;
                filterStatusLabel.Text = filterStatus;

            }
        }
        private static DataTable ConvertToDatatable<T>(List<T> data)
        {
            PropertyDescriptorCollection props =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    table.Columns.Add(prop.Name, prop.PropertyType.GetGenericArguments()[0]);
                else
                    table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;
        }

        

        private void button1_Click(object sender, EventArgs e)
        {
            table.Rows[0][0] = "999";
            //dataSource.ResetBindings(false);

            /*
            records.RemoveAt(0);
            table = ConvertToDatatable(records);

            dataSource = new BindingSource(table, null);
            dataGridView1.DataSource = dataSource;*/
        }

        

        
    }
    public static class ControlExtensions
    {
        /// <summary>
        /// Executes the Action asynchronously on the UI thread, does not block execution on the calling thread.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="code"></param>
        public static void UIThread(this Control @this, Action code)
        {
            if (@this.InvokeRequired)
            {
                @this.BeginInvoke(code);
            }
            else
            {
                code.Invoke();
            }
        }
    }

    class Record
    {
        public string ID { get; set; }
        public string GPSValid { get; set; }
        public string DateTime { get; set; }
        public string Lat_Lon { get; set; }
        public string Speed { get; set; }
        public string Direction { get; set; }
        public string Temperature { get; set; }
        public string Status { get; set; }
        public string Event { get; set; }
        public string Message { get; set; }
    }
}
