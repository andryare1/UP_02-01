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

namespace Appliances
{
    public partial class Order : Form
    {
        static SqlConnection connection;
        public int id { get; set; }
        public int user {  get; set; }
        public Order()
        {
            InitializeComponent();
            Connect();
           
         
        }
        public Order(int id, int user) : this(){
            this.id = id;
            this.user = user;
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
            Connect();
            int maxRequestId;
            using (SqlCommand command = new SqlCommand("SELECT ISNULL(MAX(RepairPartID), 0) FROM Parts", connection))
            {
                maxRequestId = (int)command.ExecuteScalar();
            }
            int request = maxRequestId + 1;
            using (SqlCommand command = new SqlCommand("INSERT INTO Parts ([RepairPartName]) VALUES (@RepairPartName)", connection))
            {
                command.Parameters.AddWithValue("@RepairPartName", textBox1.Text);
                command.ExecuteNonQuery();
            }

            using (SqlCommand command = new SqlCommand("UPDATE Requests SET [RepairPartID] = (SELECT RepairPartID FROM Parts WHERE RepairPartName = @RepairPartName) WHERE [RequestID] = @RequestID", connection))
            {
                command.Parameters.AddWithValue("@RepairPartName", textBox1.Text);
                command.Parameters.AddWithValue("@RequestID", id);
                command.ExecuteNonQuery();
            }

            int maxCommentId;
            using (SqlCommand command = new SqlCommand("SELECT ISNULL(MAX(commentID), 0) FROM Comments", connection))
            {
                maxCommentId = (int)command.ExecuteScalar();
            }
            int newCommentId = maxCommentId + 1;

            using (SqlCommand command = new SqlCommand("INSERT INTO Comments ([Message], [MasterID], [RequestID]) VALUES (@Message, @MasterID, @RequestID)", connection))
            {
                command.Parameters.AddWithValue("@Message", textBox2.Text);
                command.Parameters.AddWithValue("@MasterID", user);
                command.Parameters.AddWithValue("@RequestID", id);
                command.ExecuteNonQuery();
            }

            MessageBox.Show("Запись успешно добавлена.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
