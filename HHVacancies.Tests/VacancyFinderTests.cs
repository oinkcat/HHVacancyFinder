using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HHVacancies.Data;

namespace HHVacancies.Tests
{
    /// <summary>
    /// Тестирование нахождения списка вакансий
    /// </summary>
    [TestClass]
    public class VacancyFinderTests
    {
        private const string TestVacancySearchQuery = "программист";

        /// <summary>
        /// Тестирование поиска вакансий
        /// </summary>
        [TestMethod]
        public async Task TestSearchVacancies()
        {
            var finder = new VacancyFinder();

            await finder.StartAsync(TestVacancySearchQuery);

            Assert.IsTrue(finder.CompletedSuccessfully);
            Assert.AreNotEqual(0, finder.Vacancies.Count);
        }
    }
}
