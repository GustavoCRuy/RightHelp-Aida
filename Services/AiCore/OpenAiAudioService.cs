using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using RightHelp___Aida.Services.Constants;
using NAudio.Wave;
using System.Text.Json;
using NAudio.Wave.SampleProviders;

namespace RightHelp___Aida.Services.AiCore
{
    public class OpenAIAudioService
    {
        private readonly string _apiKey;

        public OpenAIAudioService()
        {
            _apiKey = Const.openAIKey;
        }

        public event Action<float>? OnAudioVolume;

        public async Task PlaySpeechAsync(string text, string voice, string outputFile = "output.mp3")
        {
            // 1. Gera o arquivo de áudio usando OpenAI API
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                var payload = new
                {
                    model = "tts-1",
                    input = text,
                    voice = voice,
                    response_format = "mp3"
                };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");


                var response = await client.PostAsync("https://api.openai.com/v1/audio/speech", content);
                response.EnsureSuccessStatusCode();

                var audioData = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(outputFile, audioData);
            }

            // 2. Toca o arquivo
            using (var audioFile = new AudioFileReader(outputFile))
            {
                var sampleChannel = new SampleChannel(audioFile, true);
                var meteringProvider = new MeteringSampleProvider(sampleChannel);

                meteringProvider.StreamVolume += (s, a) =>
                {
                    // a.MaxSampleValues[0] é o volume RMS do canal esquerdo (mono/estéreo)
                    OnAudioVolume?.Invoke(a.MaxSampleValues.Max());
                };

                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(meteringProvider);
                    outputDevice.Play();

                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        await Task.Delay(50);
                    }
                }
            }
            if (File.Exists(outputFile))
                File.Delete(outputFile);
        }
    }
}
 