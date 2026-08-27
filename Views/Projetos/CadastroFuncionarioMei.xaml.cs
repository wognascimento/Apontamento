using Apontamento.DataBase;
using Apontamento.DataBase.Model;
using Dapper;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Apontamento.Views.Projetos
{
    public partial class CadastroFuncionarioMei : UserControl
    {
        private readonly string _setor;

        public CadastroFuncionarioMei(string setor)
        {
            InitializeComponent();
            DataContext = new CadastroFuncionarioMeiViewModel();
            _setor = setor;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                await CarregarAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cadastro funcionario MEI", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async Task CarregarAsync()
        {
            if (DataContext is CadastroFuncionarioMeiViewModel vm)
                vm.Funcionarios = await vm.GetFuncionariosAsync(_setor);
        }

        private void dgFuncionariosMei_AddingNewDataItem(object sender, GridViewAddingNewEventArgs e)
        {
            e.NewObject = new FuncionarioMeiModel
            {
                setor = _setor,
                local_galpao = "MEI"
            };
        }

        private void dgFuncionariosMei_RowValidating(object sender, GridViewRowValidatingEventArgs e)
        {
            if (e.Row.Item is not FuncionarioMeiModel funcionario)
                return;

            funcionario.local_galpao = string.IsNullOrWhiteSpace(funcionario.local_galpao) ? "MEI" : funcionario.local_galpao.Trim();
            funcionario.setor = string.IsNullOrWhiteSpace(funcionario.setor) ? _setor : funcionario.setor.Trim();

            if (string.IsNullOrWhiteSpace(funcionario.setor))
            {
                e.IsValid = false;
                e.ValidationResults.Add(new GridViewCellValidationResult
                {
                    PropertyName = nameof(FuncionarioMeiModel.setor),
                    ErrorMessage = "Informe o setor."
                });
            }

            if (string.IsNullOrWhiteSpace(funcionario.nome_apelido))
            {
                e.IsValid = false;
                e.ValidationResults.Add(new GridViewCellValidationResult
                {
                    PropertyName = nameof(FuncionarioMeiModel.nome_apelido),
                    ErrorMessage = "Informe o nome/apelido."
                });
            }

            if (!funcionario.admissao.HasValue)
            {
                e.IsValid = false;
                e.ValidationResults.Add(new GridViewCellValidationResult
                {
                    PropertyName = nameof(FuncionarioMeiModel.admissao),
                    ErrorMessage = "Informe a data de admissão."
                });
            }

            if (string.IsNullOrWhiteSpace(funcionario.local_galpao))
            {
                e.IsValid = false;
                e.ValidationResults.Add(new GridViewCellValidationResult
                {
                    PropertyName = nameof(FuncionarioMeiModel.local_galpao),
                    ErrorMessage = "Informe o local galpão."
                });
            }
        }

        private async void dgFuncionariosMei_RowValidated(object sender, GridViewRowValidatedEventArgs e)
        {
            if (e.Row.Item is not FuncionarioMeiModel funcionario ||
                DataContext is not CadastroFuncionarioMeiViewModel vm)
            {
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                funcionario.local_galpao = "MEI";
                funcionario.setor = _setor;

                if (funcionario.codfun is null or <= 0)
                    funcionario.codfun = await vm.InserirFuncionarioAsync(funcionario);
                else
                    await vm.AlterarFuncionarioAsync(funcionario);

                dgFuncionariosMei.Rebind();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cadastro funcionario MEI", MessageBoxButton.OK, MessageBoxImage.Error);
                await CarregarAsync();
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }

    public class CadastroFuncionarioMeiViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private ObservableCollection<FuncionarioMeiModel> _funcionarios = [];
        public ObservableCollection<FuncionarioMeiModel> Funcionarios
        {
            get => _funcionarios;
            set { _funcionarios = value; RaisePropertyChanged(nameof(Funcionarios)); }
        }

        public async Task<ObservableCollection<FuncionarioMeiModel>> GetFuncionariosAsync(string setor)
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<FuncionarioMeiModel>(
                @"SELECT codfun, setor, nome_apelido, admissao, demissao, local_galpao
                  FROM ht.funcionarios_mei
                  WHERE setor = @setor
                  ORDER BY nome_apelido;",
                new { setor });

            return new ObservableCollection<FuncionarioMeiModel>(data);
        }

        public async Task<int> InserirFuncionarioAsync(FuncionarioMeiModel funcionario)
        {
            using var db = DbConnectionFactory.Create();
            return await db.ExecuteScalarAsync<int>(
                @"INSERT INTO ht.funcionarios_mei
                    (setor, nome_apelido, admissao, demissao, local_galpao)
                  VALUES
                    (@setor, @nome_apelido, @admissao, @demissao, 'MEI')
                  RETURNING codfun;",
                funcionario);
        }

        public async Task AlterarFuncionarioAsync(FuncionarioMeiModel funcionario)
        {
            using var db = DbConnectionFactory.Create();
            await db.ExecuteAsync(
                @"UPDATE ht.funcionarios_mei SET
                    setor = @setor,
                    nome_apelido = @nome_apelido,
                    admissao = @admissao,
                    demissao = @demissao,
                    local_galpao = 'MEI'
                  WHERE codfun = @codfun;",
                funcionario);
        }
    }
}
