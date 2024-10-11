using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Firebase.Auth;

namespace CsokiBet
{
    public partial class AdminPanel : Window
    {
        private const string connectionString = "Server=localhost;Database=csokibet;User ID=root;Password=;";

        public AdminPanel()
        {
            InitializeComponent();
            LoadBettors();
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

                // Firebase státusz ellenőrzés
                CheckFirebaseStatus(selectedRow["Email"].ToString());
            }
        }




        private void CheckFirebaseStatus(string email)
        {
            // Implement Firebase API call to check if the email is registered
            // Set FirebaseStatusTextBlock based on the result
            // Example:
            bool isRegistered = CheckEmailInFirebase(email); // Replace with actual check
            FirebaseStatusTextBlock.Text = isRegistered ? "Szinkronizált" : "Nincs szinkronizálva";
        }

        private bool CheckEmailInFirebase(string email)
        {
            // TODO: Implement the logic to check if the email exists in Firebase
            return false; // Placeholder
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

                // Firebase konfiguráció
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
