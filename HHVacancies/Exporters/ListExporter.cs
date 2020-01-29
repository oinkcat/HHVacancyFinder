using System;
using System.Collections.Generic;

namespace HHVacancies.Exporters
{
    /// <summary>
    /// Базовый класс экспорта списка элементов
    /// </summary>
    /// <typeparam name="T">Тип элементов списка</typeparam>
    internal abstract class ListExporter<T>
    {
        /// <summary>
        /// Элементы данных списка
        /// </summary>
        protected abstract IList<string> Columns { get; }

        /// <summary>
        /// Расширение файла для экспорта
        /// </summary>
        public abstract string FileExtension { get; }

        /// <summary>
        /// Описание формата файла
        /// </summary>
        public abstract string FormatDescription { get; }

        /// <summary>
        /// Произвести экспорт списка элементов
        /// </summary>
        /// <param name="fileName">Имя файла для экспорта</param>
        /// <param name="itemsToExport">Список элементов для экспорта</param>
        public abstract void Export(string fileName, IList<T> itemsToExport);
    }
}
