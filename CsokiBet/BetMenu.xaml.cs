using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CsokiBet
{
    public partial class BetMenu : Window
    {
        // Connection string for the MySQL database
        private string connectionString = "Server=127.0.0.1;Database=csokibet;User ID=etterem;Password=besinkaztunk;";

        public BetMenu()
        {
            InitializeComponent();
            LoadUpcomingEvents();
        }

        private void LoadUpcomingEvents()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT EventName, EventDate, Category, Location FROM events WHERE EventDate >= CURDATE() ORDER BY EventDate ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string eventName = reader.GetString("EventName");
                                DateTime eventDate = reader.GetDateTime("EventDate");
                                string category = reader.GetString("Category");
                                string location = reader.GetString("Location");

                                // Create a new ListViewItem for each event
                                ListViewItem eventItem = new ListViewItem
                                {
                                    Content = $"{eventName} - {eventDate.ToShortDateString()} ({category}) at {location}",
                                    FontSize = 16,
                                    Foreground = System.Windows.Media.Brushes.White
                                };

                                // Add the ListViewItem to the ListView
                                UpcomingEventsListView.Items.Add(eventItem);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading events: {ex.Message}");
            }
        }
    }
}