using Apontamento.DataBase;
using Apontamento.DataBase.Model;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Windows.Controls.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Apontamento.Views.Projetos
{
    /// <summary>
    /// Interação lógica para ApontamentoProjetos.xam
    /// </summary>
    public partial class ApontamentoProjetos : UserControl
    {
        private readonly string _departamento;

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
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ApontamentoProjetosViewModel vm = (ApontamentoProjetosViewModel)DataContext;
                vm.FuncProjeto = await vm.GetFuncionarioAsync(_departamento);
                if (vm.FuncProjeto == null) 
                {
                    MessageBox.Show($@"Funcionário não pertence ao departamento de {_departamento}.");
                    //var tela = ((MainWindow)Application.Current.MainWindow)._mdi.Items.
                    ((MainWindow)Application.Current.MainWindow)._mdi.Items.Remove(this);
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    return;
                }
                nomeFuncionario.Text = vm.FuncProjeto.nome_func;
                funcaoFuncionario.Text = vm.FuncProjeto.funcao_func;
                vm.CentroCustos = await vm.GetCentroCustoAsync(_departamento);
                vm.HorasApontadas = await Task.Run(() => vm.GetHorasApontadasAsync(DateTime.Now.Date, vm.FuncProjeto.cod_func));
                vm.FuroProjetos = await Task.Run(() => vm.GetFuroProjetosAsync(vm.FuncProjeto.cod_func));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

                cmbIdTarefa.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OncmbIdTarefaSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                string centroCusto = cmbIdTarefa.Text;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ApontamentoProjetosViewModel vm = (ApontamentoProjetosViewModel)DataContext;
                vm.Atividades = await vm.GetAtividadesAsync(centroCusto, _departamento);
                cmbAtividade.Text = string.Empty;
                centroCustoApontamento.Text = string.Empty;
                DescricaoAtividade.Text = string.Empty;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OncmbAtividadeSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                string centroCusto = cmbAtividade.Text;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ApontamentoProjetosViewModel vm = (ApontamentoProjetosViewModel)DataContext;
                centroCustoApontamento.Text = string.Empty;
                DescricaoAtividade.Text = string.Empty;

                if (vm?.Atividade?.tipo_custo == "CLIENTE")
                {
                    vm.Clientes = await Task.Run(vm.GetClientesAsync);
                    centroCustoApontamento.SearchItemPath = "sigla";
                    centroCustoApontamento.ValueMemberPath = "sigla";
                    centroCustoApontamento.SetBinding(SfTextBoxExt.AutoCompleteSourceProperty, new Binding("Clientes"));
                    centroCustoApontamento.IsEnabled = true;
                }
                else if(vm?.Atividade?.tipo_custo == "TEMA")
                {
                    vm.Temas = await Task.Run(vm.GetTemasAsync);
                    centroCustoApontamento.SearchItemPath = "temas";
                    centroCustoApontamento.ValueMemberPath = "temas";
                    centroCustoApontamento.SetBinding(SfTextBoxExt.AutoCompleteSourceProperty, new Binding("Temas"));
                    centroCustoApontamento.IsEnabled = true;
                }
                else if(vm?.Atividade?.tipo_custo == "CIPOLATTI")
                {
                    centroCustoApontamento.SetBinding(SfTextBoxExt.AutoCompleteSourceProperty, new Binding(""));
                    centroCustoApontamento.Text = "CIPOLATTI";
                    centroCustoApontamento.IsEnabled = false;
                    dataApontamento.Focus();
                }

                if(vm?.Atividade?.observao_obrigatoria?.Trim() == "1")
                {
                    DescricaoAtividade.Text = vm?.Atividade?.descricao_atividade;
                }

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void OncentroCustoApontamentoSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        private async void OnApontarHoraClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ApontamentoProjetosViewModel vm = (ApontamentoProjetosViewModel)DataContext;
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                var dtApontamento = dataApontamento.DateTime;
                var semana = await Task.Run(() => vm.GetSemanaAsync(dtApontamento.Value.Date));

                if (semana == null)
                {
                    Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                    MessageBox.Show("Não foi possivel identificar a semana da data selecionada. \n Selecione a data novamente e tente Gravar.", "Semana");
                    return;
                }

                var apontamento = new ApontamentoHoraModel
                {
                    cod_func = vm.FuncProjeto.cod_func,
                    cod_atividade = vm.Atividade.codigo_tarefa,
                    desc_atividade = vm.Atividade.descricao_atividade,
                    cliente_tema = centroCustoApontamento.Text,
                    data = dataApontamento.DateTime,
                    semana = semana.semana,
                    observacao = observacao.Text,
                    cadastro_por = BaseSettings.Username,
                    cadastro_data = DateTime.Now,
                    hora_trabalhada = totalHora.Value
                };
                await Task.Run(() => vm.SetApontarHoraAsync(apontamento));

                observacao.Text = string.Empty;
                DescricaoAtividade.Text = string.Empty;
                totalHora.Value = null;
                //dataApontamento.DateTime = DateTime.Now;
                centroCustoApontamento.Text = string.Empty;
                cmbAtividade.Text = string.Empty;
                cmbIdTarefa.Text = string.Empty;
                cmbIdTarefa.Focus();

                vm.HorasApontadas = await Task.Run(() => vm.GetHorasApontadasAsync(apontamento.data.Value.Date, vm.FuncProjeto.cod_func));
                vm.FuroProjetos = await Task.Run(() => vm.GetFuroProjetosAsync(vm.FuncProjeto.cod_func));

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OnRowValidating(object sender, Syncfusion.UI.Xaml.Grid.RowValidatingEventArgs e)
        {
            try
            {
                ApontamentoProjetosViewModel vm = (ApontamentoProjetosViewModel)DataContext;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                var horaApontada = e.RowData as QryApontamentoHoraModel;
                var apontamento = await Task.Run(() => vm.GetHoraApontadaAsync(horaApontada.cod_linha));
                apontamento.hora_trabalhada = horaApontada.hora_trabalhada;
                apontamento.observacao = horaApontada.observacao;
                await Task.Run(() => vm.SetApontarHoraAsync(apontamento));
                vm.HorasApontadas = await Task.Run(() => vm.GetHorasApontadasAsync(apontamento.data.Value, horaApontada.cod_func));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex?.InnerException?.Message, "Erro ao inserir", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void dataApontamento_DateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                ApontamentoProjetosViewModel vm = (ApontamentoProjetosViewModel)DataContext;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                var dtApontamento = e.NewValue as DateTime?;
                vm.HorasApontadas = await Task.Run(() => vm.GetHorasApontadasAsync(dtApontamento, vm?.FuncProjeto?.cod_func));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex?.InnerException?.Message, "Erro ao inserir", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

    }


    public class ApontamentoProjetosViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private ObservableCollection<object> _centroCustos;
        public ObservableCollection<object> CentroCustos
        {
            get { return _centroCustos; }
            set { _centroCustos = value; RaisePropertyChanged("CentroCustos"); }
        }

        private TarefaProjetosModel _atividade;
        public TarefaProjetosModel Atividade
        {
            get { return _atividade; }
            set { _atividade = value; RaisePropertyChanged("Atividade"); }
        }

        private ObservableCollection<TarefaProjetosModel> _atividades;
        public ObservableCollection<TarefaProjetosModel> Atividades
        {
            get { return _atividades; }
            set { _atividades = value; RaisePropertyChanged("Atividades"); }
        }

        private ObservableCollection<TemaModel> _temas;
        public ObservableCollection<TemaModel> Temas
        {
            get { return _temas; }
            set { _temas = value; RaisePropertyChanged("Temas"); }
        }

        private ObservableCollection<ClienteModel> _clientes;
        public ObservableCollection<ClienteModel> Clientes
        {
            get { return _clientes; }
            set { _clientes = value; RaisePropertyChanged("Clientes"); }
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

        private ApontamentoHoraModel _apontamento;
        public ApontamentoHoraModel Apontamento
        {
            get { return _apontamento; }
            set { _apontamento = value; RaisePropertyChanged("Apontamento"); }
        }


        private QryApontamentoHoraModel _horaApontada;
        public QryApontamentoHoraModel HoraApontada
        {
            get { return _horaApontada; }
            set { _horaApontada = value; RaisePropertyChanged("HoraApontada"); }
        }



        private ObservableCollection<QryApontamentoHoraModel> _horasApontadas;
        public ObservableCollection<QryApontamentoHoraModel> HorasApontadas
        {
            get { return _horasApontadas; }
            set { _horasApontadas = value; RaisePropertyChanged("HorasApontadas"); }
        }

        private ObservableCollection<QryFuroApontamentoProjeto> _furoProjetos;
        public ObservableCollection<QryFuroApontamentoProjeto> FuroProjetos
        {
            get { return _furoProjetos; }
            set { _furoProjetos = value; RaisePropertyChanged("FuroProjetos"); }
        }



        public async Task<ObservableCollection<object>> GetCentroCustoAsync(string departamento)
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.TarefaProjetos
                    .OrderBy(c => c.identificacao)
                    .Where(c => c.inativo == "0" && c.departamento == departamento)
                    .Select(s => new
                    {
                        s.identificacao
                    }).ToArrayAsync();
                return new ObservableCollection<object>(data.GroupBy(x => x.identificacao));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<TarefaProjetosModel>> GetAtividadesAsync(string centroCusto, string departamento)
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.TarefaProjetos
                    .OrderBy(c => c.atividade)
                    .Where(c => c.inativo == "0" && c.identificacao == centroCusto && c.departamento == departamento)
                    .ToArrayAsync();
                return new ObservableCollection<TarefaProjetosModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<TemaModel>> GetTemasAsync()
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.Temas
                    .OrderBy(c => c.temas)
                    .ToArrayAsync();
                return new ObservableCollection<TemaModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<ClienteModel>> GetClientesAsync()
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.Clientes
                    .OrderBy(c => c.sigla)
                    .ToArrayAsync();
                return new ObservableCollection<ClienteModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<FuncionarioProjetosModel> GetFuncionarioAsync(string departamento)
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.FuncionarioProjetos
                    .OrderBy(c => c.nome_func)
                    .Where(c => c.nick_name_func == BaseSettings.Username && c.departamento == departamento)
                    .FirstOrDefaultAsync();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ApontamentoHoraModel> GetHoraApontadaAsync(long? codLinha)
        {
            try
            {
                using DatabaseContext db = new();
                return await db.ApontamentoHoras.FindAsync(codLinha);
            }
            catch (Exception)
            {
                throw;
            }
        }



    public async Task SetApontarHoraAsync(ApontamentoHoraModel apontamento)
        {
            try
            {
                using DatabaseContext db = new();
                //await db.ApontamentoHoras.SingleMergeAsync(apontamento);
                var apontamentoExistente = await db.ApontamentoHoras.FindAsync(apontamento.cod_linha);
                if (apontamentoExistente == null)
                    await db.ApontamentoHoras.AddAsync(apontamento);
                else
                    db.Entry(apontamentoExistente).CurrentValues.SetValues(apontamento);

                await db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DataPlanejamentoModel> GetSemanaAsync(DateTime? dataApontamento)
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.DataPlanejamentos
                    .Where(c => c.data == dataApontamento)
                    .FirstOrDefaultAsync();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<QryApontamentoHoraModel>> GetHorasApontadasAsync(DateTime? dataApontamento, long? cod_func)
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.QryApontamentos
                    .OrderBy(c => c.cod_linha)
                    .Where(c => c.cod_func == cod_func && c.data == dataApontamento)
                    .ToArrayAsync();
                return new ObservableCollection<QryApontamentoHoraModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<QryFuroApontamentoProjeto>> GetFuroProjetosAsync(long? cod_func)
        {
            try
            {
                using DatabaseContext db = new();
                var data = await db.QryFuroApontamentoProjetos
                    .OrderBy(c => c.data)
                    .Where(c => c.codfun == cod_func)
                    .ToArrayAsync();
                return new ObservableCollection<QryFuroApontamentoProjeto>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }



    }
}

