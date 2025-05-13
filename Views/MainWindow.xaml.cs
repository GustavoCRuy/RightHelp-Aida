using System.Windows;
using System.Windows.Threading;
using RightHelp___Aida.Controls;

namespace RightHelp___Aida.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            /* -- Simulação volume para pusar a logo -- */
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) =>
            {
                double simulatedVolume = new Random().NextDouble(); // entre 0.0 e 1.0
                VoiceCircleControl.ReactToVolume(simulatedVolume);
            };
            timer.Start();
        }
        private async void TestAnimation_Click(object sender, RoutedEventArgs e)
        {
            await VoiceCircleControl.RunDotAndRespondAsync(async () =>
            {
                // Simula tempo de "resposta" da IA
                await Task.Delay(1000);

                MessageBox.Show("Resposta entregue!");
            });
        }

    }
}