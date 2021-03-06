﻿using System.Windows;
using System.Windows.Input;

namespace TCC.Windows
{
    /// <summary>
    /// Logica di interazione per HPbar.xaml
    /// </summary>
    public partial class CharacterWindow 
    {
        public CharacterWindow()
        {
            InitializeComponent();
            ButtonsRef = Buttons;
            MainContent = content;
            Init(SettingsManager.CharacterWindowSettings, ignoreSize: true);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //rootGrid.DataContext = CharacterWindowViewModel.Instance.Player;
        }



        private void Window_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

    }
}

