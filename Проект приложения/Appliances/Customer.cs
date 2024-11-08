using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Appliances
{
    public partial class Customer : Form
    {
        private static SqlConnection connection;
        private SqlCommand command;
        public int UserId { get; set; }

        public Customer(int userId)
        {
            InitializeComponent();
            this.UserId = userId;
            StartPosition = FormStartPosition.CenterScreen;
            LoadUserInfo();
            LoadRequests();
        }

        private static void Connect()
        {
            try
            {
                connection = new SqlConnection("Data Source=ADCLG1;Initial Catalog=$ЯремаЗелепугин;Integrated Security=True;");
                //connection = new SqlConnection("Data Source=DESKTOP-P7R9Q2V;Initial Catalog=Yarema;Integrated Security=True;");
                connection.Open();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Ошибка подключения к базе данных: {ex.Message}");
            }
        }

        private void LoadUserInfo()
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
                    command.Parameters.AddWithValue("@UserId", this.UserId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            label3.Text = reader["FullName"].ToString();
                            label5.Text = reader["Phone"].ToString();
                            label6.Text = reader["UserTypeName"].ToString();
                        }
                    }

                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Ошибка получения данных о пользователе: {ex.Message}");
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void LoadRequests()
        {
            Connect();

            string requestsQuery = @"
        SELECT 
            r.RequestID, 
            r.StartDate, 
            m.orgTechType AS TechType, 
            m.orgTechModel AS Model, 
            r.ProblemDescription, 
            s.StatusName AS RequestStatus
        FROM 
            Requests r
        LEFT JOIN 
            Statuses s ON r.RequestStatusID = s.StatusID
        LEFT JOIN 
            Models m ON r.ModelID = m.ModelID
        WHERE 
            r.ClientID = @UserId";

            try
            {
                using (var command = new SqlCommand(requestsQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", this.UserId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<string[]> data = new List<string[]>();

                        while (reader.Read())
                        {
                            string[] row = new string[6];
                            row[0] = reader["RequestID"].ToString();
                            row[1] = Convert.ToDateTime(reader["StartDate"]).ToString("yyyy-MM-dd");
                            row[2] = reader["TechType"].ToString();
                            row[3] = reader["Model"].ToString();
                            row[4] = reader["ProblemDescription"].ToString();
                            row[5] = reader["RequestStatus"].ToString();

                            data.Add(row);
                        }

                        dataGridView1.Rows.Clear();
                        foreach (string[] row in data)
                        {
                            dataGridView1.Rows.Add(row);
                        }

                        int totalRecords = data.Count;
                        label4.Text = $"Количество записей: {totalRecords}";
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Ошибка получения данных заявок: {ex.Message}");
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            Authorization authorization = new Authorization();
            authorization.Show();
            this.Close();
        }

        private void buttonAddRequest_Click(object sender, EventArgs e)
        {
            АpplicationAdd applicationAdd = new АpplicationAdd(UserId);
            applicationAdd.Show();
            LoadRequests();
        }

        private void buttonChangeRequest_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int requestId = Convert.ToInt32(dataGridView1.CurrentRow.Cells[0].Value);
                string typeEquipment = dataGridView1.CurrentRow.Cells[2].Value.ToString();
                string problem = dataGridView1.CurrentRow.Cells[3].Value.ToString();

                АpplicationСhange applicationChange = new АpplicationСhange(requestId);
                applicationChange.ShowDialog();
                LoadRequests();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заявку для изменения.");
            }
        }

        private void Customer_Load(object sender, EventArgs e)
        {
            LoadUserInfo();
            LoadRequests();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
            Authorization authorization = new Authorization();
            authorization.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            АpplicationAdd аpplicationAdd = new АpplicationAdd(UserId);
            аpplicationAdd.ShowDialog();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            LoadRequests();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                int requestId = Convert.ToInt32(selectedRow.Cells[0].Value);

                string typeEquipment = GetTypeEquipmentForRequest(requestId);
                string problem = GetProblemForRequest(requestId);

                АpplicationСhange updateForm = new АpplicationСhange(requestId);
                updateForm.ShowDialog();

                LoadRequests();
            }
        }

        private string GetTypeEquipmentForRequest(int requestId)
        {
            string typeEquipment = string.Empty;
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                string query = @"
            SELECT m.orgTechType 
            FROM Requests r 
            JOIN Models m ON r.ModelID = m.ModelID 
            WHERE r.RequestID = @RequestId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RequestId", requestId);

                typeEquipment = command.ExecuteScalar()?.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении типа оборудования: {ex.Message}");
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return typeEquipment;
        }

        private string GetProblemForRequest(int requestId)
        {
            string problem = string.Empty;
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                string query = @"
            SELECT problemDescription 
            FROM Requests 
            WHERE requestID = @RequestId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RequestId", requestId);

                problem = command.ExecuteScalar()?.ToString();

                if (problem == null)
                {
                    MessageBox.Show($"Описание проблемы для запроса {requestId} не найдено.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка SQL при получении описания проблемы: {ex.Message}\n{ex.StackTrace}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неизвестная ошибка: {ex.Message}\n{ex.StackTrace}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return problem;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LoadRequests(textBox1.Text);
        }

        private void LoadRequests(string filter = "")
        {
            Connect();

            string requestsQuery = @"
    SELECT 
        r.RequestID, 
        r.StartDate, 
        m.orgTechType AS TechType, 
        m.orgTechModel AS Model, 
        r.ProblemDescription, 
        s.StatusName AS RequestStatus
    FROM 
        Requests r
    LEFT JOIN 
        Statuses s ON r.RequestStatusID = s.StatusID
    LEFT JOIN 
        Models m ON r.ModelID = m.ModelID
    WHERE 
        r.ClientID = @UserId 
        AND (
            r.RequestID LIKE @Filter OR
            r.StartDate LIKE @Filter OR
            m.orgTechType LIKE @Filter OR
            m.orgTechModel LIKE @Filter OR
            r.ProblemDescription LIKE @Filter OR
            s.StatusName LIKE @Filter
        )";

            try
            {
                using (var command = new SqlCommand(requestsQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", this.UserId);
                    command.Parameters.AddWithValue("@Filter", "%" + filter + "%");

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        List<string[]> data = new List<string[]>();

                        while (reader.Read())
                        {
                            string[] row = new string[6];
                            row[0] = reader["RequestID"].ToString();
                            row[1] = Convert.ToDateTime(reader["StartDate"]).ToString("yyyy-MM-dd");
                            row[2] = reader["TechType"].ToString();
                            row[3] = reader["Model"].ToString();
                            row[4] = reader["ProblemDescription"].ToString();
                            row[5] = reader["RequestStatus"].ToString();

                            data.Add(row);
                        }

                        dataGridView1.Rows.Clear();
                        foreach (string[] row in data)
                        {
                            dataGridView1.Rows.Add(row);
                        }

                        int totalRecords = data.Count;
                        label4.Text = $"Количество записей: {totalRecords}";
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Ошибка получения данных заявок: {ex.Message}");
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
    }
}
