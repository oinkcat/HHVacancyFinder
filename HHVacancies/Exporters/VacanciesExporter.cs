using System;
using System.Collections.Generic;
using HHVacancies.Data;

namespace HHVacancies.Exporters
{
    /// <summary>
    /// Базовый класс экспорта списка найденных вакансий
    /// </summary>
    internal abstract class VacanciesExporter
    {
        /// <summary>
        /// Расширение файла для экспорта
        /// </summary>
        public abstract string FileExtension { get; }

        /// <summary>
        /// Описание формата файла
        /// </summary>
        public abstract string FormatDescription { get; }

        /// <summary>
        /// Произвести экспорт списка вакансий
        /// </summary>
        /// <param name="fileName">Имя файла для экспорта</param>
        /// <param name="vacancies">Список вакансий</param>
        public abstract void Export(string fileName, IList<Vacancy> vacancies);
    }
}
