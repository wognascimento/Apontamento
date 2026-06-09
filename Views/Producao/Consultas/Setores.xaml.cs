using Apontamento.DataBase;
using Apontamento.DataBase.Model;
using Dapper;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Apontamento.Views.Producao.Consultas
{
    /// <summary>
    /// Interação lógica para Setores.xam
    /// </summary>
    public partial class Setores : UserControl
    {
        public Setores()
        {
            InitializeComponent();
            DataContext = new SetoresViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                SetoresViewModel vm = (SetoresViewModel)DataContext;
                vm.Setores = await Task.Run(vm.GetSetoresAsync);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OnCellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            if (e.Cell?.Column?.UniqueName != "inativo")
                return;

            if (e.Cell.DataContext is not SetorModel setor)
                return;

            try
            {
                SetoresViewModel vm = (SetoresViewModel)DataContext;
                vm.Setor = await vm.AtivarSetorAsync(setor);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class SetoresViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<SetorModel> _setores = new();
        public ObservableCollection<SetorModel> Setores
        {
            get { return _setores; }
            set { _setores = value; RaisePropertyChanged(nameof(Setores)); }
        }

        private SetorModel? _setor;
        public SetorModel? Setor
        {
            get { return _setor; }
            set { _setor = value; RaisePropertyChanged(nameof(Setor)); }
        }

        public async Task<ObservableCollection<SetorModel>> GetSetoresAsync()
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<SetorModel>(
                @"SELECT * FROM producao.tbl_setor ORDER BY setor;");

            return new ObservableCollection<SetorModel>(data);
        }

        public async Task<SetorModel> AtivarSetorAsync(SetorModel setor)
        {
            using var db = DbConnectionFactory.Create();
            await db.ExecuteAsync(
                @"UPDATE producao.tbl_setor
                  SET inativo = @inativo
                  WHERE codigo_setor = @codigo_setor;",
                setor);
            return setor;
        }

        private void RaisePropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
