using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

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
        private void SaveData()
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
        private void ShowData(IList<Vacancy> vacancies)
        {
            grdTop.IsEnabled = true;
            pbFindProgres.Visibility = Visibility.Collapsed;
            exportBlock.Visibility = Visibility.Visible;

            // Отображение информации и статистика
            lbInfo.ItemsSource = vacancies;
            if(vacancies.Count > 0)
            {
                double avgSalary = vacancies.Average(item => item.BaseSalary);
                lblStatus.Content = String.Format(
                    "Готово. Всего: {0}, средняя зарплата: {1:C}", 
                    vacancies.Count(), Math.Round(avgSalary, 2));
                exportBlock.Visibility = Visibility.Visible;
            }
            else
            {
                lblStatus.Content = "Готово. Ничего не найдено";
                exportBlock.Visibility = Visibility.Hidden;
            }
        }

        // Показать прогресс операции
        private void showProgress(int shownValue, int maxValue)
        {
            pbFindProgres.Maximum = maxValue;
            pbFindProgres.Value = shownValue;
        }

        // Начать поиск вакансий
        private async void StartSearchAsync()
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
            ShowData(finder.Vacancies);
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            if(tbVacancyName.Text.Trim().Length > 0)
            {
                StartSearchAsync();
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
            SaveData();
        }

        private void tbVacancyName_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                btnFind_Click(sender, e);
            }
        }

        private void ItemDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = sender as ListViewItem;
            var infoUrl = (selectedItem.Content as Vacancy).Url;
            
            // Открыть ссылку на информацию о вакансии в браузере
            var browserStartInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                Verb = "Open",
                FileName = infoUrl
            };
            Process.Start(browserStartInfo);
        }
    }
}
