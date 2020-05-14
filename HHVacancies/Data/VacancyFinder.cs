using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
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

            do
            {
                string url = parser.GetNextPageUrl(searchQuery);
                var loadedDocument = GetHtmlDocument(url);

                parser.SetCurrentPage(loadedDocument);
                var vacanciesOnPage = parser.ParsePage();
                Vacancies.AddRange(vacanciesOnPage);

                int numPages = parser.PageNumber;
                int totalPages = parser.TotalPages;
                var progressArgs = new FindProgressEventArgs(numPages, totalPages);
                ProgressChanged?.Invoke(this, progressArgs);
            }
            while (parser.HasMorePages);
        }

        // Загрузить страницу результатов поиска
        private HtmlDocument GetHtmlDocument(string url)
        {
            const int TimeoutInSeconds = 5;

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
