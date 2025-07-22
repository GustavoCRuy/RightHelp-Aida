using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using RightHelp___Aida.Controls;
using RightHelp___Aida.Services.AiCore;
using RightHelp___Aida.ViewModels;
using static RightHelp___Aida.Services.AiCore.OpenAIClass;


namespace RightHelp___Aida.Views
{
    public partial class MainWindow : Window
    {
        private string respostaCompleta = ""; // Declare respostaCompleta as a class-level field
        private bool _isAnimatingSideBar = false;
        private bool _sidebarOpen = false;
        private static string history;
        private bool playSpeech = true; // Variável para controlar se o áudio deve ser reproduzido
                                      
        public AidaViewModel AidaModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            UserInputBox.Focus();
            Keyboard.Focus(UserInputBox);
            AidaModel = new AidaViewModel();
            DataContext = AidaModel;
            ButtonMenu.MenuClicked += OnMenuButtonClick;

            // Defina a imagem inicial do botão conforme playSpeech
            var uri = playSpeech
                ? new Uri("pack://application:,,,/Assets/Images/microphone-enable.png")
                : new Uri("pack://application:,,,/Assets/Images/microphone-disable.png");
            PlaySpeechImage.Source = new BitmapImage(uri);
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

            var chat = new ChatStream("gpt-4.1-nano");

            // Prefixo da IA em azul claro
            RespostaControl.AppendParagraph("\nAI.da\n", Brushes.LightBlue);

            await chat.StreamResponseAsync(
                textInput: userInput,
                context: AidaPersonalityManager.GetContext(AidaState.CurrentPersona),
                chatHistory: history,
                onUpdate: (partial) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        RespostaControl.AppendTextToLastParagraph(partial);
                        respostaCompleta += partial;
                    });
                });

            if (playSpeech == true)
            {
                try
                {
                    var openAIAudioService = new OpenAIAudioService();
                    openAIAudioService.OnAudioVolume += (volume) =>
                    {
                        Dispatcher.Invoke(() => VoiceCircleControl.ReactToVolume(volume));
                    };
                    #pragma warning disable CS4014
                    openAIAudioService.PlaySpeechAsync(respostaCompleta, AidaPersonalities.AidaVoiceManager.GetVoiceName(AidaState.CurrentVoice));
                    #pragma warning restore CS4014
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao reproduzir áudio: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            RespostaControl.AppendParagraph("", Brushes.Transparent); // Espaço entre blocos

            VoiceCircleControl.StopThinkingAnimation();
        }


        private async void TogglePlaySpeechButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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

        private void FecharAplicativo()
        {
            Application.Current.Shutdown();
        }

        private void OnSairClick(object sender, RoutedEventArgs e)
        {
            FecharAplicativo();
        }
    }
}
