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
        private const string PagerElem = "//*/*[@data-qa='pager-page']";
        private const string ItemElem = "//*/div[starts-with(@class, " +
                                        "'vacancy-serp-item__row')]";

        // Значения атрибутов элементов информации о вакансиях
        private const string TitleValue = "vacancy-serp__vacancy-title";
        private const string CompanyValue = "vacancy-serp__vacancy-employer";
        private const string MetroValue = "metro-station";

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
        public IList<Vacancy> Vacancies
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
        private void ParseVacanciesInfo(HtmlDocument doc)
        {
            HtmlNode root = doc.DocumentNode;

            // Узнать число страниц, если в первый раз
            if(pagesCount == 0)
            {
                var pageLinks = root.SelectNodes(PagerElem);
                // Если пагинатора на странице нет - результаты не найдены
                if (pageLinks == null || pageLinks.Count == 0)
                    return;
                
                pagesCount = int.Parse(pageLinks.Last().InnerText);
            }

            // Парсинг страницы
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
                    var titleLink = infoNode.Descendants("a").First(n =>
                        n.Attributes["data-qa"].Value == TitleValue);
                    string company = infoNode.Descendants("a").First(n =>
                        n.Attributes["data-qa"].Value == CompanyValue).InnerText;
                    var hnMetro = infoNode.Descendants("span").FirstOrDefault(n => 
                        n.Attributes["class"].Value == MetroValue);

                    foundVacancies.Add(new Vacancy
                    {
                        BaseSalary = int.Parse(hnSalary.Attributes["content"].Value),
                        Name = titleLink.InnerText.Trim(),
                        Company = company.Trim(),
                        MetroStation = hnMetro != null ? hnMetro.InnerText : null,
                        Url = titleLink.Attributes["href"].Value
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
                    ParseVacanciesInfo(loadedDocument);
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
