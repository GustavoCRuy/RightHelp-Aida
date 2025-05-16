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
        public MainWindow()
        {
            InitializeComponent();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) =>
            {
                double simulatedVolume = new Random().NextDouble();
                VoiceCircleControl.ReactToVolume(simulatedVolume);
            };
            timer.Start();
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

            // Limpa a entrada
            UserInputBox.Text = "";

            // anima o VoiceCircle de início (ex: bounce ou loading)
            VoiceCircleControl.StartThinkingAnimation();

            var chat = new ChatStream("gpt-4o");

            await chat.StreamResponseAsync(
                textInput: userInput,
                context: "Você é a Aida, uma assistente virtual simpática e prestativa.",
                onUpdate: (partial) =>
                {
                    // Aqui você pode mostrar a resposta na interface conforme chega
                    Console.Write(partial); // ou atualize um TextBlock, etc.
                });

            // Opcional: parar animação depois que a IA termina
            VoiceCircleControl.StopThinkingAnimation();
        }

    }
}
