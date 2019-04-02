using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

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
            var saveFileDialog = new SaveFileDialog { Filter = @"Simple text (*.txt)|*.txt" };
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    using (var sw = new StreamWriter(saveFileDialog.FileName))
                    {
                        sw.WriteLine($"Radius Cost -> {RadiusCost.Text} Vector -> {RadiusVector.Text}");
                        sw.WriteLine($"Diameter Cost -> {DiameterCost.Text} Vector -> {DiamterVector.Text}");

                        sw.WriteLine($"Table:: ");
                        foreach (DataObject res in ResultGrid.ItemsSource)
                        {
                            sw.WriteLine($"To: {res.NodeName} From: {res.Costs}");
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
