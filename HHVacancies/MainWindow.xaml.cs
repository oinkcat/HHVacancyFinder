using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        // TODO: Рефакторинг (две ViewModel)

        // Средняя заралата по текущему запросу
        private double currentAvgSalary;

        // Выбранные вакансии для сравнения
        private ObservableCollection<AverageInfo> selectedVacancies;

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
                var dataList = InfoList.ItemsSource as IEnumerable<Vacancy>;
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

        private void CompareLink_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            /*
            selectedVacancies.Add(new AverageInfo(currentQuery, currentAvgSalary));
            double maxSalary = selectedVacancies.Max(v => v.Salary);

            foreach(var vInfo in selectedVacancies)
            {
                vInfo.Percent = Math.Round(vInfo.Salary / maxSalary * 300, 2);
            }
            */
        }

        private void ExportLink_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            // SaveData();
        }

        // Двойной щелчок открывает ссылку на вакансию в браузере
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

        public MainWindow()
        {
            InitializeComponent();
            VacancyNameBox.Focus();

            selectedVacancies = new ObservableCollection<AverageInfo>();
            ComparsionList.ItemsSource = selectedVacancies;
        }
    }
}
