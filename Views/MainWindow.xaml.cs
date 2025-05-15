using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using RightHelp___Aida.Controls;

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

        private void UserInputBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
            {
                tb.Measure(new Size(tb.ActualWidth, double.PositiveInfinity));
                var desiredHeight = tb.DesiredSize.Height;
                double maxHeight = 200;

                tb.Height = Math.Min(desiredHeight, maxHeight);

                // Ajusta o offset vertical do VoiceCircle via RenderTransform
                if (VoiceCircleControl.RenderTransform is TranslateTransform transform)
                {
                    double offset = Math.Min(desiredHeight, maxHeight) - 30;
                    transform.Y = -offset;
                }
            }
        }

        private async void TestAnimation_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                await VoiceCircleControl.RunDotAndRespondAsync(async () =>
                {
                    await System.Threading.Tasks.Task.Delay(1000);
                    MessageBox.Show("Resposta entregue!");
                });
            }, DispatcherPriority.Loaded);
        }
    }
}
