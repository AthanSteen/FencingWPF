using System.Collections.Generic;
using System.Linq;
using System.Windows;
using FencingWPF.Models;

namespace FencingWPF
{
    public partial class ResultsWindow : Window
    {
        public ResultsWindow(IEnumerable<PouleRow> pouleRows)
        {
            InitializeComponent();

            var sortedRows = pouleRows
                .OrderByDescending(r => r.V)
                .ThenByDescending(r => r.I)
                .ThenByDescending(r => r.TS)
                .Select((r, index) =>
                {
                    r.Rank = index + 1;
                    return r;
                }).ToList();

            icResults.ItemsSource = sortedRows;
        }

    }
}
