﻿using System.Text;
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

        private bool isRegistering = false; // Flag to toggle between login and registration
        public MainWindow()
        {
            InitializeComponent();
            AutoLogin();
        }
        private void btnRegisztralj_Click(object sender, RoutedEventArgs e)
        {
            isRegistering = !isRegistering;

            if (isRegistering)
            {
                // Show registration fields
                gridEmail.Visibility = Visibility.Visible;
                gridConfirmPass.Visibility = Visibility.Visible;
                tbregisztralj.Text = "JELENTKEZZ BE";
                btnLogin.Content = "Regisztráció";
                tbvanfiok.Text = "Van fiókod?";

                txtEmail.Clear();
                txtPasscode.Clear();
                txtConfirmPasscode.Clear();
                txtUsername.Clear();
                cbAutoLogin.Visibility = Visibility.Hidden;
            }
            else
            {
                // Hide registration fields
                gridEmail.Visibility = Visibility.Collapsed;
                gridConfirmPass.Visibility = Visibility.Collapsed;
                tbregisztralj.Text = "REGISZTRÁLJ";
                btnLogin.Content = "Bejelentkezés";
                tbvanfiok.Text = "Nincs fiókod?";

                txtEmail.Clear();
                txtPasscode.Clear();
                txtConfirmPasscode.Clear();
                txtUsername.Clear();
                cbAutoLogin.Visibility = Visibility.Visible;
            }
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
                                        BetMenu betmenu = new BetMenu();
                                        betmenu.Show();
                                        this.Close();
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

 

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email=txtEmail.Text;
            string password=txtPasscode.Password;
            string passwordagain=txtConfirmPasscode.Password;
            string username=txtUsername.Text;

            if (isRegistering)
            {

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordagain))
                {
                    MessageBox.Show("Minden mezőt ki kell tölteni!");
                    return;
                }

                if (password != passwordagain)
                {
                    MessageBox.Show("A jelszavak nem egyeznek!");
                    return;
                }

                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Az e-mail formátuma nem érvényes!");
                    return;
                }

                if (IsUsernameOrEmailTaken(username, email))
                {
                    MessageBox.Show("A felhasználónév vagy az e-mail cím már foglalt!");
                    return;
                }
                //string query = "INSERT INTO bettors (Username, Password, Balance, Email, JoinDate, IsActive, Role) VALUES (@username, @password, @balance, @email, @joindate, @isactive @role)";
                var user = new User(username, password, 0, email, "", 1, "Bettor");
                AddUserToDatabase(user);
                MessageBox.Show("Regisztráció sikeres!");
                txtEmail.Clear();
                txtPasscode.Clear();
                txtConfirmPasscode.Clear();
                txtUsername.Clear();
            }
            else
            {
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

                                    // Felhasználói adatok mentése szöveges fájlba
                                    if (cbAutoLogin.IsChecked==true)
                                    {
                                        SaveUserData(username, enteredHash);
                                    }
                                   
                                    BetMenu betmenu= new BetMenu();
                                    betmenu.Show();
                                    this.Close();

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

            
        }
        private bool IsValidEmail(string email)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        private bool IsUsernameOrEmailTaken(string username, string email)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM bettors WHERE Username = @Username OR Email = @Email";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Email", email);

                    long userCount = (long)command.ExecuteScalar();
                    return userCount > 0;
                }
            }
        }

        private void AddUserToDatabase(User user)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "INSERT INTO bettors (Username, Password, Balance, Email, JoinDate, IsActive, Role) VALUES (@username, @password, @balance, @email, @joindate, @isactive, @role)";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@password", ComputeSha256Hash(user.Password));
                    command.Parameters.AddWithValue("@balance", 0);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@joindate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@isactive", 1);
                    command.Parameters.AddWithValue("@role", user.Role);
                    

                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                        MessageBox.Show("Új felhasználó sikeresen hozzáadva!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hiba történt: " + ex.Message);
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
                if (passwordBox.Name==txtConfirmPasscode.Name)
                {
                    tbconfirmpasscode.Visibility=Visibility.Hidden;
                }
                else
                {
                    tbpasscode.Visibility = Visibility.Hidden;
                }
               
            }
            else
            {
                if (passwordBox.Name == txtConfirmPasscode.Name)
                {
                    tbconfirmpasscode.Visibility = Visibility.Visible;
                }
                else
                {
                    tbpasscode.Visibility = Visibility.Visible;
                }
                
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