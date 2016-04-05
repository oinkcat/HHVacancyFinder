using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using HHVacancies.Data;
using Microsoft.Win32;

namespace HHVacancies
{
    /// <summary>
    /// Основное окно приложения
    /// </summary>
    public partial class MainWindow : Window
    {
        private VacancyFinder finder;

        public MainWindow()
        {
            InitializeComponent();
            tbVacancyName.Focus();
        }

        // Сохранить данные о найденных вакансиях
        private void saveData()
        {
            var dlg = new SaveFileDialog()
            {
                DefaultExt = ".csv",
                Filter = "CSV-файлы|*.csv"
            };

            if (dlg.ShowDialog().Value)
            {
                var dataList = lbInfo.ItemsSource as IEnumerable<Vacancy>;
                using(StreamWriter sw = new StreamWriter(dlg.FileName))
                {
                    byte[] bom = Encoding.UTF8.GetPreamble();
                    sw.BaseStream.Write(bom, 0, bom.Length);
                    sw.WriteLine("Должность;Организация;Станция метро;Запрлата");
                    foreach(Vacancy item in dataList)
                    {
                        sw.WriteLine("{0};{1};{2};{3}", 
                            item.Name, item.Company, item.MetroStation,
                            item.BaseSalary);
                    }
                }
            }
        }

        // Показать результат
        private void showData(IEnumerable<Vacancy> vacancies)
        {
            grdTop.IsEnabled = true;
            pbFindProgres.Visibility = Visibility.Collapsed;
            exportBlock.Visibility = Visibility.Visible;

            // Отображение информации и статистика
            lbInfo.ItemsSource = vacancies;
            double avgSalary = Math.Round(vacancies.Average(item => item.BaseSalary), 2);
            lblStatus.Content = String.Format(
                "Готово. Всего: {0}, средняя зарплата: {1:C}", 
                vacancies.Count(), avgSalary);
        }

        // Показать прогресс операции
        private void showProgress(int shownValue, int maxValue)
        {
            pbFindProgres.Maximum = maxValue;
            pbFindProgres.Value = shownValue;
        }

        // Начать поиск вакансий
        private async void startSearchAsync()
        {
            grdTop.IsEnabled = false;
            exportBlock.Visibility = Visibility.Hidden;
            lblStatus.Content = "Выполняется поиск...";
            pbFindProgres.Value = 0;
            pbFindProgres.Visibility = Visibility.Visible;

            finder = new VacancyFinder();
            finder.ProgressChanged += (s, e) => Dispatcher.Invoke(() =>
                showProgress(e.Value, e.Maximum));
            string encodedName = Uri.EscapeDataString(tbVacancyName.Text);

            await finder.StartAsync(encodedName);
            showData(finder.Vacancies);
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            if(tbVacancyName.Text.Trim().Length > 0)
            {
                startSearchAsync();
            }
            else
            {
                MessageBox.Show("Название вакансии не задано", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                tbVacancyName.Focus();
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            saveData();
        }

        private void tbVacancyName_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                btnFind_Click(sender, e);
            }
        }
    }
}
