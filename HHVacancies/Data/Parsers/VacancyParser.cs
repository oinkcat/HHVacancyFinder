using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace HHVacancies.Data.Parsers
{
    /// <summary>
    /// Базовый класс парсера сайта вакансий
    /// </summary>
    internal abstract class VacancyParser
    {
        /// <summary>
        /// Корневой элемент страницы
        /// </summary>
        protected HtmlNode RootNode;

        /// <summary>
        /// Установить текущую активную страницу
        /// </summary>
        /// <param name="doc"></param>
        public void SetCurrentPage(HtmlDocument doc)
        {
            RootNode = doc.DocumentNode;
        }

        /// <summary>
        /// Выдать строку поиска вакансии
        /// </summary>
        /// <param name="query">Строка запроса</param>
        /// <returns>Строка запроса для поиска вакансии</returns>
        public abstract string GetNextPageUrl(string query);

        /// <summary>
        /// Получить информацию о вакансиях на странице
        /// </summary>
        /// <returns>Список вакансий на странице</returns>
        public abstract IList<Vacancy> ParsePage();

        /// <summary>
        /// Перевести обозначения символов HTML в эти символы
        /// </summary>
        /// <param name="origString">Строка для замены символов</param>
        /// <returns>Строка с замененными символами HTML</returns>
        protected string UnescapeHtmlEntities(string origString)
        {
            return origString.Replace("&amp;", "&");
        }

        /// <summary>
        /// Номер текущей страницы
        /// </summary>
        public int PageNumber { get; protected set; }

        /// <summary>
        /// Общее число страниц
        /// </summary>
        public int TotalPages { get; protected set; }

        /// <summary>
        /// Имеются ли непросмотренные страницы
        /// </summary>
        public abstract bool HasMorePages { get; }
    }
}
