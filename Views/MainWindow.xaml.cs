using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using RightHelp___Aida.Controls;
using RightHelp___Aida.Services;
using static RightHelp___Aida.Services.OpenAIClass;


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

                // Scroll para o fim do texto, se desejar
                InputScrollViewer?.ScrollToEnd();
            }
        }

        private async void TestAnimation_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                await VoiceCircleControl.RunDotAndRespondAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(1000);
                });
            }, DispatcherPriority.Loaded);
        }

        private async void OnSendClick(object sender, RoutedEventArgs e)
        {
            UserInputBox.Text = "";

            var chat = new ChatStream("gpt-4o-mini");
        }
    }
}
