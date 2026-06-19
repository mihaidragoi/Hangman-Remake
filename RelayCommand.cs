using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hangman.ViewModel
{
    class RelayCommand : ICommand
    {
        private readonly Action<object> workToDo;
        private readonly Predicate<object> canExecute;

        public RelayCommand(Action<object> workToDo, Predicate<object> canExecute = null)
        {
            this.workToDo = workToDo;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            workToDo(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

    }
}
