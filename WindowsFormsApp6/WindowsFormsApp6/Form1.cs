using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using MySql.Data;
using MySqlConnector;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Data.SqlClient;

namespace WindowsFormsApp6
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Выберите студента!");
                return;
            }

            string fullName = comboBox2.SelectedItem.ToString();
            string[] nameParts = fullName.Split(' ');
            string firstName = nameParts[0];
            string lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            string selectedPeriod = comboBox3.SelectedItem != null ? comboBox3.SelectedItem.ToString() : null;

            using (MySqlConnection connection = new MySqlConnection(StrinConnect.StringConnect()))
            {
                connection.Open();

                string query = @"SELECT 
                CONCAT(Participant.name, ' ', Participant.last_name) AS ФИО,
                Metrics.name AS Метрика,
                Marks.value AS Оценка,
                Metrics.unit AS Единица_измерения,
                Period.name AS Период,
                DATE_FORMAT(Marks.evaluated, '%d.%m.%Y %H:%i') AS Дата_оценки
            FROM Marks
            JOIN Metrics ON Marks.metric_id = Metrics.id
            JOIN Period ON Marks.period_id = Period.id
            JOIN Participant ON Marks.participant_id = Participant.id
            WHERE Participant.name = @firstName 
            AND Participant.last_name = @lastName";

                if (selectedPeriod != null)
                {
                    query += " AND Period.name = @periodName";
                }

                query += " ORDER BY Period.name, Metrics.name";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@firstName", firstName);
                    command.Parameters.AddWithValue("@lastName", lastName);

                    if (selectedPeriod != null)
                        command.Parameters.AddWithValue("@periodName", selectedPeriod);

                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

 
                    dataTable.Columns["ФИО"].SetOrdinal(0);
                    dataTable.Columns["Метрика"].SetOrdinal(1);
                    dataTable.Columns["Оценка"].SetOrdinal(2);
                    dataTable.Columns["Единица_измерения"].SetOrdinal(3);
                    dataTable.Columns["Период"].SetOrdinal(4);
                    dataTable.Columns["Дата_оценки"].SetOrdinal(5);

                    dataGridView1.DataSource = dataTable;

             
                    dataGridView1.Columns["ФИО"].HeaderText = "ФИО студента";
                    dataGridView1.Columns["Метрика"].HeaderText = "Метрика";
                    dataGridView1.Columns["Оценка"].HeaderText = "Оценка";
                    dataGridView1.Columns["Единица_измерения"].HeaderText = "Ед. измерения";
                    dataGridView1.Columns["Период"].HeaderText = "Период";
                    dataGridView1.Columns["Дата_оценки"].HeaderText = "Дата оценки";
                }

                connection.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedGroup = comboBox1.SelectedItem.ToString();
            string queryStudents = "SELECT p.name, last_name FROM Participant p JOIN Groups g ON p.`group` = g.id WHERE g.name = @groupName";

            using (var connection = new MySqlConnection(StrinConnect.StringConnect()))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(queryStudents, connection))
                {
                    command.Parameters.AddWithValue("@groupName", selectedGroup);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        comboBox2.Items.Clear();

                        while (reader.Read())
                        {
                            string fullName = reader.GetString("name") + " " + reader.GetString("last_name");
                            comboBox2.Items.Add(fullName);
                        }
                    }
                }
                connection.Close();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadGroups();
            LoadPeriods();
        }

        private void LoadGroups()
        {
            string queryGroup = "SELECT id, name FROM Groups";
            using (var connection = new MySqlConnection(StrinConnect.StringConnect()))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(queryGroup, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        comboBox1.Items.Clear();

                        while (reader.Read())
                        {
                            string groupName = reader.GetString("name");
                            comboBox1.Items.Add(groupName);
                        }
                    }
                }
                connection.Close();
            }
        }

        private void LoadPeriods()
        {
            string queryPeriod = "SELECT id, name FROM Period";
            using (var connection = new MySqlConnection(StrinConnect.StringConnect()))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(queryPeriod, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        comboBox3.Items.Clear();

                        while (reader.Read())
                        {
                            string periodName = reader.GetString("name");
                            comboBox3.Items.Add(periodName);
                        }
                    }
                }
                connection.Close();
            }
        }


    }
}
