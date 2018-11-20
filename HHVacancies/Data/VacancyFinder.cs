using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HHVacancies.Data.Parsers;

namespace HHVacancies.Data
{
    /// <summary>
    /// Аргумент события отображения прогресса операции
    /// </summary>
    internal class FindProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Значение прогресса операции
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Максимальное значение прогресса
        /// </summary>
        public int Maximum { get; set; }

        public FindProgressEventArgs(int value, int max)
        {
            this.Value = value;
            this.Maximum = max;
        }
    }
    
    /// <summary>
    /// Находит вакансии на сайте hh.ru
    /// </summary>
    internal class VacancyFinder
    {
        /// <summary>
        /// События изменения прогресса операции поиска
        /// </summary>
        public event EventHandler<FindProgressEventArgs> ProgressChanged;

        /// <summary>
        /// Выдать список найденных вакансий
        /// </summary>
        public List<Vacancy> Vacancies { get; private set; }

        /// <summary>
        /// Запуск загрузки вакансий
        /// </summary>
        public Task StartAsync(string vacancyName)
        {
            this.Vacancies = new List<Vacancy>();
            
            Task findTask = Task.Factory.StartNew(() =>
            {
                VacancyParser parser = new HeadHunterParser();
                
                do
                {
                    string url = parser.GetNextPageUrl(vacancyName);
                    HtmlWeb webUtil = new HtmlWeb { OverrideEncoding = Encoding.UTF8 };
                    HtmlDocument loadedDocument = webUtil.Load(url);

                    parser.SetCurrentPage(loadedDocument);
                    var vacanciesOnPage = parser.ParsePage();
                    Vacancies.AddRange(vacanciesOnPage);

                    int numPages = parser.PageNumber;
                    int totalPages = parser.TotalPages;
                    var progressArgs = new FindProgressEventArgs(numPages, totalPages);
                    ProgressChanged?.Invoke(this, progressArgs);
                }
                while (parser.HasMorePages);
            });

            return findTask;
        }
    }
}
