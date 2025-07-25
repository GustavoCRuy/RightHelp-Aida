using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using RightHelp___Aida.Services.Constants;
using System.Text.Json;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace RightHelp___Aida.Services.AiCore
{
    public class OpenAIAudioService
    {
        private readonly string _apiKey;
        private WaveOutEvent? _waveOut;
        private MeteringSampleProvider? _meteringProvider;

        public OpenAIAudioService()
        {
            _apiKey = Const.openAIKey;
        }

        public event Action<float>? OnAudioVolume;

        public async Task PlaySpeechAsync(string text, string voice)
        {
            byte[] audioData;
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

                audioData = await response.Content.ReadAsByteArrayAsync();
            }

            using (var ms = new MemoryStream(audioData))
            using (var mp3Reader = new Mp3FileReader(ms))
            {
                var sampleProvider = mp3Reader.ToSampleProvider();

                // MeteringSampleProvider para volume
                _meteringProvider = new MeteringSampleProvider(sampleProvider);
                _meteringProvider.StreamVolume += (s, e) =>
                {
                    OnAudioVolume?.Invoke(e.MaxSampleValues[0]); // Canal esquerdo ou mono
                };

                _waveOut = new WaveOutEvent();
                _waveOut.Init(_meteringProvider);
                _waveOut.Play();

                // Aguarda até terminar
                while (_waveOut.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(50);
                }

                // Libera recursos e zera instâncias
                _waveOut.Dispose();
                _waveOut = null;
                _meteringProvider = null;
            }
        }

        public void StopAudio()
        {
            if (_waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing)
            {
                _waveOut.Stop();
            }
            _waveOut?.Dispose();
            _waveOut = null;
            _meteringProvider = null;
        }

        public bool IsAudioPlaying()
        {
            return _waveOut != null && _waveOut.PlaybackState == PlaybackState.Playing;
        }
    }
}