using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using HHVacancies.Data;

namespace HHVacancies.ViewModels
{
    /// <summary>
    /// Модель представления вкладки сравнения вакансий
    /// </summary>
    public class VacanciesComparsion : IStatsReceiver
    {
        /// <summary>
        /// Статистическая информация
        /// </summary>
        public ObservableCollection<StatInfo> Stats { get; set; }

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
        }

        public VacanciesComparsion()
        {
            Stats = new ObservableCollection<StatInfo>();
        }
    }
}
