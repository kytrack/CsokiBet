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
using Firebase.Database;
using Firebase.Database.Query;
using System.Net.Mail;
using System.Net;
using System.Net;
using System.Net.Mail;
using System.Net.Http;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;

namespace CsokiBet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=localhost;Database=csokibet;User ID=root;Password=;";
        private string userFilePath = "user_data.txt";
        private FirebaseClient firebaseClient;
        private static readonly string firebaseApiKey = "AIzaSyBXxbFR3nwUFni-dBOB4dg7i3C-Z0SNgcw";
        string windowEmail = "";
        string windowUsername = "";

        private bool isRegistering = false;
        public MainWindow()
        {
            InitializeComponent();
            //TARTSD A GITIGNORE-BAN!!!
            //TARTSD A GITIGNORE-BAN!!!
            //TARTSD A GITIGNORE-BAN!!!
            //TARTSD A GITIGNORE-BAN!!!
            string FIREBASE_JSON_KEY_PATH = "../../../firebase.json"; 

            //az alábbi kódra azért van szükség mert a "FirebaseApp.Create" kezdetű kódot csak egyszer szabad lefuttatni a program indítása során, különben crashel
            string filePath = "user_data.txt";
            if (File.Exists(filePath))
            {
                string[] fileLines = File.ReadAllLines(filePath);
                if (fileLines.Length == 1 && fileLines[0] == "logout")
                {
                    File.WriteAllText(filePath, string.Empty);
                }
                else
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(FIREBASE_JSON_KEY_PATH)
                    });
                }
            }
            else
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(FIREBASE_JSON_KEY_PATH)
                });

            }
            firebaseClient = new FirebaseClient("https://csokibetwpf-default-rtdb.firebaseio.com/");
            AutoLogin();
        }



        private void btnRegisztralj_Click(object sender, RoutedEventArgs e)
        {
            isRegistering = !isRegistering;

            if (isRegistering)
            {
                gridEmail.Visibility = Visibility.Visible;
                gridConfirmPass.Visibility = Visibility.Visible;
                tbregisztralj.Text = "JELENTKEZZ BE";
                btnLogin.Content = "Regisztráció";
                tbvanfiok.Text = "Van fiókod?";
                emailfelhasznalonev.Text = "Felhasználónév";//nem szép megoldas de mar nincs kedvem tobbet kodolni :(

                txtEmail.Clear();
                txtPasscode.Clear();
                txtConfirmPasscode.Clear();
                txtUsername.Clear();
                cbAutoLogin.Visibility = Visibility.Hidden;
            }
            else
            {
                gridEmail.Visibility = Visibility.Collapsed;
                gridConfirmPass.Visibility = Visibility.Collapsed;
                tbregisztralj.Text = "REGISZTRÁLJ";
                btnLogin.Content = "Bejelentkezés";
                tbvanfiok.Text = "Nincs fiókod?";
                emailfelhasznalonev.Text = "Email cím";

                txtEmail.Clear();
                txtPasscode.Clear();
                txtConfirmPasscode.Clear();
                txtUsername.Clear();
                cbAutoLogin.Visibility = Visibility.Visible;
            }
        }
        private async void AutoLogin()
        {
            /*if (File.Exists(userFilePath))
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
            }*/


            if (File.Exists(userFilePath))
            {
                string[] userData = File.ReadAllLines(userFilePath);

                if (userData.Length == 3)
                {
                    string savedUsername = userData[0];
                    string savedEmail = userData[1];
                    string savedPassword = userData[2];
                    string autoLoginSwitch = userData[3];

                    if (autoLoginSwitch == "true")
                    {
                        try
                        {
                            var token = await SignInWithEmailPassword(savedEmail, savedPassword);

                            if (!string.IsNullOrEmpty(token))
                            {
                                var isActive = await IsUserActive(savedEmail);
                                if (isActive)
                                {
                                    BetMenu betMenu = new BetMenu(windowEmail, windowUsername);
                                    betMenu.Show();
                                    this.Close();
                                }
                                else
                                {
                                    MessageBox.Show("Hiba a hitelesítés során: email cím nincs visszaigazolva");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "Invalid login credentials.")
                            {
                                MessageBox.Show("Hibás felhasználónév vagy jelszó.");
                            }
                            else
                            {
                                MessageBox.Show($"Login failed: {ex.Message}");
                            }

                        }
                    }


                }
            }
        }


        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text;
            string password = txtPasscode.Password;
            string passwordagain = txtConfirmPasscode.Password;
            string username = txtUsername.Text;

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
                txtEmail.IsEnabled = false;
                txtPasscode.IsEnabled = false;
                txtConfirmPasscode.IsEnabled = false;
                txtUsername.IsEnabled = false;
            }
            else
            {
                //!!! ITT A username VÁLTOZÓ AZ EMAILNEK FELEL MEG
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Felhasználónév és jelszó kitöltése kötelező!");
                    return;
                }
                try
                {
                    var token = await SignInWithEmailPassword(username, password);
                    if (!string.IsNullOrEmpty(token))
                    {
                        var isActive = await IsUserActive(username);
                        if (isActive)
                        {
                            MessageBox.Show("Sikeres bejelentkezés");

                            string actualUsername = GetUsernameFromEmail(username);


                            if (cbAutoLogin.IsChecked == true)
                            {
                                SaveUserData(actualUsername, username, password, "true");
                            }
                            else
                            {
                                SaveUserData(actualUsername, username, password, "false");
                            }

                            BetMenu betMenu = new BetMenu(windowEmail, windowUsername);
                            betMenu.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Email cím nincs visszaigazolva");
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message == "Invalid login credentials.")
                    {
                        MessageBox.Show("Hibás felhasználónév vagy jelszó.");
                    }
                    else
                    {
                        MessageBox.Show($"Login failed: {ex.Message}");
                    }
                }






                //MySql bejelentkezés (áttértünk csak Firebase-re
                /*
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
                }*/
            }


        }
        private string GetUsernameFromEmail(string email)
        {
            string retrievedUsername = null;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Username FROM bettors WHERE Email = @Email";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Email", email);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            retrievedUsername = reader["Username"].ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error retrieving username: {ex.Message}");
                }
            }

            return retrievedUsername;
        }
        private async Task<string> SignInWithEmailPassword(string email, string password)
        {
            var client = new HttpClient();
            var requestUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={firebaseApiKey}";

            var data = new
            {
                email = email,
                password = password,
                returnSecureToken = true
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(requestUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Invalid login credentials.");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic responseJson = JsonConvert.DeserializeObject(responseString);

            return responseJson.idToken;
        }
        private async Task<bool> IsUserActive(string email)
        {
            var userfb = await firebaseClient
                .Child("users")
                .Child(email.Replace(".", "_"))
                .OnceSingleAsync<UserFb>();

            return userfb?.isActive ?? false;
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
        private async Task AddUserToDatabase(User user)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                string query = "INSERT INTO bettors (Username, Password, Balance, Email, JoinDate, IsActive, Role) VALUES (@username, @password, @balance, @email, @joindate, @isactive, @role)";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", user.Username);
                    command.Parameters.AddWithValue("@password", ComputeSha256Hash(user.Password)); //ITT KELL AZ SHA
                    command.Parameters.AddWithValue("@balance", 0);
                    command.Parameters.AddWithValue("@email", user.Email);
                    command.Parameters.AddWithValue("@joindate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@isactive", 1);
                    command.Parameters.AddWithValue("@role", user.Role);


                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hiba történt (MySql): " + ex.Message);
                    }
                }






                try
                {
                    var userRecordArgs = new UserRecordArgs()
                    {
                        Email = user.Email,
                        EmailVerified = false,
                        Password = user.Password,
                        Disabled = false
                    };
                    var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(userRecordArgs);

                    await AddUserToFirebase(userRecord.Email);
                    try
                    {
                        string recipient = user.Email;
                        string htmlContent = $"<!DOCTYPE html>\r\n<html lang=\"hu\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>Casino Bonus</title>\r\n    <style>\r\n        body {{\r\n            font-family: Arial, sans-serif;\r\n            background-color: #f8f8f8;\r\n            color: #333;\r\n            padding: 0;\r\n            margin: 0;\r\n        }}\r\n        .container {{\r\n            width: 100%;\r\n            max-width: 600px;\r\n            margin: 0 auto;\r\n            background-color: #ffffff;\r\n            border-radius: 10px;\r\n            overflow: hidden;\r\n            box-shadow: 0 0 15px rgba(0, 0, 0, 0.2);\r\n        }}\r\n        .header {{\r\n            background-color: #4CAF50;\r\n            color: white;\r\n            padding: 0px;\r\n            text-align: center;\r\n        }}\r\n        .header h1 {{\r\n            margin: 0;\r\n            font-size: 24px;\r\n        }}\r\n        .content {{\r\n            padding: 20px;\r\n            text-align: center;\r\n        }}\r\n        .content p {{\r\n            font-size: 16px;\r\n            line-height: 1.6;\r\n        }}\r\n        .bonus {{\r\n            font-size: 28px;\r\n            color: #E74C3C;\r\n            font-weight: bold;\r\n            background-color: #fff3cd;\r\n            border: 2px solid #E74C3C;\r\n            border-radius: 5px;\r\n            padding: 10px;\r\n            margin: 20px 0;\r\n        }}\r\n        .casino-image {{\r\n            margin: 0;\r\n        }}\r\n        .casino-image img {{\r\n            width: 100%; /* Kaszinó kép megtartott mérete */\r\n            max-width: 560px;\r\n            border-radius: 10px;\r\n            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n        }}\r\n        .highlight {{\r\n            color: #E74C3C;\r\n            font-weight: bold;\r\n            font-size: 20px;\r\n            margin: 15px 0;\r\n        }}\r\n        .call-to-action {{\r\n            font-size: 20px;\r\n            color: #E74C3C;\r\n            font-weight: bold;\r\n            margin-top: 25px;\r\n            text-decoration: underline;\r\n        }}\r\n        .button {{\r\n            display: inline-block;\r\n            padding: 12px 24px;\r\n            font-size: 18px;\r\n            color: white;\r\n            background-color: #4CAF50;\r\n            border: none;\r\n            border-radius: 25px;\r\n            cursor: pointer;\r\n            text-decoration: none;\r\n            transition: background-color 0.3s ease;\r\n        }}\r\n        .button:hover {{\r\n            background-color: #45a049;\r\n        }}\r\n        .footer {{\r\n            margin-top: 30px;\r\n            background-color: #4CAF50;\r\n            color: white;\r\n            padding: 10px;\r\n            text-align: center;\r\n        }}\r\n        .footer p {{\r\n            margin: 0;\r\n            font-size: 12px;\r\n        }}\r\n        .money {{\r\n            color: #45a049;\r\n            font-size: 20px;\r\n            font-weight: bolder;\r\n        }}\r\n        .zoom-container img {{\r\n            width: 160px; /* Csökkentett méret a logónak */\r\n            height: auto; /* Automatikus magasság fenntartása */\r\n        }}\r\n    </style>\r\n</head>\r\n<body>\r\n    <div class=\"container\">\r\n        <div class=\"header\">\r\n            <div class=\"zoom-container\">\r\n                <img src=\"https://i.imgur.com/4ooGYni.png\" alt=\"csokibetlogo\">\r\n            </div>\r\n        </div>\r\n        <div class=\"content\">\r\n            <h1>Kedves {user.Username}!</h1>\r\n            <p class=\"bonus\">🎉 Örömmel értesítünk, hogy a CsokiBet fiókod sikeresen elkészült!</p>\r\n            <p>Már csak egy dolog maradt hátra, hogy felfedezhesd kedvenc játékaidat.</p>\r\n            <p>Kattints az alábbi gombra, amellyel visszaigazolod az <strong>email címedet</strong> és jóváírjuk az új felhasználóknak járó <span class=\"money\">💸$20💸</span> -os bónuszt!</p>\r\n            <div class=\"casino-image\">\r\n                <img src=\"https://i.imgur.com/elhWBbb.jpeg\" alt=\"Kaszinó Kép\">\r\n            </div>\r\n            <a href=\"https://ddaniel-bit.github.io/csokiBetEmailVerification/?email={recipient}\" class=\"button\">✨ Email cím igazolása 🎊</a>\r\n        </div>\r\n        <div class=\"footer\">\r\n            <p>&copy; 2024 Csokibet. Minden jog fenntartva.</p>\r\n        </div>\r\n    </div>\r\n</body>\r\n</html>\r\n";

                        var fromAddress = new MailAddress("csokibetugyfelszolgalat@gmail.com", "CsokiBet");
                        var toAddress = new MailAddress(recipient);
                        const string fromPassword = "gqvi cijd szem bosz";
                        const string subject = "CsokiBet | Email cím visszaigazolása";

                        var smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                            Timeout = 20000
                        };

                        using (var message = new MailMessage(fromAddress, toAddress)
                        {
                            Subject = subject,
                            Body = htmlContent,
                            IsBodyHtml = true
                        })
                        {
                            smtp.Send(message);
                        }
                        txtEmail.IsEnabled = true;
                        txtPasscode.IsEnabled = true;
                        txtConfirmPasscode.IsEnabled = true;
                        txtUsername.IsEnabled = true;

                        txtEmail.Clear();
                        txtPasscode.Clear();
                        txtConfirmPasscode.Clear();
                        txtUsername.Clear();
                        MessageBox.Show("A regisztáció sikeres, a visszaigazoló levelet elküldtük az email címedre!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to send email: " + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Registration failed: {ex.Message}");
                }





            }
        }
        private async Task AddUserToFirebase(string email)
        {
            await firebaseClient
                .Child("users")
                .Child(email.Replace(".", "_"))
                .PutAsync(new
                {
                    email = email,
                    isActive = false
                });
        }
        private void AddUserToDatabaseOnlyMySql(User user)
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
        private void SaveUserData(string username, string email, string passwordHash, string autoLoginSwitch)
        {
            File.WriteAllLines(userFilePath, new string[] { username, email, passwordHash, autoLoginSwitch });
            windowEmail = email;
            windowUsername = username;
        }




        private void txtUserEntry_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void txtPasscode_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;

            if (passwordBox != null && !string.IsNullOrEmpty(passwordBox.Password))
            {
                if (passwordBox.Name == txtConfirmPasscode.Name)
                {
                    tbconfirmpasscode.Visibility = Visibility.Hidden;
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
    public class UserFb
    {
        public string email { get; set; }
        public bool isActive { get; set; }
    }
}
