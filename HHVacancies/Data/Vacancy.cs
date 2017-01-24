using System;

namespace HHVacancies.Data
{
    /// <summary>
    /// Представляет информацию о вакансии
    /// </summary>
    internal class Vacancy
    {
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Компания
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Станция метро
        /// </summary>
        public string MetroStation { get; set; }

        /// <summary>
        /// Заралата
        /// </summary>
        public int BaseSalary { get; set; }

        /// <summary>
        /// URL страницы информации на сайте
        /// </summary>
        public string Url { get; set; }
    }
}
