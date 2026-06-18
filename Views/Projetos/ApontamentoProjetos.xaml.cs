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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Apontamento.Views.Projetos
{
    public partial class ApontamentoProjetos : UserControl
    {
        private readonly string _departamento;
        private bool _centroCustoFiltroConectado;

        public ApontamentoProjetos(string departamento)
        {
            DataContext = new ApontamentoProjetosViewModel();
            InitializeComponent();
            _departamento = departamento;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var vm = (ApontamentoProjetosViewModel)DataContext;
                vm.FuncProjeto = await vm.GetFuncionarioAsync(_departamento);
                if (vm.FuncProjeto == null)
                {
                    MessageBox.Show($@"Funcionario nao pertence ao departamento de {_departamento}.");
                    ((MainWindow)Application.Current.MainWindow).FecharFilho(this);
                    return;
                }

                nomeFuncionario.Text = vm.FuncProjeto?.nome_func;
                funcaoFuncionario.Text = vm.FuncProjeto?.funcao_func;
                vm.CentroCustos = await vm.GetCentroCustoAsync(_departamento);
                await vm.PreloadClienteTemaAsync();
                ConectarFiltroCentroCustoApontamento();
                dataApontamento.SelectedDate = DateTime.Now.Date;
                vm.HorasApontadas = await vm.GetHorasApontadasAsync(DateTime.Now.Date, vm.FuncProjeto?.cod_func);
                vm.FuroProjetos = await vm.GetFuroProjetosAsync(vm.FuncProjeto?.cod_func);
                cmbIdTarefa.Focus();
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

        private async void OncmbIdTarefaSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var centroCusto = ObterTextoCombo(cmbIdTarefa);
                if (string.IsNullOrWhiteSpace(centroCusto))
                    return;

                Mouse.OverrideCursor = Cursors.Wait;
                var vm = (ApontamentoProjetosViewModel)DataContext;
                vm.Atividades = await vm.GetAtividadesAsync(centroCusto, _departamento);
                cmbAtividade.Text = string.Empty;
                centroCustoApontamento.Text = string.Empty;
                DescricaoAtividade.Text = string.Empty;
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

        private void OncmbAtividadeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var vm = (ApontamentoProjetosViewModel)DataContext;
                centroCustoApontamento.Text = string.Empty;
                centroCustoApontamento.SelectedItem = null;
                DescricaoAtividade.Text = string.Empty;

                if (vm?.Atividade?.tipo_custo == "CLIENTE")
                {
                    vm.DefinirTipoCustoApontamento("CLIENTE");
                    vm.FiltrarCentroCustoApontamento(string.Empty);
                    centroCustoApontamento.IsEnabled = true;
                }
                else if (vm?.Atividade?.tipo_custo == "TEMA")
                {
                    vm.DefinirTipoCustoApontamento("TEMA");
                    vm.FiltrarCentroCustoApontamento(string.Empty);
                    centroCustoApontamento.IsEnabled = true;
                }
                else if (vm?.Atividade?.tipo_custo == "CIPOLATTI")
                {
                    vm.DefinirTipoCustoApontamento(null);
                    centroCustoApontamento.Text = "CIPOLATTI";
                    centroCustoApontamento.IsEnabled = false;
                    dataApontamento.Focus();
                }
                else
                {
                    vm.DefinirTipoCustoApontamento(null);
                    centroCustoApontamento.IsEnabled = true;
                }

                if (vm?.Atividade?.observao_obrigatoria?.Trim() == "1")
                    DescricaoAtividade.Text = vm.Atividade.descricao_atividade;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OncentroCustoApontamentoSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void OncentroCustoApontamentoTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (DataContext is not ApontamentoProjetosViewModel vm)
                    return;

                if (string.IsNullOrWhiteSpace(vm.TipoCustoApontamentoAtual))
                    return;

                vm.FiltrarCentroCustoApontamento(centroCustoApontamento.Text);
                centroCustoApontamento.IsDropDownOpen = centroCustoApontamento.IsKeyboardFocusWithin
                    && vm.CentroCustoApontamentoOpcoes.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ConectarFiltroCentroCustoApontamento()
        {
            if (_centroCustoFiltroConectado)
                return;

            if (centroCustoApontamento.Template?.FindName("PART_EditableTextBox", centroCustoApontamento) is TextBox editableTextBox)
            {
                editableTextBox.TextChanged += OncentroCustoApontamentoTextChanged;
                _centroCustoFiltroConectado = true;
            }
        }

        private async void OnApontarHoraClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var vm = (ApontamentoProjetosViewModel)DataContext;
                var baseSettings = DataBaseSettings.Instance;
                var dtApontamento = dataApontamento.SelectedDate;
                if (!dtApontamento.HasValue)
                    throw new InvalidOperationException("Selecione a data do apontamento.");

                var semana = await vm.GetSemanaAsync(dtApontamento.Value.Date);
                if (semana == null)
                {
                    MessageBox.Show("Nao foi possivel identificar a semana da data selecionada. Selecione a data novamente e tente gravar.", "Semana");
                    return;
                }

                if (vm.Atividade == null)
                    throw new InvalidOperationException("Selecione a atividade.");

                var apontamento = new ApontamentoHoraModel
                {
                    cod_func = vm.FuncProjeto.cod_func,
                    cod_atividade = vm.Atividade.codigo_tarefa,
                    desc_atividade = vm.Atividade.descricao_atividade,
                    cliente_tema = ObterTextoCombo(centroCustoApontamento),
                    data = dtApontamento,
                    semana = semana.semana,
                    observacao = observacao.Text,
                    cadastro_por = baseSettings.Username,
                    cadastro_data = DateTime.Now,
                    hora_trabalhada = totalHora.Value
                };

                await vm.SetApontarHoraAsync(apontamento);

                observacao.Text = string.Empty;
                DescricaoAtividade.Text = string.Empty;
                totalHora.Value = null;
                centroCustoApontamento.Text = string.Empty;
                cmbAtividade.Text = string.Empty;
                cmbIdTarefa.Text = string.Empty;
                cmbIdTarefa.Focus();

                vm.HorasApontadas = await vm.GetHorasApontadasAsync(apontamento.data.Value.Date, vm.FuncProjeto.cod_func);
                vm.FuroProjetos = await vm.GetFuroProjetosAsync(vm.FuncProjeto.cod_func);
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

        private async void OnRowValidated(object sender, GridViewRowValidatedEventArgs e)
        {
            try
            {
                if (e.Row.Item is not QryApontamentoHoraModel horaApontada)
                    return;

                Mouse.OverrideCursor = Cursors.Wait;
                var vm = (ApontamentoProjetosViewModel)DataContext;
                var apontamento = await vm.GetHoraApontadaAsync(horaApontada.cod_linha);
                apontamento.hora_trabalhada = horaApontada.hora_trabalhada;
                apontamento.observacao = horaApontada.observacao;
                await vm.SetApontarHoraAsync(apontamento);
                vm.HorasApontadas = await vm.GetHorasApontadasAsync(apontamento.data.Value, horaApontada.cod_func);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex?.InnerException?.Message ?? ex.Message, "Erro ao inserir", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private async void dataApontamento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var vm = (ApontamentoProjetosViewModel)DataContext;
                if (vm.FuncProjeto == null)
                    return;

                Mouse.OverrideCursor = Cursors.Wait;
                vm.HorasApontadas = await vm.GetHorasApontadasAsync(dataApontamento.SelectedDate, vm.FuncProjeto.cod_func);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex?.InnerException?.Message ?? ex.Message, "Erro ao inserir", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private static string ObterTextoCombo(RadComboBox combo)
        {
            return combo.SelectedItem switch
            {
                CentroCustoApontamentoOption opcao => opcao.Text,
                TarefaProjetosModel tarefa => tarefa.atividade,
                CentroCustoOption centroCusto => centroCusto.Key,
                _ => combo.Text
            };
        }
    }

    public class ApontamentoProjetosViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void RaisePropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private ObservableCollection<object> _centroCustos = [];
        public ObservableCollection<object> CentroCustos
        {
            get => _centroCustos;
            set { _centroCustos = value; RaisePropertyChanged(nameof(CentroCustos)); }
        }

        private TarefaProjetosModel? _atividade;
        public TarefaProjetosModel? Atividade
        {
            get => _atividade;
            set { _atividade = value; RaisePropertyChanged(nameof(Atividade)); }
        }

        private ObservableCollection<TarefaProjetosModel> _atividades = [];
        public ObservableCollection<TarefaProjetosModel> Atividades
        {
            get => _atividades;
            set { _atividades = value; RaisePropertyChanged(nameof(Atividades)); }
        }

        private ObservableCollection<TemaModel> _temas = [];
        public ObservableCollection<TemaModel> Temas
        {
            get => _temas;
            set { _temas = value; RaisePropertyChanged(nameof(Temas)); }
        }

        private ObservableCollection<ClienteModel> _clientes = [];
        public ObservableCollection<ClienteModel> Clientes
        {
            get => _clientes;
            set { _clientes = value; RaisePropertyChanged(nameof(Clientes)); }
        }

        private ObservableCollection<CentroCustoApontamentoOption> _centroCustoApontamentoOpcoes = [];
        public ObservableCollection<CentroCustoApontamentoOption> CentroCustoApontamentoOpcoes
        {
            get => _centroCustoApontamentoOpcoes;
            set { _centroCustoApontamentoOpcoes = value; RaisePropertyChanged(nameof(CentroCustoApontamentoOpcoes)); }
        }

        private string? _tipoCustoApontamentoAtual;
        public string? TipoCustoApontamentoAtual => _tipoCustoApontamentoAtual;
        private CentroCustoApontamentoOption[] _clienteOpcoesOrdenadas = [];
        private CentroCustoApontamentoOption[] _temaOpcoesOrdenadas = [];
        private const int MaximoOpcoesCentroCusto = 200;

        private FuncionarioProjetosModel? _funcProjeto;
        public FuncionarioProjetosModel? FuncProjeto
        {
            get => _funcProjeto;
            set { _funcProjeto = value; RaisePropertyChanged(nameof(FuncProjeto)); }
        }

        private QryApontamentoHoraModel? _horaApontada;
        public QryApontamentoHoraModel? HoraApontada
        {
            get => _horaApontada;
            set { _horaApontada = value; RaisePropertyChanged(nameof(HoraApontada)); }
        }

        private ObservableCollection<QryApontamentoHoraModel> _horasApontadas = [];
        public ObservableCollection<QryApontamentoHoraModel> HorasApontadas
        {
            get => _horasApontadas;
            set { _horasApontadas = value; RaisePropertyChanged(nameof(HorasApontadas)); }
        }

        private ObservableCollection<QryFuroApontamentoProjeto> _furoProjetos = [];
        public ObservableCollection<QryFuroApontamentoProjeto> FuroProjetos
        {
            get => _furoProjetos;
            set { _furoProjetos = value; RaisePropertyChanged(nameof(FuroProjetos)); }
        }

        public async Task<ObservableCollection<object>> GetCentroCustoAsync(string departamento)
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<string>(
                @"SELECT DISTINCT identificacao
                  FROM ht.t_tarefas_projetos
                  WHERE inativo = '0' AND departamento = @departamento
                  ORDER BY identificacao;",
                new { departamento });
            return new ObservableCollection<object>(data.Select(x => new CentroCustoOption(x)));
        }

        public async Task<ObservableCollection<TarefaProjetosModel>> GetAtividadesAsync(string centroCusto, string departamento)
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<TarefaProjetosModel>(
                @"SELECT *
                  FROM ht.t_tarefas_projetos
                  WHERE inativo = '0'
                    AND identificacao = @centroCusto
                    AND departamento = @departamento
                  ORDER BY atividade;",
                new { centroCusto, departamento });
            return new ObservableCollection<TarefaProjetosModel>(data);
        }

        public async Task<ObservableCollection<TemaModel>> GetTemasAsync()
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<TemaModel>(
                @"SELECT temas FROM comercial.temas ORDER BY temas;");
            return new ObservableCollection<TemaModel>(data);
        }

        public async Task<ObservableCollection<ClienteModel>> GetClientesAsync()
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<ClienteModel>(
                @"SELECT sigla FROM comercial.clientes ORDER BY sigla;");
            return new ObservableCollection<ClienteModel>(data);
        }

        public async Task PreloadClienteTemaAsync()
        {
            if (Clientes.Count == 0)
            {
                Clientes = await GetClientesAsync();
            }

            if (Temas.Count == 0)
            {
                Temas = await GetTemasAsync();
            }

            if (_clienteOpcoesOrdenadas.Length == 0)
            {
                _clienteOpcoesOrdenadas = Clientes
                    .Where(x => !string.IsNullOrWhiteSpace(x.sigla))
                    .Select(x => new CentroCustoApontamentoOption(x.sigla!, "CLIENTE"))
                    .OrderBy(x => x.NormalizedText, StringComparer.Ordinal)
                    .ToArray();
            }

            if (_temaOpcoesOrdenadas.Length == 0)
            {
                _temaOpcoesOrdenadas = Temas
                    .Where(x => !string.IsNullOrWhiteSpace(x.temas))
                    .Select(x => new CentroCustoApontamentoOption(x.temas!, "TEMA"))
                    .OrderBy(x => x.NormalizedText, StringComparer.Ordinal)
                    .ToArray();
            }
        }

        public void DefinirTipoCustoApontamento(string? tipoCusto)
        {
            _tipoCustoApontamentoAtual = tipoCusto;
            CentroCustoApontamentoOpcoes.Clear();
            RaisePropertyChanged(nameof(TipoCustoApontamentoAtual));
        }

        public void FiltrarCentroCustoApontamento(string? texto)
        {
            CentroCustoApontamentoOpcoes.Clear();

            var origem = ObterOpcoesCentroCusto();
            if (origem.Length == 0)
            {
                return;
            }

            var termo = CentroCustoApontamentoOption.Normalize(texto);
            if (string.IsNullOrWhiteSpace(termo))
            {
                foreach (var opcao in origem.Take(MaximoOpcoesCentroCusto))
                {
                    CentroCustoApontamentoOpcoes.Add(opcao);
                }

                return;
            }

            var indiceInicial = LocalizarPrimeiroIndice(origem, termo);
            for (var i = indiceInicial; i < origem.Length && CentroCustoApontamentoOpcoes.Count < MaximoOpcoesCentroCusto; i++)
            {
                var opcao = origem[i];
                if (!opcao.NormalizedText.StartsWith(termo, StringComparison.Ordinal))
                    break;

                CentroCustoApontamentoOpcoes.Add(opcao);
            }
        }

        private CentroCustoApontamentoOption[] ObterOpcoesCentroCusto()
        {
            return _tipoCustoApontamentoAtual switch
            {
                "CLIENTE" => _clienteOpcoesOrdenadas,
                "TEMA" => _temaOpcoesOrdenadas,
                _ => []
            };
        }

        private static int LocalizarPrimeiroIndice(CentroCustoApontamentoOption[] origem, string termo)
        {
            var esquerda = 0;
            var direita = origem.Length - 1;
            var resultado = origem.Length;

            while (esquerda <= direita)
            {
                var meio = esquerda + ((direita - esquerda) / 2);
                var comparacao = string.Compare(origem[meio].NormalizedText, termo, StringComparison.Ordinal);

                if (comparacao >= 0)
                {
                    resultado = meio;
                    direita = meio - 1;
                }
                else
                {
                    esquerda = meio + 1;
                }
            }

            return resultado == origem.Length ? 0 : resultado;
        }

        public async Task<FuncionarioProjetosModel?> GetFuncionarioAsync(string departamento)
        {
            var baseSettings = DataBaseSettings.Instance;
            using var db = DbConnectionFactory.Create();
            return await db.QueryFirstOrDefaultAsync<FuncionarioProjetosModel>(
                @"SELECT *
                  FROM ht.t_funcionario_proj
                  WHERE nick_name_func = @Username AND departamento = @departamento
                  ORDER BY nome_func
                  LIMIT 1;",
                new { baseSettings.Username, departamento });
        }

        public async Task<ApontamentoHoraModel?> GetHoraApontadaAsync(long? codLinha)
        {
            using var db = DbConnectionFactory.Create();
            return await db.QueryFirstOrDefaultAsync<ApontamentoHoraModel>(
                @"SELECT * FROM ht.t_apontamento_horas WHERE cod_linha = @codLinha;",
                new { codLinha });
        }

        public async Task SetApontarHoraAsync(ApontamentoHoraModel apontamento)
        {
            using var db = DbConnectionFactory.Create();
            if (apontamento.cod_linha.HasValue &&
                await db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM ht.t_apontamento_horas WHERE cod_linha = @cod_linha;", apontamento) > 0)
            {
                apontamento.alteracao_data = DateTime.Now;
                apontamento.alteracao_por = Environment.UserName;
                await db.ExecuteAsync(
                    @"UPDATE ht.t_apontamento_horas SET
                        cod_func = @cod_func,
                        cod_atividade = @cod_atividade,
                        desc_atividade = @desc_atividade,
                        cliente_tema = @cliente_tema,
                        data = @data,
                        semana = @semana,
                        hora_inicio = @hora_inicio,
                        hora_fim = @hora_fim,
                        total_hora = @total_hora,
                        observacao = @observacao,
                        intervalo = @intervalo,
                        alteracao_data = @alteracao_data,
                        alteracao_por = @alteracao_por,
                        hora_trabalhada = @hora_trabalhada
                      WHERE cod_linha = @cod_linha;",
                    apontamento);
            }
            else
            {
                apontamento.cod_linha = await db.ExecuteScalarAsync<long>(
                    @"INSERT INTO ht.t_apontamento_horas
                        (cod_func, cod_atividade, desc_atividade, cliente_tema, data, semana,
                         hora_inicio, hora_fim, total_hora, observacao, cadastro_por,
                         cadastro_data, intervalo, alteracao_data, alteracao_por, hora_trabalhada)
                      VALUES
                        (@cod_func, @cod_atividade, @desc_atividade, @cliente_tema, @data, @semana,
                         @hora_inicio, @hora_fim, @total_hora, @observacao, @cadastro_por,
                         @cadastro_data, @intervalo, @alteracao_data, @alteracao_por, @hora_trabalhada)
                      RETURNING cod_linha;",
                    apontamento);
            }
        }

        public async Task<DataPlanejamentoModel?> GetSemanaAsync(DateTime? dataApontamento)
        {
            using var db = DbConnectionFactory.Create();
            return await db.QueryFirstOrDefaultAsync<DataPlanejamentoModel>(
                @"SELECT * FROM ht.t_data_planejamento WHERE data = @dataApontamento;",
                new { dataApontamento });
        }

        public async Task<ObservableCollection<QryApontamentoHoraModel>> GetHorasApontadasAsync(DateTime? dataApontamento, long? cod_func)
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<QryApontamentoHoraModel>(
                @"SELECT *
                  FROM ht.qry_apontamento_horas
                  WHERE cod_func = @cod_func AND data = @dataApontamento
                  ORDER BY cod_linha;",
                new { dataApontamento, cod_func });
            return new ObservableCollection<QryApontamentoHoraModel>(data);
        }

        public async Task<ObservableCollection<QryFuroApontamentoProjeto>> GetFuroProjetosAsync(long? cod_func)
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<QryFuroApontamentoProjeto>(
                @"SELECT *
                  FROM ht.qryfurohtprojetos
                  WHERE codfun = @cod_func
                  ORDER BY data;",
                new { cod_func });
            return new ObservableCollection<QryFuroApontamentoProjeto>(data);
        }
    }

    public sealed record CentroCustoOption(string Key);

    public sealed record CentroCustoApontamentoOption
    {
        public CentroCustoApontamentoOption(string text, string tipo)
        {
            Text = text;
            Tipo = tipo;
            NormalizedText = Normalize(text);
        }

        public string Text { get; }
        public string Tipo { get; }
        public string NormalizedText { get; }

        public static string Normalize(string? value)
        {
            return (value ?? string.Empty).Trim().ToUpperInvariant();
        }
    }
}
