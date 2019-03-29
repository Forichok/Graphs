using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using Graphs.Sources.Models;

namespace Graphs
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
            
        }

        private void MenuItemSaveVectors_OnClick(object sender, RoutedEventArgs e)
        {
            

        }
    }
}
