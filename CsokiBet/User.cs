using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsokiBet
{
    internal class User
    {
        //string query = "INSERT INTO bettors (Username, Password, Balance, Email, JoinDate, IsActive, Role) VALUES (@username, @password, @balance, @email, @joindate, @isactive @role)";
        private string username;
        private string password;
        private int balance;
        private string email;
        private string joindate;
        private int isactive;
        private string role;

        public User(string username, string password, int balance, string email, string joindate, int isactive, string role)
        {
            this.Username = username;
            this.Password = password;
            this.Balance = balance;
            this.Email = email;
            this.Joindate = joindate;
            this.Isactive = isactive;
            this.Role = role;
        }

        public string Username { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public int Balance { get => balance; set => balance = value; }
        public string Email { get => email; set => email = value; }
        public string Joindate { get => joindate; set => joindate = value; }
        public int Isactive { get => isactive; set => isactive = value; }
        public string Role { get => role; set => role = value; }
    }
}
