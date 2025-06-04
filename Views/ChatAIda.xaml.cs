using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using RightHelp___Aida.Controls;
using RightHelp___Aida.Services.AiCore;
using RightHelp___Aida.ViewModels;
using static RightHelp___Aida.Services.AiCore.OpenAIClass;


namespace RightHelp___Aida.Views
{
    public partial class MainWindow : Window
    {
        private bool _isAnimatingSideBar = false;
        private bool _sidebarOpen = false;
        private static string history;
        public AidaViewModel AidaModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            UserInputBox.Focus();
            Keyboard.Focus(UserInputBox);
            AidaModel = new AidaViewModel();
            DataContext = AidaModel;
            ButtonMenu.MenuClicked += OnMenuButtonClick;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) =>
            {
                double simulatedVolume = new Random().NextDouble();
                VoiceCircleControl.ReactToVolume(simulatedVolume);
            };
            timer.Start();
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

            // Exibe input do usuário com prefixo
            RespostaControl.AppendText(Environment.NewLine + "Você\n" + userInput + Environment.NewLine);

            UserInputBox.Text = "";

            // Anima o círculo da IA
            await VoiceCircleControl.StartThinkingAnimation();

            var chat = new ChatStream("gpt-4.1-nano");
            var respostaCompleta = "";
         
            // Marca início da resposta da IA com prefixo
            RespostaControl.AppendText(Environment.NewLine + "AI.da\n");

            await chat.StreamResponseAsync(
                textInput: userInput,
                context: AidaPersonalityManager.GetContext(AidaState.CurrentPersona),
                chatHistory: history,
                onUpdate: (partial) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        RespostaControl.AppendText(partial);
                        respostaCompleta += partial;    
                    });
                });

            RespostaControl.AppendText(Environment.NewLine + Environment.NewLine);

            // Finaliza a animação
            VoiceCircleControl.StopThinkingAnimation();
            var openAIAudioService = new OpenAIAudioService();
            await openAIAudioService.PlaySpeechAsync(respostaCompleta, "alloy");
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
