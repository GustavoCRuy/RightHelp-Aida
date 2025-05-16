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
        private bool isAnimating;

        public VoiceCircle()
        {
            InitializeComponent();
            Loaded += VoiceCircle_Loaded;
        }

        private void VoiceCircle_Loaded(object sender, RoutedEventArgs e)
        {
            PositionDotAtCenter();
            PositionInnerGuideEllipse();
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

        private async Task PlayBounceAnimationAsync()
        {
            if (isAnimating) // Verifica se já existe uma animação em andamento
            {
                return; // Se já estiver animando, não faz nada
            }

            isAnimating = true;

            var tcs = new TaskCompletionSource<bool>();

            // Parâmetros ajustáveis
            double maxBounceDistance = -42;  // Distância máxima do salto (ajustável)
            double animationDurationFall = 300;  // Tempo de queda (ajustável)
            double animationDurationBounce = 250;  // Tempo de cada quique (ajustável)
            double animationDurationReturn = 500;  // Tempo para retornar à posição original (ajustável)

            // Posição inicial do Dot
            double startPos = 14;

            // Define a animação para o primeiro salto (cair)
            var fallAnimation = new DoubleAnimation
            {
                From = startPos,
                To = startPos - maxBounceDistance,  // Cai para baixo
                Duration = TimeSpan.FromMilliseconds(animationDurationFall),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fallAnimation.Completed += async (s1, e1) =>
            {
                var bounce1 = new DoubleAnimation
                {
                    From = startPos - maxBounceDistance,  // Começa da posição do salto
                    To = startPos - maxBounceDistance*0.33,  // Menor salto
                    Duration = TimeSpan.FromMilliseconds(animationDurationBounce),
                    AutoReverse = true,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
                };

                bounce1.Completed += async (s2, e2) =>
                {
                    var bounce2 = new DoubleAnimation
                    {
                        From = startPos - maxBounceDistance,
                        To = startPos - (maxBounceDistance*0.66),  // Menor ainda
                        Duration = TimeSpan.FromMilliseconds(animationDurationBounce),
                        AutoReverse = true,
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
                    };

                    bounce2.Completed += async (s3, e3) =>
                    {
                        // O terceiro salto é para voltar ao centro
                        var returnAnimation = new DoubleAnimation
                        {
                            From = startPos - maxBounceDistance,
                            To = startPos, // Retorna à posição original
                            Duration = TimeSpan.FromMilliseconds(animationDurationReturn*3),
                            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                        };

                        await Task.Delay(750);

                        returnAnimation.Completed += (s4, e4) =>
                        {
                            // Atualiza a posição real do Dot
                            double finalTop = Canvas.GetTop(Dot) + DotTransform.Y;
                            Canvas.SetTop(Dot, finalTop);
                            DotTransform.Y = 0;

                            isAnimating = false;
                            tcs.SetResult(true);
                        };

                        DotTransform.BeginAnimation(TranslateTransform.YProperty, returnAnimation);
                    };

                    DotTransform.BeginAnimation(TranslateTransform.YProperty, bounce2);
                };

                DotTransform.BeginAnimation(TranslateTransform.YProperty, bounce1);
            };

            DotTransform.Y = 0;
            DotTransform.BeginAnimation(TranslateTransform.YProperty, fallAnimation);

            await tcs.Task;
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

        /// <summary>
        /// Inicia a animação de pensamento (bouncing dot).
        /// Apenas um alias de controle externo — a lógica está no PlayBounceAnimationAsync.
        /// </summary>
        public async Task StartThinkingAnimation()
        {
            await PlayBounceAnimationAsync();
        }

        /// <summary>
        /// Finaliza qualquer estado de pensamento (caso precise cancelar ou limpar algo visual).
        /// </summary>
        public void StopThinkingAnimation()
        {
            // Reseta posição caso esteja interrompendo no meio
            if (isAnimating)
            {
                DotTransform.BeginAnimation(TranslateTransform.YProperty, null); // Para qualquer animação
                double finalTop = Canvas.GetTop(Dot) + DotTransform.Y;
                Canvas.SetTop(Dot, finalTop);
                DotTransform.Y = 0;
                isAnimating = false;
            }
        }


    }
}
