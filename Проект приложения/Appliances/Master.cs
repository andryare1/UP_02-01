using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;


namespace Appliances
{
    public partial class Master : Form
    {

        static SqlConnection connection;
        SqlCommand command;
        public string Type { get; set; }
        public string Fio { get; set; }
        public int User { get; set; }
        public Master()
        {
            InitializeComponent();
            Add();
            StartPosition = FormStartPosition.CenterScreen;
            Add1();

        }
        public Master( int user) : this()
        {
            this.User = user;
            Add();
            StartPosition = FormStartPosition.CenterScreen;
            Add1();         
        }

    static public void Connect()
        {
            try
            {
                connection = new SqlConnection("Data Source=ADCLG1;Initial Catalog=$ЯремаЗелепугин;Integrated Security=True;");
                //connection = new SqlConnection("Data Source=DESKTOP-P7R9Q2V;Initial Catalog=Yarema;Integrated Security=True;");
                connection.Open();
            }
            catch (SqlException ex)
            { Console.WriteLine($"Ошибка доступа к базе данных. Исключение: {ex.Message}"); }
        }
        private void Add()
        {
            Connect();
           

            string query = @"
        SELECT 
            r.RequestID, 
            r.StartDate, 
            h.orgTechType, 
            h.orgTechModel,
            r.ProblemDescription,  
            s.StatusName,  
            r.CompletionDate,  
            p.RepairPartName,  
            u.FullName AS MasterFullName,  
            ud.FullName AS ClientFullName,  
            c.Message AS Comment  
        FROM 
            Requests r
        LEFT JOIN 
            Models h ON r.ModelID = h.ModelID  
        LEFT JOIN 
            Statuses s ON r.RequestStatusID = s.StatusID  
        LEFT JOIN 
            Users u ON r.MasterID = u.UserID  
        LEFT JOIN 
            Users ud ON r.ClientID = ud.UserID  
        LEFT JOIN 
            Parts p ON r.RepairPartID = p.RepairPartID  
        LEFT JOIN 
            Comments c ON r.RequestID = c.RequestID AND r.MasterID = c.MasterID  
 WHERE 
            r.MasterID = @User AND r.RequestStatusID = @StatusID
        ORDER BY 
            r.RequestID ASC";


            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@User", User);
            command.Parameters.AddWithValue("@StatusID", 2);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

         
            dataGridView2.Rows.Clear();

         
            foreach (DataRow row in dt.Rows)
            {
                string[] rowData = new string[9];

                rowData[0] = row["RequestID"].ToString();  
                rowData[1] = Convert.ToDateTime(row["StartDate"]).ToString("yyyy-MM-dd");  
                rowData[2] = row["orgTechType"]?.ToString() ?? "";  
                rowData[3] = row["orgTechModel"]?.ToString() ?? "";  
                rowData[4] = row["ProblemDescription"]?.ToString() ?? "";  
                rowData[5] = row["StatusName"]?.ToString() ?? "";  
                rowData[6] = row["CompletionDate"] != DBNull.Value ? Convert.ToDateTime(row["CompletionDate"]).ToString("yyyy-MM-dd") : ""; 
                rowData[7] = row["RepairPartName"]?.ToString() ?? "";  
                rowData[8] = row["Comment"] != DBNull.Value ? row["Comment"].ToString() : "";  

                dataGridView1.Rows.Add(rowData);
            }

            connection.Close();
        }

        private void Add1()
        {
            Connect();
            string query = @"
        SELECT 
            r.RequestID, 
            r.StartDate, 
            h.orgTechType AS ModelType, 
            h.orgTechModel,
            r.ProblemDescription, 
            s.StatusName, 
            r.CompletionDate, 
            p.RepairPartName, 
            u.FullName AS MasterFullName,            
            c.Message AS MasterComment  
        FROM Requests r 
        INNER JOIN Models h ON r.ModelID = h.ModelID
        LEFT JOIN Statuses s ON r.RequestStatusID = s.StatusID
        LEFT JOIN Parts p ON r.RepairPartID = p.RepairPartID
        LEFT JOIN Users u ON r.MasterID = u.UserID
        LEFT JOIN Users ud ON r.ClientID = ud.UserID  
        LEFT JOIN Comments c ON r.RequestID = c.RequestID AND r.MasterID = c.MasterID 
        WHERE r.MasterID = @User AND r.RequestStatusID = 3";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@User", User);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    List<string[]> data = new List<string[]>();

                    while (reader.Read())
                    {
                       
                        string[] row = new string[9];

                        row[0] = reader["RequestID"].ToString();
                        row[1] = Convert.ToDateTime(reader["StartDate"]).ToString("yyyy-MM-dd");
                        row[2] = reader["ModelType"]?.ToString() ?? " ";
                        row[3] = reader["orgTechModel"]?.ToString() ?? " ";
                        row[4] = reader["ProblemDescription"]?.ToString() ?? " ";
                        row[5] = reader["StatusName"]?.ToString() ?? " ";
                        row[6] = reader["CompletionDate"] != DBNull.Value ? Convert.ToDateTime(reader["CompletionDate"]).ToString("yyyy-MM-dd") : "";
                        row[7] = reader["RepairPartName"]?.ToString() ?? " ";

                        row[8] = reader["MasterComment"]?.ToString() ?? " "; 

                        data.Add(row);
                    }

                    dataGridView2.Rows.Clear();
                    foreach (string[] s in data)
                    {
                        dataGridView2.Rows.Add(s);
                    }
                }
            }

  
            command = new SqlCommand("SELECT COUNT(*) FROM Requests WHERE MasterID = @User", connection);
            command.Parameters.AddWithValue("@User", User);
            int totalRecords1 = (int)command.ExecuteScalar();
            connection.Close();
            label4.Text = "Количество записей: " + dataGridView2.Rows.Count + " из " + totalRecords1;
        }




        private void button1_Click(object sender, EventArgs e)
        {
            Order order = new Order(id, User);
            order.ShowDialog();
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            Add();
            Add1();

        }
        public int id {  get; set; }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
            Authorization authorization = new Authorization();
            authorization.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UpdateOper updateOper = new UpdateOper(id);
            updateOper.ShowDialog();
            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();
            Add();
            Add1();

        }
    }
}
