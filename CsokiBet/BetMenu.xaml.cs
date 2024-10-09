using Firebase.Auth;
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace CsokiBet
{
    public partial class BetMenu : Window
    {
        private string connectionString = "Server=127.0.0.1;Database=csokibet;User ID=admin;Password=admin;";
        private string userFilePath = "user_data.txt";
        private string username; // Tárolja a felhasználó nevét

        public BetMenu()
        {
            InitializeComponent();
            LoadUsername();
            LoadBalance(); // Beolvassa az egyenleget a betöltött felhasználónév alapján
            LoadUpcomingEvents();
        }

        private void LoadUsername()
        {
            try
            {
                // Beolvassa az első sort a user_data.txt fájlból, ami a felhasználónév
                string[] userData = System.IO.File.ReadAllLines(userFilePath);
                username = userData[0]; // A fájl első sorában van a felhasználónév
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a felhasználónév beolvasása során: {ex.Message}");
            }
        }

        private void LoadBalance()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // SQL lekérdezés az egyenleg betöltésére a felhasználónév alapján
                    string query = "SELECT Balance FROM bettors WHERE Username = @username";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);

                        object result = cmd.ExecuteScalar(); // Az egyenleg lekérdezése
                        if (result != null)
                        {
                            decimal balance = Convert.ToDecimal(result); // Konvertálja a lekérdezett egyenleget

                            // Az egyenleg megjelenítése a UI-n
                            tbBalance.Text = $"Balance: ${balance}";
                        }
                        else
                        {
                            MessageBox.Show("Hiba: Nem található ilyen felhasználó az adatbázisban.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt az egyenleg betöltése során: {ex.Message}");
            }
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
            System.IO.File.WriteAllText(userFilePath, "logout");
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private async void btnPassWordReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Beolvassa a fájlból a második sort (az e-mail cím)
                string[] userData = System.IO.File.ReadAllLines(userFilePath);

                // Második sorból veszi az emailt
                string email = userData[1];

                // Firebase konfiguráció
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyBXxbFR3nwUFni-dBOB4dg7i3C-Z0SNgcw"));

                // Jelszó-visszaállító e-mail küldése
                await authProvider.SendPasswordResetEmailAsync(email);

                // Szétválasztjuk a nevet és a domain részt
                int atIndex = email.IndexOf('@');
                string namePart = email.Substring(0, atIndex);  // Az e-mail cím neve a @ előtt
                string domainPart = email.Substring(atIndex);   // Az e-mail cím domain része

                // Készítünk egy csillagos stringet a név helyett
                string maskedEmail = new string('*', namePart.Length) + domainPart;

                MessageBox.Show("Jelszó visszaállító e-mail elküldve erre az e-mail címre: " + maskedEmail);
                System.IO.File.WriteAllText(userFilePath, "logout");
                MainWindow login = new MainWindow();
                login.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a jelszó visszaállító e-mail küldésekor: {ex.Message}");
            }
        }
    }
}
