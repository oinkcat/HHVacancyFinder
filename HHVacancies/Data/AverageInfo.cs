using System;
using System.ComponentModel;

namespace HHVacancies.Data
{
    /// <summary>
    /// Средние показатели по вакансии
    /// </summary>
    public class AverageInfo : IComparable, INotifyPropertyChanged
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
        /// Средняя зарплата
        /// </summary>
        public double Salary { get; set; }

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
            return Salary.CompareTo((obj as AverageInfo).Salary);
        }

        public AverageInfo(string title, double avgSalary)
        {
            Title = title;
            Salary = avgSalary;
        }
    }
}
