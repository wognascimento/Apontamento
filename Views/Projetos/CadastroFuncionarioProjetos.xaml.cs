using Apontamento.DataBase;
using Apontamento.DataBase.Model;
using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Apontamento.Views.Projetos
{
    public partial class CadastroFuncionarioProjetos : UserControl
    {
        private readonly string _departamento;

        public CadastroFuncionarioProjetos(string departamento)
        {
            InitializeComponent();
            DataContext = new CadastroFuncionarioProjetosViewModel();
            _departamento = departamento;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var vm = (CadastroFuncionarioProjetosViewModel)DataContext;
                vm.FuncProjetos = await vm.GetFuncionariosProjetoAsync(_departamento);
                vm.Funcs = await vm.GetFuncionariosAsync(_departamento);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void dgFuncionarios_AddingNewDataItem(object sender, GridViewAddingNewEventArgs e)
        {
            e.NewObject = new FuncionarioProjetosModel { departamento = _departamento };
        }

        private void dgFuncionarios_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            if (e.Cell?.Column is not GridViewComboBoxColumn || e.Cell.DataContext is not FuncionarioProjetosModel funcionarioProjeto)
                return;

            var vm = (CadastroFuncionarioProjetosViewModel)DataContext;
            var funcionario = vm.Funcs?.FirstOrDefault(f => f.codfun == funcionarioProjeto.cod_func);
            if (funcionario is null)
                return;

            funcionarioProjeto.nome_func = funcionario.nome_apelido;
            funcionarioProjeto.funcao_func = funcionario.funcao;
            dgFuncionarios.Rebind();
        }

        private async void dgFuncionarios_RowValidated(object sender, GridViewRowValidatedEventArgs e)
        {
            try
            {
                if (e.Row.Item is not FuncionarioProjetosModel funcionario)
                    return;

                var vm = (CadastroFuncionarioProjetosViewModel)DataContext;
                var existe = vm.FuncProjetos.Any(f => f != funcionario && f.cod_func == funcionario.cod_func);

                if (existe)
                    await vm.AlterarFuncionarioProjetosAsync(funcionario);
                else
                    await vm.GravarFuncionarioProjetosAsync(funcionario);

                vm.FuncProjetos = await vm.GetFuncionariosProjetoAsync(_departamento);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dgFuncionarios_RowValidating(object sender, GridViewRowValidatingEventArgs e)
        {
            if (e.Row.Item is not FuncionarioProjetosModel rowData)
                return;

            if (!rowData.cod_func.HasValue)
            {
                e.IsValid = false;
                e.ValidationResults.Add(new GridViewCellValidationResult { PropertyName = "cod_func", ErrorMessage = "Selecione o funcionario." });
            }
            else if (rowData.nick_name_func == null)
            {
                e.IsValid = false;
                e.ValidationResults.Add(new GridViewCellValidationResult { PropertyName = "nick_name_func", ErrorMessage = "Informe o login. Padrao e o nome seguido do ultimo nome." });
            }
            else if (!rowData.horas_minimas.HasValue)
            {
                e.IsValid = false;
                e.ValidationResults.Add(new GridViewCellValidationResult { PropertyName = "horas_minimas", ErrorMessage = "Informe a quantidade de hora minima de apontamento." });
            }
        }

        private async void FirstLevelNestedGrid_RowValidated(object sender, GridViewRowValidatedEventArgs e)
        {
            try
            {
                if (e.Row.Item is not DataPlanProjetoModel dataPlan)
                    return;

                var vm = (CadastroFuncionarioProjetosViewModel)DataContext;
                await vm.AlterarDataPlanejamentoFuncionarioProjetosAsync(dataPlan);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FirstLevelNestedGrid_RowValidating(object sender, GridViewRowValidatingEventArgs e)
        {
        }

        private async void ApontarFerias_Click(object sender, RoutedEventArgs e)
        {
            await ApontarFeriasAsync();
        }

        private async void ApontarNaoTrabalhada_Click(object sender, RoutedEventArgs e)
        {
            await ApontarNaoTrabalhadaAsync();
        }

        private async Task ApontarFeriasAsync()
        {
            if (DataContext is not CadastroFuncionarioProjetosViewModel viewModel || viewModel.FuncProjeto is null)
                return;

            var iFerias = viewModel.FuncProjeto.incio_ferias;
            var tFerias = viewModel.FuncProjeto.termino_ferias;

            if (iFerias > tFerias)
            {
                MessageBox.Show("DATA INICIAL FERIAS MAIOR QUE DATA TERMINO FERIAS", "Aponta ferias");
                return;
            }
            if (iFerias == null)
            {
                MessageBox.Show("DATA INICIAL DE FERIAS NAO ESTA PREENCHIDA!", "Aponta ferias");
                return;
            }
            if (tFerias == null)
            {
                MessageBox.Show("DATA TERMINO DE FERIAS NAO ESTA PREENCHIDA!", "Aponta ferias");
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                await viewModel.GravarFeriasFuncionarioProjetosAsync(viewModel.FuncProjeto);
                MessageBox.Show("FERIAS APONTADA", "Aponta ferias");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async Task ApontarNaoTrabalhadaAsync()
        {
            if (DataContext is not CadastroFuncionarioProjetosViewModel viewModel || viewModel.FuncProjeto is null)
                return;

            var iFerias = viewModel.FuncProjeto.incio_ferias;
            var tFerias = viewModel.FuncProjeto.termino_ferias;

            if (iFerias > tFerias)
            {
                MessageBox.Show("DATA INICIAL MAIOR QUE DATA TERMINO", "Apontar");
                return;
            }
            if (iFerias == null)
            {
                MessageBox.Show("DATA INICIAL NAO ESTA PREENCHIDA!", "Apontar");
                return;
            }
            if (tFerias == null)
            {
                MessageBox.Show("DATA TERMINO NAO ESTA PREENCHIDA!", "Apontar");
                return;
            }

            RadWindow.Prompt("Informe a Observacao", async (object? sender, WindowClosedEventArgs e) =>
            {
                if (e.DialogResult == true)
                {
                    await viewModel.GravarHorasNaoFuncionarioProjetosAsync(viewModel.FuncProjeto, e.PromptResult);
                    MessageBox.Show("HORAS APONTADA", "Apontar");
                }
            });
        }
    }

    public class CadastroFuncionarioProjetosViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private FuncionarioProjetosModel? _funcProjeto;
        public FuncionarioProjetosModel? FuncProjeto
        {
            get => _funcProjeto;
            set { _funcProjeto = value; RaisePropertyChanged(nameof(FuncProjeto)); }
        }

        private ObservableCollection<FuncionarioProjetosModel> _funcProjetos = [];
        public ObservableCollection<FuncionarioProjetosModel> FuncProjetos
        {
            get => _funcProjetos;
            set { _funcProjetos = value; RaisePropertyChanged(nameof(FuncProjetos)); }
        }

        private ObservableCollection<FuncionarioModel> _funcs = [];
        public ObservableCollection<FuncionarioModel> Funcs
        {
            get => _funcs;
            set { _funcs = value; RaisePropertyChanged(nameof(Funcs)); }
        }

        public async Task<ObservableCollection<FuncionarioProjetosModel>> GetFuncionariosProjetoAsync(string departamento)
        {
            using var db = DbConnectionFactory.Create();
            var funcionarios = (await db.QueryAsync<FuncionarioProjetosModel>(
                @"SELECT *
                  FROM ht.t_funcionario_proj
                  WHERE departamento = @departamento
                  ORDER BY nome_func;",
                new { departamento })).ToList();

            var planos = (await db.QueryAsync<DataPlanProjetoModel>(
                @"SELECT *
                  FROM ht.tbldataplanprojetos
                  WHERE codfun = ANY(@codigos)
                  ORDER BY codfun, dia;",
                new { codigos = funcionarios.Where(f => f.cod_func.HasValue).Select(f => f.cod_func!.Value).ToArray() }))
                .GroupBy(p => p.codfun)
                .ToDictionary(g => g.Key, g => (ICollection<DataPlanProjetoModel>)g.ToList());

            foreach (var funcionario in funcionarios)
                funcionario.PlanosProjetos = funcionario.cod_func.HasValue && planos.TryGetValue(funcionario.cod_func.Value, out var dataPlan)
                    ? dataPlan
                    : new List<DataPlanProjetoModel>();

            return new ObservableCollection<FuncionarioProjetosModel>(funcionarios);
        }

        public async Task<ObservableCollection<FuncionarioModel>> GetFuncionariosAsync(string departamento)
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<FuncionarioModel>(
                @"SELECT
                    codfun,
                    nome_apelido,
                    data_admissao,
                    data_demissao,
                    setor,
                    funcao,
                    local_galpao
                  FROM ht.funcionarios
                  WHERE data_demissao IS NULL
                    AND setor LIKE @setor
                  UNION ALL
                  SELECT
                    codfun,
                    nome_apelido,
                    admissao AS data_admissao,
                    demissao AS data_demissao,
                    setor,
                    'MEI'::character varying AS funcao,
                    local_galpao
                  FROM ht.funcionarios_mei
                  WHERE demissao IS NULL
                    AND setor LIKE @setor
                  ORDER BY nome_apelido;",
                new { setor = $"%{departamento}%" });
            return new ObservableCollection<FuncionarioModel>(data);
        }

        public async Task GravarFuncionarioProjetosAsync(FuncionarioProjetosModel funcionario)
        {
            using var db = DbConnectionFactory.Create();
            await db.OpenAsync();
            using var transaction = await db.BeginTransactionAsync();
            try
            {
                await db.ExecuteAsync(
                    @"INSERT INTO ht.t_funcionario_proj
                        (cod_func, nome_func, funcao_func, nick_name_func, tipo_contrato, horas_minimas,
                         incio_ferias, termino_ferias, data_demissao, departamento, ferias_apontadas, cracha)
                      VALUES
                        (@cod_func, @nome_func, @funcao_func, @nick_name_func, @tipo_contrato, @horas_minimas,
                         @incio_ferias, @termino_ferias, @data_demissao, @departamento, @ferias_apontadas, @cracha)
                      ON CONFLICT (cod_func) DO UPDATE SET
                        nome_func = EXCLUDED.nome_func,
                        funcao_func = EXCLUDED.funcao_func,
                        nick_name_func = EXCLUDED.nick_name_func,
                        tipo_contrato = EXCLUDED.tipo_contrato,
                        horas_minimas = EXCLUDED.horas_minimas,
                        incio_ferias = EXCLUDED.incio_ferias,
                        termino_ferias = EXCLUDED.termino_ferias,
                        data_demissao = EXCLUDED.data_demissao,
                        departamento = EXCLUDED.departamento,
                        ferias_apontadas = EXCLUDED.ferias_apontadas,
                        cracha = EXCLUDED.cracha;",
                    funcionario,
                    transaction);

                var planos = new DataPlanProjetoModel[]
                {
                    new() { codfun = (long)funcionario.cod_func!, dia = "SEGUNDA-FEIRA", hora_minima = (double)funcionario.horas_minimas! },
                    new() { codfun = (long)funcionario.cod_func!, dia = "TERÇA-FEIRA", hora_minima = (double)funcionario.horas_minimas! },
                    new() { codfun = (long)funcionario.cod_func!, dia = "QUARTA-FEIRA", hora_minima = (double)funcionario.horas_minimas! },
                    new() { codfun = (long)funcionario.cod_func!, dia = "QUINTA-FEIRA", hora_minima = (double)funcionario.horas_minimas! },
                    new() { codfun = (long)funcionario.cod_func!, dia = "SEXTA-FEIRA", hora_minima = (double)funcionario.horas_minimas! - 1 },
                };

                await db.ExecuteAsync(
                    @"INSERT INTO ht.tbldataplanprojetos (dia, hora_minima, codfun)
                      VALUES (@dia, @hora_minima, @codfun)
                      ON CONFLICT (codfun, dia) DO UPDATE
                      SET hora_minima = EXCLUDED.hora_minima;",
                    planos,
                    transaction);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AlterarFuncionarioProjetosAsync(FuncionarioProjetosModel funcionario)
        {
            using var db = DbConnectionFactory.Create();
            await db.ExecuteAsync(
                @"UPDATE ht.t_funcionario_proj SET
                    nome_func = @nome_func,
                    funcao_func = @funcao_func,
                    nick_name_func = @nick_name_func,
                    tipo_contrato = @tipo_contrato,
                    horas_minimas = @horas_minimas,
                    incio_ferias = @incio_ferias,
                    termino_ferias = @termino_ferias,
                    data_demissao = @data_demissao,
                    departamento = @departamento,
                    ferias_apontadas = @ferias_apontadas,
                    cracha = @cracha
                  WHERE cod_func = @cod_func;",
                funcionario);
        }

        public async Task AlterarDataPlanejamentoFuncionarioProjetosAsync(DataPlanProjetoModel dataPlanejamento)
        {
            using var db = DbConnectionFactory.Create();
            await db.ExecuteAsync(
                @"UPDATE ht.tbldataplanprojetos
                  SET hora_minima = @hora_minima
                  WHERE codfun = @codfun AND dia = @dia;",
                dataPlanejamento);
        }

        public async Task GravarFeriasFuncionarioProjetosAsync(FuncionarioProjetosModel funcionario)
        {
            using var db = DbConnectionFactory.Create();
            await db.OpenAsync();
            using var transaction = await db.BeginTransactionAsync();
            try
            {
                await db.ExecuteAsync(
                    @"INSERT INTO ht.t_apontamento_horas
                        (cod_func, cod_atividade, desc_atividade, cliente_tema, data, semana,
                         observacao, cadastro_por, cadastro_data, hora_trabalhada)
                      SELECT
                        fp.cod_func,
                        29,
                        'FERIAS/FERIADO',
                        'CIPOLATTI',
                        dp.data,
                        dp.semana,
                        'FERIADO/FERIAS - AUTOMATICO VIA SISTEMA',
                        @usuario,
                        now(),
                        pl.hora_minima
                      FROM ht.t_funcionario_proj fp
                      JOIN ht.tbldataplanprojetos pl ON pl.codfun = fp.cod_func
                      JOIN ht.t_data_planejamento dp ON dp.dia = pl.dia
                      WHERE fp.cod_func = @cod_func
                        AND dp.data >= @incio_ferias
                        AND dp.data <= @termino_ferias;",
                    new
                    {
                        funcionario.cod_func,
                        funcionario.incio_ferias,
                        funcionario.termino_ferias,
                        usuario = Environment.UserName
                    },
                    transaction);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task GravarHorasNaoFuncionarioProjetosAsync(FuncionarioProjetosModel funcionario, string observacao)
        {
            using var db = DbConnectionFactory.Create();
            await db.OpenAsync();
            using var transaction = await db.BeginTransactionAsync();
            try
            {
                await db.ExecuteAsync(
                    @"INSERT INTO ht.t_apontamento_horas
                        (cod_func, cod_atividade, desc_atividade, cliente_tema, data, semana,
                         observacao, cadastro_por, cadastro_data, hora_trabalhada)
                      SELECT
                        fp.cod_func,
                        28,
                        'HORA NAO TRABALHADA',
                        'CIPOLATTI',
                        dp.data,
                        dp.semana,
                        @observacao,
                        @usuario,
                        now(),
                        pl.hora_minima
                      FROM ht.t_funcionario_proj fp
                      JOIN ht.tbldataplanprojetos pl ON pl.codfun = fp.cod_func
                      JOIN ht.t_data_planejamento dp ON dp.dia = pl.dia
                      WHERE fp.cod_func = @cod_func
                        AND dp.data >= @incio_ferias
                        AND dp.data <= @termino_ferias;",
                    new
                    {
                        funcionario.cod_func,
                        funcionario.incio_ferias,
                        funcionario.termino_ferias,
                        observacao,
                        usuario = Environment.UserName
                    },
                    transaction);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
