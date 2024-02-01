using Apontamento.DataBase;
using Apontamento.DataBase.Model;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Windows.Shared;
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
                var dtApontamento = dataApontamento.DateTime;
                vm.Semana = await Task.Run(() => vm.GetSemanaAsync(dtApontamento));
                SemanaApontamento.Text = vm.Semana.semana.ToString();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void dataApontamento_DateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                ApontamentoProducaoViewModel vm = (ApontamentoProducaoViewModel)DataContext;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                var dtApontamento = e.NewValue as DateTime?;
                vm.Semana = await Task.Run(() => vm.GetSemanaAsync(dtApontamento));
                SemanaApontamento.Text = vm.Semana.semana.ToString();
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex?.InnerException?.Message, "Erro ao inserir", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private async void OnRowValidating(object sender, RowValidatingEventArgs e)
        {
            var sfdatagrid = sender as SfDataGrid;
            ApontamentoProducaoViewModel vm = (ApontamentoProducaoViewModel)DataContext;
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                vm.Ht = await Task.Run(() => vm.AddApontamentoAsync(((HtModel)e.RowData)));

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

        private async void OnFuncionariosSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                ApontamentoProducaoViewModel vm = (ApontamentoProducaoViewModel)DataContext;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                vm.Hts = await Task.Run(() => vm.GetHtsAsync(vm.Semana.data, vm?.Funcionario?.barcode));
                vm.FutoHts = await Task.Run(() => vm.GetFuroHtsAsync(vm?.Funcionario?.codfun));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro ao inserir", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void SfDataGrid_AddNewRowInitiating(object sender, Syncfusion.UI.Xaml.Grid.AddNewRowInitiatingEventArgs e)
        {
            ApontamentoProducaoViewModel vm = (ApontamentoProducaoViewModel)DataContext;

            var data = e.NewObject as HtModel;
            data.data = vm.Semana.data;
            data.semana = vm.Semana.semana;
            data.barcode = vm.Funcionario.barcode;
            data.codfun = vm.Funcionario.codfun;
            data.cadastrado_por = Environment.UserName;
            data.inclusao = DateTime.Now;
        }

        private async void SfDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //ApontamentoProducaoViewModel vm = (ApontamentoProducaoViewModel)DataContext;


            try
            {
                ApontamentoProducaoViewModel vm = (ApontamentoProducaoViewModel)DataContext;
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                dataApontamento.DateTime = vm.FuroHt.data_ideal;
                //var dtApontamento = e.NewValue as DateTime?;
                vm.Semana = await Task.Run(() => vm.GetSemanaAsync(vm.FuroHt.data_ideal));
                SemanaApontamento.Text = vm.Semana.semana.ToString();
                vm.Hts = await Task.Run(() => vm.GetHtsAsync(vm.Semana.data, vm?.Funcionario?.barcode));
                vm.FutoHts = await Task.Run(() => vm.GetFuroHtsAsync(vm?.Funcionario?.codfun));
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });

                /*
                DGDigitacao.View.AddNew();
                this.DGDigitacao.View.CommitNew();
                */


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex?.InnerException?.Message, "Erro ao inserir", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }
    }

    public class ApontamentoProducaoViewModel : INotifyPropertyChanged
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

        private DataPlanejamentoModel _semana;
        public DataPlanejamentoModel Semana
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

        public async Task<ObservableCollection<HtModel>> GetHtsAsync(DateTime? date, string? barcode)
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.Hts
                    .OrderBy(c => c.cod)
                    .Where(c => c.data == date && c.barcode == barcode)
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

        public async Task<ObservableCollection<QryHtFuroGeralModel>> GetFuroHtsAsync(long? codfun)
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.QryHtFuros
                    .OrderBy(c => c.data_ideal)
                    .Where(c => c.codfun == codfun)
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
