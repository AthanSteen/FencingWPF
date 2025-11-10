using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FencingWPF.Models;

namespace FencingWPF
{
    public partial class StartWindow : Window
    {
        private readonly List<Player> _players = new();

        public StartWindow()
        {
            InitializeComponent();

            // Preload some players
            for (int i = 0; i < 10; ++i)
            {
                _players.Add(Player.Create((99 - i).ToString(), "Player"));
            }

            lbPlayers.ItemsSource = _players;
        }

        private void btnAddPlayer_Click(object sender, RoutedEventArgs e)
        {
            var firstName = txtFirstName.Text.Trim();
            var lastName = txtLastName.Text.Trim();

            if (!ValidateName(firstName, lastName)) return;

            var newPlayer = Player.Create(firstName, lastName);

            if (_players.Any(p => p.FullName == newPlayer.FullName))
            {
                ShowError($"{newPlayer.FullName} already exists!");
                return;
            }

            _players.Add(newPlayer);
            lbPlayers.Items.Refresh();

            txtFirstName.Clear();
            txtLastName.Clear();

            HideError();
        }

        private bool ValidateName(string first, string last)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last))
            {
                ShowError("Enter your full name!");
                return false;
            }

            if (first.Length > 42 || last.Length > 42)
            {
                ShowError("Too many characters!");
                return false;
            }

            return true;
        }

        private void ShowError(string msg)
        {
            tbError.Text = msg;
            tbError.Visibility = Visibility.Visible;
        }

        private void HideError() => tbError.Visibility = Visibility.Collapsed;

        private void lbPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnRemovePlayer.IsEnabled = lbPlayers.SelectedItem != null;
        }

        private void btnRemovePlayer_Click(object sender, RoutedEventArgs e)
        {
            if (lbPlayers.SelectedItem is Player selected)
            {
                _players.Remove(selected);
                lbPlayers.Items.Refresh();
                btnRemovePlayer.IsEnabled = false;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (_players.Count < 1) return;

            var poule = new PouleWindow(_players);
            poule.Show();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnAddPlayer.IsEnabled = txtFirstName.Text.Length > 0 && txtLastName.Text.Length > 0;
        }
    }
}
