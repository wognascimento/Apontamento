using Apontamento.DataBase;
using Apontamento.DataBase.Model;
using Apontamento.Views.Producao;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls.Scheduling;
using Telerik.Windows.Controls.TreeMap;

namespace Apontamento.Views.Projetos
{
    /// <summary>
    /// Interação lógica para CadastroFuncionarioProjetos.xam
    /// </summary>
    public partial class CadastroFuncionarioProjetos : UserControl
    {
        public CadastroFuncionarioProjetos()
        {
            InitializeComponent();
            DataContext = new CadastroFuncionarioProjetosViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                CadastroFuncionarioProjetosViewModel vm = (CadastroFuncionarioProjetosViewModel)DataContext;
                vm.FuncProjetos = await Task.Run(vm.GetFuncionariosProjetoAsync);
                vm.Funcs = await Task.Run(vm.GetFuncionariosAsync);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

               // (new System.Collections.Generic.CollectionDebugView<Apontamento.DataBase.Model.FuncionarioProjetosModel>(vm.FuncProjetos).Items[0]).PlanosProjetos

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void SfDataGrid_CurrentCellDropDownSelectionChanged(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellDropDownSelectionChangedEventArgs e)
        {
            var sfdatagrid = sender as SfDataGrid;
            RowColumnIndex rowColumnIndex = new(e.RowColumnIndex.RowIndex, 2);
            ((FuncionarioProjetosModel)sfdatagrid.View.CurrentAddItem).nome_func = ((FuncionarioModel)e.SelectedItem).nome_apelido;
            ((FuncionarioProjetosModel)sfdatagrid.View.CurrentAddItem).funcao_func = ((FuncionarioModel)e.SelectedItem).funcao;
            this.dgFuncionarios.MoveCurrentCell(rowColumnIndex);
            int rowIndex = sfdatagrid.ResolveToRecordIndex(e.RowColumnIndex.RowIndex);
            this.dgFuncionarios.SelectionController.CurrentCellManager.BeginEdit();

            /*
            var sfdatagrid = sender as SfDataGrid;
            var viewModel = sfdatagrid.DataContext as CadastroFuncionarioProjetosViewModel;
            int rowIndex = sfdatagrid.ResolveToRecordIndex(e.RowColumnIndex.RowIndex);
            var record = (sfdatagrid.View.Records[33] as RecordEntry).Data as FuncionarioProjetosModel;
            record.nome_func = "TESTE 1"; //viewModel.UnitPriceDict[e.SelectedItem.ToString()];
            record.funcao_func = "TESTE 2";//viewModel.QuantityDict[e.SelectedItem.ToString()];
            */
        }

        private async void dgFuncionarios_RowValidated(object sender, RowValidatedEventArgs e)
        {
            //SelectedIndex = -1
            //SelectedIndex = 33
            //RowData = {Apontamento.DataBase.Model.FuncionarioProjetosModel}
            try
            {
                CadastroFuncionarioProjetosViewModel vm = (CadastroFuncionarioProjetosViewModel)DataContext;
                var sfdatagrid = sender as SfDataGrid;
                if (sfdatagrid.SelectedIndex == -1)
                    await Task.Run( () => vm.GravarFuncionarioProjetosAsync((FuncionarioProjetosModel)e.RowData));
                else
                    await Task.Run(() => vm.AlterarFuncionarioProjetosAsync((FuncionarioProjetosModel)e.RowData));

                vm.FuncProjetos = await Task.Run(vm.GetFuncionariosProjetoAsync);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void dgFuncionarios_RowValidating(object sender, RowValidatingEventArgs e)
        {
            FuncionarioProjetosModel rowData = (FuncionarioProjetosModel)e.RowData;
            if (!rowData.cod_func.HasValue)
            {
                e.IsValid = false;
                e.ErrorMessages.Add("cod_func", "Seleciona o funcionário.");
            }
            else if (rowData.nick_name_func == null)
            {
                e.IsValid = false;
                e.ErrorMessages.Add("nick_name_func", "Informo que o login. Padrão é o nome seguido do último nome.");
            }
            else if (!rowData.horas_minimas.HasValue)
            {
                e.IsValid = false;
                e.ErrorMessages.Add("horas_minimas", "Informa a quantidade de hora minima de apontamento.");
            }

        }

        private void dgFuncionarios_AddNewRowInitiating(object sender, AddNewRowInitiatingEventArgs e)
        {
            CadastroFuncionarioProjetosViewModel vm = (CadastroFuncionarioProjetosViewModel)DataContext;

            ((FuncionarioProjetosModel)e.NewObject).departamento = "PROJETOS";
            //((FuncionarioProjetosModel)e.NewObject).cadastrado_por = Environment.UserName;
            //((FuncionarioProjetosModel)e.NewObject).inclusao = DateTime.Now;
        }
    }

    public class CadastroFuncionarioProjetosViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private FuncionarioProjetosModel _funcProjeto;
        public FuncionarioProjetosModel FuncProjeto
        {
            get { return _funcProjeto; }
            set { _funcProjeto = value; RaisePropertyChanged("FuncProjeto"); }
        }

        private ObservableCollection<FuncionarioProjetosModel> _funcProjetos;
        public ObservableCollection<FuncionarioProjetosModel> FuncProjetos
        {
            get { return _funcProjetos; }
            set { _funcProjetos = value; RaisePropertyChanged("FuncProjetos"); }
        }

        private FuncionarioModel _func;
        public FuncionarioModel Func
        {
            get { return _func; }
            set { _func = value; RaisePropertyChanged("Func"); }
        }

        private ObservableCollection<FuncionarioModel> _funcs;
        public ObservableCollection<FuncionarioModel> Funcs
        {
            get { return _funcs; }
            set { _funcs = value; RaisePropertyChanged("Funcs"); }
        }

        public async Task<ObservableCollection<FuncionarioProjetosModel>> GetFuncionariosProjetoAsync()
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.FuncionarioProjetos
                    .Include(f => f.PlanosProjetos) // Inclui as entradas relacionadas de DataPlanProjetoModel
                    .OrderBy(f => f.nome_func)
                    .Where(f => f.departamento == "PROJETOS")
                    .ToListAsync();
                return new ObservableCollection<FuncionarioProjetosModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<FuncionarioModel>> GetFuncionariosAsync()
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.Funcionarios
                    .Where(f => f.data_demissao == null && f.setor.Contains("PROJETOS"))
                    .OrderBy(f => f.nome_apelido)
                    .ToListAsync();
                return new ObservableCollection<FuncionarioModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task GravarFuncionarioProjetosAsync(FuncionarioProjetosModel funcionario)
        {
            using var dbContext = new DatabaseContext();
            var executionStrategy = dbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = dbContext.Database.BeginTransaction();
                try
                {
                    await dbContext.FuncionarioProjetos.AddAsync(funcionario);
                    var novoPlanoProjetoArray = new DataPlanProjetoModel[]
                    {
                        new() { codfun = (long)funcionario.cod_func, dia = "SEGUNDA-FEIRA", hora_minima = (double)funcionario.horas_minimas },
                        new() { codfun = (long)funcionario.cod_func, dia = "TERÇA-FEIRA", hora_minima = (double)funcionario.horas_minimas },
                        new() { codfun = (long)funcionario.cod_func, dia = "QUARTA-FEIRA", hora_minima = (double)funcionario.horas_minimas },
                        new() { codfun = (long)funcionario.cod_func, dia = "QUINTA-FEIRA", hora_minima = (double)funcionario.horas_minimas },
                        new() { codfun = (long)funcionario.cod_func, dia = "SEXTA-FEIRA", hora_minima = (double)funcionario.horas_minimas },
                    };

                    await dbContext.DataPlanProjetos.BulkInsertAsync(novoPlanoProjetoArray);
                    await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            });
        }

        public async Task AlterarFuncionarioProjetosAsync(FuncionarioProjetosModel funcionario)
        {
            try
            {
                using DatabaseContext db = new();
                db.FuncionarioProjetos.Update(funcionario);
                await db.SaveChangesAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }
    }


}
