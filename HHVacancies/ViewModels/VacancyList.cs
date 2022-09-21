using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Win32;
using System.Diagnostics;
using HHVacancies.Data;
using HHVacancies.Exporters;
using System.Windows;
using System.Windows.Threading;

namespace HHVacancies.ViewModels
{
    /// <summary>
    /// Модель представления вкладки списка вакансий
    /// </summary>
    public class VacancyList : INotifyPropertyChanged
    {
        // Состояние пользовательского интерфейса
        private enum UIState
        {
            Ready, Searching, Error
        }

        // Запрос поиска
        private string searchQueryText;

        // Средняя заралата по текущему запросу
        private double currentAvgSalary;

        // Ищет вакансии по запросу
        private VacancyFinder finder;

        /// <summary>
        /// Объект, принимающий статистику по вакансии
        /// </summary>
        public IStatsReceiver StatsReceiver { get; set; }

        /// <summary>
        /// Команда поиска вакансий
        /// </summary>
        public DelegateCommand SearchCommand { get; set; }

        /// <summary>
        /// Команда сохранения списка найденных вакансий
        /// </summary>
        public DelegateCommand ExportCommand { get; set; }

        /// <summary>
        /// Команда открытия информации о вакансии в браузере
        /// </summary>
        public DelegateCommand OpenInBrowserCommand { get; set; }

        /// <summary>
        /// Команда добавления информации о вакансии к сравнению
        /// </summary>
        public DelegateCommand AddToComparsionCommand { get; set; }

        /// <summary>
        /// Команда остановки процесса поиска
        /// </summary>
        public DelegateCommand StopSearchCommand { get; set; }

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
        public bool SearchComplete => !Searching;

        /// <summary>
        /// Текст статуса
        /// </summary>
        public string StatusText { get; set; }

        /// <summary>
        /// Найдены какие-либо результаты
        /// </summary>
        public bool IsResultsFound => SearchComplete &&
                                      (FoundVacancies != null) &&
                                      (FoundVacancies.Count() > 0);

        /// <summary>
        /// Элементы управления активны
        /// </summary>
        public bool ControlsEnabled => !Searching;

        /// <summary>
        /// Запрос поиска
        /// </summary>
        public string SearchQuery
        {
            get => searchQueryText;
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
        private void SetUIState(UIState newState)
        {
            Searching = newState == UIState.Searching;
            NotifyChanged(nameof(Searching));
            NotifyChanged(nameof(SearchComplete));
            NotifyChanged(nameof(ControlsEnabled));
            NotifyChanged(nameof(IsResultsFound));

            SearchProgress = 0;
            NotifyChanged(nameof(SearchProgress));

            StatusText = GetStatusBarText(newState);
            NotifyChanged(nameof(StatusText));
        }

        // Выдать текст строки состояния для текущего состояния UI
        private string GetStatusBarText(UIState state)
        {
            if (SearchComplete)
            {
                if (IsResultsFound)
                {
                    currentAvgSalary = FoundVacancies.Average(item => item.BaseSalary);

                    string shortSalary = new ShortCurrencyConverter().Convert(
                        (int)currentAvgSalary,
                        typeof(string),
                        null,
                        null
                    ) as string;

                    return String.Format(
                        "Готово. Всего: {0}, средняя зарплата: {1}",
                        FoundVacancies.Count(),
                        shortSalary
                    );
                }
                else
                {
                    return (state == UIState.Error)
                        ? "Произошла ошибка. Попробуйте повторить поиск позднее" 
                        : "Готово. Ничего не найдено";
                }
            }
            else
            {
                return "Выполняется поиск...";
            }
        }

        // Установить прогресс поиска
        private void SetProgress(int value, int maxValue)
        {
            SearchProgress = maxValue > 0 ? value * 100 / maxValue : 0;
            NotifyChanged(nameof(SearchProgress));
        }

        // Обработать возникшую ошибку
        private void HandleError(object sender, Exception e)
        {
            var msgOkBtn = MessageBoxButton.OK;
            var msgIcon = MessageBoxImage.Error;

            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                MessageBox.Show(e.Message, "Ошибка", msgOkBtn, msgIcon);
            });
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
                FoundVacancies?.Clear();
                SetUIState(UIState.Searching);

                finder = new VacancyFinder();
                finder.ProgressChanged += (s, e) => SetProgress(e.Value, e.Maximum);
                finder.ErrorOccurred += HandleError;

                string encodedName = Uri.EscapeDataString(SearchQuery);
                await finder.StartAsync(encodedName);

                if(finder.CompletedSuccessfully)
                {
                    FoundVacancies = new ObservableCollection<Vacancy>(finder.Vacancies);
                }

                NotifyChanged(nameof(FoundVacancies));

                SetUIState(finder.CompletedSuccessfully ? UIState.Ready : UIState.Error);
            };

            SearchCommand = new DelegateCommand(canFindChecker, findAction);
        }

        // Настроить действия команды сохранения результатов
        private void SetupSaveCommand()
        {
            ExportCommand = new DelegateCommand(param =>
            {
                var exporter = new CsvVacanciesExporter();

                var dlg = new SaveFileDialog()
                {
                    DefaultExt = exporter.FileExtension,
                    Filter = exporter.FormatDescription
                };

                if (dlg.ShowDialog().Value)
                {
                    exporter.Export(dlg.FileName, FoundVacancies);
                }
            });
        }

        // Настроить команду открытия информации в браузере
        private void SetupOpenInBrowserCommand()
        {
            OpenInBrowserCommand = new DelegateCommand(param =>
            {
                if (param != null)
                {
                    var infoUrl = (param as Vacancy).Url;

                    // Открыть ссылку на информацию о вакансии в браузере
                    var browserStartInfo = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        Verb = "Open",
                        FileName = infoUrl
                    };
                    Process.Start(browserStartInfo);
                }
            });
        }

        // Настройка команды добавления к сравнению
        private void SetupAddToComparsionCommand()
        {
            AddToComparsionCommand = new DelegateCommand(_ =>
            {
                if (FoundVacancies.Count > 0)
                {
                    var stats = StatInfo.Compute(SearchQuery, FoundVacancies);
                    StatsReceiver.ReceiveStats(stats);
                }
            });
        }

        // Настройка команды остановки поиска
        private void SetupStopSearchCommand()
        {
            StopSearchCommand = new DelegateCommand(_ => finder.Stop());
        }

        public VacancyList()
        {
            Searching = false;
            StatusText = "Готово";

            // Установка действий для команд
            SetupFindCommand();
            SetupSaveCommand();
            SetupOpenInBrowserCommand();
            SetupAddToComparsionCommand();
            SetupStopSearchCommand();
        }
    }
}
