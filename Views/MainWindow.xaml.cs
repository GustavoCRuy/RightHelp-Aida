using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using RightHelp___Aida.Controls;
using static RightHelp___Aida.Services.AiCore.OpenAIClass;


namespace RightHelp___Aida.Views
{
    public partial class MainWindow : Window
    {
        private bool _sidebarOpen = false;
        public MainWindow()
        {
            InitializeComponent();

            ButtonMenu.MenuClicked += MenuButton_MenuClicked;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) =>
            {
                double simulatedVolume = new Random().NextDouble();
                VoiceCircleControl.ReactToVolume(simulatedVolume);
            };
            timer.Start();
        }

        private void MenuButton_MenuClicked(object sender, EventArgs e)
        {
            ToggleSidebar();
        }

        private void ToggleSidebar()
        {
            double width = Sidebar.Width;
            double from = _sidebarOpen ? 0 : -width;
            double to = _sidebarOpen ? -width : 0;

            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            SidebarTransform.BeginAnimation(TranslateTransform.XProperty, animation);

            _sidebarOpen = !_sidebarOpen;
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
                context: "Você é a Aida, uma assistente virtual mas prestativa.",
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
    }
}
