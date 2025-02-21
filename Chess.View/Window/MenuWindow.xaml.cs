namespace Chess.View.Window
{
    using Chess.Model.Data;
    using Chess.Model.Rule;
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public partial class MenuWindow : Window
    {
        private string _selectedGameMode;
        private Button _lastSelectedButton;

        public string SelectedGameMode
        {
            get { return _selectedGameMode; }
            set
            {
                _selectedGameMode = value;
            }
        }

        public MenuWindow()
        {
            InitializeComponent();

            // Set default selected mode to Normal Chess and highlight button
            NormalChessClick(NormalChessButton, null); // Pass actual button to simulate click
        }

        private void NormalChessClick(object sender, RoutedEventArgs e)
        {
            // Set game mode to Normal Chess and rulebook
            SelectedGameMode = "NormalChess";
            SetSelectedButton(sender as Button);
            GameSettings.Instance.Rulebook = new StandardRulebook();
        }

        private void Chess960Click(object sender, RoutedEventArgs e)
        {
            // Set game mode to Chess960 and rulebook
            SelectedGameMode = "Chess960";
            SetSelectedButton(sender as Button);
            GameSettings.Instance.Rulebook = new FischerRulebook();
        }

        private void SetSelectedButton(Button selectedButton)
        {
            if (_lastSelectedButton != null)
            {
                _lastSelectedButton.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            }

            selectedButton.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100));

            _lastSelectedButton = selectedButton;
        }

        private void StartGameClick(object sender, RoutedEventArgs e)
        {
            // Pass the selected rulebook to MainWindow or ChessGameVM
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Event handler that closes the window.
        /// </summary>
        private void ExitClick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}