using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Threading;

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
            PositionInnerGuideEllipse();
            if (OrbitPath == null)
                MessageBox.Show("OrbitPath está nulo!");
            else
                MessageBox.Show("OrbitPath está carregado corretamente!");
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
            if (!IsLoaded)
            {
                await this.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);
            }

            // 1. Bouncing (quicar) 2 vezes
            await PlayBounceAnimationAsync();

            // 2. Volta completa ao redor da elipse
            await PlayCircleAnimation();

            // 3. Espera terminar a volta
            await animationFinishedTCS.Task;

            // 4. Só agora entrega a resposta da IA
            await deliverResponse();
        }

        private async Task PlayBounceAnimationAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            double offSet = 30;

            double ellipseRadius = VoiceEllipse.ActualHeight / 2 - offSet;

            var fallAnimation = new DoubleAnimation
            {
                From = 0,
                To = ellipseRadius,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fallAnimation.Completed += (s1, e1) =>
            {
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
                    var bounce2 = new DoubleAnimation
                    {
                        From = ellipseRadius,
                        To = ellipseRadius - 10,
                        Duration = TimeSpan.FromMilliseconds(120),
                        AutoReverse = true,
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
                    };

                    bounce2.Completed += (s3, e3) =>
                    {
                        // Atualiza a posição real do Dot
                        double finalTop = Canvas.GetTop(Dot) + DotTransform.Y;
                        Canvas.SetTop(Dot, finalTop);
                        DotTransform.Y = 0;

                        tcs.SetResult(true);
                    };

                    DotTransform.BeginAnimation(TranslateTransform.YProperty, bounce2);
                };

                DotTransform.BeginAnimation(TranslateTransform.YProperty, bounce1);
            };

            DotTransform.Y = 0;
            DotTransform.BeginAnimation(TranslateTransform.YProperty, fallAnimation);

            await tcs.Task;
        }


        private async Task PlayCircleAnimation()
        {
            PathGeometry pathGeometry = OrbitPath.Data.GetFlattenedPathGeometry();

            DoubleAnimationUsingPath pathAnimationX = new DoubleAnimationUsingPath
            {
                PathGeometry = pathGeometry, // A geometria do caminho
                Duration = TimeSpan.FromSeconds(1.5), // Duração da animação
                Source = PathAnimationSource.X // Animando a posição X
            };

            DoubleAnimationUsingPath pathAnimationY = new DoubleAnimationUsingPath
            {
                PathGeometry = pathGeometry, // A geometria do caminho
                Duration = TimeSpan.FromSeconds(1.5), // Duração da animação
                Source = PathAnimationSource.Y // Animando a posição Y
            };

            // Cria o TranslateTransform que moverá o Dot
            TranslateTransform translateTransform = new TranslateTransform();
            Dot.RenderTransform = translateTransform;

            Storyboard storyboard = new Storyboard();

            storyboard.Children.Add(pathAnimationY);
            storyboard.Children.Add(pathAnimationX);

            Storyboard.SetTarget(pathAnimationX, translateTransform);
            Storyboard.SetTarget(pathAnimationY, translateTransform);

            Storyboard.SetTargetProperty(pathAnimationX, new PropertyPath(TranslateTransform.XProperty));
            Storyboard.SetTargetProperty(pathAnimationY, new PropertyPath(TranslateTransform.YProperty));

            storyboard.Begin();
        }

        private void PositionInnerGuideEllipse()
        {
            double outerWidth = VoiceEllipse.ActualWidth;
            double outerHeight = VoiceEllipse.ActualHeight;
            double offSet = 19.5;

            double innerWidth = outerWidth - offSet;
            double innerHeight = outerHeight - offSet;

            InnerGuideEllipse.Width = innerWidth;
            InnerGuideEllipse.Height = innerHeight;

            // Pega o centro da VoiceEllipse
            var center = VoiceEllipse.TransformToAncestor(this).Transform(new Point(0, 0));
            double centerX = center.X + outerWidth / 2;
            double centerY = center.Y + outerHeight / 2;

            // Posiciona a elipse interna centralizada
            Canvas.SetLeft(InnerGuideEllipse, centerX - innerWidth / 2);
            Canvas.SetTop(InnerGuideEllipse, centerY - innerHeight / 2);

            InnerGuideEllipse.Visibility = Visibility.Visible;
        }

    }
}
