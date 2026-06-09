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

namespace Apontamento.Views.Producao
{
    /// <summary>
    /// Interação lógica para ControleFuncionarioPCP.xam
    /// </summary>
    public partial class ControleFuncionarioPCP : UserControl
    {
        public ControleFuncionarioPCP()
        {
            InitializeComponent();
            DataContext = new ControleFuncionarioPCPViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ControleFuncionarioPCPViewModel vm = (ControleFuncionarioPCPViewModel)DataContext;
                vm.Funcionarios = await Task.Run(vm.GetFuncionariosAsync);
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
            var columnName = e.Cell?.Column?.UniqueName;
            if (columnName != "ativo" && columnName != "exibir_furo")
                return;

            if (e.Cell.DataContext is not FuncionarioModel funcionario)
                return;

            try
            {
                ControleFuncionarioPCPViewModel vm = (ControleFuncionarioPCPViewModel)DataContext;
                vm.Funcionario = await vm.AtivarExibFuroAsync(funcionario);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class ControleFuncionarioPCPViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<FuncionarioModel> _funcionarios = new();
        public ObservableCollection<FuncionarioModel> Funcionarios
        {
            get { return _funcionarios; }
            set { _funcionarios = value; RaisePropertyChanged(nameof(Funcionarios)); }
        }

        private FuncionarioModel? _funcionario;
        public FuncionarioModel? Funcionario
        {
            get { return _funcionario; }
            set { _funcionario = value; RaisePropertyChanged(nameof(Funcionario)); }
        }

        public async Task<ObservableCollection<FuncionarioModel>> GetFuncionariosAsync()
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<FuncionarioModel>(
                @"SELECT * FROM ht.funcionarios ORDER BY nome_apelido;");

            return new ObservableCollection<FuncionarioModel>(data);
        }

        public async Task<FuncionarioModel> AtivarExibFuroAsync(FuncionarioModel func)
        {
            using var db = DbConnectionFactory.Create();
            await db.ExecuteAsync(
                @"UPDATE ht.funcionarios
                  SET exibir_furo = @exibir_furo,
                      ativo = @ativo
                  WHERE codfun = @codfun;",
                func);

            var funcionario = await db.QueryFirstOrDefaultAsync<FuncionarioModel>(
                @"SELECT * FROM ht.funcionarios WHERE codfun = @codfun;",
                func);
            if (funcionario is null)
                throw new InvalidOperationException("Funcionário não encontrado para atualização.");
            return funcionario;
        }

        private void RaisePropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
