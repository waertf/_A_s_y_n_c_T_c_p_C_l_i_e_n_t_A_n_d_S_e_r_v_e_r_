using System;
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
        public Form1()
        {
            InitializeComponent();
            

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
                      Thread.Sleep(100);
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
}
