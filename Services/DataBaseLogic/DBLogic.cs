using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// Detecção permissiva de SQL Injection:
        /// Permite perguntas sobre comandos SQL, bloqueia apenas padrões clássicos de ataque.
        /// </summary>
        private bool DetectaSQLInjection(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return false;

            string[] padroesPerigosos = new[]
            {
                @"(['\""]\s*;?\s*(or|and)\s+\d+\s*=\s*\d+\s*(--|#|/\*)?)",
                @"(;+\s*(drop|delete|insert|update|alter|truncate|exec)\b)",
                @"(--|#|/\*)",
                @"(['\""];+\s*drop\s+table)",
                @"(xp_cmdshell)",
                @"(\bwaitfor\b|\bsleep\b)",
                @"(\bbenchmark\b|\bload_file\b|\boutfile\b)",
            };

            foreach (var padrao in padroesPerigosos)
            {
                if (Regex.IsMatch(texto, padrao, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                    return true;
            }

            // Permite perguntas e respostas com palavras reservadas (select, drop, etc) fora desses padrões
            return false;
        }

        public bool InserirConversa(int usuarioId, string pergunta, string resposta)
        {
            if (DetectaSQLInjection(pergunta) || DetectaSQLInjection(resposta))
            {
                return false;
            }

            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    "INSERT INTO Conversas (UsuarioId, Pergunta, Resposta, Timestamp) VALUES (@usuarioId, @pergunta, @resposta, @timestamp)", conn))
                {
                    cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                    cmd.Parameters.AddWithValue("@pergunta", pergunta);
                    cmd.Parameters.AddWithValue("@resposta", resposta);
                    cmd.Parameters.AddWithValue("@timestamp", DateTime.Now);

                    int rows = cmd.ExecuteNonQuery();
                    return rows > 0;
                }
            }
        }

        // Método para buscar o histórico de conversa de um usuário
        public List<(string Pergunta, string Resposta)> BuscarHistorico(int usuarioId, int limite = 20)
        {
            var historico = new List<(string, string)>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    "SELECT Pergunta, Resposta FROM Conversas WHERE UsuarioId = @usuarioId ORDER BY Timestamp DESC LIMIT @limite", conn))
                {
                    cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
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
            return historico;
        }

        // Método para montar contexto a partir do histórico (para API)
        public string MontarContexto(int usuarioId, string systemPrompt, int limite = 20)
        {
            var historico = BuscarHistorico(usuarioId, limite);
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