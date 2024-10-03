using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.IO;

namespace CsokiBet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=localhost;Database=csokibet;User ID=root;Password=;";
        private string userFilePath = "user_data.txt"; // Fájl elérési útvonala a mentett adatokhoz
        public MainWindow()
        {
            InitializeComponent();
            AutoLogin();
        }
        private void AutoLogin()
        {
            if (File.Exists(userFilePath))
            {
                string[] userData = File.ReadAllLines(userFilePath);

                if (userData.Length == 2)
                {
                    string savedUsername = userData[0];
                    string savedPasswordHash = userData[1];

                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        string query = "SELECT Password FROM Bettors WHERE Username = @username";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@username", savedUsername);
                            connection.Open();

                            try
                            {
                                object result = command.ExecuteScalar();

                                if (result != null)
                                {
                                    string storedHash = result.ToString();
                                    if (storedHash == savedPasswordHash)
                                    {
                                        // Sikeres automatikus bejelentkezés

                                        MessageBox.Show("Siekres bejelentkezés (automatikusan)");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Hiba történt az automatikus bejelentkezés során: " + ex.Message);
                            }
                        }
                    }
                }
            }
        }

        private void btnRegisztralj_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ez egy gomb ");
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = ""/*UsernameTextBox.Text""*/;
            string password = ""/*PasswordBox.Password""*/;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Felhasználónév és jelszó kitöltése kötelező!");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "SELECT Password FROM Bettors WHERE Username = @username";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    connection.Open();

                    try
                    {
                        object result = command.ExecuteScalar();

                        if (result != null)
                        {
                            string storedHash = result.ToString();
                            string enteredHash = ComputeSha256Hash(password);

                            if (storedHash == enteredHash)
                            {
                                MessageBox.Show("Sikeres bejelentkezés!");

                                // Felhasználói adatok mentése szöveges fájlba
                                SaveUserData(username, enteredHash);

                                
                            }
                            else
                            {
                                MessageBox.Show("Hibás jelszó!");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Nincs ilyen felhasználónév!");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hiba történt a bejelentkezés során: " + ex.Message);
                    }
                }
            }
        }
        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        private void SaveUserData(string username, string passwordHash)
        {
            File.WriteAllLines(userFilePath, new string[] { username, passwordHash });
        }




        private void txtUserEntry_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void txtPasscode_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Az új jelszó beírása esetén automatikusan eltünteti a szöveget
            var passwordBox = sender as PasswordBox;

            if (passwordBox != null && !string.IsNullOrEmpty(passwordBox.Password))
            {
                tbpasscode.Visibility = Visibility.Hidden;
            }
            else
            {
                tbpasscode.Visibility = Visibility.Visible;
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