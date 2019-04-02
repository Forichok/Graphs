using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Graphs.Sources.Models;
using Graphs.Sources.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace Graphs.Views
{
    /// <summary>
    /// Логика взаимодействия для DijkstraResultWindow.xaml
    /// </summary>
    public partial class DijkstraResultWindow : Window
    {

        public string FromName { get; private set; } = "";
        public string Vector { get; private set; } = "";

        public string SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                Block.Text = _vectors[_selectedItem];
            }

        }

        public class DataObject
        {
            public string Names { get; set; }
            public string Values { get; set; }
        }


        private Dictionary<string, UniversalGraphNodeData> _dictResult;

        private Dictionary<string, string> _vectors;
        private string _selectedItem = "";

        public DijkstraResultWindow(Dictionary<string, UniversalGraphNodeData> resDictionary, string from)
        {
            DataContext = this;
            InitializeComponent();

            _vectors = new Dictionary<string, string>();
            _dictResult = resDictionary;
            FromName = from;
            InitAll();
        }

        private void InitAll()
        {
            var list = new ObservableCollection<DataObject>();
            foreach (var data in _dictResult)
            {
                list.Add(new DataObject(){Names = data.Key, Values = data.Value.Cost.ToString()});

                var vector = UniversalGraphNodeData.GetVector(_dictResult, data.Key); 
                _vectors.Add(data.Key, vector);
            }
            ResultGrid.ItemsSource = list;
            Box.ItemsSource = _dictResult.Keys;
        }

        private void MenuItemSaveMatrix_OnClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog {Filter = @"Simple text (*.txt)|*.txt"};
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    using (var sw = new StreamWriter(saveFileDialog.FileName))
                    {
                        foreach (DataObject res in ResultGrid.ItemsSource)
                        {
                            sw.WriteLine($"{res.Names} -> {res.Values}");
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuItemSaveVectors_OnClick(object sender, RoutedEventArgs e)
        {
            MainViewModel.SaveVectors(_vectors.Values);
        }
    }
}
