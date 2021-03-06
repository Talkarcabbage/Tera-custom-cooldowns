﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TCC.Data;
using TCC.ViewModels;

namespace TCC.Windows
{
    /// <summary>
    /// Logica di interazione per LfgListWindow.xaml
    /// </summary>
    public partial class LfgListWindow
    {
        public LfgListViewModel VM => Dispatcher.Invoke(() => DataContext as LfgListViewModel);
        public IntPtr Handle => Dispatcher.Invoke(() => new WindowInteropHelper(this).Handle);

        private readonly ColorAnimation _colAn = new ColorAnimation { Duration = TimeSpan.FromMilliseconds(200) };


        public LfgListWindow()
        {
            InitializeComponent();
            DataContext = new LfgListViewModel();
            VM.PropertyChanged += VM_PropertyChanged;
            WindowManager.ForegroundManager.VisibilityChanged += () =>
            {
                if (WindowManager.ForegroundManager.Visible) RefreshTopmost();
            };
            Closing += (_, ev) =>
            {
                ev.Cancel = true;
                CloseWindow();
            };
        }

        private void RefreshTopmost()
        {
            Dispatcher.Invoke(() =>
            {
                Topmost = false;
                Topmost = true;
            });
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private void VM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(VM.Creating):
                case nameof(VM.NewMessage):
                    if (VM.Creating)
                    {
                        _colAn.To = string.IsNullOrEmpty(VM.NewMessage)
                            ? ((SolidColorBrush)Application.Current.FindResource("HpColor")).Color
                            : ((SolidColorBrush)Application.Current.FindResource("GreenColor")).Color;
                    }
                    else
                    {
                        _colAn.To = (Application.Current.FindResource("BackgroundDarkColor") as SolidColorBrush).Color;
                    }
                    var currBg = CreateMessageBtn.Background as SolidColorBrush;
                    var currCol = currBg.Color;
                    var newBg = new SolidColorBrush(currCol);
                    CreateMessageBtn.Background = newBg;
                    CreateMessageBtn.Background.BeginAnimation(SolidColorBrush.ColorProperty, _colAn);
                    break;
                case nameof(VM.AmIinLfg) when VM.AmIinLfg:
                    LfgMgmtBtn.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty,
                        new DoubleAnimation(1,
                            TimeSpan.FromMilliseconds(150))
                        { EasingFunction = new QuadraticEase() });
                    CreateMessageBtn.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty,
                        new DoubleAnimation(0,
                            TimeSpan.FromMilliseconds(150))
                        { EasingFunction = new QuadraticEase() });
                    CreateMessageBtn.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(4, 0, 4, 0), TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() });
                    break;
                case nameof(VM.AmIinLfg):
                    LfgMgmtBtn.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() });
                    CreateMessageBtn.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() });
                    CreateMessageBtn.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(4), TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() });
                    break;
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }
        public void CloseWindow()
        {
            Dispatcher.InvokeIfRequired(() =>
            {
                var a = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
                a.Completed += (s, ev) => { Hide(); };
                BeginAnimation(OpacityProperty, a);
            }, DispatcherPriority.DataBind);
        }

        internal void ShowWindow()
        {
            Dispatcher.Invoke(() =>
            {
                var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                if (IsVisible) return;
                Opacity = 0;
                Show();
                Activate();
                BeginAnimation(OpacityProperty, animation);
            });
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!((sender as FrameworkElement)?.DataContext is Listing l)) return;
            if (l.IsExpanded)
            {
                l.IsExpanded = false;
            }
            else
            {
                var id = l.LeaderId;
                VM._lastClicked = l;
                Proxy.RequestPartyInfo(id);
            }
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is User dc)
                Proxy.AskInteractive(SessionManager.CurrentPlayer.ServerId, dc.Name);
        }

        private void CreateMessageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!VM.Creating)
            {
                NewMessageGrid.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() });
                NewMessageTextBox.Focus();
                VM.NewMessage = VM.MyLfg != null ? VM.MyLfg.Message : "";
                VM.Creating = true;
            }
            else if (VM.Creating && !string.IsNullOrEmpty(VM.NewMessage))
            {
                NewMessageGrid.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() });
                Proxy.RegisterLfg(VM.NewMessage, RaidSwitch.IsOn);
                VM.Creating = false;
                //VM.NewMessage = "";
                VM.NewMessage = VM.MyLfg != null ? VM.MyLfg.Message : "";
                Task.Delay(200).ContinueWith(t => Proxy.RequestLfgList());
            }
            else
            {
                NewMessageGrid.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase() });
                //VM.NewMessage = "";
                VM.NewMessage = VM.MyLfg != null ? VM.MyLfg.Message : "";
                VM.Creating = false;
            }
        }

        private void PublicizeBtn_Click(object sender, RoutedEventArgs e)
        {
            PublicizeBtn.IsEnabled = false;
            PublicizeBtn.IsHitTestVisible = false;
            var an = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(3000));
            an.Completed += (s, ev) =>
            {
                PublicizeBarGovernor.LayoutTransform = new ScaleTransform(0, 1);
                PublicizeBtn.IsEnabled = true;
                PublicizeBtn.IsHitTestVisible = true;
            };
            PublicizeBarGovernor.LayoutTransform.BeginAnimation(ScaleTransform.ScaleXProperty, an);
            Proxy.PublicizeLfg();
        }

        private void RemoveMessageButton_Click(object sender, RoutedEventArgs e)
        {
            Proxy.RemoveLfg();
            Proxy.RequestLfgList();
            Proxy.RequestLfgList();
        }

        private void AcceptApply(object sender, RoutedEventArgs e)
        {
            var user = (sender as FrameworkElement).DataContext as User;
            Proxy.PartyInvite(user.Name);
        }

        private void InspectApplicant(object sender, RoutedEventArgs e)
        {
            var user = (sender as FrameworkElement).DataContext as User;
            Proxy.Inspect(user.Name);
        }

        private void RefuseApplicant(object sender, RoutedEventArgs e)
        {
            var user = (sender as FrameworkElement).DataContext as User;
            Proxy.DeclineApply(user.PlayerId);
            Proxy.RequestCandidates();
        }

        private void ReloadLfgList(object sender, RoutedEventArgs e)
        {
            Proxy.RequestLfgList();
        }

        private void OnLfgMessageMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listing = (sender as FrameworkElement).DataContext as Listing;
            if (!listing.IsTwitch) return;
            Process.Start(listing.TwitchLink);
        }
    }
}
