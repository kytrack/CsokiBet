using Firebase.Auth;
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
        private string connectionString = "Server=localhost;Database=csokibet;User ID=root;Password=;";
        private string userFilePath = "user_data.txt";
        private Random _random = new Random();
        private double playerBalance;
        private int bettorID;
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
                    string query = "SELECT BettorsID, Balance FROM bettors WHERE Email = @Email";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bettorID = reader.GetInt32("BettorsID");
                                playerBalance = reader.GetDouble("Balance");
                                tbBalance.Text = $"${playerBalance}";
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
                            tbBalance.Text = $"${balance}";
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
                                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#102923")),
                                    CornerRadius = new CornerRadius(10),
                                    Width = 200,
                                    Height = 150,
                                    Margin = new Thickness(10)
                                };

                                // A DockPanel használata a StackPanel helyett
                                DockPanel tileContent = new DockPanel();

                                // Category text (mindig a tetején marad a DockPanel Dock-ing segítségével)
                                TextBlock categoryText = new TextBlock
                                {
                                    Text = $"{category}",
                                    Foreground = Brushes.White,
                                    FontWeight = FontWeights.Bold,
                                    FontSize = 14,
                                    TextAlignment = TextAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Top,
                                    Margin = new Thickness(0, 10, 0, 0)
                                };
                                DockPanel.SetDock(categoryText, Dock.Top); // Fent tartjuk a DockPanelben
                                tileContent.Children.Add(categoryText);

                                // StackPanel a tartalom többi részének
                                StackPanel eventInfoPanel = new StackPanel
                                {
                                    Orientation = Orientation.Vertical,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center
                                };

                                // Event name
                                TextBlock eventText = new TextBlock
                                {
                                    Text = $"{eventName}",
                                    Foreground = Brushes.White,
                                    FontWeight = FontWeights.Bold,
                                    FontSize = 14,
                                    TextAlignment = TextAlignment.Center,
                                    TextWrapping = TextWrapping.Wrap,
                                    MaxWidth = 180 // Maximális szélesség a kártyához igazítva
                                };
                                eventInfoPanel.Children.Add(eventText);

                                // Odds text
                                TextBlock oddsText = new TextBlock
                                {
                                    Text = $"Odds: {odds:F2}",
                                    Foreground = Brushes.LightGreen,
                                    FontWeight = FontWeights.Bold,
                                    FontSize = 12,
                                    TextAlignment = TextAlignment.Center
                                };
                                eventInfoPanel.Children.Add(oddsText);

                                // Location text
                                TextBlock locationText = new TextBlock
                                {
                                    Text = location,
                                    Foreground = Brushes.LightGray,
                                    FontSize = 12,
                                    TextAlignment = TextAlignment.Center
                                };
                                eventInfoPanel.Children.Add(locationText);

                                // Bet button
                                Button betButton = new Button
                                {
                                    Content = "Bet",
                                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C3934")),
                                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1C3934")),
                                    Foreground = Brushes.White,
                                    Margin = new Thickness(5),
                                    Padding = new Thickness(5),
                                    Tag = new { EventID = eventID, Odds = odds },
                                    Style = (Style)FindResource("TransparentButtonStyle")
                                };
                                betButton.Click += BetButton_Click;
                                eventInfoPanel.Children.Add(betButton);

                                // Az eventInfoPanel a DockPanel maradék helyén helyezkedik el
                                tileContent.Children.Add(eventInfoPanel);

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
                    selectedEventTile.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#102923"));
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
                    WinningsTextBlock.Text = "Válassz eseményt";
                    return;
                }
                double odds = betInfo.Odds;
                double potentialWinnings = amount * odds;
                WinningsTextBlock.Text = $"${potentialWinnings:F2}";
            }
            else
            {
                WinningsTextBlock.Text = "Nem megfelelő összeg";
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
                ErrorMessage.Text = "Válassz eseményt";
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
                    tbBalance.Text = $"${playerBalance}";
                    PlaceBetInDatabase(eventID, odds, betAmount);
                    AmountTextBox.Clear();
                    WinningsTextBlock.Text = "0";
                    AmountTextBox.Tag = null;
                    BettingSection.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ErrorMessage.Text = "Nincs elegendő egyenleged";
                    ErrorMessage.Visibility = Visibility.Visible;
                }
            }
            else
            {
                ErrorMessage.Text = "Nem megfelelő érték";
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
                    string query = "INSERT INTO bets (BettorsID, EventID, Odds, Amount) VALUES (@BettorsID, @EventID, @Odds, @Amount)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@BettorsID", bettorID);
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

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}