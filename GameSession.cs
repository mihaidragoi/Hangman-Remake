using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Model
{
    internal class GameSession : INotifyPropertyChanged
    {
        public string Username { get; set; }
        public string Category { get; set; }
        public string SecretWord { get; set; }
        public List<char> GuessedLetters { get; set; } = new List<char>();
        public int Level { get; set; }
        public int ConsecutiveWins { get; set; }

        private int mistakes;
        public int Mistakes
        {
            get => mistakes;
            set { mistakes = value; OnPropertyChanged(); }
        }

        private int timeRemaining;
        public int TimeRemaining
        {
            get => timeRemaining;
            set { timeRemaining = value; OnPropertyChanged(); }
        }

        public int CurrentLevel { get; set; }
        public ObservableCollection<char> WordProgress { get; set; } = new ObservableCollection<char>();
        public GameSession() { }

        public bool IsWordGuessed() => SecretWord.All(c => GuessedLetters.Contains(c));

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
