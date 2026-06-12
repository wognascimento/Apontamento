using Apontamento.DataBase;
using Apontamento.DataBase.Model;
using Dapper;
using Npgsql;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Apontamento.Views.Producao
{
    /// <summary>
    /// Interação lógica para ApontamentoProducao.xam
    /// </summary>
    public partial class ApontamentoProducao : UserControl
    {
        public ApontamentoProducao()
        {
            InitializeComponent();
            DataContext = new ApontamentoProducaoViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ApontamentoProducaoViewModel vm = (ApontamentoProducaoViewModel)DataContext;
                vm.Funcionarios = await Task.Run(vm.GetFuncionariosAsync);

                dataApontamento.SelectedDate ??= DateTime.Today;
                await AtualizarSemanaAsync(dataApontamento.SelectedDate);

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void dataApontamento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                await AtualizarSemanaAsync(dataApontamento.SelectedDate);
                await CarregarApontamentosFuncionarioAsync();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex?.InnerException?.Message ?? ex.Message, "Erro ao inserir", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async Task AtualizarSemanaAsync(DateTime? data)
        {
            ApontamentoProducaoViewModel vm = (ApontamentoProducaoViewModel)DataContext;
            vm.Semana = await Task.Run(() => vm.GetSemanaAsync(data));
            SemanaApontamento.Text = vm.Semana?.semana.ToString() ?? string.Empty;
        }

        private async Task CarregarApontamentosFuncionarioAsync()
        {
            ApontamentoProducaoViewModel vm = (ApontamentoProducaoViewModel)DataContext;
            if (vm.Semana is null || vm.Funcionario is null)
                return;

            vm.Hts = await Task.Run(() => vm.GetHtsAsync(vm.Semana.data, vm.Funcionario.barcode));
            vm.FutoHts = await Task.Run(() => vm.GetFuroHtsAsync(vm.Funcionario.codfun));
        }

        private async void DGDigitacao_RowValidated(object sender, GridViewRowValidatedEventArgs e)
        {
            if (e.Row.Item is not HtModel data)
                return;

            ApontamentoProducaoViewModel vm = (ApontamentoProducaoViewModel)DataContext;
            if (LinhaNovaVazia(data))
            {
                RemoverLinhaNovaVazia(vm, data);
                return;
            }

            var isNewRow = !data.cod.HasValue;
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                vm.Ht = await Task.Run(() => vm.AddApontamentoAsync(data));
                data.cod = vm.Ht.cod;
                DGDigitacao.Rebind();
                vm.FutoHts = await Task.Run(() => vm.GetFuroHtsAsync(vm.Funcionario?.codfun));
                if (isNewRow)
                    PosicionarNaLinhaDeAdicao();

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message, "Erro ao inserir", MessageBoxButton.OK, MessageBoxImage.Error);

                var toRemove = vm.Hts.Where(x => x.cod == null).ToList();
                foreach (var item in toRemove)
                    vm.Hts.Remove(item);
            }
        }

        private void DGDigitacao_RowValidating(object sender, GridViewRowValidatingEventArgs e)
        {
            if (e.Row.Item is not HtModel rowData)
                return;

            if (LinhaNovaVazia(rowData))
                return;

            if (!rowData.num_os.HasValue)
            {
                AplicarErroLinha(e, "Informe o número da O.S.", "num_os");
            }
            else if (!rowData.quantidadehorastrabalhadas.HasValue)
            {
                AplicarErroLinha(e, "Informe a quantidade de horas trabalhada.", "quantidadehorastrabalhadas");
            }
        }

        private static void AplicarErroLinha(GridViewRowValidatingEventArgs e, string mensagem, string propertyName)
        {
            e.IsValid = false;
            e.ValidationResults.Add(new GridViewCellValidationResult
            {
                ErrorMessage = mensagem,
                PropertyName = propertyName
            });
        }

        private static bool LinhaNovaVazia(HtModel data)
        {
            return !data.cod.HasValue &&
                   !data.num_os.HasValue &&
                   !data.tarefas.HasValue &&
                   !data.quantidadehorastrabalhadas.HasValue &&
                   !data.quantidadeunidadeproduzida.HasValue &&
                   string.IsNullOrWhiteSpace(data.obs) &&
                   !data.tbl_servicos_num_os.HasValue &&
                   !data.apontamnetos_num_os.HasValue;
        }

        private void RemoverLinhaNovaVazia(ApontamentoProducaoViewModel vm, HtModel data)
        {
            if (vm.Hts.Contains(data))
            {
                vm.Hts.Remove(data);
                DGDigitacao.Rebind();
            }
        }

        private void PosicionarNaLinhaDeAdicao()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DGDigitacao.Focus();
                DGDigitacao.BeginInsert();
            }));
        }

        private async void OnFuncionariosSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                await CarregarApontamentosFuncionarioAsync();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro ao inserir", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void DGDigitacao_AddingNewDataItem(object sender, GridViewAddingNewEventArgs e)
        {
            ApontamentoProducaoViewModel vm = (ApontamentoProducaoViewModel)DataContext;
            if (vm.Semana is null || vm.Funcionario is null)
                throw new InvalidOperationException("Selecione a data e o funcionário antes de inserir apontamento.");

            e.NewObject = new HtModel
            {
                data = vm.Semana.data,
                semana = vm.Semana.semana,
                barcode = vm.Funcionario.barcode,
                codfun = vm.Funcionario.codfun,
                cadastrado_por = Environment.UserName,
                inclusao = DateTime.Now
            };
        }

        private async void DGFuro_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ApontamentoProducaoViewModel vm = (ApontamentoProducaoViewModel)DataContext;
                if (vm.FuroHt is null)
                    return;

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                dataApontamento.SelectedDate = vm.FuroHt.data_ideal;
                await AtualizarSemanaAsync(vm.FuroHt.data_ideal);
                await CarregarApontamentosFuncionarioAsync();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex?.InnerException?.Message ?? ex.Message, "Erro ao inserir", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }
    }

    public class ApontamentoProducaoViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private ObservableCollection<FuncionarioAtivoModel> _funcionarios = new();
        public ObservableCollection<FuncionarioAtivoModel> Funcionarios
        {
            get { return _funcionarios; }
            set { _funcionarios = value; RaisePropertyChanged(nameof(Funcionarios)); }
        }

        private FuncionarioAtivoModel? _funcionario;
        public FuncionarioAtivoModel? Funcionario
        {
            get { return _funcionario; }
            set { _funcionario = value; RaisePropertyChanged(nameof(Funcionario)); }
        }

        private ObservableCollection<HtModel> _hts = new();
        public ObservableCollection<HtModel> Hts
        {
            get { return _hts; }
            set { _hts = value; RaisePropertyChanged(nameof(Hts)); }
        }

        private HtModel? _ht;
        public HtModel? Ht
        {
            get { return _ht; }
            set { _ht = value; RaisePropertyChanged(nameof(Ht)); }
        }

        private DataPlanejamentoModel? _semana;
        public DataPlanejamentoModel? Semana
        {
            get { return _semana; }
            set { _semana = value; RaisePropertyChanged(nameof(Semana)); }
        }

        private ObservableCollection<QryHtFuroGeralModel> _futoHts = new();
        public ObservableCollection<QryHtFuroGeralModel> FutoHts
        {
            get { return _futoHts; }
            set { _futoHts = value; RaisePropertyChanged(nameof(FutoHts)); }
        }

        private QryHtFuroGeralModel? _furoHt;
        public QryHtFuroGeralModel? FuroHt
        {
            get { return _furoHt; }
            set { _furoHt = value; RaisePropertyChanged(nameof(FuroHt)); }
        }

        public async Task<ObservableCollection<FuncionarioAtivoModel>> GetFuncionariosAsync()
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<FuncionarioAtivoModel>(
                @"SELECT * FROM ht.funcionarios_ativos_geral ORDER BY nome_apelido;");

            return new ObservableCollection<FuncionarioAtivoModel>(data);
        }

        public async Task<DataPlanejamentoModel?> GetSemanaAsync(DateTime? dataApontamento)
        {
            using var db = DbConnectionFactory.Create();
            return await db.QueryFirstOrDefaultAsync<DataPlanejamentoModel>(
                @"SELECT * FROM ht.t_data_planejamento WHERE data = @dataApontamento;",
                new { dataApontamento });
        }

        public async Task<ObservableCollection<HtModel>> GetHtsAsync(DateTime? date, string? barcode)
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<HtModel>(
                @"SELECT * FROM ht.ht WHERE data = @date AND barcode = @barcode ORDER BY cod;",
                new { date, barcode });

            return new ObservableCollection<HtModel>(data);
        }

        public async Task<HtModel> AddApontamentoAsync(HtModel ht)
        {
            try
            {
                using var db = DbConnectionFactory.Create();
                if (ht.cod.HasValue && await db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM ht.ht WHERE cod = @cod;", ht) > 0)
                {
                    await db.ExecuteAsync(
                        @"UPDATE ht.ht SET
                            data = @data,
                            num_os = @num_os,
                            tarefas = @tarefas,
                            quantidadehorastrabalhadas = @quantidadehorastrabalhadas,
                            quantidadeunidadeproduzida = @quantidadeunidadeproduzida,
                            semana = @semana,
                            alterado_por = @alterado_por,
                            alteracao = COALESCE(@alteracao, now()),
                            barcode = @barcode,
                            obs = @obs,
                            tbl_servicos_num_os = @tbl_servicos_num_os,
                            apontamnetos_num_os = @apontamnetos_num_os,
                            codfun = @codfun
                          WHERE cod = @cod;",
                        ht);
                }
                else
                {
                    ht.cod = await db.ExecuteScalarAsync<long>(
                        @"INSERT INTO ht.ht
                            (data, num_os, tarefas, quantidadehorastrabalhadas, quantidadeunidadeproduzida,
                             semana, cadastrado_por, inclusao, alterado_por, alteracao, barcode, obs,
                             tbl_servicos_num_os, apontamnetos_num_os, codfun)
                          VALUES
                            (@data, @num_os, @tarefas, @quantidadehorastrabalhadas, @quantidadeunidadeproduzida,
                             @semana, @cadastrado_por, @inclusao, @alterado_por, @alteracao, @barcode, @obs,
                             @tbl_servicos_num_os, @apontamnetos_num_os, @codfun)
                          RETURNING cod;",
                        ht);
                }

                return ht;
            }
            catch (NpgsqlException)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<QryHtFuroGeralModel>> GetFuroHtsAsync(long? codfun)
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<QryHtFuroGeralModel>(
                @"SELECT * FROM ht.qry_ht_furo_geral WHERE codfun = @codfun ORDER BY data_ideal;",
                new { codfun });

            return new ObservableCollection<QryHtFuroGeralModel>(data);
        }

        private void RaisePropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
