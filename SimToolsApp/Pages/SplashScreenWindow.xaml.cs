using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Reflection;
using System.Windows.Threading;

namespace SimTools
{
    public partial class SplashScreenWindow : Window
    {
        private DispatcherTimer _typingTimer = null!;
        private readonly string _fullText = LanguageManager.Get("Splash", "Retic", "Reticulating Splines...");
        private int _charIndex = 0;
        private readonly bool _skippable;
        private TaskCompletionSource<bool>? _skipTcs;

        public SplashScreenWindow(bool skippable = false)
        {
            InitializeComponent();
            _skippable = skippable;

            // Populate version dynamically from the compiled assembly
            var ver = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(4, 0, 1, 0);
            VersionText.Text = ver.Revision > 0
                ? $"v {ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}"
                : $"v {ver.Major}.{ver.Minor}.{ver.Build}";
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
                    await Task.Delay(500);
                    Retic.Text = "";
                    _charIndex = 0;
                    _typingTimer.Start();
                }
            };

            _typingTimer.Start();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (_skippable)
                _skipTcs?.TrySetResult(true);
        }

        public async Task RunAsync()
        {
            _skipTcs = new TaskCompletionSource<bool>();

            await FadeAsync(to: 1, durationMs: 600);

            if (_skippable)
                await Task.WhenAny(Task.Delay(13_500), _skipTcs.Task);
            else
                await Task.Delay(13_500);

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