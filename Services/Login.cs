using MySqlConnector;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RightHelp___Aida.Services.Constants;
using System.Collections.Generic;
using System.Windows;

namespace RightHelp___Aida.Services
{
    internal class User
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }

        public static int UserSessionId;

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

            errorMessage = "";
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
                using var connection = new MySqlConnection(Constants.Const.connectionString);
                await connection.OpenAsync();

                var query = "INSERT INTO usuarios (username, password, email, first_name) VALUES (@username, @password, @email, @firstName)";
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
                using var connection = new MySqlConnection(Constants.Const.connectionString);
                await connection.OpenAsync();

                // Primeira query: autenticação
                var queryAuth = "SELECT COUNT(*) FROM usuarios WHERE username = @username AND password = @password";
                using var cmdAuth = new MySqlCommand(queryAuth, connection);
                cmdAuth.Parameters.AddWithValue("@username", Username);
                cmdAuth.Parameters.AddWithValue("@password", HashUtils.ComputeSha256Hash(Password));

                var result = Convert.ToInt32(await cmdAuth.ExecuteScalarAsync());
                if (result != 0)
                {
                    // Segunda query: busca do usuario_id
                    var queryId = "SELECT id FROM usuarios WHERE username = @username";
                    using var cmdId = new MySqlCommand(queryId, connection);
                    cmdId.Parameters.AddWithValue("@username", Username);

                    var idResult = await cmdId.ExecuteScalarAsync();
                    if (idResult != null && int.TryParse(idResult.ToString(), out int userId))
                    {
                        UserSessionId = userId;
                    }
                    return true;
                }
                else
                {
                    UserSessionId = 0;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao autenticar: " + ex.Message);
                UserSessionId = 0;
                return false;
            }
        }
    }
}
