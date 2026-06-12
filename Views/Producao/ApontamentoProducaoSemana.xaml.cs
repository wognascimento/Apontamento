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
    /// Interação lógica para ApontamentoProducaoSemana.xam
    /// </summary>
    public partial class ApontamentoProducaoSemana : UserControl
    {
        public ApontamentoProducaoSemana()
        {
            InitializeComponent();
            DataContext = new ApontamentoProducaoSemanaViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ApontamentoProducaoSemanaViewModel vm = (ApontamentoProducaoSemanaViewModel)DataContext;
                vm.Funcionarios = await Task.Run(vm.GetFuncionariosAsync);

                var semana = await Task.Run(() => vm.GetSemanaAsync(DateTime.Now.Date));
                SemanaApontamento.Text = semana?.semana.ToString() ?? string.Empty;

                if (semana?.semana is not null)
                    await CarregarSemanaAsync((int)semana.semana.Value);

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void SemanaApontamento_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(SemanaApontamento.Text, out var semana))
                    throw new InvalidOperationException("Semana inválida.");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                await CarregarSemanaAsync(semana);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async Task CarregarSemanaAsync(int semana)
        {
            ApontamentoProducaoSemanaViewModel vm = (ApontamentoProducaoSemanaViewModel)DataContext;
            vm.Semana = await Task.Run(() => vm.GetSemanasAsync(semana));
            vm.Hts = await Task.Run(() => vm.GetHtsAsync(semana));
            vm.FutoHts = await Task.Run(() => vm.GetFuroHtsAsync(vm.Semana));
        }

        private void DGDigitacao_AddingNewDataItem(object sender, GridViewAddingNewEventArgs e)
        {
            ApontamentoProducaoSemanaViewModel vm = (ApontamentoProducaoSemanaViewModel)DataContext;
            if (vm.Semana is null)
                throw new InvalidOperationException("Semana não identificada.");

            e.NewObject = new HtModel
            {
                semana = vm.Semana.Semana,
                cadastrado_por = Environment.UserName,
                inclusao = DateTime.Now.Date
            };
        }

        private void DGDigitacao_RowValidating(object sender, GridViewRowValidatingEventArgs e)
        {
            if (e.Row.Item is not HtModel rowData)
                return;

            if (LinhaNovaVazia(rowData))
                return;

            if (!rowData.semana.HasValue)
            {
                AplicarErroLinha(e, "Semana não identificada.", "codfun");
            }
            else if (!rowData.codfun.HasValue)
            {
                AplicarErroLinha(e, "Informe O funcionário.", "codfun");
            }
            else if (!rowData.data.HasValue)
            {
                AplicarErroLinha(e, "Informa a data.", "data");
            }
            else if (!rowData.num_os.HasValue)
            {
                AplicarErroLinha(e, "Informe o número da O.S.", "num_os");
            }
            else if (!rowData.quantidadehorastrabalhadas.HasValue)
            {
                AplicarErroLinha(e, "Informe a quantidade de horas trabalhada.", "quantidadehorastrabalhadas");
            }
            else if (DataContext is ApontamentoProducaoSemanaViewModel { Semana: not null } vm &&
                     (rowData.data.Value.Date < vm.Semana.PrimeiraData.Date ||
                      rowData.data.Value.Date > vm.Semana.UltimaData.Date))
            {
                AplicarErroLinha(e, "Data fora da semana informada.", "data");
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

        private async void DGDigitacao_RowValidated(object sender, GridViewRowValidatedEventArgs e)
        {
            if (e.Row.Item is not HtModel ht)
                return;

            ApontamentoProducaoSemanaViewModel vm = (ApontamentoProducaoSemanaViewModel)DataContext;
            if (LinhaNovaVazia(ht))
            {
                RemoverLinhaNovaVazia(vm, ht);
                return;
            }

            var isNewRow = !ht.cod.HasValue;
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                if (ht.data.HasValue)
                    ht.data = ht.data.Value.Date;

                vm.Ht = await Task.Run(() => vm.AddApontamentoAsync(ht));
                vm.FutoHts = await Task.Run(() => vm.GetFuroHtsAsync(vm.Semana));
                ht.cod = vm.Ht.cod;
                DGDigitacao.Rebind();
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

        private void PosicionarNaLinhaDeAdicao()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DGDigitacao.Focus();
                DGDigitacao.BeginInsert();
            }));
        }

        private static bool LinhaNovaVazia(HtModel data)
        {
            return !data.cod.HasValue &&
                   !data.codfun.HasValue &&
                   !data.data.HasValue &&
                   !data.num_os.HasValue &&
                   !data.tarefas.HasValue &&
                   !data.quantidadehorastrabalhadas.HasValue &&
                   !data.quantidadeunidadeproduzida.HasValue &&
                   string.IsNullOrWhiteSpace(data.barcode) &&
                   string.IsNullOrWhiteSpace(data.obs) &&
                   !data.tbl_servicos_num_os.HasValue &&
                   !data.apontamnetos_num_os.HasValue;
        }

        private void RemoverLinhaNovaVazia(ApontamentoProducaoSemanaViewModel vm, HtModel data)
        {
            if (vm.Hts.Contains(data))
            {
                vm.Hts.Remove(data);
                DGDigitacao.Rebind();
            }
        }

        private void DGDigitacao_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return)
                return;

            e.Handled = true;

            if (Keyboard.FocusedElement is UIElement focusedElement)
            {
                var direction = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)
                    ? System.Windows.Input.FocusNavigationDirection.Previous
                    : System.Windows.Input.FocusNavigationDirection.Next;

                focusedElement.MoveFocus(new TraversalRequest(direction));
            }
        }
    }

    public class ApontamentoProducaoSemanaViewModel : INotifyPropertyChanged
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

        private ObservableCollection<DataSemanaModel> _semanas = new();
        public ObservableCollection<DataSemanaModel> Semanas
        {
            get { return _semanas; }
            set { _semanas = value; RaisePropertyChanged(nameof(Semanas)); }
        }

        private DataSemanaModel? _semana;
        public DataSemanaModel? Semana
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

        public async Task<DataSemanaModel?> GetSemanasAsync(int? semana)
        {
            using var db = DbConnectionFactory.Create();
            return await db.QueryFirstOrDefaultAsync<DataSemanaModel>(
                @"SELECT
                    semana::int AS Semana,
                    MIN(data)::timestamp AS PrimeiraData,
                    MAX(data)::timestamp AS UltimaData
                  FROM ht.t_data_planejamento
                  WHERE semana = @semana
                  GROUP BY semana
                  ORDER BY semana DESC;",
                new { semana });
        }

        public async Task<ObservableCollection<HtModel>> GetHtsAsync(double? semana)
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<HtModel>(
                @"SELECT * FROM ht.ht WHERE semana = @semana AND codfun IS NOT NULL ORDER BY cod;",
                new { semana });

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

        public async Task<ObservableCollection<QryHtFuroGeralModel>> GetFuroHtsAsync(DataSemanaModel? semana)
        {
            if (semana is null)
                return new ObservableCollection<QryHtFuroGeralModel>();

            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<QryHtFuroGeralModel>(
                @"SELECT *
                  FROM ht.qry_ht_furo_geral
                  WHERE data_ideal >= @PrimeiraData AND data_ideal <= @UltimaData
                  ORDER BY data_ideal, setor, nome_apelido;",
                semana);

            return new ObservableCollection<QryHtFuroGeralModel>(data);
        }

        private void RaisePropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
