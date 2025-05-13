using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows;
using System.Threading.Tasks;

namespace RightHelp___Aida.Controls
{
    /// <summary>
    /// Interação lógica para VoiceCircle.xaml
    /// </summary>
    public partial class VoiceCircle : UserControl
    {
        private TaskCompletionSource<bool> animationFinishedTCS;
        private double DotOffsetX = -3;
        private double DotOffsetY = 15;

        public VoiceCircle()
        {
            InitializeComponent();
            Loaded += VoiceCircle_Loaded;
        }

        private void VoiceCircle_Loaded(object sender, RoutedEventArgs e)
        {
            PositionDotAtCenter();
        }

        private void PositionDotAtCenter()
        {
            // Obtém posição relativa do centro da VoiceEllipse
            var ellipseCenter = VoiceEllipse.TransformToAncestor(this).Transform(new Point(0, 0));
            double centerX = ellipseCenter.X + VoiceEllipse.ActualWidth / 2;
            double centerY = ellipseCenter.Y + VoiceEllipse.ActualHeight / 2;

            Canvas.SetLeft(Dot, centerX - Dot.Width / 2 + DotOffsetX);
            Canvas.SetTop(Dot, centerY - Dot.Height / 2 + DotOffsetY);
        }

        public void ReactToVolume(double volumeLevel)
        {
            double scale = 1 + Math.Min(volumeLevel, 1.0) * 0.5;

            var animationX = new DoubleAnimation
            {
                To = scale,
                Duration = TimeSpan.FromMilliseconds(200),
                AutoReverse = true,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            var animationY = animationX.Clone();

            VoiceCircleScale.BeginAnimation(ScaleTransform.ScaleXProperty, animationX);
            VoiceCircleScale.BeginAnimation(ScaleTransform.ScaleYProperty, animationY);
        }

        public async Task RunDotAndRespondAsync(Func<Task> deliverResponse)
        {
            // 1. Bouncing (quicar) 2 vezes
            await PlayBounceAnimationAsync();

            // 2. Volta completa ao redor da elipse
            animationFinishedTCS = new TaskCompletionSource<bool>();
            PlayCircleAnimation();

            // 3. Espera terminar a volta
            await animationFinishedTCS.Task;

            // 4. Só agora entrega a resposta da IA
            await deliverResponse();
        }

        private async Task PlayBounceAnimationAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            // 1. Animação de queda até a borda inferior da elipse
            double ellipseRadius = VoiceEllipse.ActualHeight / 2 - 30;
            var fallAnimation = new DoubleAnimation
            {
                From = 0,
                To = ellipseRadius,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fallAnimation.Completed += async (s1, e1) =>
            {
                // 2. Primeira quicada (altura normal)
                var bounce1 = new DoubleAnimation
                {
                    From = ellipseRadius,
                    To = ellipseRadius - 20,
                    Duration = TimeSpan.FromMilliseconds(150),
                    AutoReverse = true,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
                };

                bounce1.Completed += (s2, e2) =>
                {
                    // 3. Segunda quicada (altura menor)
                    var bounce2 = new DoubleAnimation
                    {
                        From = ellipseRadius,
                        To = ellipseRadius - 10,
                        Duration = TimeSpan.FromMilliseconds(120),
                        AutoReverse = true,
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
                    };

                    bounce2.Completed += (s3, e3) => tcs.SetResult(true);
                    DotTransform.BeginAnimation(TranslateTransform.YProperty, bounce2);
                };

                DotTransform.BeginAnimation(TranslateTransform.YProperty, bounce1);
            };

            DotTransform.Y = 0; // Garante que o ponto está no topo antes de começar
            DotTransform.BeginAnimation(TranslateTransform.YProperty, fallAnimation);

            await tcs.Task;
        }


        private void PlayCircleAnimation()
        {
            // Verifique se o OrbitPath é acessível
            if (OrbitPath != null)
            {
                var pathGeometry = OrbitPath.Data.GetFlattenedPathGeometry(); // ou use o PathGeometry diretamente

                var pathAnimationX = new DoubleAnimationUsingPath
                {
                    PathGeometry = pathGeometry,
                    Duration = TimeSpan.FromSeconds(2),
                    Source = PathAnimationSource.X
                };

                var pathAnimationY = new DoubleAnimationUsingPath
                {
                    PathGeometry = pathGeometry,
                    Duration = TimeSpan.FromSeconds(2),
                    Source = PathAnimationSource.Y
                };

                var translateTransform = new TranslateTransform();
                Dot.RenderTransform = translateTransform;

                var storyboard = new Storyboard();
                storyboard.Children.Add(pathAnimationX);
                storyboard.Children.Add(pathAnimationY);

                Storyboard.SetTarget(pathAnimationX, translateTransform);
                Storyboard.SetTarget(pathAnimationY, translateTransform);
                Storyboard.SetTargetProperty(pathAnimationX, new PropertyPath(TranslateTransform.XProperty));
                Storyboard.SetTargetProperty(pathAnimationY, new PropertyPath(TranslateTransform.YProperty));

                storyboard.Completed += (s, e) => animationFinishedTCS?.TrySetResult(true);

                storyboard.Begin();
            }
            else
            {
                // Se OrbitPath não está definido, faça um log ou tratamento de erro.
                Console.WriteLine("OrbitPath não foi encontrado.");
            }
        }
    }
}
