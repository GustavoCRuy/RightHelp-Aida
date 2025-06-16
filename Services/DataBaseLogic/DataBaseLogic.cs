using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using RightHelp___Aida.Services.Constants;
using System.Windows.Forms;

namespace RightHelp___Aida.Services.DataBaseLogic
{
    internal class DataBaseLogic
    {
        private static string connectionString = "server=localhost;database=Righthelp;uid=root;pwd=;";

        public static async Task SalvarMensagemAsync(MessageObject mensagem)
        {
            try
            {
                using var conn = new MySqlConnection(Const.connectionString);
                await conn.OpenAsync();
                var cmd = new MySqlCommand(
                    @"INSERT INTO righthelp.chat_history (session_id, user_id, role, message, timestamp)
                    VALUES (@sessionId, @userId, @role, @message, @timestamp)", conn);
                cmd.Parameters.AddWithValue("@sessionId", mensagem.SessionId);
                cmd.Parameters.AddWithValue("@userId", mensagem.UserId);
                cmd.Parameters.AddWithValue("@role", mensagem.Role);
                cmd.Parameters.AddWithValue("@message", mensagem.Message);
                cmd.Parameters.AddWithValue("@timestamp", mensagem.Timestamp);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar mensagem: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static async Task<List<MessageObject>> BuscarHistoricoAsync(string sessionId)
        {
            var listMenssages = new List<MessageObject>();
            try
            {
                using var conn = new MySqlConnection(Const.connectionString);
                await conn.OpenAsync();
                var cmd = new MySqlCommand(
                    @"SELECT id, session_id, user_id, role, message, timestamp
                    FROM righthelp.chat_history
                    WHERE session_id = @sessionId
                    ORDER BY timestamp ASC", conn);
                cmd.Parameters.AddWithValue("@sessionId", sessionId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var msg = new MessageObject
                    {
                        Id = reader.GetInt32("id"),
                        SessionId = reader.GetString("session_id"),
                        UserId = reader.GetString("user_id"),
                        Role = reader.GetString("role"),
                        Message = reader.GetString("message"),
                        Timestamp = reader.GetDateTime("timestamp")
                    };
                    listMenssages.Add(msg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao buscar histórico: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return listMenssages;
        }
    }
}
