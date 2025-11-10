using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FencingWPF.Models;

namespace FencingWPF
{
    public partial class PouleWindow : Window
    {
        private readonly List<PlayerOption> _winnerOptions;
        private readonly List<PlayerOption> _loserOptions;

        public PouleWindow(List<Player> players)
        {
            InitializeComponent();

            _winnerOptions = players.Select((p, i) => new PlayerOption
            {
                Player = new Player
                { Id = i, 
                  FirstName = p.FirstName,
                  LastName = p.LastName },
                IsEnabled = true
            }).ToList();


            // Clone the list for losers
            _loserOptions = _winnerOptions.Select(o => new PlayerOption
            {
                Player = o.Player,
                IsEnabled = true
            }).ToList();

            cbWinner.ItemsSource = _winnerOptions;
            cbLoser.ItemsSource = _loserOptions;

            cbWinner.DisplayMemberPath = "DisplayName";
            cbLoser.DisplayMemberPath = "DisplayName";

            cbScoreWinner.ItemsSource = Enumerable.Range(0, 6).ToList();
            cbScoreLoser.ItemsSource = Enumerable.Range(0, 6).ToList();

            BuildPouleGrid();
        }

        private void ComboBoxPlayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var winner = cbWinner.SelectedItem as PlayerOption;
            var loser = cbLoser.SelectedItem as PlayerOption;

            // Reset all items to enabled
            foreach (var opt in _winnerOptions) opt.IsEnabled = true;
            foreach (var opt in _loserOptions) opt.IsEnabled = true;

            // Disable the selected player in the other ComboBox
            if (winner != null)
                _loserOptions.First(x => x.Player == winner.Player).IsEnabled = false;

            if (loser != null)
                _winnerOptions.First(x => x.Player == loser.Player).IsEnabled = false;

            // Refresh ComboBoxes
            cbWinner.Items.Refresh();
            cbLoser.Items.Refresh();
        }

        private void BuildPouleGrid()
        {
            dgPoule.Columns.Clear();

            // Create columns for each opponent
            for (int i = 0; i < _winnerOptions.Count; i++)
            {
                var player = _winnerOptions[i];
                int colIndex = i; // capture for lambda

                var column = new DataGridTextColumn
                {
                    Header = player.Player.Id, // just ID
                    Binding = new System.Windows.Data.Binding($"Scores[{player.Player.Id}]"),
                    Width = 50,
                    IsReadOnly = false
                };

                // Create style for the cells
                var style = new Style(typeof(DataGridCell));

                // Default: editable cells, white background
                style.Setters.Add(new Setter(DataGridCell.BackgroundProperty, System.Windows.Media.Brushes.White));
                style.Setters.Add(new Setter(DataGridCell.IsEnabledProperty, true));
                style.Setters.Add(new Setter(DataGridCell.FocusableProperty, true));

                // Disabled self-cell (diagonal) style
                var trigger = new DataTrigger
                {
                    Binding = new System.Windows.Data.Binding("Player.Id"),
                    Value = player.Player.Id
                };
                trigger.Setters.Add(new Setter(DataGridCell.BackgroundProperty, System.Windows.Media.Brushes.LightGray));
                trigger.Setters.Add(new Setter(DataGridCell.IsEnabledProperty, false));
                trigger.Setters.Add(new Setter(DataGridCell.FocusableProperty, false));

                style.Triggers.Add(trigger);

                // Assign the style to the column
                column.CellStyle = style;


                dgPoule.Columns.Add(column);
            }


            // Stat columns
            var statColumns = new (string Header, string Binding)[]
            {
                ("V", "V"),
                ("V/M", "VM"),
                ("TS", "TS"),
                ("TR", "TR"),
                ("I", "I")
            }; 
            foreach (var (header, binding) in statColumns)
            {
                dgPoule.Columns.Add(new DataGridTextColumn
                {
                    Header = header,
                    Binding = new System.Windows.Data.Binding(binding),
                    Width = 60,
                    IsReadOnly = true
                });
            }

            // Row source
            var rows = _winnerOptions.Select(p => new PouleRow
            {
                Player = p.Player,
                Scores = new ObservableCollection<int?>(new int?[_winnerOptions.Count])
            }).ToList();


            dgPoule.ItemsSource = rows;

            dgPoule.CellEditEnding += (s, e) =>
            {
                var row = e.Row.Item as PouleRow;
                int colIndex = e.Column.DisplayIndex;

                if (row.Player.Id == colIndex) return; // skip self-cell

                var textBox = e.EditingElement as TextBox;
                if (!int.TryParse(textBox.Text, out int newValue))
                {
                    row.Scores[colIndex] = null;
                    return;
                }

                row.Scores[colIndex] = newValue;

                // Check if opposite cell has a value
                var rows = dgPoule.ItemsSource.Cast<PouleRow>().ToList();
                var oppositeRow = rows[colIndex];

                if (oppositeRow.Scores[row.Player.Id].HasValue)
                {
                    int oppositeValue = oppositeRow.Scores[row.Player.Id].Value;
                    if (newValue == oppositeValue)
                    {
                        MessageBox.Show("Draws are not allowed!", "Invalid Score", MessageBoxButton.OK, MessageBoxImage.Warning);
                        row.Scores[colIndex] = null; // reset
                        return;
                    }
                }

                // Do not auto-set opposite cell to 0 — leave it blank until user enters
                RecalculateStats();
            };
        }


        private void ComboBoxScore_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbScoreWinner.SelectedItem == null || cbScoreLoser.SelectedItem == null)
                return;

            if ((int)cbScoreWinner.SelectedItem == (int)cbScoreLoser.SelectedItem)
            {
                MessageBox.Show("Scores cannot be the same — no draws allowed.", "Invalid Score", MessageBoxButton.OK, MessageBoxImage.Warning);
                ((ComboBox)sender).SelectedItem = null;
            }
        }

        private void btnAddMatch_Click(object sender, RoutedEventArgs e)
        {
            if (cbWinner.SelectedItem is not PlayerOption winner ||
                cbLoser.SelectedItem is not PlayerOption loser ||
                cbScoreWinner.SelectedItem is not int sw ||
                cbScoreLoser.SelectedItem is not int sl)
            {
                MessageBox.Show("Select both players and their scores first.", "Incomplete Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (sw == sl)
            {
                MessageBox.Show("No ties allowed!", "Invalid Match", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var rows = dgPoule.ItemsSource.Cast<PouleRow>().ToList();
            var rowWinner = rows.First(r => r.Player == winner.Player);
            var rowLoser = rows.First(r => r.Player == loser.Player);

            rowWinner.Scores[loser.Player.Id] = sw;
            rowLoser.Scores[winner.Player.Id] = sl;

            RecalculateStats();

            //dgPoule.Items.Refresh();

            // reset inputs
            cbWinner.SelectedItem = null;
            cbLoser.SelectedItem = null;
            cbScoreWinner.SelectedItem = null;
            cbScoreLoser.SelectedItem = null;
        }

        private void RecalculateStats()
        {
            var rows = dgPoule.ItemsSource.Cast<PouleRow>().ToList();

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                int wins = 0;
                int touchesScored = 0;
                int touchesReceived = 0;

                for (int j = 0; j < rows.Count; j++)
                {
                    if (i == j) continue; // skip self

                    var opponentRow = rows[j];

                    // If current score exists but opponent is null, set opponent score = 0
                    if (row.Scores[j].HasValue && !opponentRow.Scores[i].HasValue)
                    {
                        opponentRow.Scores[i] = 0;
                    }

                    var score = row.Scores[j] ?? 0;
                    var opponentScore = opponentRow.Scores[i] ?? 0;

                    touchesScored += score;
                    touchesReceived += opponentScore;

                    if (score > opponentScore)
                        wins++;
                }

                row.V = wins;
                row.TS = touchesScored;
                row.TR = touchesReceived;
                row.VM = rows.Count > 1 ? (double)wins / (rows.Count - 1) : 0;
                row.VM = Math.Round(row.VM, 3);
                row.I = row.TR == 0 ? row.TS : row.TS - row.TR;
            }
        }
    }
}
