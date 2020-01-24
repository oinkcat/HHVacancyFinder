using System;
using HHVacancies.Data;

namespace HHVacancies.ViewModels
{
    /// <summary>
    /// Принимает статистические данные о вакансии
    /// </summary>
    public interface IStatsReceiver
    {
        /// <summary>
        /// Получить статистику по вакансии
        /// </summary>
        /// <param name="stats">Статистическая информация</param>
        void ReceiveStats(StatInfo stats);
    }
}
