using MySqlConnector;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RightHelp___Aida.Services
{
    internal class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }

        private readonly string connectionString = "server=localhost;user=root;password=;database=righthelp";
        public bool IsValid(out string errorMessage)
        {
            Username = Username?.Trim();
            Password = Password?.Trim();
            Email = Email?.Trim();
            FirstName = FirstName?.Trim();

            if (string.IsNullOrWhiteSpace(Username) || Username.Length < 3)
            {
                errorMessage = "O nome de usuário deve ter pelo menos 3 caracteres.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 6)
            {
                errorMessage = "A senha deve ter pelo menos 6 caracteres.";
                return false;
            }

            if (!Regex.IsMatch(Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$"))
            {
                errorMessage = "A senha deve conter letras maiúsculas, minúsculas e números.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email) ||
                !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                errorMessage = "E-mail inválido.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstName) || FirstName.Any(char.IsDigit))
            {
                errorMessage = "O nome não deve conter números.";
                return false;
            }

            errorMessage = null;
            return true;
        }


        /// <summary>
        /// Registra um novo usuário no banco de dados.
        /// @return true se o registro for bem-sucedido, false caso contrário.
        /// </summary>
        public async Task<bool> RegisterAsync()
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "INSERT INTO users (username, password, email, first_name) VALUES (@username, @password, @email, @firstName)";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@username", Username);
                cmd.Parameters.AddWithValue("@password", HashUtils.ComputeSha256Hash(Password));
                cmd.Parameters.AddWithValue("@email", Email);
                cmd.Parameters.AddWithValue("@firstName", FirstName);

                int result = await cmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao registrar usuário: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Verifica se o login é válido.
        /// @return true se as credenciais estiverem corretas, false caso contrário.
        /// </summary>
        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                var query = "SELECT COUNT(*) FROM users WHERE username = @username AND password = @password";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@username", Username);
                cmd.Parameters.AddWithValue("@password", HashUtils.ComputeSha256Hash(Password));

                var result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao autenticar: " + ex.Message);
                return false;
            }
        }
    }
}
