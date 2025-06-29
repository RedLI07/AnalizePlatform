using System;
using System.Data;
using System.Windows.Forms;
using MySqlConnector;

namespace WindowsFormsApp6
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ButtonSearch_Click(object sender, EventArgs e)
        {
            if (comboBoxStudent.SelectedItem == null)
            {
                MessageBox.Show("Выберите студента!");
                return;
            }

            string fullName = comboBoxStudent.SelectedItem.ToString();
            string[] nameParts = fullName.Split(' ');
            string firstName = nameParts[0];
            string lastName = string.Empty;

            if (nameParts.Length > 1)
            {
                lastName = nameParts[1];
            }

            string selectedPeriod = null;
            if (comboBoxPeriod.SelectedItem != null)
            {
                selectedPeriod = comboBoxPeriod.SelectedItem.ToString();
            }

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
                    {
                        command.Parameters.AddWithValue("@periodName", selectedPeriod);
                    }

                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataTable.Columns["ФИО"].SetOrdinal(0);
                    dataTable.Columns["Метрика"].SetOrdinal(1);
                    dataTable.Columns["Оценка"].SetOrdinal(2);
                    dataTable.Columns["Единица_измерения"].SetOrdinal(3);
                    dataTable.Columns["Период"].SetOrdinal(4);
                    dataTable.Columns["Дата_оценки"].SetOrdinal(5);

                    dataGridViewResults.DataSource = dataTable;

                    dataGridViewResults.Columns["ФИО"].HeaderText = "ФИО студента";
                    dataGridViewResults.Columns["Метрика"].HeaderText = "Метрика";
                    dataGridViewResults.Columns["Оценка"].HeaderText = "Оценка";
                    dataGridViewResults.Columns["Единица_измерения"].HeaderText = "Ед. измерения";
                    dataGridViewResults.Columns["Период"].HeaderText = "Период";
                    dataGridViewResults.Columns["Дата_оценки"].HeaderText = "Дата оценки";
                }

                connection.Close();
            }
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedGroup = comboBoxGroup.SelectedItem.ToString();
            string queryStudents = "SELECT p.name, last_name FROM Participant p JOIN Groups g ON p.`group` = g.id WHERE g.name = @groupName";

            using (var connection = new MySqlConnection(StrinConnect.StringConnect()))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(queryStudents, connection))
                {
                    command.Parameters.AddWithValue("@groupName", selectedGroup);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        comboBoxStudent.Items.Clear();

                        while (reader.Read())
                        {
                            string fullName = $"{reader.GetString("name")} {reader.GetString("last_name")}";
                            comboBoxStudent.Items.Add(fullName);
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
                        comboBoxGroup.Items.Clear();

                        while (reader.Read())
                        {
                            string groupName = reader.GetString("name");
                            comboBoxGroup.Items.Add(groupName);
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
                        comboBoxPeriod.Items.Clear();

                        while (reader.Read())
                        {
                            string periodName = reader.GetString("name");
                            comboBoxPeriod.Items.Add(periodName);
                        }
                    }
                }
                connection.Close();
            }
        }
    }
}