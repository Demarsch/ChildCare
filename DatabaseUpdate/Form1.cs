using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace UpdateDB
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            var localMachine = Environment.MachineName.ToUpper();
            var sqlSources = System.Data.Sql.SqlDataSourceEnumerator.Instance.GetDataSources();
            var source = sqlSources.Rows.Cast<DataRow>().FirstOrDefault(x => x["ServerName"].ToString().ToUpper() == localMachine);
            if (source != null)
            {
                var servername = source["ServerName"].ToString();
                var instanceName = source["InstanceName"].ToString();
                if (!String.IsNullOrEmpty(instanceName)) servername += '\\' + source["InstanceName"].ToString();
                textBox1.Text = servername;
            }
            Fill();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Fill();
        }

        private const string ConnectionStringTemplate = "data source=@source@;initial catalog=ChildCare;integrated security=True";
        private const string GetTablesCommand = "select TABLE_NAME from INFORMATION_SCHEMA.TABLES";
        private const string GetDataCommand = "select * from @table@";
        private const string InsertDataCommand = "insert into @table@ (@fields@) VALUES (@values@)";
        private const string MergeDataCommand = "merge @table@ as target using (values (@values@)) AS Source (@fields@) on (target.id = source.id) when matched then update set @update@ when not matched by target then insert (@targetfields@) values (@sourcefields@);";

        private void Fill()
        {
            int s = listBox1.SelectedIndex;
            listBox1.Items.Clear();

            var constr = ConnectionStringTemplate.Replace("@source@", textBox1.Text);
            System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(constr);
            try
            {
                con.Open();
            }
            catch
            {
                listBox1.Items.Add("Не удалось подключиться к БД");
            }

            List<string> tables = new List<string>();
            try
            {
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(GetTablesCommand, con);
                var rd = cmd.ExecuteReader(); 
                while (rd.Read())
                {
                    var tb = rd.GetString(0);
                    if (tb == "sysdiagrams")
                        continue;
                    if (System.IO.File.Exists(textBox2.Text + System.IO.Path.DirectorySeparatorChar + tb + ".xlsx")) 
                        tb = tb.PadRight(50) + "+ файл";
                    tables.Add(tb);
                }
                rd.Close();
            }
            catch
            {
                listBox1.Items.Add("Не удалось получить список таблиц БД");
                con.Close();
                return;
            }
            con.Close();
            listBox1.Items.AddRange(tables.OrderBy(x => !x.Contains("+ файл")).ThenBy(x => x).ToArray());

            try
            {
                listBox1.SelectedIndex = s;
            }
            catch
            {
                listBox1.SelectedIndex = -1;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null) return;
            var st = (listBox1.SelectedItem as string).Replace("+ файл", "").Trim();

            var constr = ConnectionStringTemplate.Replace("@source@", textBox1.Text);
            System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(constr);
            try
            {
                con.Open();
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к БД");
                return;
            }

            var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add(st + " Data");

            try
            {
                System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(GetDataCommand.Replace("@table@", st), con);
                var rd = cmd.ExecuteReader();
                int row = 1;
                for (int col = 0; col < rd.FieldCount; col++)
                {
                    sheet.Cell(row, col + 1).SetDataType(XLCellValues.Text);
                    sheet.Cell(row, col + 1).SetValue<string>(rd.GetName(col));
                }
                row++;
                while (rd.Read())
                {
                    for (int col = 0; col < rd.FieldCount; col++)
                    {
                        if (rd.IsDBNull(col)) 
                        {
                            sheet.Cell(row, col + 1).SetDataType(XLCellValues.Text);
                            sheet.Cell(row, col + 1).SetValue("NULL");
                        }
                        else if (rd.GetFieldType(col) == typeof(DateTime))
                        {
                            sheet.Cell(row, col + 1).SetDataType(XLCellValues.DateTime);
                            sheet.Cell(row, col + 1).SetValue(rd.GetDateTime(col));
                        }
                        else
                        {
                            sheet.Cell(row, col + 1).SetDataType(XLCellValues.Text);
                            sheet.Cell(row, col + 1).SetValue(rd.GetValue(col).ToString());
                        }
                    }
                    row++;
                }
                rd.Close();
            }
            catch
            {
                MessageBox.Show("Не удалось загрузить таблицу в файл");
                con.Close();
                sheet.Dispose();
                workbook.Dispose();
                return;
            }
            con.Close();
            workbook.SaveAs(textBox2.Text + System.IO.Path.DirectorySeparatorChar + st + ".xlsx");
            sheet.Dispose();
            workbook.Dispose();
            MessageBox.Show("Загружено успешно");
            Fill();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null) return;
            var st = (listBox1.SelectedItem as string).Replace("+ файл", "").Trim();
            if (SaveToTable(st)) MessageBox.Show("Таблица заполнена из файла успешно");
            else MessageBox.Show("Не удалось заполнить таблицу из файла");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count < 3) return;
            int ok = 0;
            for(int i = 0; i < listBox1.Items.Count; i++)
            {
                var st = (listBox1.Items[i] as string).Replace("+ файл", "").Trim();
                if (SaveToTable(st)) ok++;
            }
            MessageBox.Show("Заполнено из файлов " + ok.ToString() + " из " + listBox1.Items.Count.ToString());
        }

        public bool SaveToTable(string tbl)
        {
            if (!System.IO.File.Exists(textBox2.Text + System.IO.Path.DirectorySeparatorChar + tbl + ".xlsx")) return false;

            string fields = "";
            string targetfields = "";
            string sourcefields = "";
            string update = "";
            bool hasid = false;
            int idcol = 0;

            List<string> values = new List<string>();
            try
            {
                var workbook = new XLWorkbook(textBox2.Text + System.IO.Path.DirectorySeparatorChar + tbl + ".xlsx");
                var sheet = workbook.Worksheets.First();
                var cols = sheet.ColumnsUsed().Count();
                var rows = sheet.RowsUsed().Count();
                for (int col = 1; col <= cols; col++)
                {
                    var fld = sheet.Cell(1, col).GetString().ToLower();
                    if (fields.Length > 0) fields += ",";
                    fields += fld;
                    if (fld == "id")
                    {
                        hasid = true;
                        idcol = col;
                    }
                    else
                    {
                        if (targetfields.Length > 0) targetfields += ",";
                        targetfields += fld;
                        if (sourcefields.Length > 0) sourcefields += ",";
                        sourcefields += ("source." + fld);
                        if (update.Length > 0) update += ",";
                        update += ("target." + fld + "=source." + fld);
                    }
                }
                for (int row = 2; row <= rows; row++)
                {
                    string val = "";
                    for (int col = 1; col <= cols; col++)
                    {
                        var vl = sheet.Cell(row, col).DataType != XLCellValues.DateTime ? sheet.Cell(row, col).GetString().Replace("'", "''") : sheet.Cell(row, col).GetDateTime().ToString("yyyy-MM-dd HH:mm:ss");
                        if (vl.ToLower() != "null" && idcol != col) vl = "'" + vl + "'";
                        val += ((col > 1 ? "," : "") + vl);
                    }
                    values.Add(val);
                }
            }
            catch
            {
                return false;
            }
                
            var constr = ConnectionStringTemplate.Replace("@source@", textBox1.Text);
            System.Data.SqlClient.SqlConnection con = new System.Data.SqlClient.SqlConnection(constr);
            try
            {
                con.Open();
            }
            catch
            {
                return false;
            }

            string cmdtext = (hasid ? MergeDataCommand.Replace("@update@", update).Replace("@targetfields@", targetfields).Replace("@sourcefields@", sourcefields) : InsertDataCommand).Replace("@table@", tbl).Replace("@fields@", fields);
            System.Data.SqlClient.SqlCommand cmd = new System.Data.SqlClient.SqlCommand(cmdtext, con);

            bool ret = true;
            try
            {
                foreach(var val in values)
                {
                    cmd.CommandText = cmdtext.Replace("@values@", val);
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                ret = false;
            }
            
            con.Close();
            return ret;
        }
    }
}
