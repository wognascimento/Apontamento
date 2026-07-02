using Apontamento.DataBase;
using Apontamento.Views.Producao;
using Apontamento.Views.Producao.Consultas;
using Apontamento.Views.Projetos;
using Dapper;
using Npgsql;
using ClosedXML.Excel;
using System.Data;
using System.Collections.Generic;
using System.IO;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace Apontamento
{
    /// <summary>
    /// Lógica interna para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        private async void OnAtualizarSistemaClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            await ((App)Application.Current).CheckForUpdatesAsync(true);
        }

        private void OnSobreSistemaClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            var version = ((App)Application.Current).CurrentVersion;
            MessageBox.Show($"Sistema Integrado de Gerenciamento - Apontamento\n\nVersão atual: {version}", "Sobre o sistema", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string GetImpressosPath(string fileName)
        {
            var impressosPath = Path.Combine(BaseSettings.CaminhoSistema, "Impressos");
            Directory.CreateDirectory(impressosPath);
            return Path.Combine(impressosPath, fileName);
        }

        private string ExportToExcelAndOpen<T>(IEnumerable<T> data, string fileName)
        {
            var filePath = GetImpressosPath(fileName);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Dados");
            worksheet.Cell(1, 1).InsertTable(data, "Dados", true);
            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);

            OpenFile(filePath);
            return filePath;
        }

        private string ExportDataTableToExcelAndOpen(DataTable dataTable, string fileName)
        {
            var filePath = GetImpressosPath(fileName);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Dados");
            worksheet.Cell(1, 1).InsertTable(dataTable, "Dados", true);
            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);

            OpenFile(filePath);
            return filePath;
        }

        private static async Task<DataTable> QueryDataTableAsync(string sql, object? parameters = null)
        {
            using var connection = DbConnectionFactory.Create();
            using var reader = await connection.ExecuteReaderAsync(sql, parameters);
            var dataTable = new DataTable();
            dataTable.Load(reader);
            return dataTable;
        }

        private static void OpenFile(string filePath)
        {
            Process.Start(new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            });
        }

        public MainWindow()
        {
            InitializeComponent();
            
            var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
            if (!string.IsNullOrWhiteSpace(appSettings?["Username"]))
                BaseSettings.Username = appSettings["Username"];

            txtUsername.Text = BaseSettings.Username;
            txtDataBase.Text = BaseSettings.Database;
        }

        private void OnAlterarUsuario(object sender, MouseButtonEventArgs e)
        {
            Login window = new();
            window.ShowDialog();

            try
            {
                var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
                if (!string.IsNullOrWhiteSpace(appSettings?["Username"]))
                    BaseSettings.Username = appSettings["Username"];

                txtUsername.Text = BaseSettings.Username;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OnAlterarAno(object sender, MouseButtonEventArgs e)
        {
            RadWindow.Prompt(new DialogParameters()
            {
                Header = "Ano Sistema",
                Content = "Alterar o Ano do Sistema",
                Closed = (object sender, WindowClosedEventArgs e) =>
                {
                    if (e.PromptResult != null)
                    {
                        BaseSettings.Database = e.PromptResult;
                        txtDataBase.Text = BaseSettings.Database;
                        BaseSettings.RefreshConnectionString();
                        documentGroup.Items.Clear();
                    }
                }
            });
        }

        public void adicionarFilho(object filho, string title, string name)
        {
            var paneGroup = documentGroup ?? radDocking.FindChildByType<RadPaneGroup>();
            if (paneGroup == null)
            {
                radDocking.ApplyTemplate();
                radDocking.UpdateLayout();
                paneGroup = radDocking.FindChildByType<RadPaneGroup>();
            }

            if (paneGroup == null)
            {
                MessageBox.Show(
                    "Não foi possível localizar o container de abas do sistema.",
                    "Erro ao abrir tela",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var pane = FindOpenPane(paneGroup, name, title);
            if (pane != null)
            {
                paneGroup.SelectedItem = pane;
                pane.IsActive = true;
                return;
            }

            var doc = filho as UserControl ?? filho as FrameworkElement;
            if (doc == null)
                return;

            doc.Name = name.ToLower();
            pane = new RadPane
            {
                Header = title,
                Content = doc,
                Tag = name.ToUpperInvariant(),
                CanUserClose = true,
                CanFloat = false
            };

            paneGroup.Items.Add(pane);
            paneGroup.SelectedItem = pane;
            pane.IsActive = true;
        }

        private static RadPane? FindOpenPane(RadPaneGroup paneGroup, string name_, string title)
        {
            var normalizedName = name_.ToUpperInvariant();
            return paneGroup.Items.OfType<RadPane>()
                .FirstOrDefault(p =>
                    string.Equals(p.Tag as string, normalizedName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.Header?.ToString(), title, StringComparison.OrdinalIgnoreCase));
        }

        public void FecharFilho(FrameworkElement filho)
        {
            var pane = documentGroup.Items.OfType<RadPane>()
                .FirstOrDefault(p => ReferenceEquals(p.Content, filho));

            if (pane != null)
                documentGroup.Items.Remove(pane);
        }

        private void OnDigitarApontamentoProjetosClick(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ApontamentoProjetos("PROJETOS"), "APONTAMENTO PROJETOS", "ApontamentoProjetos");
        }

        private void OnDigitarApontamentoArtesClick(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ApontamentoProjetos("ARTES"), "APONTAMENTO ARTES", "ApontamentoArtes");
        }

        private async void OnFuroProducaoClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(@"SELECT * FROM ht.qry_ht_furo_geral;");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_FURO.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private void OnControleFuncionarioPCP(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ControleFuncionarioPCP(), "CONTROLE FUNCIONÁRIO P.C.P", "ControleFuncionarioPCP");

        }

        private void OnSetoresProducaoClick(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new Setores(), "SETORES", "SetoresProducao");
        }

        private void OnImprimirProducaoClick(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ImprimirFicha(), "IMPRIMIR FICHA", "ImprimirFicha");
        }

        private void OnDigitarApontamentoProducaoFuncionarioClick(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ApontamentoProducao(), "APONTAMENTO PRODUCAO", "ApontamentoProducao");
        }

        private void OnDigitarApontamentoProducaoSemanaClick(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ApontamentoProducaoSemana(), "APONTAMENTO PRODUCAO POR SEMANA", "ApontamentoProducaoSemana");
        }

        private void OnDigitarApontamentoProducaoSetorClick(object sender, RoutedEventArgs e)
        {

        }

        private async void OnApontamentoProjetosFuroClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(
                    @"SELECT
                        f.departamento,
                        q.codfun,
                        f.nome_func,
                        q.data,
                        q.hora_minima AS minima,
                        q.hora_trabalhada AS trabalhada,
                        q.verificacao_novo AS furo
                      FROM ht.qryfurohtprojetos q
                      JOIN ht.t_funcionario_proj f ON f.cod_func = q.codfun
                      WHERE f.departamento = 'PROJETOS'
                      ORDER BY f.departamento, f.nome_func, q.data;");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_FURO_PROJETOS.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnApontamentoArtesFuroClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(
                    @"SELECT
                        f.departamento,
                        q.codfun,
                        f.nome_func,
                        q.data,
                        q.hora_minima AS minima,
                        q.hora_trabalhada AS trabalhada,
                        q.verificacao_novo AS furo
                      FROM ht.qryfurohtprojetos q
                      JOIN ht.t_funcionario_proj f ON f.cod_func = q.codfun
                      WHERE f.departamento = 'ARTES'
                      ORDER BY f.departamento, f.nome_func, q.data;");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_FURO_ARTES.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private void OnApontamentoCadastroFuncionarioProjetosClick(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new CadastroFuncionarioProjetos("PROJETOS"), "CADASTRO FUNCIONÁRIO PROJETOS", "ApontamentoProjetosCadastroFuncionario");
        }

        private void OnApontamentoCadastroFuncionarioArtesClick(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new CadastroFuncionarioProjetos("ARTES"), "CADASTRO FUNCIONÁRIO ARTES", "ApontamentoArtesCadastroFuncionario");
        }

        private async void OnApontamentosProducaoClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(@"SELECT * FROM ht.qry_apontamento_geral;");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_APONTAMENTOS_GERAL.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnApontamentoProjetosGeralClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(
                    @"SELECT * FROM ht.qry_apontamento_geral_projetos WHERE departamento = 'PROJETOS';");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_APONTAMENTOS_GERAL_PROJETOS.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnApontamentoArtesGeralClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(
                    @"SELECT * FROM ht.qry_apontamento_geral_projetos WHERE departamento = 'ARTES';");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_APONTAMENTOS_GERAL_ARTES.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnApontamentoProjetosAtividadesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(
                    @"SELECT
                        departamento AS ""Departamento"",
                        identificacao AS ""Identificacao"",
                        centro_custo AS ""CentroCusto"",
                        desc_atividade AS ""DescAtividade"",
                        SUM(hora_trabalhada) AS ""HoraTrabalhada""
                      FROM ht.qry_apontamento_geral_projetos
                      WHERE departamento = 'PROJETOS'
                      GROUP BY departamento, identificacao, centro_custo, desc_atividade
                      ORDER BY departamento, identificacao, centro_custo;");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_APONTAMENTOS_ATIVIDADES_PROJETOS.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnApontamentoArtesAtividadesClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(
                    @"SELECT
                        departamento AS ""Departamento"",
                        identificacao AS ""Identificacao"",
                        centro_custo AS ""CentroCusto"",
                        desc_atividade AS ""DescAtividade"",
                        SUM(hora_trabalhada) AS ""HoraTrabalhada""
                      FROM ht.qry_apontamento_geral_projetos
                      WHERE departamento = 'ARTES'
                      GROUP BY departamento, identificacao, centro_custo, desc_atividade
                      ORDER BY departamento, identificacao, centro_custo;");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_APONTAMENTOS_ATIVIDADES_ARTES.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnApontamentoProjetosAtividadesCentroCustoClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(
                    @"SELECT
                        departamento AS ""Departamento"",
                        identificacao AS ""Identificacao"",
                        centro_custo AS ""CentroCusto"",
                        SUM(hora_trabalhada) AS ""HoraTrabalhada""
                      FROM ht.qry_apontamento_geral_projetos
                      WHERE departamento = 'PROJETOS'
                      GROUP BY departamento, identificacao, centro_custo
                      ORDER BY departamento, identificacao, centro_custo;");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_APONTAMENTOS_ATIVIDADES_PROJETOS_CENTRO_CUSTO.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnApontamentoArtesAtividadesCentroCustoClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(
                    @"SELECT
                        departamento AS ""Departamento"",
                        identificacao AS ""Identificacao"",
                        centro_custo AS ""CentroCusto"",
                        SUM(hora_trabalhada) AS ""HoraTrabalhada""
                      FROM ht.qry_apontamento_geral_projetos
                      WHERE departamento = 'ARTES'
                      GROUP BY departamento, identificacao, centro_custo
                      ORDER BY departamento, identificacao, centro_custo;");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_APONTAMENTOS_ATIVIDADES_ARTES_CENTRO_CUSTO.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnApontamentoProjetosAtividadesFuncionarioClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(
                    @"SELECT
                        departamento AS ""Departamento"",
                        nome_func AS ""Funcionario"",
                        desc_atividade AS ""DescAtividade"",
                        identificacao AS ""Identificacao"",
                        centro_custo AS ""CentroCusto"",
                        SUM(hora_trabalhada) AS ""HoraTrabalhada""
                      FROM ht.qry_apontamento_geral_projetos
                      WHERE departamento = 'PROJETOS'
                      GROUP BY departamento, nome_func, identificacao, centro_custo, desc_atividade
                      ORDER BY nome_func, identificacao, centro_custo;");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_APONTAMENTOS_ATIVIDADES_FUNCIONARIO.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnApontamentoArtesAtividadesFuncionarioClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(
                    @"SELECT
                        departamento AS ""Departamento"",
                        nome_func AS ""Funcionario"",
                        desc_atividade AS ""DescAtividade"",
                        identificacao AS ""Identificacao"",
                        centro_custo AS ""CentroCusto"",
                        SUM(hora_trabalhada) AS ""HoraTrabalhada""
                      FROM ht.qry_apontamento_geral_projetos
                      WHERE departamento = 'ARTES'
                      GROUP BY departamento, nome_func, identificacao, centro_custo, desc_atividade
                      ORDER BY nome_func, identificacao, centro_custo;");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_APONTAMENTOS_ATIVIDADES_FUNCIONARIO_ARTES.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnApontamentoProjetosAtividadesClienteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(
                    @"SELECT
                        departamento AS ""Departamento"",
                        cliente_tema,
                        desc_atividade AS ""DescAtividade"",
                        identificacao AS ""Identificacao"",
                        centro_custo AS ""CentroCusto"",
                        SUM(hora_trabalhada) AS ""HoraTrabalhada""
                      FROM ht.qry_apontamento_geral_projetos
                      WHERE departamento = 'PROJETOS'
                      GROUP BY departamento, cliente_tema, identificacao, centro_custo, desc_atividade
                      ORDER BY cliente_tema, identificacao, centro_custo;");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_APONTAMENTOS_ATIVIDADES_CLENTE.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnApontamentoArtesAtividadesClienteClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                var data = await QueryDataTableAsync(
                    @"SELECT
                        departamento AS ""Departamento"",
                        cliente_tema,
                        desc_atividade AS ""DescAtividade"",
                        identificacao AS ""Identificacao"",
                        centro_custo AS ""CentroCusto"",
                        SUM(hora_trabalhada) AS ""HoraTrabalhada""
                      FROM ht.qry_apontamento_geral_projetos
                      WHERE departamento = 'ARTES'
                      GROUP BY departamento, cliente_tema, identificacao, centro_custo, desc_atividade
                      ORDER BY cliente_tema, identificacao, centro_custo;");
                ExportDataTableToExcelAndOpen(data, "CONSULTA_APONTAMENTOS_ATIVIDADES_CLENTE_ARTES.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnFuncionariosPresentes(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });
                /*string sql = @"
                    SELECT
                        nome_apelido,
                        setor,
                        semana,
                        presente,
                        local_galpao
                    FROM ht.qry_base_funcionario_x_semana_grupo
                    ORDER BY setor, nome_apelido;";*/

                using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
                await connection.OpenAsync();


                var sql = await connection.QueryFirstOrDefaultAsync<string>(@"SELECT ht.pivot_semanas();");

                //SELECT ht.pivot_semanas()

                var dataTable = new System.Data.DataTable();
                using (var command = new NpgsqlCommand(sql, connection))
                using (var dataAdapter = new NpgsqlDataAdapter(command))
                {
                    dataAdapter.Fill(dataTable);
                }

                await connection.CloseAsync();
                ExportDataTableToExcelAndOpen(dataTable, "FUNCIONARIOS-PRESENTES-SEMANA.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }

        private async void OnFuncionariosPresentesMes(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                using var connection = new NpgsqlConnection(BaseSettings.ConnectionString);
                await connection.OpenAsync();


                var sql = await connection.QueryFirstOrDefaultAsync<string>(@"SELECT ht.pivot_mes();");

                var dataTable = new System.Data.DataTable();
                using (var command = new NpgsqlCommand(sql, connection))
                using (var dataAdapter = new NpgsqlDataAdapter(command))
                {
                    dataAdapter.Fill(dataTable);
                }

                await connection.CloseAsync();
                ExportDataTableToExcelAndOpen(dataTable, "FUNCIONARIOS-PRESENTES-MES.xlsx");

                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = null; });
                MessageBox.Show(ex.Message);
            }
        }
    
    }
}

