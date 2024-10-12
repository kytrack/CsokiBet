using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace CsokiBet
{
    public partial class OrganizerPanel : Window
    {
        private const string connectionString = "Server=localhost;Database=csokibet;User ID=root;Password=;";

        public OrganizerPanel()
        {
            InitializeComponent();
            LoadEvents(); // Események betöltése az ablak inicializálásakor
        }

        private void LoadEvents()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("SELECT * FROM events", connection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                EventDataGrid.ItemsSource = dt.DefaultView;
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EventDataGrid.SelectedItem is DataRowView selectedRow)
            {
                EventNameTextBox.Text = selectedRow["EventName"].ToString();

                // A dátum stringként van, próbáljuk meg formázni yyyy-MM-dd formátumra
                string rawDate = selectedRow["EventDate"].ToString();
                DateTime dateValue;
                if (DateTime.TryParse(rawDate, out dateValue))
                {
                    EventDateTextBox.Text = dateValue.ToString("yyyy-MM-dd"); // Megjelenítés yyyy-MM-dd formátumban
                }
                else
                {
                    EventDateTextBox.Text = rawDate; // Ha valamiért nem sikerül, akkor marad az eredeti formátum
                }

                CategoryTextBox.Text = selectedRow["Category"].ToString();
                LocationTextBox.Text = selectedRow["Location"].ToString();
            }
        }

        private void CreateEvent_Click(object sender, RoutedEventArgs e)
        {
            string eventName = EventNameTextBox.Text;
            string eventDate = EventDateTextBox.Text; // Dátum stringként kezelve
            string category = CategoryTextBox.Text;
            string location = LocationTextBox.Text;

            if (!IsValidDateFormat(eventDate))
            {
                MessageBox.Show("A dátum formátuma helytelen! Használja a következő formátumot: yyyy-MM-dd.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(
                    "INSERT INTO events (EventName, EventDate, Category, Location) VALUES (@EventName, @EventDate, @Category, @Location)",
                    connection);
                command.Parameters.AddWithValue("@EventName", eventName);
                command.Parameters.AddWithValue("@EventDate", eventDate); // Stringként tároljuk
                command.Parameters.AddWithValue("@Category", category);
                command.Parameters.AddWithValue("@Location", location);
                command.ExecuteNonQuery();
            }

            LoadEvents(); // Frissítjük a DataGrid-et
            ClearTextBoxes(); // TextBox-ok tartalmának törlése
        }

        private void ModifyEvent_Click(object sender, RoutedEventArgs e)
        {
            if (EventDataGrid.SelectedItem is DataRowView selectedRow)
            {
                int eventId = Convert.ToInt32(selectedRow["EventID"]);
                string eventName = EventNameTextBox.Text;
                string eventDate = EventDateTextBox.Text; // Stringként kezeljük a dátumot
                string category = CategoryTextBox.Text;
                string location = LocationTextBox.Text;

                if (!IsValidDateFormat(eventDate))
                {
                    MessageBox.Show("A dátum formátuma helytelen! Használja a következő formátumot: yyyy-MM-dd.");
                    return;
                }

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand(
                        "UPDATE events SET EventName = @EventName, EventDate = @EventDate, Category = @Category, Location = @Location WHERE EventID = @EventID",
                        connection);
                    command.Parameters.AddWithValue("@EventID", eventId);
                    command.Parameters.AddWithValue("@EventName", eventName);
                    command.Parameters.AddWithValue("@EventDate", eventDate); // Stringként mentjük
                    command.Parameters.AddWithValue("@Category", category);
                    command.Parameters.AddWithValue("@Location", location);
                    command.ExecuteNonQuery();
                }

                LoadEvents(); // Frissítjük a DataGrid-et
                ClearTextBoxes(); // TextBox-ok tartalmának törlése
            }
        }

        private void DeleteEvent_Click(object sender, RoutedEventArgs e)
        {
            if (EventDataGrid.SelectedItem is DataRowView selectedRow)
            {
                int eventId = Convert.ToInt32(selectedRow["EventID"]);

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand("DELETE FROM events WHERE EventID = @EventID", connection);
                    command.Parameters.AddWithValue("@EventID", eventId);
                    command.ExecuteNonQuery();
                }

                LoadEvents(); // Frissítjük a DataGrid-et
                ClearTextBoxes(); // TextBox-ok tartalmának törlése
            }
        }

        // Helper metódus a TextBox-ok tartalmának törlésére
        private void ClearTextBoxes()
        {
            EventNameTextBox.Clear();
            EventDateTextBox.Clear();
            CategoryTextBox.Clear();
            LocationTextBox.Clear();
        }

        // Helper metódus a dátum formátum ellenőrzésére
        private bool IsValidDateFormat(string date)
        {
            DateTime tempDate;
            return DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out tempDate);
        }
    }
}
