using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Model
{
    class GameRecord
    {
        public string PlayerName { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public string Result { get; set; } 
        public int LevelReached { get; set; }
    }
}
