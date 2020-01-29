using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace HHVacancies.Data
{
    /// <summary>
    /// Статистические показатели по вакансии
    /// </summary>
    public class StatInfo : IComparable, INotifyPropertyChanged
    {
        /// <summary>
        /// Свойство было изменено
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Наименование вакансии
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Число найденных вакансий
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Минимальное значение зарплаты
        /// </summary>
        public decimal Minimum { get; set; }

        /// <summary>
        /// Средняя зарплата
        /// </summary>
        public decimal Average { get; set; }

        /// <summary>
        /// 90 перцентиль
        /// </summary>
        public decimal Percentile90 { get; set; }

        /// <summary>
        /// Максимальное значение зарплаты
        /// </summary>
        public decimal Maximum { get; set; }

        /// <summary>
        /// Сравнить со средней зарплатой по другой вакансии
        /// </summary>
        /// <param name="obj">Информация по сравниваемой вакансии</param>
        /// <returns>Результат сравнения зарплат</returns>
        public int CompareTo(object obj)
        {
            return Average.CompareTo((obj as StatInfo).Average);
        }

        /// <summary>
        /// Вычислить статистические показатели
        /// </summary>
        /// <param name="title">Наименование</param>
        /// <param name="vacancies">Список вакансий</param>
        /// <returns>Статистические показатели по результату запроса</returns>
        public static StatInfo Compute(string title, IList<Vacancy> vacancies)
        {
            const int Percentile = 90;

            var sorted = vacancies.OrderBy(v => v.BaseSalary);
            int count90pct = sorted.Count() * Percentile / 100;

            return new StatInfo(title)
            {
                Count = sorted.Count(),
                Minimum = sorted.Min(v => v.BaseSalary),
                Average = (decimal)sorted.Average(v => v.BaseSalary),
                Percentile90 = sorted.SkipWhile((_, i) => i < count90pct - 1)
                                     .First().BaseSalary,
                Maximum = sorted.Max(v => v.BaseSalary)
            };
        }

        public StatInfo(string title)
        {
            Title = title.Trim();
        }
    }
}
