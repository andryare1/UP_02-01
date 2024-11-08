using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Appliances
{
    public partial class UpdateOper : Form
    {
        public int id { get; set; }

        static SqlConnection connection;
        SqlCommand command;
        public UpdateOper()
        {
            InitializeComponent();
            textBox1.ReadOnly = true;
            textBox2.ReadOnly = true;
            textBox4.ReadOnly = true;
            textBox5.ReadOnly = true;
            textBox6.ReadOnly = true;
            richTextBox1.ReadOnly = true;
        }
        public UpdateOper(int id) : this()
        {
            this.id = id;
          
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
            { Console.WriteLine($"Ощибка доступа к базе данных. Исключение: {ex.Message}"); }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Connect();

                string updateQuery = $@"
            UPDATE Requests 
            SET 
                RequestStatusID = {3}, 
                CompletionDate = '{dateTimePicker2.Value}' 
            WHERE RequestID = {id}";

                command = new SqlCommand(updateQuery, connection);
                command.ExecuteNonQuery();

                MessageBox.Show("Заявка обновлена успешно.");
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка обновления заявки: {ex.Message}");
            }
        }

        private void UpdateOper_Load(object sender, EventArgs e)
        {
            Connect();

            try
            {
                string query = @"
            SELECT 
                r.StartDate,               -- Дата создания
                h.orgTechType,             -- Тип оборудования
                h.orgTechModel,            -- Модель техники
                r.ProblemDescription,      -- Проблема
                u.FullName AS MasterFullName, -- ФИО мастера
                r.CompletionDate,          -- Дата выполнения
                p.RepairPartName,          -- Необходимые детали
                ud.FullName AS ClientFullName, -- Клиент
                c.Message AS Comment       -- Комментарий
            FROM 
                Requests r
            LEFT JOIN 
                Models h ON r.ModelID = h.ModelID
            LEFT JOIN 
                Users u ON r.MasterID = u.UserID
            LEFT JOIN 
                Users ud ON r.ClientID = ud.UserID
            LEFT JOIN 
                Parts p ON r.RepairPartID = p.RepairPartID
            LEFT JOIN 
                Comments c ON r.RequestID = c.RequestID AND r.MasterID = c.MasterID
            WHERE 
                r.RequestID = @RequestID";

                command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@RequestID", id); 

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        dateTimePicker1.Value = Convert.ToDateTime(reader["StartDate"]); 
                        textBox1.Text = reader["orgTechType"].ToString(); 
                        textBox2.Text = reader["orgTechModel"].ToString(); 
                        richTextBox1.Text = reader["ProblemDescription"].ToString(); 
                        textBox5.Text = reader["MasterFullName"].ToString(); 
                        textBox4.Text = reader["RepairPartName"] != DBNull.Value ? reader["RepairPartName"].ToString() : ""; 
                        textBox6.Text = reader["ClientFullName"].ToString();
                        textBox7.Text = reader["Comment"] != DBNull.Value ? reader["Comment"].ToString() : ""; 

                        if (reader["CompletionDate"] != DBNull.Value)
                        {
                            dateTimePicker2.Value = Convert.ToDateTime(reader["CompletionDate"]);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Данные не найдены для данной заявки.");
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
