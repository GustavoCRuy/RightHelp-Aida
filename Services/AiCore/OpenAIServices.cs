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

namespace RightHelp___Aida.Services.AiCore
{
    internal class OpenAIClass
    {
        internal class ChatStream
        {
            private readonly string _modelName = "gpt-4o-mini";
            private readonly ChatClient _client;

            public ChatStream(string model)
            {
                _modelName = model;
                try
                {
                    _client = new ChatClient(_modelName, Constants.Constants.openAIKey);
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
            public async Task StreamResponseAsync(string textInput, string context, Action<string> onUpdate)
            {
                if (string.IsNullOrEmpty(textInput))
                {
                    MessageBox.Show("ERROR: A entrada do usuário não pode ser vazia.");
                    return;
                }

                try
                {
                    var messages = new List<ChatMessage>
                    {
                        ChatMessage.CreateSystemMessage(context),
                        ChatMessage.CreateUserMessage(textInput)
                    };
                    var options = new ChatCompletionOptions
                    {
                        MaxOutputTokenCount = 500
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
