using Apontamento.DataBase;
using Apontamento.Localization;
using BibliotecasSIG;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Telerik.Windows.Controls;

namespace Apontamento
{

    public partial class App : Application
    {

        private readonly string CURRENT_VERSION = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string CurrentVersion => CURRENT_VERSION;

        public App()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            ConfigureCulture();
            DapperTypeHandlers.Configure();
            LocalizationManager.Manager = new SigTelerikLocalizationManager();
            StyleManager.ApplicationTheme = new FluentTheme();
            DataBaseSettings BaseSettings = DataBaseSettings.Instance;
            BaseSettings.LoadFromConfiguration();

        }

        private static void ConfigureCulture()
        {
            var culture = new CultureInfo("pt-BR");

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            await CheckForUpdatesAsync();
        }

        public async Task CheckForUpdatesAsync(bool showUpToDate = false)
        {
            try
            {
                var updateInfoUrl = DataBaseSettings.Instance.UpdateInfoUrl;

                if (string.IsNullOrWhiteSpace(updateInfoUrl))
                    return;

                var updateChecker = new UpdateChecker(updateInfoUrl, CURRENT_VERSION);
                var updateInfo = await updateChecker.CheckForUpdatesAsync();

                var updateInfoJson = JsonSerializer.Serialize<UpdateInfo>(updateInfo);

                if (updateInfo == null)
                {
                    if (showUpToDate)
                        MessageBox.Show($"O sistema já está atualizado.\n\nVersão atual: {CURRENT_VERSION}", "Atualização do sistema", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (updateInfo != null)
                {
                    // Pergunta ao usuário se deseja atualizar
                    var result = MessageBox.Show(
                        $"Nova versão disponível!\n\n" +
                        $"Versão atual: {CURRENT_VERSION}\n" +
                        $"Nova versão: {updateInfo.updateVersion}\n\n" +
                        "Changelog:\n" +
                        string.Join("\n", updateInfo.changelog) +
                        "\n\nDeseja baixar a atualização?",
                        "Atualização Disponível",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information
                    );

                    if (result == MessageBoxResult.Yes)
                    {

                        var options = new JsonSerializerOptions { WriteIndented = true };
                        string jsonString = JsonSerializer.Serialize(updateInfo, options);

                        //Process.Start("Update.exe", @$"{updateInfoJson}, Apontamento.exe");

                        string jsonData = JsonSerializer.Serialize(updateInfo); // Garante que o JSON está bem formatado
                        string appName = "Apontamento.exe";

                        string arguments = $"\"{jsonData.Replace("\"", "\\\"")}\" \"{appName}\"";
                        Process.Start("Update.exe", arguments);
                        this.Shutdown();

                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Log do erro ou tratamento de exceção
                MessageBox.Show(
                    $"Erro ao verificar atualizações: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erro ao verificar atualizações: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}


