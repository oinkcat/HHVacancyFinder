using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HHVacancies.Data;

namespace HHVacancies.Data.Parsers
{
    /// <summary>
    /// Парсер вакансий с сайта HeadHunter
    /// </summary>
    internal class HeadHunterParser : VacancyParser
    {
        // Адрес страницы поиска
        private const string BaseSearchUrl = "http://hh.ru/search/vacancy";

        // Дополнительные параметры строки запроса
        private const string QueryParams = "currency_code=RUR&items_on_page=100&area=1";

        // Пути XPath к нужным элементам страницы
        private const string PagerElem = "//*/*[@data-qa='pager-page']";
        private const string ItemElem = "//*/div[@data-qa='vacancy-serp__vacancy']";

        // Значения атрибутов элементов информации о вакансиях
        private const string TitleValue = "vacancy-serp__vacancy-title";
        private const string CompanyValue = "vacancy-serp__vacancy-employer";
        private const string MetroValue = "metro-station";
        private const string SalaryValue = "vacancy-serp__vacancy-compensation";

        /// <summary>
        /// Имеются ли еще непросмотренные страницы
        /// </summary>
        public override bool HasMorePages => PageNumber < TotalPages;

        /// <summary>
        /// Выдать ссылку для поиска вакансий
        /// </summary>
        /// <param name="query">Строка запроса пользователя</param>
        /// <returns>Ссылка на страницу поиска</returns>
        public override string GetNextPageUrl(string query)
        {
            PageNumber++;
            
            return $"{BaseSearchUrl}?text={query}&page={PageNumber}&{QueryParams}";
        }

        /// <summary>
        /// Выдать вакансии на текущей странице
        /// </summary>
        /// <returns>Список доступных вакансий на странице</returns>
        public override IList<Vacancy> ParsePage()
        {
            var vacanciesOnPage = new List<Vacancy>();

            // Узнать число страниц, если в первый раз
            if (TotalPages == 0)
            {
                TotalPages = GetPagesCount();
            }

            // Парсинг страницы
            var listItemNodes = GetItemNodes();
            if (listItemNodes == null) { return vacanciesOnPage; }

            // Элементы списка вакансий
            foreach (HtmlNode infoNode in listItemNodes)
            {
                // Средняя зарплата
                var averageSalary = GetAverageSalaryForItemNode(infoNode);
                if (averageSalary == 0) continue;

                try
                {
                    vacanciesOnPage.Add(new Vacancy()
                    {
                        BaseSalary = averageSalary,
                        Name = GetVacancyTitleForItemNode(infoNode),
                        Company = GetCompanyNameForItemNode(infoNode),
                        MetroStation = GetMetroStationForItemNode(infoNode),
                        Url = GetVacancyUrlForItemNode(infoNode)
                    });
                }
                catch
                {
                    continue;
                }
            }

            return vacanciesOnPage;
        }

        // Считать значение средней зарплаты из заданного HTML узла
        private int GetAverageSalaryForItemNode(HtmlNode itemNode)
        {
            // Поиск узла информации о зарплате
            var salaryNode = itemNode.Descendants("span")
                .FirstOrDefault(n => n.Attributes["data-qa"]?.Value == SalaryValue);
            if (salaryNode == null) { return 0; }

            // Считать значение зарплаты
            var valueBuilder = new StringBuilder();
            int valuesSumm = 0;
            int valuesCount = 0;

            foreach (char c in salaryNode.InnerText)
            {
                if (Char.IsDigit(c))
                {
                    valueBuilder.Append(c);
                }
                else if (c == '-' || c == '.')
                {
                    valuesSumm += int.Parse(valueBuilder.ToString());
                    valueBuilder.Clear();
                    valuesCount++;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // Иностранная валюта не рассматривается
                    return 0;
                }
            }

            int avgSalary = valuesSumm / valuesCount;

            return avgSalary;
        }

        // Выдать число найденных страниц для узла страницы
        private int GetPagesCount()
        {
            var pageLinks = RootNode.SelectNodes(PagerElem);
            // Если пагинатора на странице нет - результаты не найдены
            if (pageLinks == null || pageLinks.Count == 0)
                return 0;

            return int.Parse(pageLinks.Last().InnerText);
        }

        // Выдать узлы элементов списка вакансий
        private HtmlNodeCollection GetItemNodes()
        {
            return RootNode.SelectNodes(ItemElem);
        }

        // Выдать наименование вакансии из элемента списка
        private string GetVacancyTitleForItemNode(HtmlNode itemNode)
        {
            var titleNode = itemNode.Descendants("a")
                .First(n => n.Attributes["data-qa"].Value == TitleValue);
            string title = UnescapeHtmlEntities(titleNode.InnerText.Trim());

            return title;
        }

        // Выдать ссылку на страницу информации о вакансии из элемента списка
        private string GetVacancyUrlForItemNode(HtmlNode itemNode)
        {
            string vacancyPageUrl = itemNode.Descendants("a")
                        .First(n => n.Attributes["data-qa"].Value == TitleValue)
                        .Attributes["href"].Value;

            return vacancyPageUrl;
        }

        // Выдать название компании из жлемента списка
        private string GetCompanyNameForItemNode(HtmlNode itemNode)
        {
            string company = itemNode.Descendants("a")
                .First(n => n.Attributes["data-qa"].Value == CompanyValue)
                .InnerText;
            string companyName = UnescapeHtmlEntities(company.Trim());

            return companyName;
        }

        // Выдать название станции метро из элемента списка
        private string GetMetroStationForItemNode(HtmlNode htmlNode)
        {
            var metroSpan = htmlNode.Descendants("span")
                .FirstOrDefault(n => n.Attributes["class"].Value == MetroValue);
            string metroStation = metroSpan?.InnerText;

            return metroStation;
        }

        public HeadHunterParser()
        {
            PageNumber = -1;
        }
    }
}
