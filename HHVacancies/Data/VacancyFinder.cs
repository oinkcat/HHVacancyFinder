using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HHVacancies.Data.Parsers;
using System.Linq;

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
        // Таймаут запросов
        private const int TimeoutInSeconds = 10;

        // Максимальное число параллельных запросов
        private int MaxParallelRequests = 2;

        /// <summary>
        /// События изменения прогресса операции поиска
        /// </summary>
        public event EventHandler<FindProgressEventArgs> ProgressChanged;

        /// <summary>
        /// Возникла ошибка при поиске вакансий
        /// </summary>
        public event EventHandler<Exception> ErrorOccurred;

        /// <summary>
        /// Выдать список найденных вакансий
        /// </summary>
        public List<Vacancy> Vacancies { get; private set; }

        /// <summary>
        /// Поиск завершен успешно
        /// </summary>
        public bool CompletedSuccessfully { get; private set; }

        /// <summary>
        /// Запуск загрузки вакансий
        /// </summary>
        public async Task StartAsync(string searchQuery)
        {
            this.Vacancies = new List<Vacancy>();
            
            await Task.Factory.StartNew(() => {
                try
                {
                    FindVacancies(searchQuery);
                    CompletedSuccessfully = true;
                }
                catch(Exception e)
                {
                    ErrorOccurred?.Invoke(this, e);
                }
            });
        }

        // Найти вакансии по заданному запросу
        private void FindVacancies(string searchQuery)
        {
            VacancyParser parser = new HeadHunterParser();

            // Первая страница
            string firstPageUrl = parser.GetNextPageUrl(searchQuery);
            parser.SetCurrentPage(GetHtmlDocument(firstPageUrl));
            Vacancies.AddRange(parser.ParsePage());

            int currentPageNumber = parser.PageNumber;
            int totalPages = parser.TotalPages;

            // Остальные страницы
            Parallel.ForEach(parser.GetSearchResultsPages(searchQuery).Skip(1), 
                new ParallelOptions {  MaxDegreeOfParallelism = MaxParallelRequests },
                url => {
                    parser.SetCurrentPage(GetHtmlDocument(url));
                    var vacanciesOnPage = parser.ParsePage();
                    Vacancies.AddRange(vacanciesOnPage);

                    int numPages = Interlocked.Increment(ref currentPageNumber);
                    var progressArgs = new FindProgressEventArgs(numPages, totalPages);
                    ProgressChanged?.Invoke(this, progressArgs);
            });
        }

        // Загрузить страницу результатов поиска
        private HtmlDocument GetHtmlDocument(string url)
        {
            var pageRequest = HttpWebRequest.CreateHttp(url);
            pageRequest.Timeout = TimeoutInSeconds * 1000;
            pageRequest.ReadWriteTimeout = pageRequest.Timeout;

            var dataStream = pageRequest.GetResponse().GetResponseStream();
            using (var reader = new StreamReader(dataStream, Encoding.UTF8))
            {
                var resultsPageDocument = new HtmlDocument();
                resultsPageDocument.LoadHtml(reader.ReadToEnd());

                return resultsPageDocument;
            }
        }
    }
}
