using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FencingWPF.Models
{
    public class Player
    {
        public int Id {  get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public string DisplayName => $"{Id+1}. {FullName}";

        public static Player Create(string firstName, string lastName)
        {
            return new Player { FirstName = firstName, LastName = lastName };
        }

        public override string ToString() => FullName;
    }

    public class PlayerOption
    {
        public Player Player { get; set; }
        public bool IsEnabled { get; set; } = true;

        public string DisplayName => Player.DisplayName;
    }

    public class PouleRow : INotifyPropertyChanged
    {
        public Player Player { get; set; }

        private ObservableCollection<int?> _scores;
        public ObservableCollection<int?> Scores
        {
            get => _scores;
            set { _scores = value; OnPropertyChanged(nameof(Scores)); }
        }

        private int _v;
        public int V { get => _v; set { _v = value; OnPropertyChanged(nameof(V)); } }

        private double _vm;
        public double VM { get => _vm; set { _vm = value; OnPropertyChanged(nameof(VM)); } }

        private int _ts;
        public int TS { get => _ts; set { _ts = value; OnPropertyChanged(nameof(TS)); } }

        private int _tr;
        public int TR { get => _tr; set { _tr = value; OnPropertyChanged(nameof(TR)); } }

        private int _i;
        public int I { get => _i; set { _i = value; OnPropertyChanged(nameof(I)); } }

        public string DisplayName => Player.DisplayName;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
