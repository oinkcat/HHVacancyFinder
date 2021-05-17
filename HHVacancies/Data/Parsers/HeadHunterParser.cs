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
        const string BaseSearchUrl = "http://hh.ru/search/vacancy";

        // Дополнительные параметры строки запроса
        const string QueryParams = "currency_code=RUR&only_with_salary=true&area=1";

        // Пути XPath к нужным элементам страницы
        const string PagerElem = "//*/*[@data-qa='pager-page']";
        const string ItemElem = "//*/div[contains(@data-qa, 'vacancy-serp__vacancy')]";

        // Значения атрибутов элементов информации о вакансиях
        const string TitleValue = "vacancy-serp__vacancy-title";
        const string CompanyValue = "vacancy-serp__vacancy-employer";
        const string MetroValue = "metro-station";
        const string SalaryValue = "vacancy-serp__vacancy-compensation";

        /// <summary>
        /// Выдать ссылку для поиска вакансий
        /// </summary>
        /// <param name="query">Строка запроса пользователя</param>
        /// <returns>Ссылка на страницу поиска</returns>
        public override string GetResultsPageUrl(string query)
        {
            return GetUrlForPage(query, 0);
        }

        /// <summary>
        /// Выдать все ссылки на страницы результатов поиска для запроса
        /// </summary>
        /// <returns>Ссылки на страницы результатов</returns>
        public override IEnumerable<string> GetSearchResultsPages(string query)
        {
            return Enumerable.Range(0, TotalResultsPages)
                .Select(num => GetUrlForPage(query, num));
        }

        // Выдать ссылку на заданную страницу резудьтатов
        private string GetUrlForPage(string query, int pageNumber)
        {
            return $"{BaseSearchUrl}?text={query}&page={pageNumber}&{QueryParams}";
        }

        /// <summary>
        /// Выдать вакансии на текущей странице
        /// </summary>
        /// <returns>Список доступных вакансий на странице</returns>
        public override IList<Vacancy> ParsePage(HtmlDocument pageDoc)
        {
            var rootNode = pageDoc.DocumentNode;
            var vacanciesOnPage = new List<Vacancy>();

            // Узнать число страниц, если в первый раз
            if (TotalResultsPages == 0)
            {
                TotalResultsPages = GetPagesCount(rootNode);
            }

            // Парсинг страницы
            var listItemNodes = GetItemNodes(rootNode);
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

        // Выдать число найденных страниц для узла страницы
        private int GetPagesCount(HtmlNode rootNode)
        {
            var pageLinks = rootNode.SelectNodes(PagerElem);
            // Если пагинатора на странице нет - результаты не найдены
            if (pageLinks == null || pageLinks.Count == 0)
                return 0;

            // Номера страниц начинаются с нуля
            return int.Parse(pageLinks.Last().InnerText) - 1;
        }

        // Выдать узлы элементов списка вакансий
        private HtmlNodeCollection GetItemNodes(HtmlNode rootNode)
        {
            return rootNode.SelectNodes(ItemElem);
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
                else if (c == '-' || c == '–' || c == '.')
                {
                    if(valueBuilder.Length > 0)
                    {
                        valuesSumm += int.Parse(valueBuilder.ToString());
                        valueBuilder.Clear();
                        valuesCount++;
                    }
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

        // Выдать наименование вакансии из элемента списка
        private string GetVacancyTitleForItemNode(HtmlNode itemNode)
        {
            var titleNode = GetNodeByDataQa(itemNode.Descendants("a"), TitleValue);
            string title = UnescapeHtmlEntities(titleNode.InnerText.Trim());

            return StripComments(title);
        }

        // Выдать ссылку на страницу информации о вакансии из элемента списка
        private string GetVacancyUrlForItemNode(HtmlNode itemNode)
        {
            string vacancyPageUrl = GetNodeByDataQa(itemNode.Descendants("a"), TitleValue)
                .Attributes["href"].Value;

            return vacancyPageUrl;
        }

        // Выдать название компании из жлемента списка
        private string GetCompanyNameForItemNode(HtmlNode itemNode)
        {
            var companyInfoNode = GetNodeByDataQa(itemNode.Descendants("a"), CompanyValue);
            string companyName = UnescapeHtmlEntities(companyInfoNode?.InnerText ?? "?");

            return StripComments(companyName);
        }

        // Выдать название станции метро из элемента списка
        private string GetMetroStationForItemNode(HtmlNode htmlNode)
        {
            var metroSpan = htmlNode.Descendants("span")
                .FirstOrDefault(n => n.Attributes["class"]?.Value == MetroValue);
            string metroStation = metroSpan?.InnerText;

            return metroStation;
        }

        // Выдать первый совпадающий по аттрибуту data-qa элемент
        private HtmlNode GetNodeByDataQa(IEnumerable<HtmlNode> nodes, string qaValue)
        {
            return nodes.FirstOrDefault(n => n.Attributes.Any(attr => {
                return attr.Name == "data-qa" && attr.Value == qaValue;
            }));
        }
    }
}
