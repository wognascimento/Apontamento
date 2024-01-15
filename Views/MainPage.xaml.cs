using System.Windows.Controls;

using Apontamento.ViewModels;

namespace Apontamento.Views
{
    public partial class MainPage : Page
    {
        public MainPage(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
