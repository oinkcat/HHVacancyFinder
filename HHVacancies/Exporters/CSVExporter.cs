using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HHVacancies.Data;

namespace HHVacancies.Exporters
{
    /// <summary>
    /// Выполняет экспорт списка найденных вакансий в формат CSV
    /// </summary>
    internal class CSVExporter : VacanciesExporter
    {
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
        /// <param name="fileName">Имя файла для жкспорта</param>
        /// <param name="vacancies">Список найденных вакансий</param>
        public override void Export(string fileName, IList<Vacancy> vacancies)
        {
            using (var writer = new StreamWriter(fileName))
            {
                byte[] bom = Encoding.UTF8.GetPreamble();
                writer.BaseStream.Write(bom, 0, bom.Length);
                writer.WriteLine("Должность;Организация;Станция метро;Запрлата");
                foreach (Vacancy item in vacancies)
                {
                    writer.WriteLine("{0};{1};{2};{3}",
                        item.Name, item.Company, item.MetroStation,
                        item.BaseSalary);
                }
            }
        }
    }
}
