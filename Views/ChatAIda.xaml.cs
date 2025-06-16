using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using RightHelp___Aida.Services;
using RightHelp___Aida.Services.AiCore;
using RightHelp___Aida.Services.DataBaseLogic;
using RightHelp___Aida.ViewModels;
using static RightHelp___Aida.Services.AiCore.OpenAIClass;

namespace RightHelp___Aida.Views
{
    public partial class MainWindow : Window
    {
        private bool _isAnimatingSideBar = false;
        private bool _sidebarOpen = false;
        private static string history;
        private string CurrentSessionId;
        public AidaViewModel AidaModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            CurrentSessionId = Guid.NewGuid().ToString();
            UserInputBox.Focus();
            Keyboard.Focus(UserInputBox);
            AidaModel = new AidaViewModel();
            DataContext = AidaModel;
            ButtonMenu.MenuClicked += OnMenuButtonClick;
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

            RespostaControl.AppendText(Environment.NewLine + "Você:\n" + userInput + Environment.NewLine);

            var userMessage = new MessageObject
            {
                SessionId = CurrentSessionId,
                UserId = UserSession.UserId,
                Role = "user",
                Message = userInput,
                Timestamp = DateTime.UtcNow
            };
            await DataBaseLogic.SalvarMensagemAsync(userMessage);

            UserInputBox.Text = "";

            await VoiceCircleControl.StartThinkingAnimation();

            var chat = new ChatStream("gpt-4o");
            var respostaCompleta = "";

            RespostaControl.AppendText(Environment.NewLine + "AI.da:\n");

            var temp = new MessageObject();
            var chatHistory = await DataBaseLogic.BuscarHistoricoAsync(CurrentSessionId);

            await chat.StreamResponseAsync(
                textInput: userInput,
                context: AidaPersonalityManager.GetContext(AidaState.CurrentPersona),
                chatHistory: chatHistory,
                onUpdate: (partial) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        RespostaControl.AppendText(partial);
                        respostaCompleta += partial;
                    });
                }
             );

            RespostaControl.AppendText(Environment.NewLine + Environment.NewLine);

            var assistantMessage = new MessageObject
            {
                SessionId = CurrentSessionId,
                UserId = UserSession.UserId,
                Role = "assistant",
                Message = respostaCompleta,
                Timestamp = DateTime.UtcNow
            };
            await DataBaseLogic.SalvarMensagemAsync(assistantMessage);

            VoiceCircleControl.StopThinkingAnimation();

            var openAIAudioService = new OpenAIAudioService();
            openAIAudioService.OnAudioVolume += (volume) =>
            {
                Dispatcher.Invoke(() => VoiceCircleControl.ReactToVolume(volume));
            };

            string vozSelecionada = AidaState.CurrentVoice.ToString().ToLower();
            await openAIAudioService.PlaySpeechAsync(respostaCompleta, vozSelecionada);
        }

        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            await SendUserMessageAsync();
        }

        private void UserInputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(UserInputBox);
        }

        private void TrocarParaLeitura()
        {
           // AidaModel.ModoAtual = AidaModoInteracao.ModoLeitura;
        }

        private void TrocarParaEscrita()
        {
           // AidaModel.ModoAtual = AidaModoInteracao.ModoEscrita;
        }

        private void PersonaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PersonaComboBox.SelectedItem is AidaPersonalities.AidaPersona selected)
            {
                AidaState.CurrentPersona = selected;
                // Atualiza o contexto da IA, se necessário
                // Ex: recarregar o ChatStream com novo contexto ou salvar preferência
            }
        }

        private void VoiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VoiceComboBox.SelectedItem is AidaPersonalities.AidaVoice selected)
            {
                AidaState.CurrentVoice = selected;
                // Atualiza a voz da IA.
                // Ex: recarregar o ChatStream com novo contexto ou salvar preferência
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PersonaComboBox.ItemsSource = Enum.GetValues(typeof(AidaPersonalities.AidaPersona));
            PersonaComboBox.SelectedItem = AidaState.CurrentPersona;

            VoiceComboBox.ItemsSource = Enum.GetValues(typeof(AidaPersonalities.AidaVoice));
            VoiceComboBox.SelectedItem = AidaState.CurrentVoice;
        }
    }
}
