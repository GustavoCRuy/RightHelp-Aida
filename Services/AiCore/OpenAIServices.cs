using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenAI;
using OpenAI.Chat;
using RightHelp___Aida.Services.Constants;
using RightHelp___Aida.Services.DataBaseLogic;
using SharpToken;

namespace RightHelp___Aida.Services.AiCore
{
    internal class OpenAIClass
    {
        internal class ChatStream
        {
            private readonly string _modelName = "gpt-4o";
            private readonly ChatClient _client;

            public ChatStream(string model)
            {
                _modelName = model;
                try
                {
                    _client = new ChatClient(_modelName, Const.openAIKey);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao criar ChatClient: {ex.Message}");
                }


                if (_client == null)
                {
                    MessageBox.Show("ERROR: Failed to create OpenAI client.");
                    return;
                }
            }

            /// <summary>
            /// Processa a entrada do usuário com contexto e retorna as respostas por streaming.
            /// </summary>
            /// <param name="textInput">Entrada do usuário</param>
            /// <param name="context">Contexto anterior ou instruções</param>
            /// <param name="onUpdate">Callback chamado a cada atualização da IA</param>
            public async Task StreamResponseAsync(string textInput, string context, List<MessageObject> chatHistory, Action<string> onUpdate)
            {
                var encoding = GptEncoding.GetEncoding("cl100k_base");
                const int maxModelTokens = 16384;
                const int maxOutputTokens = 1000;
                const int maxInputTokens = maxModelTokens - maxOutputTokens;

                if (string.IsNullOrEmpty(textInput))
                {
                    MessageBox.Show("ERROR: A entrada do usuário não pode ser vazia.");
                    return;
                }

                try
                {
                    var messages = new List<ChatMessage>();
                    int totalTokens = 0;

                    // Mensagem de sistema
                    var systemMsg = ChatMessage.CreateSystemMessage(context);
                    messages.Add(systemMsg);
                    totalTokens += encoding.Encode(systemMsg.Content.ToString()).Count + 4;

                    // Prepara histórico limitado por tokens (adiciona de trás para frente)
                    var selectedHistory = new List<MessageObject>();
                    if (chatHistory != null && chatHistory.Count > 0)
                    {
                        // Adiciona do fim para o início (mensagem mais recente primeiro)
                        for (int i = chatHistory.Count - 1; i >= 0; i--)
                        {
                            var msg = chatHistory[i];
                            string msgText = msg.Message;
                            int msgTokens = encoding.Encode(msgText).Count + 4; // +4 para metadados role
                                                                                // Mensagem atual do usuário também será somada depois
                            if (totalTokens + msgTokens >= maxInputTokens)
                                break;
                            selectedHistory.Insert(0, msg); // Mantém ordem original
                            totalTokens += msgTokens;
                        }
                    }

                    // Adiciona histórico selecionado
                    foreach (var msg in selectedHistory)
                    {
                        if (msg.Role == "user")
                            messages.Add(ChatMessage.CreateUserMessage(msg.Message));
                        else if (msg.Role == "assistant")
                            messages.Add(ChatMessage.CreateAssistantMessage(msg.Message));
                    }

                    // Mensagem atual do usuário
                    var userMsg = ChatMessage.CreateUserMessage(textInput);
                    int userMsgTokens = encoding.Encode(userMsg.Content.ToString()).Count + 4;
                    if (totalTokens + userMsgTokens > maxInputTokens)
                    {
                        MessageBox.Show("ERROR: Contexto muito grande. Por favor, apague parte da conversa anterior.");
                        return;
                    }
                    messages.Add(userMsg);

                    var options = new ChatCompletionOptions
                    {
                        MaxOutputTokenCount = maxOutputTokens,
                        Temperature = 0.1f
                    };

                    await foreach (var update in _client.CompleteChatStreamingAsync(messages, options))
                    {
                        foreach (var item in update.ContentUpdate)
                        {
                            onUpdate?.Invoke(item.Text);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ERROR: Erro ao processar a entrada do usuário: {ex.Message}");
                }
            }
        }
    }
}
