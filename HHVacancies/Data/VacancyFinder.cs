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
        private const string SalaryValue = "vacancy-serp__vacancy-compensation";

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

        // Перевести обозначения символов HTML в эти символы
        private string UnescapeHtmlEntities(string origString)
        {
            return origString.Replace("&amp;", "&");
        }

        // Считать значение средней зарплаты из заданного HTML узла
        private int ParseAverageSalaryValue(HtmlNode node)
        {
            var valueBuilder = new StringBuilder();
            int valuesSumm = 0;
            int valuesCount = 0;

            foreach(char c in node.InnerText)
            {
                if(Char.IsDigit(c))
                {
                    valueBuilder.Append(c);
                }
                else if(c == '-' || c == '.')
                {
                    valuesSumm += int.Parse(valueBuilder.ToString());
                    valueBuilder.Clear();
                    valuesCount++;
                }
            }

            int avgSalary = valuesSumm / valuesCount;

            return avgSalary;
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
                // Зарплата
                var hnSalary = infoNode.Descendants("div")
                    .FirstOrDefault(n => n.Attributes["data-qa"] != null &&
                                         n.Attributes["data-qa"].Value == SalaryValue);
                if (hnSalary == null) continue;

                try
                {
                    // Название
                    var titleLink = infoNode.Descendants("a")
                        .First(n => n.Attributes["data-qa"].Value == TitleValue);
                    string title = UnescapeHtmlEntities(titleLink.InnerText.Trim());
                    string url = titleLink.Attributes["href"].Value;

                    // Компания
                    string company = infoNode.Descendants("a")
                        .First(n => n.Attributes["data-qa"].Value == CompanyValue)
                        .InnerText;
                    string companyName = UnescapeHtmlEntities(company.Trim());

                    // Станция метро
                    var metroSpan = infoNode.Descendants("span")
                        .FirstOrDefault(n => n.Attributes["class"].Value == MetroValue);
                    string metroStation = metroSpan != null ? metroSpan.InnerText : null;

                    foundVacancies.Add(new Vacancy()
                    {
                        BaseSalary = ParseAverageSalaryValue(hnSalary),
                        Name = title,
                        Company = companyName,
                        MetroStation = metroStation,
                        Url = url
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

                    var progressArgs = new FindProgressEventArgs(p, pagesCount);
                    ProgressChanged?.Invoke(this, progressArgs);
                }
                while (p < pagesCount);
            });

            return findTask;
        }
    }
}
