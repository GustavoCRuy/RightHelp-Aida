using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using RightHelp___Aida.Controls;
using RightHelp___Aida.Enums;
using RightHelp___Aida.Services.AiCore;
using RightHelp___Aida.ViewModels;
using static RightHelp___Aida.Services.AiCore.OpenAIClass;


namespace RightHelp___Aida.Views
{
    public partial class MainWindow : Window
    {
        private bool _isAnimatingSideBar = false;
        private bool _sidebarOpen = false;
        public AidaViewModel AidaModel { get; set; }
        public MainWindow()
        {
            InitializeComponent();
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

            // Referências para a animação da sidebar
            double width = SidebarContainer.ActualWidth;
            double from = SidebarTransform.X;
            double to = _sidebarOpen ? -150 : 0;

            // Animação de slide da sidebar
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

                // Garante que o valor final esteja fixo no transform após animação
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

        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            string userInput = UserInputBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(userInput))
            {
                MessageBox.Show("Por favor, digite algo antes de enviar.");
                return;
            }

            UserInputBox.Text = "";

            // Sempre adiciona espaçamento e marcador antes da resposta
            RespostaControl.AppendText(Environment.NewLine + Environment.NewLine + "• ");

            // anima o VoiceCircle de início
            await VoiceCircleControl.StartThinkingAnimation();

            var chat = new ChatStream("gpt-4.1-nano");

            await chat.StreamResponseAsync(
                textInput: userInput,
                context: "Você é Aida, extremamente analítica e perceptiva. Observa e entende os sentimentos humanos com precisão lógica, mesmo que não saiba expressar os próprios. " +
                "Fale com objetividade técnica e empatia velada. Seja gentil com dados.",
                onUpdate: (partial) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        RespostaControl.AppendText(partial);
                    });
                });
            RespostaControl.AppendText(Environment.NewLine + Environment.NewLine + "\n\n\n ");

            // parar animação depois que a IA termina
            VoiceCircleControl.StopThinkingAnimation();
        }

        private void TrocarParaLeitura()
        {
           // AidaModel.ModoAtual = AidaModoInteracao.ModoLeitura;
        }

        private void TrocarParaEscrita()
        {
           // AidaModel.ModoAtual = AidaModoInteracao.ModoEscrita;
        }
    }
}
