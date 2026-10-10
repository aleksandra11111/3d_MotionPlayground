using MotionPlayground3D;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MotionPlayground.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public Playground3DApp Engine3D { get; set; }
        public Frame Stage { get; set; }

        public ObservableCollection<string> Themes { get; } = new ObservableCollection<string> { "Dark", "Neon", "Pastel" };
        private string _selectedTheme = "Neon";
        public string SelectedTheme
        {
            get => _selectedTheme;
            set { if (Set(ref _selectedTheme, value)) ApplyTheme(); }
        }

        private Color _pageBackground; public Color PageBackground { get => _pageBackground; set => Set(ref _pageBackground, value); }
        private Color _foreground; public Color Foreground { get => _foreground; set => Set(ref _foreground, value); }
        private Color _accent; public Color Accent { get => _accent; set => Set(ref _accent, value); }
        private Color _accent2; public Color Accent2 { get => _accent2; set => Set(ref _accent2, value); }

        private double _speedMultiplier = 1.0;
        public double SpeedMultiplier
        {
            get => _speedMultiplier;
            set
            {
                if (Set(ref _speedMultiplier, value))
                    Engine3D?.SetSpeed(_speedMultiplier);
            }
        }

        private bool _combineRotatePulse;
        public bool CombineRotatePulse
        {
            get => _combineRotatePulse;
            set
            {
                if (Set(ref _combineRotatePulse, value))
                    Engine3D?.SetCombineRotatePulse(_combineRotatePulse);
            }
        }

        private bool _showCircle = true; public bool ShowCircle { get => _showCircle; set => Set(ref _showCircle, value); }
        private bool _showSquare = false; public bool ShowSquare { get => _showSquare; set => Set(ref _showSquare, value); }

        public class AnimationItem
        {
            public string Key { get; set; }             
            public string Name { get; set; }
            public Color PreviewColor { get; set; }
            public Color StageBackground { get; set; } 
            public string ShapeKey { get; set; }        
        }

        public ObservableCollection<AnimationItem> Animations { get; } = new ObservableCollection<AnimationItem>
        {
            new AnimationItem{ Key="pulse",  Name="Pulse",  PreviewColor=Color.FromHex("#38BDF8"), StageBackground=Color.FromHex("#0EA5E9"), ShapeKey="circle"    },
            new AnimationItem{ Key="rotate", Name="Rotate", PreviewColor=Color.FromHex("#A78BFA"), StageBackground=Color.FromHex("#7C3AED"), ShapeKey="square"  },
            new AnimationItem{ Key="slide",  Name="Slide",  PreviewColor=Color.FromHex("#34D399"), StageBackground=Color.FromHex("#10B981"), ShapeKey="square"    },
            new AnimationItem{ Key="fade",   Name="Fade",   PreviewColor=Color.FromHex("#FBBF24"), StageBackground=Color.FromHex("#F59E0B"), ShapeKey="circle"    },
            new AnimationItem{ Key="bounce", Name="Bounce", PreviewColor=Color.FromHex("#F472B6"), StageBackground=Color.FromHex("#DB2777"), ShapeKey="square"    }
        };

        private AnimationItem _selectedAnimation;
        public AnimationItem SelectedAnimation
        {
            get => _selectedAnimation;
            set
            {
                if (Set(ref _selectedAnimation, value))
                {
                    ApplyStageBackground(animated: true);
                    if (Engine3D != null)
                    {
                        _ = StartAsync();
                    }
                }
            }
        }


        // ==== Команды ====
        public Command StartCommand { get; }
        public Command StopCommand { get; }

        public MainViewModel()
        {
            ApplyTheme();
            SelectedAnimation = Animations[0];
            StartCommand = new Command(async () => await StartAsync());
            StopCommand = new Command(Stop);
        }

        private CancellationTokenSource _cts;

        private async Task StartAsync()
        {
            Stop();

            if (SelectedAnimation == null && Animations.Count > 0)
                SelectedAnimation = Animations[0];

            ApplyStageBackground(animated: true);

            if (Engine3D != null)
            {
                Engine3D.SetSpeed(SpeedMultiplier);
                Engine3D.SetCombineRotatePulse(CombineRotatePulse);
                Engine3D.SetAnimation(SelectedAnimation.Key);
            }

            await Task.CompletedTask;
        }

        private void Stop()
        {
            _cts?.Cancel();
            Stage?.AbortAnimation("color");
            Engine3D?.Stop();
        }




        // ===== Тема/фон/фигура =====
        private void ApplyTheme()
        {
            switch (SelectedTheme)
            {
                case "Dark":
                    PageBackground = Color.FromHex("#0B1020");
                    Foreground = Color.FromHex("#E5E7EB");
                    Accent = Color.FromHex("#60A5FA");
                    Accent2 = Color.FromHex("#2563EB");
                    break;
                case "Pastel":
                    PageBackground = Color.FromHex("#F7F7FB");
                    Foreground = Color.FromHex("#1E293B");
                    Accent = Color.FromHex("#8BD3DD");
                    Accent2 = Color.FromHex("#F6C2D9");
                    break;
                default: // Neon
                    PageBackground = Color.FromHex("#070711");
                    Foreground = Color.FromHex("#F8FAFC");
                    Accent = Color.FromHex("#22D3EE");
                    Accent2 = Color.FromHex("#A78BFA");
                    break;
            }
        }

        private void ApplyStageBackground(bool animated = false)
        {
            if (Stage == null || SelectedAnimation == null) return;

            var to = SelectedAnimation.StageBackground;

            if (!animated)
            {
                Stage.BackgroundColor = to;
                return;
            }

            var from = Stage.BackgroundColor;
            var anim = new Animation(v =>
            {
                Stage.BackgroundColor = Color.FromRgba(
                    from.R + (to.R - from.R) * v,
                    from.G + (to.G - from.G) * v,
                    from.B + (to.B - from.B) * v,
                    from.A + (to.A - from.A) * v);
            });

            StartStageColorAnimation(anim, 350);
        }

        
        private void StartStageColorAnimation(Animation anim, uint length = 350)
        {
            if (Stage == null) return;
            anim.Commit(Stage, "color", 16, length, Easing.CubicInOut, (v, c) => { }, () => false);
        }


        // ==== INotifyPropertyChanged ====
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
