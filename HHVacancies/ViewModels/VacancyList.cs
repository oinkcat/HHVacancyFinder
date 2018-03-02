using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using HHVacancies.Data;

namespace HHVacancies.ViewModels
{
    /// <summary>
    /// Модель представления вкладки списка вакансий
    /// </summary>
    public class VacancyList : INotifyPropertyChanged
    {
        // Запрос поиска
        private string searchQueryText;

        // Средняя заралата по текущему запросу
        private double currentAvgSalary;

        /// <summary>
        /// Команда поиска вакансий
        /// </summary>
        public DelegateCommand SearchCommand { get; set; }

        /// <summary>
        /// Свойство модели представления изменено
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Показатель прогресса поиска
        /// </summary>
        public int SearchProgress { get; set; }

        /// <summary>
        /// Найденные вакансии
        /// </summary>
        public ObservableCollection<Vacancy> FoundVacancies { get; set; }

        /// <summary>
        /// Выполняется поиск
        /// </summary>
        public bool Searching { get; set; }

        /// <summary>
        /// Поиск завершен
        /// </summary>
        public bool SearchComplete { get; set; }

        /// <summary>
        /// Текст статуса
        /// </summary>
        public string StatusText { get; set; }

        /// <summary>
        /// Элементы управления активны
        /// </summary>
        public bool ControlsEnabled
        {
            get { return !Searching; }
        }

        /// <summary>
        /// Запрос поиска
        /// </summary>
        public string SearchQuery
        {
            get { return searchQueryText; }
            set
            {
                searchQueryText = value;
                SearchCommand.CheckCanExecute();
            }
        }

        /// <summary>
        /// Оповестить об изменении свойства
        /// </summary>
        /// <param name="propertyName">Имя свойства</param>
        public void NotifyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Установить состояние интерфейса
        private void SetUIState(bool searching)
        {
            Searching = searching;
            NotifyChanged(nameof(Searching));
            SearchComplete = !searching;
            NotifyChanged(nameof(SearchComplete));
            NotifyChanged(nameof(ControlsEnabled));
            SearchProgress = 0;
            NotifyChanged(nameof(SearchProgress));

            if(SearchComplete && (FoundVacancies != null))
            {
                if(FoundVacancies.Count > 0)
                {
                    currentAvgSalary = FoundVacancies.Average(item => item.BaseSalary);
                    StatusText = String.Format(
                        "Готово. Всего: {0}, средняя зарплата: {1:C}",
                        FoundVacancies.Count(), Math.Round(currentAvgSalary, 2));
                }
                else
                {
                    StatusText = "Готово. Ничего не найдено";
                }
            }
            else
            {
                StatusText = "Выполняется поиск...";
            }
            NotifyChanged(nameof(StatusText));
        }

        // Установить прогресс поиска
        private void SetProgress(int value, int maxValue)
        {
            SearchProgress = maxValue > 0 ? value * 100 / maxValue : 0;
            NotifyChanged(nameof(SearchProgress));
        }

        // Настроить действия команды поиска
        private void SetupFindCommand()
        {
            Func<object, bool> canFindChecker = param =>
            {
                return SearchQuery?.Length > 0;
            };

            Action<object> findAction = async param =>
            {
                SetUIState(true);

                var finder = new VacancyFinder();
                finder.ProgressChanged += (s, e) => SetProgress(e.Value, e.Maximum);
                
                string encodedName = Uri.EscapeDataString(SearchQuery);
                await finder.StartAsync(encodedName);

                FoundVacancies = new ObservableCollection<Vacancy>(finder.Vacancies);
                NotifyChanged(nameof(FoundVacancies));
                SetUIState(false);
            };

            SearchCommand = new DelegateCommand(canFindChecker, findAction);
        }

        public VacancyList()
        {
            Searching = false;
            StatusText = "Готово";
            SetupFindCommand();
        }
    }
}
