using System.Windows;

namespace Graphs.Sources.Tasks.Task6
{
    /// <summary>
    /// Логика взаимодействия для Task6Window.xaml
    /// </summary>
    public partial class Task6Window : Window
    {
        public Task6Window(Task6Logick.ResultData resTask6)
        {
            InitializeComponent();
            RadiusCost.Text = resTask6.RadiusData.Key.ToString();
            RadiusVector.Text = resTask6.RadiusData.Value;

            DiameterCost.Text = resTask6.DiametrData.Key.ToString();
            DiamterVector.Text = resTask6.DiametrData.Value;

            DegreeVector.Text = resTask6.DegreeVector;
        }

        private void SaveAll(object sender, RoutedEventArgs e)
        {

        }
    }
}
