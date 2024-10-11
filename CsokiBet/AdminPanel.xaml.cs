using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using FirebaseAdmin;
using FirebaseAdmin.Auth; // FirebaseAdmin Auth SDK
using Google.Apis.Auth.OAuth2;
using Firebase.Auth; // Firebase.Auth SDK (REST API)
using System.Windows.Media;

namespace CsokiBet
{
    public partial class AdminPanel : Window
    {
        private const string connectionString = "Server=localhost;Database=csokibet;User ID=root;Password=;";

        public AdminPanel()
        {
            InitializeComponent();
            InitializeFirebase();  // Firebase inicializálása
            LoadBettors();  // Bettor-ok betöltése a DataGrid-be
        }

        private void InitializeFirebase()
        {
            // Ellenőrizzük, hogy már létezik-e az alapértelmezett FirebaseApp
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("../../../firebase.json")
                });
            }
        }


        private async void CheckFirebaseStatus(string email)
        {
            FirebaseStatusTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            FirebaseStatusTextBlock.Text = "Betöltés...";
            try
            {
                // FirebaseAdmin felhasználó lekérdezése az email cím alapján
                FirebaseAdmin.Auth.UserRecord userRecord = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);

                if (userRecord != null)
                {
                    FirebaseStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);
                    FirebaseStatusTextBlock.Text = "Szinkronizált";
                }
                else
                {
                    FirebaseStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    FirebaseStatusTextBlock.Text = "Nincs szinkronizálva (csak MySql)";
                }
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex)
            {
                // Ha nincs ilyen email a Firebase-ben
                if (ex.AuthErrorCode == FirebaseAdmin.Auth.AuthErrorCode.UserNotFound)
                {
                    FirebaseStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                    FirebaseStatusTextBlock.Text = "Nincs szinkronizálva (csak MySql)";
                }
                else
                {
                    MessageBox.Show($"Hiba történt a Firebase ellenőrzésekor: {ex.Message}");
                }
            }
        }

        private void LoadBettors()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("SELECT * FROM bettors", connection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                BettorDataGrid.ItemsSource = dt.DefaultView;
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BettorDataGrid.SelectedItem is DataRowView selectedRow)
            {
                UsernameTextBox.Text = selectedRow["Username"].ToString();
                BalanceTextBox.Text = selectedRow["Balance"].ToString();

                // IsActive státusz beállítása
                int isActive = Convert.ToInt32(selectedRow["IsActive"]);
                IsActiveComboBox.SelectedItem = isActive == 1 ? IsActiveComboBox.Items[0] : IsActiveComboBox.Items[1];

                // Szerepkör beállítása ComboBox-ban
                string role = selectedRow["Role"].ToString();
                foreach (ComboBoxItem item in RoleComboBox.Items)
                {
                    if (item.Tag.ToString() == role)
                    {
                        RoleComboBox.SelectedItem = item;
                        break;
                    }
                }

                EmailTextBlock.Text = selectedRow["Email"].ToString();

                // Firebase státusz ellenőrzése
                CheckFirebaseStatus(selectedRow["Email"].ToString());
            }
        }

        private void ModifyBettor_Click(object sender, RoutedEventArgs e)
        {
            if (BettorDataGrid.SelectedItem is DataRowView selectedRow)
            {
                int bettorsId = Convert.ToInt32(selectedRow["BettorsID"]);
                string username = UsernameTextBox.Text;
                decimal balance = Convert.ToDecimal(BalanceTextBox.Text);
                int isActive = IsActiveComboBox.SelectedItem is ComboBoxItem selectedItem
                    ? Convert.ToInt32(selectedItem.Tag)
                    : 0;

                // A kiválasztott szerepkör értéke a ComboBox-ból
                string role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand("UPDATE bettors SET Username = @Username, Balance = @Balance, IsActive = @IsActive, Role = @Role WHERE BettorsID = @BettorsID", connection);
                    command.Parameters.AddWithValue("@BettorsID", bettorsId);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Balance", balance);
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    command.Parameters.AddWithValue("@Role", role);
                    command.ExecuteNonQuery();
                }

                LoadBettors(); // Frissítjük az adatokat a DataGrid-ben
            }
        }

        private async void SendPasswordResetEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string email = EmailTextBlock.Text;

                // Firebase konfiguráció a Firebase REST API használatával
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyBXxbFR3nwUFni-dBOB4dg7i3C-Z0SNgcw"));

                // Jelszó-visszaállító e-mail küldése
                await authProvider.SendPasswordResetEmailAsync(email);

                MessageBox.Show("Jelszó visszaállító e-mail elküldve erre az e-mail címre: " + email);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a jelszó visszaállító e-mail küldésekor: {ex.Message}");
            }
        }
    }
}
