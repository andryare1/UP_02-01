﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Appliances
{
    public partial class АpplicationAdd : Form
    {
        static SqlConnection connection;
        public int User { get; set; }

        public АpplicationAdd()
        {
            InitializeComponent();
            Connect();
            LoadHomeTechTypes();
        }

        public АpplicationAdd(int user) : this()
        {
            this.User = user;
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
            {
                MessageBox.Show($"Ошибка доступа к базе данных. Исключение: {ex.Message}");
            }
        }

        private void LoadHomeTechTypes()
        {
            SqlCommand command = new SqlCommand("SELECT DISTINCT orgTechType FROM Models", connection);
            HashSet<string> uniqueHomeTechTypes = new HashSet<string>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    uniqueHomeTechTypes.Add(reader["orgTechType"].ToString());
                }
            }

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(uniqueHomeTechTypes.ToArray());
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                comboBox2.DropDownStyle = ComboBoxStyle.DropDown;
                SqlCommand command1 = new SqlCommand($"SELECT orgTechModel FROM Models WHERE orgTechType = @techType", connection);
                command1.Parameters.AddWithValue("@techType", comboBox1.Text);

                using (var reader = command1.ExecuteReader())
                {
                    comboBox2.Items.Clear();
                    while (reader.Read())
                    {
                        comboBox2.Items.Add(reader["orgTechModel"].ToString());
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Выберите модель техники.");
                return;
            }

            Connect();

            SqlCommand command = new SqlCommand("SELECT ModelID FROM Models WHERE orgTechModel = @model", connection);
            command.Parameters.AddWithValue("@model", comboBox2.Text);
            int modelID = (int)command.ExecuteScalar();

            command = new SqlCommand("SELECT StatusID FROM Statuses WHERE StatusName = 'Новая заявка'", connection);
            int statusID = (int)command.ExecuteScalar();
            command = new SqlCommand("INSERT INTO Requests (StartDate, ProblemDescription, RequestStatusID, CompletionDate, RepairPartID, MasterID, ClientID, ModelID) " +
                                      "VALUES (@startDate, @problemDescription, @statusID, @completionDate, @repairPartID, @masterID, @clientID, @modelID)", connection);

            command.Parameters.AddWithValue("@startDate", DateTime.Now);
            command.Parameters.AddWithValue("@problemDescription", richTextBox1.Text);
            command.Parameters.AddWithValue("@statusID", statusID);
            command.Parameters.AddWithValue("@completionDate", DBNull.Value); 
            command.Parameters.AddWithValue("@repairPartID", DBNull.Value);
            command.Parameters.AddWithValue("@masterID", DBNull.Value); 
            command.Parameters.AddWithValue("@clientID", User);
            command.Parameters.AddWithValue("@modelID", modelID);

            command.ExecuteNonQuery();
            MessageBox.Show("Заявка успешно создана.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
