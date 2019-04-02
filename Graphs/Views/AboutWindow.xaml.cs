using System.Windows;

namespace Graphs.Views
{
    /// <summary>
    /// Логика взаимодействия для AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            Block.Text = "Authors of first: Сона Геворгян (Superuser)\n" +
                         "Кирилл Стульников\n" +
                         "Никита Удалов\n" +
                         "Группа 303б (МАИ)";
        }
    }
}
