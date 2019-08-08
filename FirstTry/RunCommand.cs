using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstTry
{
    class RunCommand
    {
        private readonly Predicate<object> canExecute;

        /// <summary>  
        /// Action to be executed when the command is invoked  
        /// </summary>  
        private readonly Action<object> execute;

        /// <summary>  
        /// Creates a new command  
        /// </summary>  
        /// <param name="execute"></param>  
        /// <param name="canExecute"></param>  
        public RunCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }
}
