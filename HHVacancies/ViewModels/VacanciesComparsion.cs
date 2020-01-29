using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Win32;
using HHVacancies.Data;
using HHVacancies.Exporters;

namespace HHVacancies.ViewModels
{
    /// <summary>
    /// Модель представления вкладки сравнения вакансий
    /// </summary>
    public class VacanciesComparsion : IStatsReceiver, INotifyPropertyChanged
    {
        /// <summary>
        /// Статистическая информация
        /// </summary>
        public ObservableCollection<StatInfo> Stats { get; set; }

        /// <summary>
        /// Возможно ли выполнить экспорт списка сравнения
        /// </summary>
        public bool CanExport { get; set; }

        /// <summary>
        /// Команда экспорта списка сравниваемых вакансий
        /// </summary>
        public DelegateCommand ExportCommand { get; set; }

        /// <summary>
        /// Свойство модели было изменено
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Обработать статистическую информацию
        /// </summary>
        /// <param name="stats">Статистика по вакансии</param>
        public void ReceiveStats(StatInfo stats)
        {
            if(!Stats.Any(si => si.Title.Equals(stats.Title)))
            {
                Stats.Add(stats);
            }

            CanExport = true;
            var propChangeArgs = new PropertyChangedEventArgs(nameof(CanExport));
            PropertyChanged?.Invoke(this, propChangeArgs);
        }

        // Выполнить экспорт списка сравнения
        private void ExportList(object dummy)
        {
            var exporter = new CsvStatsExporter();

            var saveDlg = new SaveFileDialog()
            {
                DefaultExt = exporter.FileExtension,
                Filter = exporter.FormatDescription
            };

            if (saveDlg.ShowDialog().Value)
            {
                exporter.Export(saveDlg.FileName, Stats);
            }
        }

        public VacanciesComparsion()
        {
            Stats = new ObservableCollection<StatInfo>();
            ExportCommand = new DelegateCommand(ExportList);
        }
    }
}
