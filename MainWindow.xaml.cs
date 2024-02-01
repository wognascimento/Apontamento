﻿using Apontamento.DataBase;
using Apontamento.Views.Producao;
using Apontamento.Views.Projetos;
using Microsoft.EntityFrameworkCore;
using Producao;
using Syncfusion.Linq;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Tools;
using Syncfusion.Windows.Tools.Controls;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
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
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using SizeMode = Syncfusion.SfSkinManager.SizeMode;

namespace Apontamento
{
    /// <summary>
    /// Lógica interna para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Fields
        private string currentVisualStyle;
        private string currentSizeMode;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the current visual style.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string CurrentVisualStyle
        {
            get { return currentVisualStyle; }
            set { currentVisualStyle = value; OnVisualStyleChanged();}
        }

        /// <summary>
        /// Gets or sets the current Size mode.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public string CurrentSizeMode
        {
            get { return currentSizeMode; }
            set { currentSizeMode = value; OnSizeModeChanged(); }
        }

        #endregion

        /// <summary>
        /// On Visual Style Changed.
        /// </summary>
        /// <remarks></remarks>
        private void OnVisualStyleChanged()
        {
            VisualStyles visualStyle = VisualStyles.Default;
            Enum.TryParse(CurrentVisualStyle, out visualStyle);
            if (visualStyle != VisualStyles.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetVisualStyle(this, visualStyle);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }

        /// <summary>
        /// On Size Mode Changed event.
        /// </summary>
        /// <remarks></remarks>
        private void OnSizeModeChanged()
        {
            SizeMode sizeMode = SizeMode.Default;
            Enum.TryParse(CurrentSizeMode, out sizeMode);
            if (sizeMode != SizeMode.Default)
            {
                SfSkinManager.ApplyStylesOnApplication = true;
                SfSkinManager.SetSizeMode(this, sizeMode);
                SfSkinManager.ApplyStylesOnApplication = false;
            }
        }

        /// <summary>
        /// Called when [loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CurrentVisualStyle = "Office365"; // "FluentLight";
            CurrentSizeMode = "Default";
        }

        DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            StyleManager.ApplicationTheme = new Windows11Theme();

            var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
            if (appSettings[0].Length > 0)
                BaseSettings.Username = appSettings[0];

            txtUsername.Text = BaseSettings.Username;
            txtDataBase.Text = BaseSettings.Database;
        }

        private void _mdi_CloseAllTabs(object sender, Syncfusion.Windows.Tools.Controls.CloseTabEventArgs e)
        {
            _mdi.Items.Clear();
        }

        private void _mdi_CloseButtonClick(object sender, Syncfusion.Windows.Tools.Controls.CloseButtonEventArgs e)
        {
            var tab = (DocumentContainer)sender;
            _mdi.Items.Remove(tab.ActiveDocument);
        }

        private void OnAlterarUsuario(object sender, MouseButtonEventArgs e)
        {
            Login window = new();
            window.ShowDialog();

            try
            {
                var appSettings = ConfigurationManager.GetSection("appSettings") as NameValueCollection;
                BaseSettings.Username = appSettings[0];
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
                        _mdi.Items.Clear();
                    }
                }
            });
        }

        public void adicionarFilho(object filho, string title, string name)
        {
            var doc = ExistDocumentInDocumentContainer(name);
            if (doc == null)
            {
                doc = (FrameworkElement?)filho;
                DocumentContainer.SetHeader(doc, title);
                doc.Name = name.ToLower();
                _mdi.Items.Add(doc);
            }
            else
            {
                //_mdi.RestoreDocument(doc as UIElement);
                _mdi.ActiveDocument = doc;
            }
        }

        private FrameworkElement ExistDocumentInDocumentContainer(string name_)
        {
            foreach (FrameworkElement element in _mdi.Items)
            {
                if (name_.ToLower() == element.Name)
                {
                    return element;
                }
            }
            return null;
        }

        private void OnDigitarApontamentoProjetosClick(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ApontamentoProjetos(), "APONTAMENTO PROJETOS", "ApontamentoProjetos");
        }

        private void OnDigitarApontamentoProducaoClick(object sender, RoutedEventArgs e)
        {
            adicionarFilho(new ApontamentoProducao(), "APONTAMENTO PRODUCAO", "ApontamentoProducao");
        }

        private async void OnFuroProducaoClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => { Mouse.OverrideCursor = Cursors.Wait; });

                using DatabaseContext db = new();

                var data = await db.QryHtFuros.ToListAsync();


                using ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;

                application.DefaultVersion = ExcelVersion.Xlsx;

                //Create a workbook
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet worksheet = workbook.Worksheets[0];
                //worksheet.IsGridLinesVisible = false;
                worksheet.ImportData(data, 1, 1, true);

                workbook.SaveAs("Impressos/CONSULTA_FURO.xlsx");
                Process.Start(new ProcessStartInfo("Impressos\\CONSULTA_FURO.xlsx")
                {
                    UseShellExecute = true
                });

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

        private void OnImprimirProducaoClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
