using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Model
{
    public class LetterItem : INotifyPropertyChanged
    {
        public char Character { get; set; }
        private bool isAvailable = true;
        public bool IsAvailable
        {
            get { return isAvailable; }
            set
            {
                if (isAvailable != value)
                {
                    isAvailable = value;
                    OnPropertyChanged(nameof(IsAvailable));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
