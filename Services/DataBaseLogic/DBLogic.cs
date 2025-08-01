using System.Text.RegularExpressions;
using System.Windows;
using MySqlConnector;
using RightHelp___Aida.Services.Constants;

namespace RightHelp___Aida.Services.DataBaseLogic
{
    internal class DBLogic
    {
        private readonly string _connectionString;

        public DBLogic()
        {
            _connectionString = Const.connectionString;
        }

        public bool InserirConversa(int usuario_id, string pergunta, string resposta)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    "INSERT INTO historico_conversa (usuario_id, Pergunta, Resposta, Timestamp) VALUES (@usuario_id, @pergunta, @resposta, @timestamp)", conn))
                {
                    cmd.Parameters.AddWithValue("@usuario_id", usuario_id);
                    cmd.Parameters.AddWithValue("@pergunta", pergunta);
                    cmd.Parameters.AddWithValue("@resposta", resposta);
                    cmd.Parameters.AddWithValue("@timestamp", DateTime.Now);

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }

        // Método para buscar o histórico de conversa de um usuário
        public List<(string Pergunta, string Resposta)> BuscarHistorico(int usuario_id, int limite = 20)
        {
            var historico = new List<(string, string)>();
            try
            {
                using (var conn = new MySqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(
                        "SELECT Pergunta, Resposta FROM historico_conversa WHERE usuario_id = @usuario_id ORDER BY Timestamp DESC LIMIT @limite", conn))
                    {
                        cmd.Parameters.AddWithValue("@usuario_id", usuario_id);
                        cmd.Parameters.AddWithValue("@limite", limite);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string pergunta = reader.GetString("Pergunta");
                                string resposta = reader.GetString("Resposta");
                                historico.Add((pergunta, resposta));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log do erro (pode ser substituído por log em arquivo, banco, etc.)
                MessageBox.Show($"Erro ao buscar histórico: {ex.Message}");
            }
            return historico;
        }

        // Método para montar contexto a partir do histórico (para API)
        public string MontarContexto(int usuario_id, string systemPrompt, int limite = 20)
        {
            var historico = BuscarHistorico(usuario_id, limite);
            var contexto = new List<Dictionary<string, string>>();

            contexto.Add(new Dictionary<string, string>
            {
                ["role"] = "system",
                ["content"] = systemPrompt
            });

            foreach (var par in historico)
            {
                contexto.Add(new Dictionary<string, string>
                {
                    ["role"] = "user",
                    ["content"] = par.Pergunta
                });
                contexto.Add(new Dictionary<string, string>
                {
                    ["role"] = "assistant",
                    ["content"] = par.Resposta
                });
            }

            // Serializa para JSON Array (padrão OpenAI Chat API)
            return System.Text.Json.JsonSerializer.Serialize(contexto);
        }
    }
}