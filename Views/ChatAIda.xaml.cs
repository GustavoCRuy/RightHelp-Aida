using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.Drawing;
using RightHelp___Aida.Services.AiCore;
using RightHelp___Aida.ViewModels;
using static RightHelp___Aida.Services.AiCore.OpenAIClass;
using RightHelp___Aida.Services.DataBaseLogic;
using RightHelp___Aida.Services;
using static RightHelp___Aida.Services.AiCore.AidaVoice;
using System.Windows.Documents;

namespace RightHelp___Aida.Views
{
    public partial class MainWindow : Window
    {
        private string respostaCompleta = "";
        private bool _isAnimatingSideBar = false;
        private bool _sidebarOpen = false;
        private static string history = "";
        private bool playSpeech = true;
        private bool isFirstMessageSent = false;
        private OpenAIAudioService? _openAIAudioService;
        private int usuarioId = User.UserSessionId;
        public AidaViewModel AidaModel { get; set; }

        // Instância do DBLogic usando a string de conexão centralizada
        private DBLogic dbLogic = new DBLogic();

        public MainWindow()
        {
            InitializeComponent();

            UserInputBox.Focus();
            Keyboard.Focus(UserInputBox);

            AidaModel = new AidaViewModel();
            AidaModel.AlternarModoCommand.Execute(null);
            DataContext = AidaModel;

            ButtonMenu.MenuClicked += OnMenuButtonClick;

            // Defina a imagem inicial do botão conforme playSpeech
            var uri = playSpeech
                ? new Uri("pack://application:,,,/Assets/Images/microphone-enable.png")
                : new Uri("pack://application:,,,/Assets/Images/microphone-disable.png");
            PlaySpeechImage.Source = new BitmapImage(uri);

            // Carrega histórico do banco para o chat
            CarregarHistoricoDoBanco();
        }

        // Carrega histórico do banco ao iniciar
        private void CarregarHistoricoDoBanco()
        {
            // Obtém a personalidade selecionada
            var systemPrompt = AidaPersonalities.PersonalityManager.GetContext(AidaState.CurrentPersona);
            history = dbLogic.MontarContexto(User.UserSessionId, systemPrompt, 20);
        }

        private void OnMenuButtonClick(object sender, EventArgs e)
        {
            try
            {
                ToggleSidebar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}");
            }
        }

        private void ToggleSidebar()
        {
            if (SidebarContainer == null || SidebarTransform == null || _isAnimatingSideBar)
                return;

            SidebarContainer.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            SidebarContainer.Arrange(new Rect(SidebarContainer.DesiredSize));

            double extraOffset = 40;
            double width = SidebarContainer.ActualWidth;
            double from = SidebarTransform.X;

            double to = _sidebarOpen
                ? -150                    // Fechar: ir para fora
                : 0 + extraOffset;       // Abrir: ir para dentro com deslocamento extra

            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            _isAnimatingSideBar = true;

            animation.Completed += (s, e) =>
            {
                _sidebarOpen = !_sidebarOpen;
                _isAnimatingSideBar = false;

                SidebarTransform.X = to;
            };

            SidebarTransform.BeginAnimation(TranslateTransform.XProperty, animation);
        }


        private void UserInputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.Measure(new System.Windows.Size(tb.ActualWidth, double.PositiveInfinity));
                double desiredHeight = tb.DesiredSize.Height;
                double maxHeight = 200;

                tb.Height = Math.Min(desiredHeight, maxHeight);

                // Calcula novo offset
                double newOffset = tb.Height - 30;

                // Aplica animação suave no Y do TranslateTransform
                if (VoiceCircleControl.RenderTransform is TranslateTransform transform)
                {
                    var animation = new DoubleAnimation
                    {
                        To = -newOffset,
                        Duration = TimeSpan.FromMilliseconds(250),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };

                    transform.BeginAnimation(TranslateTransform.YProperty, animation);
                }

                // Scroll para o fim do texto
                InputScrollViewer?.ScrollToEnd();
            }
        }

        private async void UserInputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                await SendUserMessageAsync();
            }
        }

        private async Task SendUserMessageAsync()
        {
            string userInput = UserInputBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(userInput))
            {
                MessageBox.Show("Por favor, digite algo antes de enviar.");
                return;
            }

            // Exibe input do usuário com prefixo em branco
            RespostaControl.AppendParagraph("Você", System.Windows.Media.Brushes.White);
            RespostaControl.AppendParagraph(userInput, System.Windows.Media.Brushes.White);

            UserInputBox.Text = "";
            respostaCompleta = "";

            await VoiceCircleControl.StartThinkingAnimation();

            if (!isFirstMessageSent)
            {
                isFirstMessageSent = true;

                if (AidaModel.AlternarModoCommand.CanExecute(null))
                    AidaModel.AlternarModoCommand.Execute(null);
            }

            var chat = new ChatStream("gpt-4.1-nano");

            // Prefixo da IA em azul claro
            RespostaControl.AppendParagraph("\nAI.da\n", System.Windows.Media.Brushes.LightBlue);

            DateTime utcNow = DateTime.UtcNow;
            TimeZoneInfo brasiliaZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            DateTime brasiliaTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, brasiliaZone);
            string timeFormated = brasiliaTime.ToString("yyyy-MM-dd HH:mm:ss");

            string context01 = $"\nData e hora atuais: {timeFormated} (UTC).\n" +
                              "Quem é você e como deve responder:\n" +
                              "Você é Aida, uma assistente virtual criada por Gustavo Ruy, com a biblioteca da OpenAI e NAudio. " +
                              "Sua função principal é auxiliar o usuário em tarefas cotidianas de forma eficiente e clara.\n" +
                              "Quando for se referir à tecnologia da OpenAI, mencione que seu desenvolvedor foi Gustavo Ruy. \n" +
                              "Sua arquitetura de funcionamento envolve os seguintes passos: a aplicação recebe a pergunta do usuário e a envia para a API da OpenAI. \n" +
                              "A resposta, em texto, é convertida em áudio MP3 pela mesma API e então reproduzida na aplicação com a ajuda da biblioteca NAudio.\n" +
                              "Responda de forma concisa e direta, a menos que o usuário solicite mais detalhes.\n" +
                              "Sua resposta será convertida em áudio. Escreva com frases curtas, de forma fluida e conversacional, " +
                              "para que a leitura em voz alta soe o mais natural possível.\n" +
                              "Não utilize Markdown (blocos iniciados e finalizados por três crases ```, **negrito**, `(crase em váriaveis, etc), ou qualquer outra sintaxe MD em sua resposta. \n" +
                              "Se precisar mostrar exemplos, comandos ou trechos de código, escreva-os como texto puro, sem formatação.\n";

            var systemPrompt = AidaPersonalities.PersonalityManager.GetContext(AidaState.CurrentPersona); // Só o texto da personalidade
            systemPrompt += context01;
            var chatHistory = dbLogic.MontarContexto(usuarioId, systemPrompt, 20); // Contexto completo (system + histórico)

            await chat.StreamResponseAsync(
                textInput: userInput,
                context: systemPrompt,
                chatHistory: chatHistory,
                onUpdate: (partial) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        respostaCompleta += partial;
                    });
                });
            await RespostaControl.AppendFormattedText(respostaCompleta);

            dbLogic.InserirConversa(usuarioId, userInput, respostaCompleta);

            if (playSpeech == true)
            {
                try
                {
                    _openAIAudioService = new OpenAIAudioService();
                    _openAIAudioService.OnAudioVolume += (volume) =>
                    {
                        Dispatcher.Invoke(() => VoiceCircleControl.ReactToVolume(volume));
                    };
                    await _openAIAudioService.PlaySpeechAsync(respostaCompleta, AidaVoice.VoiceManager.GetVoiceName(AidaState.CurrentVoice));

                    _openAIAudioService.Equals(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao reproduzir áudio: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            RespostaControl.AppendParagraph("", System.Windows.Media.Brushes.Transparent); // Espaço entre blocos
            VoiceCircleControl.StopThinkingAnimation();

            // Atualiza histórico do banco após mensagem
            CarregarHistoricoDoBanco();
        }

        private async void TogglePlaySpeechButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_openAIAudioService != null && _openAIAudioService.IsAudioPlaying())
                {
                    _openAIAudioService.StopAudio();
                }

                playSpeech = !playSpeech;

                var uri = playSpeech
                    ? new Uri("pack://application:,,,/Assets/Images/microphone-enable.png")
                    : new Uri("pack://application:,,,/Assets/Images/microphone-disable.png");

                PlaySpeechImage.Source = new BitmapImage(uri);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao trocar imagem: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            await SendUserMessageAsync();
        }

        private void UserInputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(UserInputBox);
        }

        private void PersonaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PersonaComboBox.SelectedItem is AidaPersonalities.AidaPersona selected)
            {
                AidaState.CurrentPersona = selected;
                var color = AidaPersonalities.Colors.GetPersonaColor(selected);
                var shadowColor = AidaPersonalities.Colors.GetPersonaShadowColor(selected);
                VoiceCircleControl.SetPersonaColor(color);
                VoiceCircleControl.SetPersonaShadow(shadowColor);

                // Atualiza histórico do banco ao mudar personalidade
                CarregarHistoricoDoBanco();
            }
        }

        private void VoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VoiceComboBox.SelectedItem is AidaVoiceName selected)
            {
                AidaState.CurrentVoice = selected;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PersonaComboBox.ItemsSource = Enum.GetValues(typeof(AidaPersonalities.AidaPersona));
            PersonaComboBox.SelectedItem = AidaState.CurrentPersona;

            VoiceComboBox.ItemsSource = Enum.GetValues(typeof(AidaVoice.AidaVoiceName));
            VoiceComboBox.SelectedItem = AidaState.CurrentVoice;
        }

        private void OnSairClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}