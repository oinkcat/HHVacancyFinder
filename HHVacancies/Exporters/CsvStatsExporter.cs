using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HHVacancies.Data;

namespace HHVacancies.Exporters
{
    /// <summary>
    /// Выполняет экспорт списка статистических показателей
    /// </summary>
    internal class CsvStatsExporter : ListExporter<StatInfo>
    {
        /// <summary>
        /// Столбцы данных таблицы
        /// </summary>
        protected override IList<string> Columns => new string[] {
            "Поисковый запрос", "Найдено вакансий",
            "Мин. з/п", "Средняя з/п", "90 P", "Макс. з/п"
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
        /// <param name="stats">Список статистических данных</param>
        public override void Export(string fileName, IList<StatInfo> stats)
        {
            using (var writer = new StreamWriter(fileName))
            {
                var bom = Encoding.UTF8.GetPreamble();
                writer.BaseStream.Write(bom, 0, bom.Length);

                writer.WriteLine(String.Join(";", Columns));

                foreach (var item in stats)
                {
                    object[] row = {
                        item.Title, item.Count,
                        item.Minimum, item.Average, item.Percentile90, item.Maximum
                    };
                    writer.WriteLine(String.Join(";", row));
                }
            }
        }
    }
}
