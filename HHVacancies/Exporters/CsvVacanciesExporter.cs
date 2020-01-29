using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HHVacancies.Data;

namespace HHVacancies.Exporters
{
    /// <summary>
    /// Выполняет экспорт списка найденных вакансий в формат CSV
    /// </summary>
    internal class CsvVacanciesExporter : ListExporter<Vacancy>
    {
        /// <summary>
        /// Столбцы данных таблицы
        /// </summary>
        protected override IList<string> Columns => new string[] {
            "Должность", "Организация", "Станция метро", "Запрлата"
        };

        /// <summary>
        /// Расширение экспортируемого файла
        /// </summary>
        public override string FileExtension => ".csv";

        /// <summary>
        /// Описание формата файла
        /// </summary>
        public override string FormatDescription => "CSV-файлы|*.csv";

        /// <summary>
        /// Выполнить экспорт данных в файл
        /// </summary>
        /// <param name="fileName">Имя файла для экспорта</param>
        /// <param name="vacancies">Список найденных вакансий</param>
        public override void Export(string fileName, IList<Vacancy> vacancies)
        {
            using (var writer = new StreamWriter(fileName))
            {
                byte[] bom = Encoding.UTF8.GetPreamble();
                writer.BaseStream.Write(bom, 0, bom.Length);

                writer.WriteLine(String.Join(";", Columns));

                foreach (Vacancy item in vacancies)
                {
                    object[] row = {
                        item.Name, item.Company, item.MetroStation, item.BaseSalary
                    };
                    writer.WriteLine(String.Join(";", row));
                }
            }
        }
    }
}
