using System;
using System.ComponentModel;

namespace HHVacancies.Data
{
    /// <summary>
    /// Статистические показатели по вакансии
    /// </summary>
    public class StatInfo : IComparable, INotifyPropertyChanged
    {
        private double percentValue;

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
        /// Максимальное значение зарплаты
        /// </summary>
        public decimal Maximum { get; set; }

        /// <summary>
        /// Процент от максимальной
        /// </summary>
        public double Percent
        {
            get { return percentValue; }
            set
            {
                percentValue = value;
                var eventArgs = new PropertyChangedEventArgs(nameof(Percent));
                PropertyChanged?.Invoke(this, eventArgs);
            }
        }

        /// <summary>
        /// Сравнить со средней зарплатой по другой вакансии
        /// </summary>
        /// <param name="obj">Информация по сравниваемой вакансии</param>
        /// <returns>Результат сравнения зарплат</returns>
        public int CompareTo(object obj)
        {
            return Average.CompareTo((obj as StatInfo).Average);
        }

        public StatInfo(string title)
        {
            Title = title.Trim();
        }
    }
}
