using Apontamento.DataBase;
using Apontamento.DataBase.Model;
using ClosedXML.Excel;
using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Apontamento.Views.Producao
{
    /// <summary>
    /// Interação lógica para ImprimirFicha.xam
    /// </summary>
    public partial class ImprimirFicha : UserControl
    {
        private readonly DataBaseSettings BaseSettings = DataBaseSettings.Instance;

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
                var funcionarios = DGFuncionarios.Items.OfType<FuncionarioModel>().ToList();
                var semanaSelecionada = semana.SelectedItem as DataSemanaModel;

                if (semanaSelecionada == null)
                {
                    MessageBox.Show("Seleciona a Semana para imprimir a ficha", "Semana");
                    return;
                }

                if (funcionarios.Count == 0)
                {
                    MessageBox.Show("Precisa de pelo menos um funcionário para imprimir a ficha.", "Pessoas");
                    return;
                }

                var modeloPath = Path.Combine(BaseSettings.CaminhoSistema, "Modelos", "MODELO-FICHA-APONTAMENTO.xlsx");
                var impressosPath = Path.Combine(BaseSettings.CaminhoSistema, "Impressos");
                var arquivoPath = Path.Combine(impressosPath, "FICHA-APONTAMENTO.xlsx");

                Directory.CreateDirectory(impressosPath);

                for (var index = 0; index < funcionarios.Count; index += 2)
                {
                    using var workbook = new XLWorkbook(modeloPath);
                    var worksheet = workbook.Worksheet(1);

                    PreencherFicha(workbook, worksheet, semanaSelecionada, funcionarios[index], FichaLado.Esquerda);

                    if (index + 1 < funcionarios.Count)
                    {
                        PreencherFicha(workbook, worksheet, semanaSelecionada, funcionarios[index + 1], FichaLado.Direita);
                    }

                    workbook.SaveAs(arquivoPath);

                    Process.Start(new ProcessStartInfo(arquivoPath)
                    {
                        Verb = "Print",
                        UseShellExecute = true,
                    });

                    Thread.Sleep(3000);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static void PreencherFicha(IXLWorkbook workbook, IXLWorksheet worksheet, DataSemanaModel semana, FuncionarioModel funcionario, FichaLado lado)
        {
            foreach (var linha in ObterLinhasFicha(lado))
            {
                worksheet.Cell(linha.SemanaCell).Value = semana.Semana;
                worksheet.Cell(linha.FuncionarioCell).Value = funcionario.codfun ?? 0;
                worksheet.Cell(linha.NomeCell).Value = funcionario.nome_apelido ?? string.Empty;
                worksheet.Cell(linha.SetorCell).Value = FormatarSetor(funcionario);

                if (linha.DiaSemana.HasValue && linha.DataCell != null)
                {
                    worksheet.Cell(linha.DataCell).Value = semana.PrimeiraData.AddDays(linha.DiaSemana.Value);
                }

                AplicarBordaData(workbook, worksheet, linha.RangeData);
            }
        }

        private static string FormatarSetor(FuncionarioModel funcionario)
        {
            if (string.Equals(funcionario.sub_setor, funcionario.setor, StringComparison.OrdinalIgnoreCase))
            {
                return funcionario.setor ?? string.Empty;
            }

            return $"{funcionario.setor} {funcionario.sub_setor}".Trim();
        }

        private static IEnumerable<LinhaFicha> ObterLinhasFicha(FichaLado lado)
        {
            return lado == FichaLado.Esquerda
                ? new[]
                {
                    new LinhaFicha("B1", "D1", "B2", "B3", "B4", "FUNC1_DATA1", 0),
                    new LinhaFicha("B12", "D12", "B13", "B14", "B15", "FUNC1_DATA2", 1),
                    new LinhaFicha("B23", "D23", "B24", "B25", "B26", "FUNC1_DATA3", 2),
                    new LinhaFicha("B34", "D34", "B35", "B36", "B37", "FUNC1_DATA4", 3),
                    new LinhaFicha("G1", "I1", "G2", "G3", "G4", "FUNC1_DATA5", 4),
                    new LinhaFicha("G12", "I12", "G13", "G14", "G15", "FUNC1_DATA6", 5),
                    new LinhaFicha("G23", "I23", "G24", "G25", "G26", "FUNC1_DATA7", 6),
                    new LinhaFicha("G34", "I34", "G35", "G36", null, "FUNC1_DATA8", null),
                }
                : new[]
                {
                    new LinhaFicha("L1", "N1", "L2", "L3", "L4", "FUNC2_DATA1", 0),
                    new LinhaFicha("L12", "N12", "L13", "L14", "L15", "FUNC2_DATA2", 1),
                    new LinhaFicha("L23", "N23", "L24", "L25", "L26", "FUNC2_DATA3", 2),
                    new LinhaFicha("L34", "N34", "L35", "L36", "L37", "FUNC2_DATA4", 3),
                    new LinhaFicha("Q1", "S1", "Q2", "Q3", "Q4", "FUNC2_DATA5", 4),
                    new LinhaFicha("Q12", "S12", "Q13", "Q14", "Q15", "FUNC2_DATA6", 5),
                    new LinhaFicha("Q23", "S23", "Q24", "Q25", "Q26", "FUNC2_DATA7", 6),
                    new LinhaFicha("Q34", "S34", "Q35", "Q36", null, "FUNC2_DATA8", null),
                };
        }

        private static void AplicarBordaData(IXLWorkbook workbook, IXLWorksheet worksheet, string rangeName)
        {
            var range = ObterRangeNomeado(workbook, worksheet, rangeName);

            if (range == null)
            {
                return;
            }

            range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            range.Style.Font.FontColor = XLColor.Black;
        }

        private static IXLRange? ObterRangeNomeado(IXLWorkbook workbook, IXLWorksheet worksheet, string rangeName)
        {
            try
            {
                return workbook.DefinedName(rangeName)?.Ranges.FirstOrDefault() as IXLRange;
            }
            catch
            {
                // Mantém compatibilidade com modelos antigos que podem ter ranges definidos no escopo da planilha.
            }

            try
            {
                return worksheet.DefinedName(rangeName)?.Ranges.FirstOrDefault() as IXLRange;
            }
            catch
            {
                return null;
            }
        }

        private enum FichaLado
        {
            Esquerda,
            Direita
        }

        private sealed record LinhaFicha(
            string SemanaCell,
            string FuncionarioCell,
            string NomeCell,
            string SetorCell,
            string? DataCell,
            string RangeData,
            int? DiaSemana);
    }

    public class ImprimirFichaViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private ObservableCollection<FuncionarioModel>? _funcionarios;
        public ObservableCollection<FuncionarioModel>? Funcionarios
        {
            get { return _funcionarios; }
            set { _funcionarios = value; RaisePropertyChanged(nameof(Funcionarios)); }
        }

        private FuncionarioModel? _funcionario;
        public FuncionarioModel? Funcionario
        {
            get { return _funcionario; }
            set { _funcionario = value; RaisePropertyChanged(nameof(Funcionario)); }
        }

        private ObservableCollection<DataSemanaModel>? _semanas;
        public ObservableCollection<DataSemanaModel>? Semanas
        {
            get { return _semanas; }
            set { _semanas = value; RaisePropertyChanged(nameof(Semanas)); }
        }

        private DataSemanaModel? _semanaSelecionada;
        public DataSemanaModel? SemanaSelecionada
        {
            get { return _semanaSelecionada; }
            set { _semanaSelecionada = value; RaisePropertyChanged(nameof(SemanaSelecionada)); }
        }

        public async Task<ObservableCollection<FuncionarioModel>> GetFuncionariosAsync()
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<FuncionarioModel>(
                @"SELECT *
                  FROM ht.funcionarios
                  WHERE data_demissao IS NULL
                    AND ativo IS NOT NULL
                    AND ativo LIKE '%1%'
                  ORDER BY sub_setor, setor, nome_apelido;");

            return new ObservableCollection<FuncionarioModel>(data);
        }

        public async Task<ObservableCollection<DataSemanaModel>> GetSemanasAsync()
        {
            using var db = DbConnectionFactory.Create();
            var data = await db.QueryAsync<DataSemanaModel>(
                @"SELECT
                    semana::int AS Semana,
                    MIN(data)::timestamp AS PrimeiraData,
                    MAX(data)::timestamp AS UltimaData
                  FROM ht.t_data_planejamento
                  GROUP BY semana
                  ORDER BY semana DESC;");

            return new ObservableCollection<DataSemanaModel>(data);
        }
    }
}
