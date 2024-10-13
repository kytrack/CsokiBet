using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace CsokiBet
{
    public partial class EgyenlegFeltoltes : Window
    {
        public EgyenlegFeltoltes()
        {
            InitializeComponent();
        }

        private void FeltoltesButton_Click(object sender, RoutedEventArgs e)
        {
            // Ellenőrizd, hogy minden mező ki van-e töltve
            if (string.IsNullOrWhiteSpace(CardNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(ExpiryDateTextBox.Text) ||
                string.IsNullOrWhiteSpace(CvcTextBox.Text) ||
                string.IsNullOrWhiteSpace(AmountTextBox.Text)) // Az AmountTextBox az összeg beírására
            {
                MessageBox.Show("Kérjük, töltsd ki az összes mezőt.");
                return;
            }

            // Összeg beolvasása és validálása
            if (!decimal.TryParse(AmountTextBox.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Kérjük, adj meg egy érvényes összeget.");
                return;
            }

            // Kapcsolódás az adatbázishoz
            string connectionString = "Server=localhost;Database=csokibet;User ID=root;Password=;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // A felhasználónév alapján frissítjük a Balance mezőt
                    string username = "vakond"; // Cseréld ki a tényleges felhasználónévre
                    string query = "UPDATE bettors SET Balance = Balance + @Amount WHERE Username = @Username";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@Username", username);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Sikeresen feltöltötted az egyenlegedet!");
                        }
                        else
                        {
                            MessageBox.Show("A felhasználó nem található.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hiba történt: {ex.Message}");
                }
            }
        }
    }
}
