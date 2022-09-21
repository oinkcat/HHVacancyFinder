using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace HHVacancies.ViewModels
{
    /// <summary>
    /// Команда, вызывающая коллбэки
    /// </summary>
    public class DelegateCommand : ICommand
    {
        // Делегат для проверки возможности выполнения
        private Func<object, bool> canExecuteCallback;

        // Делегат выполнения действия
        private readonly Action<object> actionCallback;

        /// <summary>
        /// Возможно ли выполнить команду
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Проверить возможность выполнения команды
        /// </summary>
        /// <param name="parameter">Параметр команды</param>
        /// <returns>Возможность выполнения</returns>
        public bool CanExecute(object parameter)
        {
            bool can = canExecuteCallback(parameter);
            return can;
        }

        /// <summary>
        /// Выполнить команду
        /// </summary>
        /// <param name="parameter">Параметр команды</param>
        public void Execute(object parameter)
        {
            actionCallback(parameter);
        }

        /// <summary>
        /// Проверить возможность выполнения
        /// </summary>
        public void CheckCanExecute()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        // Реализация проверки возможности выполнения по умолчанию
        private bool EnableCheckerDefaultImpl(object arg)
        {
            return true;
        }

        public DelegateCommand(Func<object, bool> checker, Action<object> action)
        {
            this.canExecuteCallback = checker;
            this.actionCallback = action;
        }

        public DelegateCommand(Action<object> action)
        {
            this.canExecuteCallback = EnableCheckerDefaultImpl;
            this.actionCallback = action;
        }
    }
}
