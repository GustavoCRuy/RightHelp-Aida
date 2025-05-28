using MySqlConnector;
using System;
using System.Threading.Tasks;

namespace RightHelp___Aida.Services
{
    internal class Login
    {
        public string Username { get; set; }
        public string Password { get; set; } // Idealmente, armazene como hash
        public string Email { get; set; }
        public string FirstName { get; set; }

        private readonly string connectionString = "server=localhost;user=root;password=;database=righthelp";

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
