using Apontamento.DataBase;
using Apontamento.DataBase.Model;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Apontamento.Views.Producao.Consultas
{
    /// <summary>
    /// Interação lógica para Setores.xam
    /// </summary>
    public partial class Setores : UserControl
    {
        public Setores()
        {
            InitializeComponent();
            DataContext = new SetoresViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                SetoresViewModel vm = (SetoresViewModel)DataContext;
                vm.Setores = await Task.Run(vm.GetSetoresAsync);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void OnRowValidating(object sender, Syncfusion.UI.Xaml.Grid.RowValidatingEventArgs e)
        {

        }

        private async void OnCurrentCellValueChanged(object sender, Syncfusion.UI.Xaml.Grid.CurrentCellValueChangedEventArgs e)
        {
            SetorModel? dado = e.Record as SetorModel;
            SetoresViewModel vm = (SetoresViewModel)DataContext;

            SfDataGrid? grid = sender as SfDataGrid;
            int columnindex = grid.ResolveToGridVisibleColumnIndex(e.RowColumnIndex.ColumnIndex);
            var column = grid.Columns[columnindex];
            var rowIndex = grid.ResolveToRecordIndex(e.RowColumnIndex.RowIndex);

            try
            {
                if (column.GetType() == typeof(GridCheckBoxColumn) && column.MappingName == "inativo")
                {
                    vm.Setor = await Task.Run(() => vm.AtivarSetorAsync(dado));
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class SetoresViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private ObservableCollection<SetorModel> _setores;
        public ObservableCollection<SetorModel> Setores
        {
            get { return _setores; }
            set { _setores = value; RaisePropertyChanged("Setores"); }
        }

        private SetorModel _setor;
        public SetorModel Setor
        {
            get { return _setor; }
            set { _setor = value; RaisePropertyChanged("Setor"); }
        }

        public async Task<ObservableCollection<SetorModel>> GetSetoresAsync()
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.Setores
                    .OrderBy(f => f.setor)
                    .ToListAsync();
                return new ObservableCollection<SetorModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SetorModel> AtivarSetorAsync(SetorModel setor)
        {
            try
            {
                using DatabaseContext db = new();
                db.Entry(setor).Property(p => p.inativo).IsModified = true;
                await db.SaveChangesAsync();
                return setor;
            }
            catch (NpgsqlException)
            {
                throw;
            }
        }
    }
}
