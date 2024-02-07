using Apontamento.DataBase;
using Apontamento.DataBase.Model;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Linq;
using Syncfusion.XlsIO;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace Apontamento.Views.Producao
{
    /// <summary>
    /// Interação lógica para ImprimirFicha.xam
    /// </summary>
    public partial class ImprimirFicha : UserControl
    {
        public ImprimirFicha()
        {
            InitializeComponent();
            DataContext = new ImprimirFichaViewModel();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                ImprimirFichaViewModel vm = (ImprimirFichaViewModel)DataContext;
                vm.Funcionarios = await Task.Run(vm.GetFuncionariosAsync);
                vm.Semanas = await Task.Run(vm.GetSemanasAsync);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
        }

        private void OnPrintFichaClick(object sender, RoutedEventArgs e)
        {

            try
            {

                var filteredResult = this.DGFuncionarios.View.Records.Select(recordentry => recordentry.Data).ToList();
                ImprimirFichaViewModel vm = (ImprimirFichaViewModel)DataContext;
                DataSemanaModel semana = (DataSemanaModel)this.semana.SelectedItem;

                if (semana == null)
                {
                    MessageBox.Show("Seleciona a Semana para imprimir a ficha","Semana");
                    return;
                }

                if (filteredResult.Count == 0)
                {
                    MessageBox.Show("Precisa de pelo menos um funcionário para imprimir a ficha.", "Pessoas");
                    return;
                }

                using ExcelEngine excelEngine = new();
                IApplication application = excelEngine.Excel;
                application.DefaultVersion = ExcelVersion.Xlsx;
                IWorkbook workbook = application.Workbooks.Open("ModelosImpressoes/MODELO-FICHA-APONTAMENTO.xlsx");
                IWorksheet worksheet = workbook.Worksheets[0];

                int TotFunc = filteredResult.Count;
                int print = 0;
                var ficha = "ESQUERDA";

                for (int i = 0; i < filteredResult.Count; i++)
                {
                    FuncionarioModel func = (FuncionarioModel)filteredResult[i];

                    switch (ficha)
                    {
                        case "ESQUERDA":
                            {

                                worksheet.Range["B1"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["D1"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["B2"].Text = func.nome_apelido;
                                worksheet.Range["B3"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["B4"].DateTime = semana.PrimeiraData;
                                worksheet.Range["FUNC1_DATA1"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA1"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA1"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA1"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA1"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["B12"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["D12"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["B13"].Text = func.nome_apelido;
                                worksheet.Range["B14"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["B15"].DateTime = semana.PrimeiraData.AddDays(1);
                                worksheet.Range["FUNC1_DATA2"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA2"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA2"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA2"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA2"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["B23"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["D23"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["B24"].Text = func.nome_apelido;
                                worksheet.Range["B25"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["B26"].DateTime = semana.PrimeiraData.AddDays(2);
                                worksheet.Range["FUNC1_DATA3"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA3"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA3"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA3"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA3"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["B34"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["D34"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["B35"].Text = func.nome_apelido;
                                worksheet.Range["B36"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["B37"].DateTime = semana.PrimeiraData.AddDays(3);
                                worksheet.Range["FUNC1_DATA4"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA4"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA4"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA4"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA4"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["G1"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["I1"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["G2"].Text = func.nome_apelido;
                                worksheet.Range["G3"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["G4"].DateTime = semana.PrimeiraData.AddDays(4);
                                worksheet.Range["FUNC1_DATA5"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA5"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA5"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA5"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA5"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["G12"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["I12"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["G13"].Text = func.nome_apelido;
                                worksheet.Range["G14"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["G15"].DateTime = semana.PrimeiraData.AddDays(5);
                                worksheet.Range["FUNC1_DATA6"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA6"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA6"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA6"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA6"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["G23"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["I23"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["G24"].Text = func.nome_apelido;
                                worksheet.Range["G25"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["G26"].DateTime = semana.PrimeiraData.AddDays(6);
                                worksheet.Range["FUNC1_DATA7"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA7"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA7"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA7"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA7"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["G34"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["I34"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["G35"].Text = func.nome_apelido;
                                worksheet.Range["G36"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["FUNC1_DATA8"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA8"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA8"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA8"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC1_DATA8"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                Thread.Sleep(3000);
                                workbook.SaveAs(@"Impressos\FICHA-APONTAMENTO.xlsx");
                                ficha = "DIREITA";

                                break;
                            }
                        case "DIREITA":
                            {
                                worksheet.Range["L1"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["N1"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["L2"].Text = func.nome_apelido;
                                worksheet.Range["L3"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["L4"].DateTime = semana.PrimeiraData;
                                worksheet.Range["FUNC2_DATA1"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA1"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA1"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA1"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA1"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["L12"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["N12"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["L13"].Text = func.nome_apelido;
                                worksheet.Range["L14"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["L15"].DateTime = semana.PrimeiraData.AddDays(1);
                                worksheet.Range["FUNC2_DATA2"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA2"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA2"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA2"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA2"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["L23"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["N23"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["L24"].Text = func.nome_apelido;
                                worksheet.Range["L25"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["L26"].DateTime = semana.PrimeiraData.AddDays(2);
                                worksheet.Range["FUNC2_DATA3"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA3"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA3"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA3"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA3"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["L34"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["N34"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["L35"].Text = func.nome_apelido;
                                worksheet.Range["L36"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["L37"].DateTime = semana.PrimeiraData.AddDays(3);
                                worksheet.Range["FUNC2_DATA4"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA4"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA4"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA4"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA4"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["Q1"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["S1"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["Q2"].Text = func.nome_apelido;
                                worksheet.Range["Q3"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["Q4"].DateTime = semana.PrimeiraData.AddDays(4);
                                worksheet.Range["FUNC2_DATA5"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA5"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA5"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA5"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA5"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["Q12"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["S12"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["Q13"].Text = func.nome_apelido;
                                worksheet.Range["Q14"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["Q15"].DateTime = semana.PrimeiraData.AddDays(5);
                                worksheet.Range["FUNC2_DATA6"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA6"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA6"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA6"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA6"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["Q23"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["S23"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["Q24"].Text = func.nome_apelido;
                                worksheet.Range["Q25"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["Q26"].DateTime = semana.PrimeiraData.AddDays(6);
                                worksheet.Range["FUNC2_DATA7"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA7"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA7"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA7"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA7"].CellStyle.Font.Color = ExcelKnownColors.Black;

                                worksheet.Range["Q34"].Number = Convert.ToDouble(semana.Semana);
                                worksheet.Range["S34"].Number = Convert.ToDouble(func.codfun);
                                worksheet.Range["Q35"].Text = func.nome_apelido;
                                worksheet.Range["Q36"].Text = $"{func.setor} {(func.sub_setor == func.setor ? null : func.sub_setor)}";
                                worksheet.Range["FUNC2_DATA8"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA8"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA8"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA8"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.Thin;
                                worksheet.Range["FUNC2_DATA8"].CellStyle.Font.Color = ExcelKnownColors.Black;


                                workbook.SaveAs(@"Impressos\FICHA-APONTAMENTO.xlsx");

                                print++;
                                ficha = "ESQUERDA";

                                Process.Start(
                                    new ProcessStartInfo(@"Impressos\FICHA-APONTAMENTO.xlsx")
                                    {
                                        Verb = "Print",
                                        UseShellExecute = true,
                                    });

                                Thread.Sleep(3000);

                                worksheet.Range["FUNC2_DATA1"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA1"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA1"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA1"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA1"].CellStyle.Font.Color = ExcelKnownColors.White;

                                worksheet.Range["FUNC2_DATA2"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA2"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA2"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA2"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA2"].CellStyle.Font.Color = ExcelKnownColors.White;

                                worksheet.Range["FUNC2_DATA3"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA3"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA3"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA3"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA3"].CellStyle.Font.Color = ExcelKnownColors.White;

                                worksheet.Range["FUNC2_DATA4"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA4"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA4"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA4"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA4"].CellStyle.Font.Color = ExcelKnownColors.White;

                                worksheet.Range["FUNC2_DATA5"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA5"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA5"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA5"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA5"].CellStyle.Font.Color = ExcelKnownColors.White;

                                worksheet.Range["FUNC2_DATA6"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA6"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA6"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA6"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA6"].CellStyle.Font.Color = ExcelKnownColors.White;

                                worksheet.Range["FUNC2_DATA7"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA7"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA7"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA7"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA7"].CellStyle.Font.Color = ExcelKnownColors.White;

                                worksheet.Range["FUNC2_DATA8"].Borders[ExcelBordersIndex.EdgeLeft].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA8"].Borders[ExcelBordersIndex.EdgeRight].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA8"].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA8"].Borders[ExcelBordersIndex.EdgeBottom].LineStyle = ExcelLineStyle.None;
                                worksheet.Range["FUNC2_DATA8"].CellStyle.Font.Color = ExcelKnownColors.White;


                                break;
                            }
                    }

                    if (TotFunc == 1 && ficha == "DIREITA")
                    {
                        Process.Start(
                            new ProcessStartInfo(@"Impressos\FICHA-APONTAMENTO.xlsx")
                            {
                                Verb = "Print",
                                UseShellExecute = true,
                            });
                        Thread.Sleep(3000);
                    }
                    TotFunc--;

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            

        }
    }

    public class ImprimirFichaViewModel : INotifyPropertyChanged
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

        public async Task<ObservableCollection<FuncionarioModel>> GetFuncionariosAsync()
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.Funcionarios
                    .OrderBy(f => f.sub_setor)
                    .ThenBy(f => f.setor)
                    .ThenBy(f => f.nome_apelido)
                    .Where(f => f.data_demissao == null && f.ativo == "1")
                    .ToListAsync();
                return new ObservableCollection<FuncionarioModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ObservableCollection<DataSemanaModel>> GetSemanasAsync()
        {
            try
            {
                DataBaseSettings BaseSettings = DataBaseSettings.Instance;
                using DatabaseContext db = new();
                var data = await db.DataPlanejamentos
                    .GroupBy(dp => dp.semana)
                    .Select(grupo => new DataSemanaModel
                    {
                        Semana = (int)grupo.Key,
                        PrimeiraData = (DateTime)grupo.Min(dp => dp.data),
                        UltimaData = (DateTime)grupo.Max(dp => dp.data)
                    })
                    .OrderByDescending(resultado => resultado.Semana)
                    .ToListAsync();
                return new ObservableCollection<DataSemanaModel>(data);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
