using Board_Burning.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static Scanner scanner;
        public class cell 
        { 
            public int a { get; set; }
            public int b { get; set; }
            public bool 置位 { get; set; }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            IList<cell> cells = new BindingList<cell>();
            for (int i = 0; i < 10; i++)
            {
                cells.Add(new cell() { a = i, b = i, 置位 = false });
            }


            this.dataGridView1.DataSource = cells;



            dataGridView1[0, 0].Style.BackColor = Color.Pink;



            ////////////////////

            DataGridViewButtonColumn dgv_button_col = new DataGridViewButtonColumn();

            // 设定列的名字
            dgv_button_col.Name = "Detail";

            // 在所有按钮上表示"查看详情"
            dgv_button_col.UseColumnTextForButtonValue = true;
            dgv_button_col.Text = "查看详情";

            // 设置列标题
            dgv_button_col.HeaderText = "详情";

            // 向DataGridView追加
            dataGridView1.Columns.Insert(dataGridView1.Columns.Count, dgv_button_col);



            //////////////////////////////////////////////////
            ///
            scanner = new Scanner();
            scanner.IP = "192.168.1.104";
            scanner.Port = 24;
            scanner.ServerStart();


        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "Detail")
            {
                MessageBox.Show("行: " + e.RowIndex.ToString() + ", 列: " + e.ColumnIndex.ToString() + "; 被点击了");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            scanner.ServerSend(new byte[] { 0x02, 0x54, 0x03 });
            label1.Text = Encoding.Default.GetString(scanner.ServerReceive());
        }
    }
}
