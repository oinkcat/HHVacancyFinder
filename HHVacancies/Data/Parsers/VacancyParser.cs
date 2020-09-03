﻿using System;
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
        /// Выдать ссылку страницы результатов поиска
        /// </summary>
        /// <param name="query">Поисковый запрос</param>
        /// <returns>Ссылка на первую страницу поисковой выдачи</returns>
        public abstract string GetResultsPageUrl(string query);

        /// <summary>
        /// Выдать ссылки для всех страниц поисковой выдачи для запроса
        /// </summary>
        /// <param name="query">Поисковый запрос</param>
        /// <returns>Ссылки страниц результатов поиска</returns>
        public abstract IEnumerable<string> GetSearchResultsPages(string query);

        /// <summary>
        /// Получить информацию о вакансиях на странице
        /// </summary>
        /// <param name="pageDoc">Документ страницы результатов</param>
        /// <returns>Список вакансий на странице</returns>
        public abstract IList<Vacancy> ParsePage(HtmlDocument pageDoc);

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
        /// Общее число страниц
        /// </summary>
        public int TotalResultsPages { get; protected set; }
    }
}
