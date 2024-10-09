using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.IO;
using Firebase.Auth;

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


        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            System.IO.File.WriteAllText(userFilePath, string.Empty);
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private async void btnPassWordReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Replace with the user's email address
                string email = "20.danko.daniel.janos@mechwart.com"; // This can be dynamically retrieved from your application

                // Firebase configuration
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyBXxbFR3nwUFni-dBOB4dg7i3C-Z0SNgcw"));

                // Send password reset email
                await authProvider.SendPasswordResetEmailAsync(email);

                MessageBox.Show("Password reset email has been sent to: " + email);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending password reset email: {ex.Message}");
            }
        }

    }
}