using System.Collections.ObjectModel;
using System.Windows;
using Graphs.Sources.Models;

namespace Graphs.Sources.Tasks.Task6
{
    /// <summary>
    /// Логика взаимодействия для Task6Window.xaml
    /// </summary>
    public partial class Task6Window : Window
    {
        public class DataObject
        {
            public string NodeName { get; set; }
            public string Costs { get; set; }
        }

        public Task6Window(Task6Logick.ResultData resTask6)
        {
            InitializeComponent();
            RadiusCost.Text = resTask6.RadiusData.Key.ToString();
            RadiusVector.Text = resTask6.RadiusData.Value;

            DiameterCost.Text = resTask6.DiametrData.Key.ToString();
            DiamterVector.Text = resTask6.DiametrData.Value;

            DegreeVector.Text = resTask6.DegreeVector;

            var list = new ObservableCollection<DataObject>();
            foreach (var data in resTask6.AllCosts)
            {
                list.Add(new DataObject(){NodeName = data.Key, Costs = string.Join(", ", data.Value)});
            }
            ResultGrid.ItemsSource = list;

        }

        private void SaveAll(object sender, RoutedEventArgs e)
        {

        }
    }
}
