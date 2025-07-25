using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using RightHelp___Aida.Services.AiCore;
using RightHelp___Aida.ViewModels;
using static RightHelp___Aida.Services.AiCore.OpenAIClass;

// Adicione:
using RightHelp___Aida.Services.DataBaseLogic;
using RightHelp___Aida.Services.Constants;

namespace RightHelp___Aida.Views
{
    public partial class MainWindow : Window
    {
        private string respostaCompleta = ""; // Declare respostaCompleta as a class-level field
        private bool _isAnimatingSideBar = false;
        private bool _sidebarOpen = false;
        private static string history;
        private bool playSpeech = true; // Variável para controlar se o áudio deve ser reproduzido
        private bool isFirstMessageSent = false;
        private OpenAIAudioService? _openAIAudioService;

        public AidaViewModel AidaModel { get; set; }

        // Instância do DBLogic usando a string de conexão centralizada
        private DBLogic dbLogic = new DBLogic();

        // Id do usuário - ajuste para pegar do login/autenticação real
        private int usuarioId = 1; // exemplo fixo para teste

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
            var systemPrompt = AidaPersonalityManager.GetContext(AidaState.CurrentPersona);
            history = dbLogic.MontarContexto(usuarioId, systemPrompt, 20);
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

            SidebarContainer.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
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
                tb.Measure(new Size(tb.ActualWidth, double.PositiveInfinity));
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
            RespostaControl.AppendParagraph("Você", Brushes.White);
            RespostaControl.AppendParagraph(userInput, Brushes.White);

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
            RespostaControl.AppendParagraph("\nAI.da\n", Brushes.LightBlue);

            // Obtém a personalidade atual para o contexto
            var systemPrompt = AidaPersonalityManager.GetContext(AidaState.CurrentPersona); // Só o texto da personalidade
            var chatHistory = dbLogic.MontarContexto(usuarioId, systemPrompt, 20); // Contexto completo (system + histórico)

            await chat.StreamResponseAsync(
                textInput: userInput,
                context: systemPrompt,       // Só a personalidade/instrução
                chatHistory: chatHistory,    // O contexto JSON completo
                onUpdate: (partial) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        RespostaControl.AppendTextToLastParagraph(partial);
                        respostaCompleta += partial;
                    });
                });

            // Salva pergunta e resposta no banco de dados
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
                    await _openAIAudioService.PlaySpeechAsync(respostaCompleta, AidaPersonalities.AidaVoiceManager.GetVoiceName(AidaState.CurrentVoice));

                    _openAIAudioService.Equals(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao reproduzir áudio: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            RespostaControl.AppendParagraph("", Brushes.Transparent); // Espaço entre blocos
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
                var color = AidaPersonalities.GetPersonaColor(selected);
                var shadowColor = AidaPersonalities.GetPersonaShadowColor(selected);
                VoiceCircleControl.SetPersonaColor(color);
                VoiceCircleControl.SetPersonaShadow(shadowColor);

                // Atualiza histórico do banco ao mudar personalidade
                CarregarHistoricoDoBanco();
            }
        }

        private void VoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VoiceComboBox.SelectedItem is AidaPersonalities.AidaVoice selected)
            {
                AidaState.CurrentVoice = selected;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PersonaComboBox.ItemsSource = Enum.GetValues(typeof(AidaPersonalities.AidaPersona));
            PersonaComboBox.SelectedItem = AidaState.CurrentPersona;

            VoiceComboBox.ItemsSource = Enum.GetValues(typeof(AidaPersonalities.AidaVoice));
            VoiceComboBox.SelectedItem = AidaState.CurrentVoice;
        }

        private void OnSairClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}