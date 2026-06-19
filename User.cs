using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Model
{
    public class User
    {
        private string name;
        private string imagePath;

        public User(string name, string imagePath)
        {
            this.name = name;
            this.imagePath = imagePath;
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string ImagePath
        {
            get { return imagePath; }
            set { imagePath = value; }
        }
    }
}
