using DailyToDo.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DailyToDo
{
    public partial class MainWindow : Window
    {
        private enum DockEdge { None, Left, Right, Top }

        private DockEdge _dockedEdge = DockEdge.None;
        private const double SnapThreshold = 20;
        private const double HiddenStrip = 6;
        private const int AnimMs = 180;
        private readonly DispatcherTimer _hideTimer;

        // 拖动状态追踪，用于拦截 Aero Snap
        private bool _isDragging = false;
        private bool _blockNextMaximize = false;

        // 动画进行中时忽略鼠标事件，防止闪烁
        private bool _isAnimating = false;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            Closing += MainWindow_Closing;

            _hideTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
            _hideTimer.Tick += (s, e) => { _hideTimer.Stop(); AnimateToDocked(); };

            MouseEnter += (s, e) =>
            {
                if (_dockedEdge != DockEdge.None && !_isAnimating)
                {
                    _hideTimer.Stop();
                    AnimateToVisible();
                }
            };

            MouseLeave += (s, e) =>
            {
                if (_dockedEdge == DockEdge.None || _isAnimating) return;

                // 鼠标从贴边方向离开时不隐藏（否则会上下闪烁）
                var pos = e.GetPosition(this);
                bool leftThroughDockedEdge = _dockedEdge switch
                {
                    DockEdge.Left  => pos.X <= 0,
                    DockEdge.Right => pos.X >= ActualWidth,
                    DockEdge.Top   => pos.Y <= 0,
                    _              => false
                };

                if (!leftThroughDockedEdge)
                    _hideTimer.Start();
            };
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_ENTERSIZEMOVE = 0x0231;
            const int WM_EXITSIZEMOVE  = 0x0232;
            const int WM_SYSCOMMAND    = 0x0112;
            const int SC_MAXIMIZE      = 0xF030;

            if (msg == WM_ENTERSIZEMOVE)
            {
                _isDragging = true;
                if (_dockedEdge != DockEdge.None)
                {
                    // 解除贴边，将窗口恢复到边缘可见位置再让用户拖动
                    var screen = SystemParameters.WorkArea;
                    BeginAnimation(LeftProperty, null);
                    BeginAnimation(TopProperty, null);
                    _isAnimating = false;
                    _hideTimer.Stop();
                    switch (_dockedEdge)
                    {
                        case DockEdge.Left:  Left = screen.Left; break;
                        case DockEdge.Right: Left = screen.Right - Width; break;
                        case DockEdge.Top:   Top  = screen.Top; break;
                    }
                    _dockedEdge = DockEdge.None;
                }
            }
            else if (msg == WM_EXITSIZEMOVE)
            {
                _isDragging = false;
                // SC_MAXIMIZE 可能在 WM_EXITSIZEMOVE 之后到来，需短暂拦截
                _blockNextMaximize = true;
                TryDockToEdge();
                Dispatcher.BeginInvoke(new Action(() => _blockNextMaximize = false), DispatcherPriority.Background);
            }
            else if (msg == WM_SYSCOMMAND
                     && (_isDragging || _blockNextMaximize)
                     && (wParam.ToInt32() & 0xFFF0) == SC_MAXIMIZE)
            {
                // 拦截 Aero Snap 最大化，用我们自己的贴边代替
                handled = true;
                return IntPtr.Zero;
            }

            return IntPtr.Zero;
        }

        private void TryDockToEdge()
        {
            var screen = SystemParameters.WorkArea;

            if (Left <= screen.Left + SnapThreshold)
                DockToEdge(DockEdge.Left);
            else if (Left + Width >= screen.Right - SnapThreshold)
                DockToEdge(DockEdge.Right);
            else if (Top <= screen.Top + SnapThreshold)
                DockToEdge(DockEdge.Top);
            else
                _dockedEdge = DockEdge.None;
        }

        private void DockToEdge(DockEdge edge)
        {
            _dockedEdge = edge;
            AnimateToDocked();
        }

        private void AnimateToDocked()
        {
            if (_dockedEdge == DockEdge.None) return;
            var screen = SystemParameters.WorkArea;
            var dur = new Duration(TimeSpan.FromMilliseconds(AnimMs));
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            switch (_dockedEdge)
            {
                case DockEdge.Left:
                    Animate(LeftProperty, screen.Left - (Width - HiddenStrip), dur, ease);
                    break;
                case DockEdge.Right:
                    Animate(LeftProperty, screen.Right - HiddenStrip, dur, ease);
                    break;
                case DockEdge.Top:
                    Animate(TopProperty, screen.Top - (Height - HiddenStrip), dur, ease);
                    break;
            }
        }

        private void AnimateToVisible()
        {
            if (_dockedEdge == DockEdge.None) return;
            var screen = SystemParameters.WorkArea;
            var dur = new Duration(TimeSpan.FromMilliseconds(AnimMs));
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            switch (_dockedEdge)
            {
                case DockEdge.Left:
                    Animate(LeftProperty, screen.Left, dur, ease, checkMouseAfter: true);
                    break;
                case DockEdge.Right:
                    Animate(LeftProperty, screen.Right - Width, dur, ease, checkMouseAfter: true);
                    break;
                case DockEdge.Top:
                    Animate(TopProperty, screen.Top, dur, ease, checkMouseAfter: true);
                    break;
            }
        }

        private void Animate(DependencyProperty prop, double to, Duration dur, IEasingFunction ease,
                              bool checkMouseAfter = false)
        {
            _isAnimating = true;
            var anim = new DoubleAnimation(to, dur) { EasingFunction = ease };
            anim.Completed += (s, e) =>
            {
                _isAnimating = false;
                // 展开动画结束后：若鼠标已离开窗口，补发隐藏
                if (checkMouseAfter && _dockedEdge != DockEdge.None)
                {
                    var pos = Mouse.GetPosition(this);
                    if (pos.X < 0 || pos.X > ActualWidth || pos.Y < 0 || pos.Y > ActualHeight)
                        _hideTimer.Start();
                }
            };
            BeginAnimation(prop, anim);
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
                viewModel.SavePendingTasks();
        }

        private void PinBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.ToggleButton toggleBtn)
                Topmost = toggleBtn.IsChecked == true;
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.CheckBox checkBox)
            {
                var parent = System.Windows.Media.VisualTreeHelper.GetParent(checkBox);
                while (parent != null && !(parent is System.Windows.Controls.Grid))
                    parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);

                if (parent is System.Windows.Controls.Grid grid)
                {
                    foreach (var child in grid.Children)
                    {
                        if (child is Controls.ConfettiControl confetti)
                        {
                            confetti.Burst();
                            break;
                        }
                    }
                }
            }
        }
    }
}
