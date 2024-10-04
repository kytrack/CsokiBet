using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CsokiBet
{
    public partial class BetMenu : Window
    {
        private string connectionString = "Server=127.0.0.1;Database=csokibet;User ID=etterem;Password=besinkaztunk;";
        private string userFilePath = "user_data.txt";

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

                                ListViewItem eventItem = new ListViewItem
                                {
                                    Content = $"{eventName} - {eventDate.ToShortDateString()} ({category}) at {location}",
                                    FontSize = 16,
                                    Foreground = System.Windows.Media.Brushes.White
                                };

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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle visibility of the settings panel
            SettingsPanel.Visibility = SettingsPanel.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            // Logic to save changes to email and password
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // Assume you have the userId available to update user data
                    int userId = 1; // Replace with the actual user ID

                    string query = "UPDATE bettors SET Email = @Email, Password = @Password WHERE Id = @UserId";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", EmailTextBox.Text);
                        cmd.Parameters.AddWithValue("@Password", PasswordBox.Password); // Use PasswordBox.Password for the password
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Settings saved successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}");
            }
        }

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            System.IO.File.WriteAllText(userFilePath, string.Empty);
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }
    }
}