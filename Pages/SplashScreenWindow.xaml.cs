using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SimTools
{
    public partial class SplashScreenWindow : Window
    {
        private DispatcherTimer _typingTimer = null!;
        private readonly string _fullText = LanguageManager.Get("Splash", "Retic", "Reticulating Splines...");
        private int _charIndex = 0;

        public SplashScreenWindow()
        {
            InitializeComponent();
            LoadingText.Text = LanguageManager.Get("Splash", "Loading", "Loading...");
            StartTypingAnimation();
        }

        private void StartTypingAnimation()
        {
            Retic.Text = "";
            _charIndex = 0;

            _typingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(40)
            };

            _typingTimer.Tick += async (s, e) =>
            {
                if (_charIndex < _fullText.Length)
                {
                    Retic.Text = _fullText[..++_charIndex];
                }
                else
                {
                    _typingTimer.Stop();
                    await Task.Delay(500);   // pause at full text for 0.5 s
                    Retic.Text = "";
                    _charIndex = 0;
                    _typingTimer.Start();
                }
            };

            _typingTimer.Start();
        }

        public async Task RunAsync()
        {
            await FadeAsync(to: 1, durationMs: 600);
            await Task.Delay(10_000);
            await FadeAsync(to: 0, durationMs: 600);
        }

        private Task FadeAsync(double to, int durationMs)
        {
            var tcs = new TaskCompletionSource<bool>();
            var anim = new DoubleAnimation(to, TimeSpan.FromMilliseconds(durationMs))
            { FillBehavior = FillBehavior.HoldEnd };
            anim.Completed += (_, _) => tcs.TrySetResult(true);
            BeginAnimation(OpacityProperty, anim);
            return tcs.Task;
        }
    }
}