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
    /// Interação lógica para ControleFuncionarioPCP.xam
    /// </summary>
    public partial class ControleFuncionarioPCP : UserControl
    {
        public ControleFuncionarioPCP()
        {
            InitializeComponent();
            DataContext = new ControleFuncionarioPCPViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ControleFuncionarioPCPViewModel vm = (ControleFuncionarioPCPViewModel)DataContext;
                vm.Funcionarios = await Task.Run(vm.GetFuncionariosAsync);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void OnRowValidating(object sender, RowValidatingEventArgs e)
        {

        }

        private async void SfDataGrid_CurrentCellValueChanged(object sender, CurrentCellValueChangedEventArgs e)
        {
            FuncionarioModel? dado = e.Record as FuncionarioModel;
            ControleFuncionarioPCPViewModel vm = (ControleFuncionarioPCPViewModel)DataContext;

            SfDataGrid? grid = sender as SfDataGrid;
            int columnindex = grid.ResolveToGridVisibleColumnIndex(e.RowColumnIndex.ColumnIndex);
            var column = grid.Columns[columnindex];
            var rowIndex = grid.ResolveToRecordIndex(e.RowColumnIndex.RowIndex);

            try
            {
                //if (column.GetType() == typeof(GridCheckBoxColumn) && column.MappingName == "ativo")
                //{
                    vm.Funcionario = await Task.Run(() => vm.AtivarExibFuroAsync(dado));
                //}


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class ControleFuncionarioPCPViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private ObservableCollection<FuncionarioModel> _funcionarios;
        public ObservableCollection<FuncionarioModel> Funcionarios
        {
            get { return _funcionarios; }
            set { _funcionarios = value; RaisePropertyChanged("Funcionarios"); }
        }

        private FuncionarioModel _funcionario;
        public FuncionarioModel Funcionario
        {
            get { return _funcionario; }
            set { _funcionario = value; RaisePropertyChanged("Funcionario"); }
        }

        public async Task<ObservableCollection<FuncionarioModel>> GetFuncionariosAsync()
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.Funcionarios
                    .OrderBy(f => f.nome_apelido)
                    .ToListAsync();
                return new ObservableCollection<FuncionarioModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<FuncionarioModel> AtivarExibFuroAsync(FuncionarioModel func)
        {
            try
            {
                using DatabaseContext db = new();

                var funcionario = db.Funcionarios.FirstOrDefault(p => p.codfun == func.codfun);
                //if (det != null)
                //{

                    funcionario.exibir_furo = func.exibir_furo;
                    funcionario.ativo = func.ativo;
                    //det.confirmado_por = detCompl.confirmado_por;
                    //det.desabilitado_confirmado_data = detCompl.desabilitado_confirmado_data;
                    //det.desabilitado_confirmado_por = detCompl.desabilitado_confirmado_por;

                    db.Entry(funcionario).Property(p => p.exibir_furo).IsModified = true;
                    db.Entry(funcionario).Property(p => p.ativo).IsModified = true;
                    //db.Entry(det).Property(p => p.confirmado_por).IsModified = true;
                    //db.Entry(det).Property(p => p.desabilitado_confirmado_data).IsModified = true;
                    //db.Entry(det).Property(p => p.desabilitado_confirmado_por).IsModified = true;

                    // Salva apenas a atualização do campo modificado
                    await db.SaveChangesAsync();
                //}


                return funcionario;
            }
            catch (NpgsqlException)
            {
                throw;
            }
        }
    }
}
