using Apontamento.DataBase;
using Apontamento.DataBase.Model;
using Microsoft.EntityFrameworkCore;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Utility;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

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

        private async void FirstLevelNestedGrid_RowValidated(object sender, RowValidatedEventArgs e)
        {
            try
            {
                CadastroFuncionarioProjetosViewModel vm = (CadastroFuncionarioProjetosViewModel)DataContext;
                var sfdatagrid = sender as SfDataGrid;
                if (e.RowIndex > -1) //RowIndex = 2
                    await Task.Run(() => vm.AlterarDataPlanejamentoFuncionarioProjetosAsync((DataPlanProjetoModel)e.RowData));

                //vm.FuncProjetos = await Task.Run(vm.GetFuncionariosProjetoAsync);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FirstLevelNestedGrid_RowValidating(object sender, RowValidatingEventArgs e)
        {

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
                        new() { codfun = (long)funcionario.cod_func, dia = "SEXTA-FEIRA", hora_minima = (double)funcionario.horas_minimas -1 },
                    };

                    foreach (var item in novoPlanoProjetoArray)
                    {
                        await dbContext.DataPlanProjetos.AddAsync(item);
                    }

                    //await dbContext.DataPlanProjetos.BulkInsertAsync(novoPlanoProjetoArray);
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

        public async Task AlterarDataPlanejamentoFuncionarioProjetosAsync(DataPlanProjetoModel dataPlanejamento)
        {
            try
            {
                using DatabaseContext db = new();
                db.DataPlanProjetos.Update(dataPlanejamento);
                await db.SaveChangesAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task GravarFeriasFuncionarioProjetosAsync(FuncionarioProjetosModel funcionario)
        {
            using var dbContext = new DatabaseContext();
            var executionStrategy = dbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = dbContext.Database.BeginTransaction();
                try
                {
                    var resultado = await dbContext.FuncionarioProjetos
                        .Join(dbContext.DataPlanProjetos,
                              funcionario => funcionario.cod_func,
                              dataplan => dataplan.codfun,
                              (funcionario, dataplan) => new { Funcionario = funcionario, DataPlan = dataplan })
                        .Join(dbContext.DataPlanejamentos,
                              dataPlan => dataPlan.DataPlan.dia,
                              dataPlanejamento => dataPlanejamento.dia,
                              (dataPlan, dataPlanejamento) => new
                              {
                                  dataPlan.Funcionario.cod_func,
                                  dataPlanejamento.dia,
                                  dataPlanejamento.data,
                                  dataPlan.Funcionario.incio_ferias,
                                  dataPlan.Funcionario.termino_ferias,
                                  dataPlan.DataPlan.hora_minima,
                                  dataPlan.Funcionario.nome_func,
                                  dataPlanejamento.semana,
                                  dataPlan.Funcionario.departamento,
                                  dataPlan.Funcionario.data_demissao
                              })
                        .Where(resultado => resultado.data >= funcionario.incio_ferias && resultado.data <= funcionario.termino_ferias && resultado.cod_func == funcionario.cod_func )
                        .ToListAsync();

                    //var apontamentoHoraModelArray = new ApontamentoHoraModel[]

                    var apontamentoHoraModelArray = new ObservableCollection<ApontamentoHoraModel>();
                    foreach (var item in resultado)
                    {
                        apontamentoHoraModelArray.Add(
                            new ApontamentoHoraModel 
                            {
                                cod_func = item.cod_func,
                                cod_atividade = 29,
                                desc_atividade = "FÉRIAS/FERIADO",
                                cliente_tema = "CIPOLATTI",
                                data = item.data,
                                semana = item.semana,
                                observacao = "FERIADO/FÉRIAS - AUTOMÁTICO VIA SISTEMA",
                                cadastro_por = Environment.UserName,
                                cadastro_data = DateTime.Now,
                                hora_trabalhada = item.hora_minima
                            });
                    }

                    //await dbContext.ApontamentoHoras.BulkInsertAsync(apontamentoHoraModelArray);
                    foreach (var item in apontamentoHoraModelArray)
                        await dbContext.ApontamentoHoras.AddAsync(item);

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

        public async Task GravarHorasNaoFuncionarioProjetosAsync(FuncionarioProjetosModel funcionario, string observacao)
        {
            using var dbContext = new DatabaseContext();
            var executionStrategy = dbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = dbContext.Database.BeginTransaction();
                try
                {
                    var resultado = await dbContext.FuncionarioProjetos
                        .Join(dbContext.DataPlanProjetos,
                              funcionario => funcionario.cod_func,
                              dataplan => dataplan.codfun,
                              (funcionario, dataplan) => new { Funcionario = funcionario, DataPlan = dataplan })
                        .Join(dbContext.DataPlanejamentos,
                              dataPlan => dataPlan.DataPlan.dia,
                              dataPlanejamento => dataPlanejamento.dia,
                              (dataPlan, dataPlanejamento) => new
                              {
                                  dataPlan.Funcionario.cod_func,
                                  dataPlanejamento.dia,
                                  dataPlanejamento.data,
                                  dataPlan.Funcionario.incio_ferias,
                                  dataPlan.Funcionario.termino_ferias,
                                  dataPlan.DataPlan.hora_minima,
                                  dataPlan.Funcionario.nome_func,
                                  dataPlanejamento.semana,
                                  dataPlan.Funcionario.departamento,
                                  dataPlan.Funcionario.data_demissao
                              })
                        .Where(resultado => resultado.data >= funcionario.incio_ferias && resultado.data <= funcionario.termino_ferias && resultado.cod_func == funcionario.cod_func)
                        .ToListAsync();

                    //var apontamentoHoraModelArray = new ApontamentoHoraModel[]

                    var apontamentoHoraModelArray = new ObservableCollection<ApontamentoHoraModel>();
                    foreach (var item in resultado)
                    {
                        apontamentoHoraModelArray.Add(
                            new ApontamentoHoraModel
                            {
                                cod_func = item.cod_func,
                                cod_atividade = 28,
                                desc_atividade = "HORA NÃO TRABALHADA",
                                cliente_tema = "CIPOLATTI",
                                data = item.data,
                                semana = item.semana,
                                observacao = observacao,
                                cadastro_por = Environment.UserName,
                                cadastro_data = DateTime.Now,
                                hora_trabalhada = item.hora_minima
                            });
                    }

                    //await dbContext.ApontamentoHoras.BulkInsertAsync(apontamentoHoraModelArray);
                    foreach (var item in apontamentoHoraModelArray)
                        await dbContext.ApontamentoHoras.AddAsync(item);

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


    }

    public static class ContextMenuCommands
    {
        static BaseCommand cut;
        public static BaseCommand Cut
        {
            get
            {
                cut ??= new BaseCommand(OnCutClicked);
                return cut;
            }
        }

        private async static void OnCutClicked(object obj)
        {
            var grid = ((GridRecordContextMenuInfo)obj).DataGrid;
            var viewModel = grid.DataContext as CadastroFuncionarioProjetosViewModel;

            var iFerias = viewModel.FuncProjeto.incio_ferias;
            var tFerias = viewModel.FuncProjeto.termino_ferias;

            if (iFerias > tFerias)
            {
                MessageBox.Show("DATA INICIAL FÉRIAS MAIOR QUE DATA TÉRMINO FÉRIAS", "Aponta férias");
                return;
            }
            else if (iFerias == null)
            {
                MessageBox.Show("DATA INICIAL DE FÉRIAS NÃO ESTA PREENCHIDA!", "Aponta férias");
                return;
            }
            else if (tFerias == null)
            {
                MessageBox.Show("DATA TÉRMINO DE FÉRIAS NÃO ESTA PREENCHIDA!", "Aponta férias");
                return;
            }

            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                await Task.Run(() => viewModel.GravarFeriasFuncionarioProjetosAsync(viewModel.FuncProjeto));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show("FÉRIAS APONTADA", "Aponta férias");
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        static BaseCommand nTrabalhada;
        public static BaseCommand NTrabalhada
        {
            get
            {
                nTrabalhada ??= new BaseCommand(OnApontarNTrabalhadaClicked);
                return nTrabalhada;
            }
        }

        private async static void OnApontarNTrabalhadaClicked(object obj)
        {
            var grid = ((GridRecordContextMenuInfo)obj).DataGrid;
            var viewModel = grid.DataContext as CadastroFuncionarioProjetosViewModel;

            var iFerias = viewModel.FuncProjeto.incio_ferias;
            var tFerias = viewModel.FuncProjeto.termino_ferias;

            if (iFerias > tFerias)
            {
                MessageBox.Show("DATA INICIAL MAIOR QUE DATA TÉRMINO", "Apontar");
                return;
            }
            else if (iFerias == null)
            {
                MessageBox.Show("DATA INICIAL NÃO ESTA PREENCHIDA!", "Apontar");
                return;
            }
            else if (tFerias == null)
            {
                MessageBox.Show("DATA TÉRMINO NÃO ESTA PREENCHIDA!", "Apontar");
                return;
            }

            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });


                RadWindow.Prompt(
                    "Informe a Observação",
                    async (object? sender, WindowClosedEventArgs e) =>
                    {
                        if (e.DialogResult == true)
                        {
                            await viewModel.GravarHorasNaoFuncionarioProjetosAsync(viewModel.FuncProjeto, e.PromptResult);
                            MessageBox.Show("HORAS APONTADA", "Apontar");
                        }
                    });

                //await Task.Run(() => viewModel.GravarHorasNaoFuncionarioProjetosAsync(viewModel.FuncProjeto));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }
    }
}
