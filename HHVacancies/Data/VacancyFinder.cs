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

        // Минимальное число параллельных запросов
        private const int MinParallelRequests = 2;

        private readonly VacancyParser parser;

        private readonly CancellationTokenSource stopSource;

        private int currentPageNumber;

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
                    FindVacancies(Uri.EscapeDataString(searchQuery), stopSource.Token);
                    CompletedSuccessfully = true;
                }
                catch(Exception e)
                {
                    ErrorOccurred?.Invoke(this, e);
                }
            });
        }

        /// <summary>
        /// Остановить поиск
        /// </summary>
        public void Stop()
        {
            stopSource.Cancel();
        }

        // Найти вакансии по заданному запросу
        private void FindVacancies(string searchQuery, CancellationToken stopToken)
        {
            // Первая страница
            string firstPageUrl = parser.GetResultsPageUrl(searchQuery);
            LoadAndParseVacancies(firstPageUrl);

            int numParallelTasks = Math.Max(Environment.ProcessorCount / 2, MinParallelRequests);

            // Остальные страницы
            Parallel.ForEach (
                parser.GetSearchResultsPages(searchQuery).Skip(1), 
                new ParallelOptions {  MaxDegreeOfParallelism = numParallelTasks },
                (string pageUrl) =>
                {
                    if(!stopToken.IsCancellationRequested)
                    {
                        LoadAndParseVacancies(pageUrl);
                    }
                }
            );
        }

        // Загрузить страницу результатов поиска
        private HtmlDocument GetHtmlDocument(string url)
        {
            var pageRequest = WebRequest.CreateHttp(url);
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

        // Загрузить страницу результатов поиска и парсить вакансии на ней
        private void LoadAndParseVacancies(string resultsUrl)
        {
            // Найти вакансии на странице
            var vacanciesOnPage = parser.ParsePage(GetHtmlDocument(resultsUrl));
            Vacancies.AddRange(vacanciesOnPage);

            // Выдать прогресс поиска
            int pageNum = Interlocked.Increment(ref currentPageNumber);
            int totalPages = parser.TotalResultsPages;
            var progressArgs = new FindProgressEventArgs(pageNum, totalPages);
            ProgressChanged?.Invoke(this, progressArgs);
        }

        public VacancyFinder()
        {
            parser = new HeadHunterParser();
            stopSource = new CancellationTokenSource();
        }
    }
}
