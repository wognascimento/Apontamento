using Apontamento.Custom;
using Apontamento.DataBase;
using Apontamento.DataBase.Model;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            this.DGDigitacao.SelectionController = new CustomSelectionController(DGDigitacao);
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ApontamentoProducaoSemanaViewModel vm = (ApontamentoProducaoSemanaViewModel)DataContext;
                vm.Funcionarios = await Task.Run(vm.GetFuncionariosAsync);
                var semana = await Task.Run(() => vm.GetSemanaAsync(DateTime.Now.Date));
                SemanaApontamento.Text = semana.semana.ToString();
                vm.Semana = await Task.Run(() => vm.GetSemanasAsync(semana.semana));
                vm.Hts = await Task.Run(() => vm.GetHtsAsync(semana.semana));
                vm.FutoHts = await Task.Run(() => vm.GetFuroHtsAsync(vm.Semana));
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
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ApontamentoProducaoSemanaViewModel vm = (ApontamentoProducaoSemanaViewModel)DataContext;
                var semana = int.Parse(SemanaApontamento.Text);
                vm.Semana = await Task.Run(() => vm.GetSemanasAsync(semana));
                vm.Hts = await Task.Run(() => vm.GetHtsAsync(semana));
                vm.FutoHts = await Task.Run(() => vm.GetFuroHtsAsync(vm.Semana));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void SfDataGrid_AddNewRowInitiating(object sender, Syncfusion.UI.Xaml.Grid.AddNewRowInitiatingEventArgs e)
        {
            ApontamentoProducaoSemanaViewModel vm = (ApontamentoProducaoSemanaViewModel)DataContext;

            ((HtModel)e.NewObject).semana = vm.Semana.Semana;
            ((HtModel)e.NewObject).cadastrado_por = Environment.UserName;
            ((HtModel)e.NewObject).inclusao = DateTime.Now.Date;
        }

        private void OnRowValidating(object sender, Syncfusion.UI.Xaml.Grid.RowValidatingEventArgs e)
        {
            HtModel rowData = (HtModel)e.RowData;
            if (!rowData.semana.HasValue)
            {
                e.IsValid = false;
                e.ErrorMessages.Add("codfun", "Semana não identificada.");
                e.ErrorMessages.Add("data", "Semana não identificada.");
                e.ErrorMessages.Add("num_os", "Semana não identificada.");
                e.ErrorMessages.Add("quantidadehorastrabalhadas", "Semana não identificada.");
                e.ErrorMessages.Add("cod", "Semana não identificada.");
            }
            else if (!rowData.codfun.HasValue)
            {
                e.IsValid = false;
                e.ErrorMessages.Add("codfun", "Informe O funcionário.");
            }
            else if (!rowData.data.HasValue)
            {
                e.IsValid = false;
                e.ErrorMessages.Add("data", "Informa a data.");
            }
            else if (!rowData.num_os.HasValue)
            {
                e.IsValid = false;
                e.ErrorMessages.Add("num_os", "Informe o número da O.S.");
            }
            else if (!rowData.quantidadehorastrabalhadas.HasValue)
            {
                e.IsValid = false;
                e.ErrorMessages.Add("quantidadehorastrabalhadas", "Informe a quantidade de horas trabalhada.");
            }
        }

        private async void DGDigitacao_RowValidated(object sender, Syncfusion.UI.Xaml.Grid.RowValidatedEventArgs e)
        {
            var sfdatagrid = sender as SfDataGrid;
            ApontamentoProducaoSemanaViewModel vm = (ApontamentoProducaoSemanaViewModel)DataContext;
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                var ht = (HtModel)e.RowData;
                ht.data = ((HtModel)e.RowData).data.Value;
                vm.Ht = await Task.Run(() => vm.AddApontamentoAsync(ht));
                //vm.Ht = await Task.Run(() => vm.AddApontamentoAsync(new HtModel { cod = ht.cod, codfun = ht.codfun, data = ht.data.Value.Date}));
                vm.FutoHts = await Task.Run(() => vm.GetFuroHtsAsync(vm.Semana));
                ((HtModel)e.RowData).cod = vm.Ht.cod;
                sfdatagrid.View.Refresh();
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
    }

    public class ApontamentoProducaoSemanaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private ObservableCollection<FuncionarioAtivoModel> _funcionarios;
        public ObservableCollection<FuncionarioAtivoModel> Funcionarios
        {
            get { return _funcionarios; }
            set { _funcionarios = value; RaisePropertyChanged("Funcionarios"); }
        }

        private FuncionarioAtivoModel _funcionario;
        public FuncionarioAtivoModel Funcionario
        {
            get { return _funcionario; }
            set { _funcionario = value; RaisePropertyChanged("Funcionario"); }
        }


        private ObservableCollection<HtModel> _hts;
        public ObservableCollection<HtModel> Hts
        {
            get { return _hts; }
            set { _hts = value; RaisePropertyChanged("Hts"); }
        }

        private HtModel _ht;
        public HtModel Ht
        {
            get { return _ht; }
            set { _ht = value; RaisePropertyChanged("Ht"); }
        }

        /*
        private DataPlanejamentoModel _semana;
        public DataPlanejamentoModel Semana
        {
            get { return _semana; }
            set { _semana = value; RaisePropertyChanged("Semana"); }
        }
        */

        private ObservableCollection<DataSemanaModel> _semanas;
        public ObservableCollection<DataSemanaModel> Semanas
        {
            get { return _semanas; }
            set { _semanas = value; RaisePropertyChanged("Semanas"); }
        }

        private DataSemanaModel _semana;
        public DataSemanaModel Semana
        {
            get { return _semana; }
            set { _semana = value; RaisePropertyChanged("Semana"); }
        }

        private ObservableCollection<QryHtFuroGeralModel> _futoHts;
        public ObservableCollection<QryHtFuroGeralModel> FutoHts
        {
            get { return _futoHts; }
            set { _futoHts = value; RaisePropertyChanged("FutoHts"); }
        }

        private QryHtFuroGeralModel _furoHt;
        public QryHtFuroGeralModel FuroHt
        {
            get { return _furoHt; }
            set { _furoHt = value; RaisePropertyChanged("FuroHt"); }
        }

        public async Task<ObservableCollection<FuncionarioAtivoModel>> GetFuncionariosAsync()
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.FuncionarioAtivos
                    .OrderBy(c => c.nome_apelido)
                    .ToListAsync();
                return new ObservableCollection<FuncionarioAtivoModel>(data);
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

        public async Task<DataSemanaModel> GetSemanasAsync(int? Semana)
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.DataPlanejamentos
                    .Where(dp => dp.semana == Semana)
                    .GroupBy(dp => dp.semana)
                    .Select(grupo => new DataSemanaModel
                    {
                        Semana = (int)grupo.Key,
                        PrimeiraData = (DateTime)grupo.Min(dp => dp.data),
                        UltimaData = (DateTime)grupo.Max(dp => dp.data)
                    })
                    .OrderByDescending(resultado => resultado.Semana)
                    .FirstOrDefaultAsync();
                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<HtModel>> GetHtsAsync(double? semana)
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.Hts
                    .OrderBy(c => c.cod)
                    .Where(c => c.semana == semana && c.codfun != null)
                    .ToListAsync();
                return new ObservableCollection<HtModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<HtModel> AddApontamentoAsync(HtModel ht)
        {
            try
            {
                using DatabaseContext db = new();
                await db.Hts.SingleMergeAsync(ht);
                await db.SaveChangesAsync();
                return ht;
            }
            catch (NpgsqlException)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<QryHtFuroGeralModel>> GetFuroHtsAsync(DataSemanaModel? semana)
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.QryHtFuros
                    .OrderBy(c => c.data_ideal)
                    .ThenBy(c => c.setor)
                    .ThenBy(c => c.nome_apelido)
                    .Where(dp => dp.data_ideal >= semana.PrimeiraData && dp.data_ideal <= semana.UltimaData)
                    .ToListAsync();
                return new ObservableCollection<QryHtFuroGeralModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
