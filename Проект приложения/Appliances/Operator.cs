﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SortOrder = System.Data.SqlClient.SortOrder;

namespace Appliances
{
    public partial class Operator : Form
    {
        public static SqlConnection connection;
        SqlCommand command;

        public int UserID { get; set; }
        public Operator()
        {
            InitializeComponent();
            LoadUserInfo();
            Pass();
            StartPosition = FormStartPosition.CenterScreen;
            Add();  
            AddCompletedRequests(); 
        }

        public Operator(int userId) : this()
        {
            this.UserID = userId;
            LoadUserInfo();
            Add();  
            AddCompletedRequests();  
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
        private void Pass()
        {
            Connect();
            string query = @"
SELECT   
    r.loginDate, 
    h.login, 
    r.failure,
    r.UserId
FROM LoginAttempts r
LEFT JOIN Users h ON r.userId = h.userID;"; 

            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();

            List<string[]> data = new List<string[]>();
            int successCount = 0;
            int failureCount = 0;

            while (reader.Read())
            {
                int failureValue = Convert.ToInt32(reader["failure"]);
                bool isSuccess = failureValue == 1;

                if (isSuccess)
                    successCount++;
                else
                    failureCount++;


                object userIdValue = reader["UserId"];
                string userId = userIdValue == DBNull.Value ? "Unknown" : userIdValue.ToString();

        
                string login = reader["login"] == DBNull.Value ? "Неверный логин" : reader["login"].ToString();

                data.Add(new string[4]);
                data[data.Count - 1][0] = Convert.ToDateTime(reader["loginDate"]).ToString();
                data[data.Count - 1][1] = login; 
                data[data.Count - 1][2] = isSuccess ? "Успешно" : "Ошибка входа";
                data[data.Count - 1][3] = userId;  
            }
            reader.Close();

 
            dataGridView3.DataSource = null;
            dataGridView3.Rows.Clear();
            foreach (string[] s in data)
                dataGridView3.Rows.Add(s);
        }


        public void Add()
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
        ud.FullName AS ClientFullName,  
        c.Message AS MasterComment  
    FROM Requests r 
    INNER JOIN Models h ON r.ModelID = h.ModelID
    LEFT JOIN Statuses s ON r.RequestStatusID = s.StatusID
    LEFT JOIN Parts p ON r.RepairPartID = p.RepairPartID
    LEFT JOIN Users u ON r.MasterID = u.UserID
    LEFT JOIN Users ud ON r.ClientID = ud.UserID  
    LEFT JOIN Comments c ON r.RequestID = c.RequestID AND r.MasterID = c.MasterID  
    WHERE r.MasterID IS NULL";

            SqlCommand command = new SqlCommand(query, connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

          
            dataGridView1.Rows.Clear();

       
            foreach (DataRow row in dt.Rows)
            {
            
                string[] rowData = new string[11];

              
                rowData[0] = row["RequestID"].ToString();  
                rowData[1] = Convert.ToDateTime(row["StartDate"]).ToString("yyyy-MM-dd HH:mm");  
                rowData[2] = row["ModelType"].ToString();  
                rowData[3] = row["orgTechModel"].ToString();  
                rowData[4] = row["ProblemDescription"].ToString();  
                rowData[5] = row["StatusName"].ToString(); 
                rowData[6] = row["CompletionDate"] != DBNull.Value ? Convert.ToDateTime(row["CompletionDate"]).ToString("yyyy-MM-dd HH:mm") : "";  // Дата завершения
                rowData[7] = row["RepairPartName"].ToString();  
                rowData[8] = row["MasterFullName"].ToString();  
                rowData[9] = row["ClientFullName"].ToString();  
                rowData[10] = row["MasterComment"].ToString();  

                
                dataGridView1.Rows.Add(rowData);
            }

            command = new SqlCommand("SELECT Count(*) FROM Requests WHERE MasterID IS NULL", connection);
            int totalRecords1 = (int)command.ExecuteScalar();

            connection.Close();
            label4.Text = "Количество записей: " + dt.Rows.Count + " из " + totalRecords1;
        }


        private void button1_Click(object sender, EventArgs e)
        {

            if (dataGridView1.SelectedRows.Count > 0)
            {

                int requestId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["RequestID"].Value);  
                DateTime start = Convert.ToDateTime(dataGridView1.SelectedRows[0].Cells["StartDate"].Value);
                string type1 = dataGridView1.SelectedRows[0].Cells["OrgTechType"].Value.ToString();
                string problem = dataGridView1.SelectedRows[0].Cells["ProblemDescription"].Value.ToString();
                string status = dataGridView1.SelectedRows[0].Cells["StatusName"].Value.ToString();
                string client = dataGridView1.SelectedRows[0].Cells["ClientFullName"].Value.ToString();
                string model = dataGridView1.SelectedRows[0].Cells["orgTechModel"].Value.ToString();

                AddOper addOperForm = new AddOper(requestId, start, type1, model, problem, status, client); 
                addOperForm.ShowDialog();

   
                dataGridView1.Rows.Clear(); 
                Add(); 
            }
            else
            {
              
                MessageBox.Show("Пожалуйста, выберите заказ.");
            }

            Add(); 
            AddCompletedRequests();  
        }
        public int id { get; set; }
        public int id1 { get; set; }
        public DateTime start { get; set; }
        public string type1 { get; set; }
        public string problem { get; set; }
        public string status { get; set; }
        public string master { get; set; }
        public DateTime finish { get; set; }
        public string parts { get; set; }

        public string client { get; set; }
        public string massage { get; set; }

        public void AssignMaster(int requestId, int masterId)
        {
            Connect();

           
            string updateQuery = @"
    UPDATE Requests 
    SET MasterID = @MasterID, 
        RequestStatusID = 2  -- В процессе ремонта
    WHERE RequestID = @RequestID";

            SqlCommand command = new SqlCommand(updateQuery, connection);
            command.Parameters.AddWithValue("@MasterID", masterId);
            command.Parameters.AddWithValue("@RequestID", requestId);

            command.ExecuteNonQuery();
            connection.Close();
        }

        public void AddCompletedRequests()
        {
            Connect();

           
            string query = @"
    SELECT 
        r.RequestID, 
        r.StartDate, 
        h.orgTechType, 
 h.orgTechModel,-- Тип оборудования
        r.ProblemDescription,  -- Описание проблемы
        s.StatusName,  -- Статус заявки
        r.CompletionDate,  -- Дата выполнения
        p.RepairPartName,  -- Детали
        u.FullName AS MasterFullName,  -- Полное имя мастера
        ud.FullName AS ClientFullName,  -- Полное имя клиента
        c.Message AS Comment  -- Комментарий
    FROM 
        Requests r
    LEFT JOIN 
        Models h ON r.ModelID = h.ModelID  -- Тип оборудования
    LEFT JOIN 
        Statuses s ON r.RequestStatusID = s.StatusID  -- Статус заявки
    LEFT JOIN 
        Users u ON r.MasterID = u.UserID  -- Мастер
    LEFT JOIN 
        Users ud ON r.ClientID = ud.UserID  -- Клиент
    LEFT JOIN 
        Parts p ON r.RepairPartID = p.RepairPartID  -- Детали (запчасти)
    LEFT JOIN 
        Comments c ON r.RequestID = c.RequestID AND r.MasterID = c.MasterID  -- Комментарии
    ORDER BY 
        r.RequestID ASC";

            SqlCommand command = new SqlCommand(query, connection);
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            DataTable dt = new DataTable();
            adapter.Fill(dt);

           
            dataGridView2.Rows.Clear();

           
            foreach (DataRow row in dt.Rows)
            {
                string[] rowData = new string[11];

                rowData[0] = row["RequestID"].ToString();  
                rowData[1] = Convert.ToDateTime(row["StartDate"]).ToString("yyyy-MM-dd");  
                rowData[2] = row["orgTechType"].ToString(); 
                rowData[3] = row["orgTechModel"].ToString(); 
                rowData[4] = row["ProblemDescription"].ToString();  
                rowData[5] = row["StatusName"].ToString();  
                rowData[6] = row["CompletionDate"] != DBNull.Value ? Convert.ToDateTime(row["CompletionDate"]).ToString("yyyy-MM-dd") : "";  // Дата завершения
              

                rowData[7] = row["RepairPartName"] != DBNull.Value ? row["RepairPartName"].ToString() : "";  
                rowData[8] = row["MasterFullName"].ToString();  
                rowData[9] = row["ClientFullName"].ToString(); 
                rowData[10] = row["Comment"] != DBNull.Value ? row["Comment"].ToString() : "";  

                dataGridView2.Rows.Add(rowData);
            }

            command = new SqlCommand("SELECT COUNT(*) FROM Requests", connection);
            int totalRecords = (int)command.ExecuteScalar();

            command = new SqlCommand("SELECT COUNT(*) FROM Requests WHERE RequestStatusID = 3", connection);
            int completedRequests = (int)command.ExecuteScalar();

            label6.Text = $"Всего заявок: {totalRecords}, завершенных: {completedRequests}";

            connection.Close();
        }


        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                if (dataGridView1.SelectedRows[0].Cells.Count >= 9)
                {
                    id = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells[0].Value.ToString());

                    if (DateTime.TryParse(dataGridView1.SelectedRows[0].Cells[1].Value.ToString(), out DateTime parsedStart))
                    {
                        start = parsedStart;
                    }

                    type1 = dataGridView1.SelectedRows[0].Cells[2].Value?.ToString() ?? string.Empty;
                    problem = dataGridView1.SelectedRows[0].Cells[3].Value?.ToString() ?? string.Empty;
                    status = dataGridView1.SelectedRows[0].Cells[4].Value?.ToString() ?? string.Empty;
                    client = dataGridView1.SelectedRows[0].Cells[8].Value?.ToString() ?? string.Empty;
                }
                else
                {
                    MessageBox.Show("Недостаточно ячеек в выделенной строке.");
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            UpdateOper updateOper = new UpdateOper(id1);
            updateOper.ShowDialog();
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var selectedRow = dataGridView2.SelectedRows[0];
            id1 = Convert.ToInt32(dataGridView2.SelectedRows[0].Cells[0].Value.ToString());
            if (DateTime.TryParse(dataGridView2.SelectedRows[0].Cells[1].Value.ToString(), out DateTime parsedStart))
            {
                start = parsedStart;
            }
            type1 = dataGridView2.SelectedRows[0].Cells[2].Value.ToString();
            problem = dataGridView2.SelectedRows[0].Cells[3].Value.ToString();
            status = dataGridView2.SelectedRows[0].Cells[4].Value.ToString();
            if (selectedRow.Cells[5].Value != null && DateTime.TryParse(selectedRow.Cells[5].Value.ToString(), out DateTime parsedFinish))
            {
                finish = parsedFinish; 
            }

            parts = dataGridView2.SelectedRows[0].Cells[6].Value.ToString();
            master = dataGridView2.SelectedRows[0].Cells[7].Value.ToString();
            client = dataGridView2.SelectedRows[0].Cells[8].Value.ToString();
            massage = dataGridView2.SelectedRows[0].Cells[9].Value.ToString();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Connect();
            command = new SqlCommand($"SELECT COUNT(*) FROM Comment WHERE [requestID] = {id1}", connection);
            int count = (int)command.ExecuteScalar();

            if (count >= 0)
            {
                DialogResult result = MessageBox.Show("Вы точно хотите удалить этот запрос?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    command = new SqlCommand($"DELETE FROM Request WHERE [requestID] = {id1} and [statusID] = 1", connection);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Запрос успешно удален.", "Удаление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Удаление отменено.", "Отмена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Запрос с указанным ID не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            dataGridView2.Rows.Clear();
            AddCompletedRequests();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Connect();
            command = new SqlCommand($"SELECT COUNT(*) FROM Comments WHERE [requestID] = {id}", connection);
            int count = (int)command.ExecuteScalar();

            if (count >= 0)
            {
                DialogResult result = MessageBox.Show("Вы точно хотите удалить этот запрос?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    command = new SqlCommand($"DELETE FROM Requests WHERE [requestID] = {id}", connection);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Запрос успешно удален.", "Удаление", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Удаление отменено.", "Отмена", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Запрос с указанным ID не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            dataGridView1.Rows.Clear();
            Add();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.Close();
            Authorization authorization = new Authorization();
            authorization.ShowDialog();
        }
        private void Add2(string searchTerm)
        {
            Connect();
            string query = $@"
    SELECT 
        r.requestID, 
        r.startDate, 
        h.homeTechType,
        r.problemDescryption, 
        a.requestStatus, 
        r.completionDate, 
        b.repairParts, 
        с.fio, 
        d.fio,
        f.message
    FROM Request r
    INNER JOIN HomeTech h ON r.typeEqipmentID = h.typeEqipmentID
    LEFT JOIN [Status] a ON r.statusID = a.statusID
    LEFT JOIN Parts b ON r.partsID = b.partsID
    LEFT JOIN Customer с ON r.masterID = с.userID
    LEFT JOIN Customer d ON r.clientID = d.userID
    LEFT JOIN Comment f ON r.requestID = f.requestID
    WHERE r.statusID <> {3} 
    AND (
        r.requestID LIKE '%' + @searchTerm + '%' OR
        r.problemDescryption LIKE '%' + @searchTerm + '%' OR
        h.homeTechType LIKE '%' + @searchTerm + '%' OR
        a.requestStatus LIKE '%' + @searchTerm + '%' OR
        b.repairParts LIKE '%' + @searchTerm + '%' OR
        с.fio LIKE '%' + @searchTerm + '%' OR
        d.fio LIKE '%' + @searchTerm + '%' OR
        f.message LIKE '%' + @searchTerm + '%'
    )";
            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@searchTerm", searchTerm.Replace("'", "''")); 

            SqlDataReader reader = command.ExecuteReader();

            List<string[]> data = new List<string[]>();

            while (reader.Read())
            {
                data.Add(new string[10]);

                data[data.Count - 1][0] = reader[0].ToString();
                data[data.Count - 1][1] = Convert.ToDateTime(reader[1]).ToString("yyyy-MM-dd");
                data[data.Count - 1][2] = reader[2].ToString();
                data[data.Count - 1][3] = reader[3].ToString();
                data[data.Count - 1][4] = reader[4].ToString();
                if (reader[5] != DBNull.Value)
                {
                    data[data.Count - 1][5] = Convert.ToDateTime(reader[5]).ToString("yyyy-MM-dd");
                }
                data[data.Count - 1][6] = reader[6].ToString();
                data[data.Count - 1][7] = reader[7].ToString();
                data[data.Count - 1][8] = reader[8].ToString();
                data[data.Count - 1][9] = reader[9].ToString();
            }
            reader.Close();
            dataGridView2.DataSource = null;
            foreach (string[] s in data)
                dataGridView2.Rows.Add(s);
            int Records = data.Count;
            command = new SqlCommand($"SELECT Count(*) FROM Request", connection);
            int Records1 = (int)command.ExecuteScalar();
            connection.Close();
            label6.Text = "Количество записей: " + Records + " из " + Records1;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Connect();
            if (textBox2.Text == "")
            {
                AddCompletedRequests();
            }
            else
            {
                dataGridView2.Rows.Clear();
                Add2(textBox2.Text);

            }
        }
        public void LoadUserInfo()
        {
            Connect();

            string userInfoQuery = @"
                SELECT FullName, Phone, UserTypeName 
                FROM Users 
                JOIN UserTypes ON Users.UserTypeID = UserTypes.UserTypeID 
                WHERE UserID = @UserId";

            try
            {
                using (command = new SqlCommand(userInfoQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", this.UserID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            label1.Text = reader["FullName"].ToString();
                            label2.Text = reader["UserTypeName"].ToString();
                            label5.Text = reader["Phone"].ToString();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка получения данных о пользователе: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            Close();
            Authorization authorization = new Authorization();
            authorization.Show();
        }     
    }
}
