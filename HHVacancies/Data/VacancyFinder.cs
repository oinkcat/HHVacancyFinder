using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

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
        // Сколько вакансий на странице
        private const int ItemsPerPage = 20;

        // Адрес сайта
        private const string URL_BASE = "http://hh.ru/search/vacancy";

        // Валюта
        private const string DesiredCurrency = "RUR";

        // Пути XPath к нужным элементам страницы
        private const string TotalElem = "//*/div[@data-qa='vacancy-serp__found']";
        private const string ItemElem = "//*/div[starts-with(@class, " +
                                        "'search-result-description__item')]";

        // Найденные в настоящее время какансии
        private List<Vacancy> foundVacancies;

        // Число страниц с найденными вакансиями
        private int pagesCount;

        /// <summary>
        /// События изменения прогресса операции поиска
        /// </summary>
        public event EventHandler<FindProgressEventArgs> ProgressChanged;

        /// <summary>
        /// Выдать список найденных вакансий
        /// </summary>
        public IEnumerable<Vacancy> Vacancies
        {
            get { return this.foundVacancies; }
        }

        // Выдать URL поиска
        private string buildUrl(string query, int page)
        {
            string templ = "{0}?area=1&text={1}&currency_code=RUR&page={2}";
            return String.Format(templ, URL_BASE, query, page);
        }

        // Разобрать информацию о вакансиях на странице
        // TODO: Парсинг на основе конфигурации
        private void parseVacanciesInfo(HtmlDocument doc)
        {
            HtmlNode root = doc.DocumentNode;

            // Узнать число страниц, если в первый раз
            if(pagesCount == 0)
            {
                string countValue = root.SelectSingleNode(TotalElem).InnerText;
                string valueWOSpaces = countValue.Replace(((char)160).ToString(), String.Empty);
                int totalVacancies = int.Parse(valueWOSpaces.Split(' ')[1]);
                pagesCount = (int)Math.Ceiling((double)totalVacancies / ItemsPerPage);
            }

            // Парсинг стараницы
            var infos = root.SelectNodes(ItemElem);
            if (infos == null) { return; }

            foreach(HtmlNode infoNode in infos)
            {
                var hnSalary = infoNode.Descendants("meta").FirstOrDefault(n => 
                    n.Attributes["itemprop"].Value == "baseSalary");
                if (hnSalary == null) continue;
                var currency = infoNode.Descendants("meta")
                    .FirstOrDefault(n => n.Attributes["itemprop"].Value == "salaryCurrency")
                    .Attributes["content"].Value;
                if (currency != DesiredCurrency)
                    continue;

                try
                {
                    string name = infoNode.Descendants("a").First(n =>
                        n.Attributes["data-qa"].Value == "vacancy-serp__vacancy-title").InnerText;
                    string company = infoNode.Descendants("a").First(n =>
                        n.Attributes["data-qa"].Value == "vacancy-serp__vacancy-employer").InnerText;
                    var hnMetro = infoNode.Descendants("span").FirstOrDefault(n => 
                        n.Attributes["class"].Value == "metro-station");

                    foundVacancies.Add(new Vacancy
                    {
                        BaseSalary = int.Parse(hnSalary.Attributes["content"].Value),
                        Name = name,
                        Company = company,
                        MetroStation = hnMetro != null ? hnMetro.InnerText : null
                    });
                }
                catch
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// Запуск загрузки вакансий
        /// </summary>
        public Task StartAsync(string vacancyName)
        {
            this.foundVacancies = new List<Vacancy>();
            
            Task findTask = Task.Factory.StartNew(() =>
            {
                int p = 0;
                do
                {
                    string url = buildUrl(vacancyName, p);
                    HtmlWeb webUtil = new HtmlWeb { OverrideEncoding = Encoding.UTF8 };
                    HtmlDocument loadedDocument = webUtil.Load(url);
                    parseVacanciesInfo(loadedDocument);
                    p++;

                    if (ProgressChanged != null)
                    {
                        ProgressChanged(this, new FindProgressEventArgs(p, pagesCount));
                    }
                }
                while (p < pagesCount);
            });

            return findTask;
        }
    }
}
