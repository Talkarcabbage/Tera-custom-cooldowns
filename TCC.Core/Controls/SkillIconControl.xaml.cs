﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TCC.Data;
using TCC.ViewModels;

namespace TCC.Controls
{
    public partial class SkillIconControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        private DispatcherTimer NumberTimer;
        private DispatcherTimer CloseTimer;
        private int ending = SkillManager.Ending;

        private SkillCooldown _context;

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string p)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        }

        public string CurrentCD => _context == null? "" : Utils.TimeFormatter(Convert.ToUInt32((
            _context.Cooldown < _secondsPassed? 0 : _context.Cooldown - _secondsPassed) / 1000));

        public SkillIconControl()
        {
            InitializeComponent();           
        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            if(DesignerProperties.GetIsInDesignMode(this)) return;
            _context = (SkillCooldown)DataContext;
            _context.PropertyChanged += _context_PropertyChanged;

            //LayoutTransform = new ScaleTransform(.9, .9, .5, .5);

            //CurrentCD = (double)_context.Cooldown / 1000;
            NotifyPropertyChanged(nameof(CurrentCD));

            NumberTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(1000) };
            CloseTimer = new  DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(ending) };

            CloseTimer.Tick += CloseTimer_Tick;
            NumberTimer.Tick += (s, o) =>
            {
                _secondsPassed+=1000;
                NotifyPropertyChanged(nameof(CurrentCD));
            };
            AnimateCooldown();
        }

        private ulong _secondsPassed = 0;
        private void _context_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Refresh")
            {
                if (_context.Cooldown == _context.OriginalCooldown) return;
                NumberTimer.Stop();
                NumberTimer.IsEnabled = true;
                _secondsPassed = 0;
                //CurrentCD = (double)_context.Cooldown / 1000;
                NotifyPropertyChanged(nameof(CurrentCD));
                var newAngle = (double)_context.Cooldown / (double)_context.OriginalCooldown;
                if (_context.Cooldown == 0) newAngle = 0;
                if (newAngle > 1) newAngle = 1;

                AnimateCooldown(newAngle);
            }
            else if(e.PropertyName == "Ending")
            {
                var w = new DoubleAnimation(0, TimeSpan.FromMilliseconds(ending))
                {
                    EasingFunction = new QuadraticEase()
                };
                var h = new DoubleAnimation(0, TimeSpan.FromMilliseconds(ending))
                {
                    EasingFunction = new QuadraticEase()
                };
                //_context.Dispatcher.Invoke(() =>
                //{
                //    this.LayoutTransform.BeginAnimation(ScaleTransform.ScaleXProperty, w);
                //    this.LayoutTransform.BeginAnimation(ScaleTransform.ScaleYProperty, h);
                //});
                CloseTimer.IsEnabled = true;
            }
        }

        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            Dispose();
            //if (SettingsManager.ClassWindowOn && ViewModels.ClassWindowViewModel.ClassWindowExists())
            //{
            //    WindowManager.ClassWindow.Dispatcher.Invoke(() =>
            //    {
            //        ((ClassWindowViewModel)WindowManager.ClassWindow.DataContext).RemoveSkill(_context.Skill);
            //    });
            //}
            //else
            //{
            //    CooldownWindowManager.Instance.NormalCd_RemoveSkill(_context.Skill);
            //}
            CooldownWindowViewModel.Instance.Remove(_context.Skill);

        }

        private void AnimateCooldown(double angle = 1)
        {
            var an = new DoubleAnimation(angle*359.9, 0, TimeSpan.FromMilliseconds(_context.Cooldown));
            var fps = _context.Cooldown > 80000 ? 1 : 30;
            Timeline.SetDesiredFrameRate(an, fps);
            if(_context.Pre) PreArc.BeginAnimation(Arc.EndAngleProperty, an);
            else
            {
                PreArc.BeginAnimation(Arc.EndAngleProperty, null);
                PreArc.EndAngle = 0.01;
                Arc.BeginAnimation(Arc.EndAngleProperty, an);
            }
            NumberTimer.IsEnabled = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        public void Dispose()
        {
            NumberTimer.Stop();
            CloseTimer.Stop();
        }

        private void SkillIconControl_OnToolTipOpening(object sender, ToolTipEventArgs e)
        {
            FocusManager.FocusTimer.Enabled = false;
        }

        private void SkillIconControl_OnToolTipClosing(object sender, ToolTipEventArgs e)
        {
            FocusManager.FocusTimer.Enabled = true;
        }

        private void HideButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CooldownWindowViewModel.Instance.AddHiddenSkill(_context);
            CloseTimer_Tick(null,null);
        }

        private void Rectangle_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HideButton.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HideButton.Visibility = Visibility.Collapsed;
        }
    }
}

