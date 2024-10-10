﻿using Firebase.Auth;
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CsokiBet
{
    public partial class BetMenu : Window
    {
        string Loadedemail;
        string Loadedusername;
        private string connectionString = "Server=127.0.0.1;Database=csokibet;User ID=root;Password=;";
        private string userFilePath = "user_data.txt";
        private Random _random = new Random();
        private double playerBalance;
        private int bettorID;
        private string role;
        private Border selectedEventTile = null;

        public BetMenu(string email, string username)
        {
            InitializeComponent();
            this.Loadedemail = email;
            this.Loadedusername = username;
            LoadUser();
            LoadUpcomingEvents();
        }

        // Constructor for the other version where no email/username is passed
        public BetMenu()
        {
            InitializeComponent();
            LoadUsername();
            LoadBalance();
            LoadUpcomingEvents();
        }

        private void LoadUser()
        {
            try
            {
                var userData = System.IO.File.ReadAllLines(userFilePath);
                if (userData.Length < 3)
                {
                    MessageBox.Show("User data is missing or incomplete.");
                    return;
                }

                string email = Loadedemail;
                string username = Loadedusername;

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT BettorsID, Balance, Role FROM bettors WHERE Email = @Email";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bettorID = reader.GetInt32("BettorsID");
                                playerBalance = reader.GetDouble("Balance");
                                role = reader.GetString("Role");
                                tbBalance.Text = $"Balance: ${playerBalance}";
                            }
                            else
                            {
                                MessageBox.Show("Invalid email");
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user: {ex.Message}");
            }

            if (role == "Admin") {
                btnAdmin.Visibility = Visibility.Visible;
            }
        }

        private void LoadUsername()
        {
            try
            {
                string[] userData = System.IO.File.ReadAllLines(userFilePath);
                Loadedusername = userData[0];
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
                    string query = "SELECT Balance FROM bettors WHERE Username = @username";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", Loadedusername);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            decimal balance = Convert.ToDecimal(result);
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
                    string query = "SELECT EventID, EventName, EventDate, Category, Location FROM events WHERE EventDate >= CURDATE() ORDER BY EventDate ASC";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int eventID = reader.GetInt32("EventID");
                                string eventName = reader.GetString("EventName");
                                string category = reader.GetString("Category");
                                string location = reader.GetString("Location");

                                double odds = Math.Round(1.5 + _random.NextDouble() * 2, 2);

                                Border eventTile = new Border
                                {
                                    Background = new SolidColorBrush(Colors.DarkSlateGray),
                                    CornerRadius = new CornerRadius(10),
                                    Width = 200,
                                    Height = 150,
                                    Margin = new Thickness(10)
                                };

                                StackPanel tileContent = new StackPanel
                                {
                                    Orientation = Orientation.Vertical,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center
                                };

                                TextBlock eventText = new TextBlock
                                {
                                    Text = $"{eventName} ({category})",
                                    Foreground = Brushes.White,
                                    FontWeight = FontWeights.Bold,
                                    FontSize = 14,
                                    TextAlignment = TextAlignment.Center
                                };
                                tileContent.Children.Add(eventText);

                                TextBlock oddsText = new TextBlock
                                {
                                    Text = $"Odds: {odds:F2}",
                                    Foreground = Brushes.LightGreen,
                                    FontWeight = FontWeights.Bold,
                                    FontSize = 12,
                                    TextAlignment = TextAlignment.Center
                                };
                                tileContent.Children.Add(oddsText);

                                TextBlock locationText = new TextBlock
                                {
                                    Text = location,
                                    Foreground = Brushes.LightGray,
                                    FontSize = 12,
                                    TextAlignment = TextAlignment.Center
                                };
                                tileContent.Children.Add(locationText);

                                Button betButton = new Button
                                {
                                    Content = "Bet",
                                    Background = Brushes.Green,
                                    Foreground = Brushes.White,
                                    Margin = new Thickness(5),
                                    Padding = new Thickness(5),
                                    Tag = new { EventID = eventID, Odds = odds }
                                };
                                betButton.Click += BetButton_Click;
                                tileContent.Children.Add(betButton);

                                eventTile.Child = tileContent;

                                EventTilesPanel.Children.Add(eventTile);
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

        private void BetButton_Click(object sender, RoutedEventArgs e)
        {
            Button betButton = sender as Button;
            var betInfo = (dynamic)betButton.Tag;
            int eventID = betInfo.EventID;
            double odds = betInfo.Odds;

            ErrorMessage.Visibility = Visibility.Collapsed;
            AmountTextBox.Tag = new { EventID = eventID, Odds = odds };
            BettingSection.Visibility = Visibility.Visible;

            Border parentTile = FindParent<Border>(betButton);
            if (parentTile != null)
            {
                if (selectedEventTile != null)
                {
                    selectedEventTile.Background = new SolidColorBrush(Colors.DarkSlateGray);
                }
                parentTile.Background = new SolidColorBrush(Color.FromRgb(20, 60, 50));
                selectedEventTile = parentTile;
            }
        }

        private void AmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AmountTextBox.Text == string.Empty)
            {
                WinningsTextBlock.Text = "0";
                return;
            }

            if (double.TryParse(AmountTextBox.Text, out double amount))
            {
                var betInfo = (dynamic)AmountTextBox.Tag;
                if (betInfo == null)
                {
                    WinningsTextBlock.Text = "Invalid bet selection";
                    return;
                }
                double odds = betInfo.Odds;
                double potentialWinnings = amount * odds;
                WinningsTextBlock.Text = $"${potentialWinnings:F2}";
            }
            else
            {
                WinningsTextBlock.Text = "Invalid amount";
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            return parent != null ? parent : FindParent<T>(parentObject);
        }

        private void PlaceBet_Click(object sender, RoutedEventArgs e)
        {
            var betInfo = (dynamic)AmountTextBox.Tag;
            if (betInfo == null)
            {
                ErrorMessage.Text = "Please select an event to bet on.";
                ErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            int eventID = betInfo.EventID;
            double odds = betInfo.Odds;

            if (double.TryParse(AmountTextBox.Text, out double betAmount))
            {
                if (betAmount <= playerBalance)
                {
                    playerBalance -= betAmount;
                    UpdatePlayerBalanceInDatabase(bettorID, playerBalance);
                    tbBalance.Text = $"Balance: ${playerBalance}";
                    PlaceBetInDatabase(eventID, odds, betAmount);
                    AmountTextBox.Clear();
                    WinningsTextBlock.Text = "0";
                    AmountTextBox.Tag = null;
                    BettingSection.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ErrorMessage.Text = "Insufficient balance to place this bet.";
                    ErrorMessage.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ErrorMessage.Text = "Invalid amount.";
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private void UpdatePlayerBalanceInDatabase(int bettorID, double newBalance)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE bettors SET Balance = @NewBalance WHERE BettorsID = @BettorID";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NewBalance", newBalance);
                        cmd.Parameters.AddWithValue("@BettorID", bettorID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating balance: {ex.Message}");
            }
        }

        private void PlaceBetInDatabase(int eventID, double odds, double amount)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO bets (BettorID, EventID, Odds, Amount) VALUES (@BettorID, @EventID, @Odds, @Amount)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BettorID", bettorID);
                        cmd.Parameters.AddWithValue("@EventID", eventID);
                        cmd.Parameters.AddWithValue("@Odds", odds);
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error placing bet: {ex.Message}");
            }
        }

        private void CancelBet_Click(object sender, RoutedEventArgs e)
        {
            BettingSection.Visibility = Visibility.Collapsed;
            AmountTextBox.Clear();
            WinningsTextBlock.Text = "0";
            AmountTextBox.Tag = null;

            if (selectedEventTile != null)
            {
                selectedEventTile.Background = new SolidColorBrush(Colors.DarkSlateGray);
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

        private void btnAdmin_Click(object sender, RoutedEventArgs e)
        {
            AdminPanel admin = new AdminPanel();
            admin.Show();
            this.Close();
        }
    }
}