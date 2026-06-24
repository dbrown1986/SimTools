using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimToolsUninstaller
{
    public partial class MainWindow : Form
    {
        // Central Language State Backing Field
        private LanguageStrings _currentLanguage = new LanguageStrings();

        public MainWindow()
        {
            // =================================================================================
            // LANGUAGE TESTING SUITE (Uncomment ONE line below to force-test that localization)
            // =================================================================================
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ar"); // Arabic (RTL)
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("de"); // German
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("es"); // Spanish
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fr"); // French
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ja"); // Japanese
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("pt"); // Portuguese
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("ru"); // Russian
            // System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh"); // Chinese
            // =================================================================================

            InitializeComponent();
            InitializeLocalization(); // Maps strings right into the initialized controls
        }

        private async void MainWindow_Load(object? sender, EventArgs e)
        {
            // 1. Prompt the User
            var result = MessageBox.Show(
                _currentLanguage.MsgConfirmText,
                _currentLanguage.MsgConfirmTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.No)
            {
                Application.Exit();
                return;
            }

            // 2. Locate Manifest
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string manifestPath = Path.Combine(baseDir, "uninstall_manifest.json");

            if (!File.Exists(manifestPath))
            {
                MessageBox.Show(
                    _currentLanguage.MsgNoManifestText,
                    _currentLanguage.MsgNoManifestTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            try
            {
                // 3. Read and Deserialize JSON
                string jsonContent = await File.ReadAllTextAsync(manifestPath);
                List<string>? installedPaths = JsonSerializer.Deserialize<List<string>>(jsonContent);

                if (installedPaths != null && installedPaths.Count > 0)
                {
                    // 4. Reverse the list (files inside directories MUST be deleted before the directories themselves)
                    installedPaths.Reverse();

                    progOverall.Maximum = installedPaths.Count;
                    int progress = 0;

                    // 5. Deletion Loop
                    foreach (string path in installedPaths)
                    {
                        progress++;
                        progOverall.Value = progress;

                        // Apply localized string formatting pattern
                        lblStatus.Text = string.Format(_currentLanguage.RemovingPattern, Path.GetFileName(path));

                        // Slight delay so the UI doesn't freeze and the user can actually see progress
                        await Task.Delay(10);

                        try
                        {
                            if (File.Exists(path))
                            {
                                File.Delete(path);
                            }
                            else if (Directory.Exists(path))
                            {
                                Directory.Delete(path, false);
                            }
                        }
                        catch
                        {
                            // Ignore locked files or non-empty directories to ensure the uninstaller doesn't crash mid-way
                        }
                    }
                }

                // Clean up installer artifacts
                try { File.Delete(manifestPath); } catch { }
                try { File.Delete(Path.Combine(baseDir, "install_log.txt")); } catch { }

                // 6. Queue Self-Deletion
                QueueSelfDeletion(baseDir);

                MessageBox.Show(
                    _currentLanguage.MsgSuccessText,
                    _currentLanguage.MsgSuccessTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format(_currentLanguage.MsgErrorText, ex.Message),
                    _currentLanguage.MsgErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                Application.Exit();
            }
        }

        private void QueueSelfDeletion(string installDir)
        {
            string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exePath)) return;

            string batPath = Path.Combine(Path.GetTempPath(), "SimTools_Cleanup.bat");

            string batScript = $@"
@echo off
ping 127.0.0.1 -n 3 > nul
del ""{exePath}"" /q
rmdir ""{installDir}"" /s /q
del ""%~f0""
";

            File.WriteAllText(batPath, batScript);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{batPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };

            Process.Start(psi);
        }

        #region Localization Storage Infrastructure
        private void InitializeLocalization()
        {
            string langCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();

            if (LocalizationData.Translations.ContainsKey(langCode))
            {
                _currentLanguage = LocalizationData.Translations[langCode];
            }
            else
            {
                _currentLanguage = LocalizationData.Translations["en"]; // Fallback
            }

            // Apply to baseline components
            this.Text = _currentLanguage.FormTitle;
            lblStatus.Text = _currentLanguage.Initializing;
        }

        public class LanguageStrings
        {
            public string FormTitle { get; set; } = "SimTools Uninstaller";
            public string Initializing { get; set; } = "Initializing uninstaller...";
            public string RemovingPattern { get; set; } = "Removing: {0}";
            public string MsgConfirmText { get; set; } = "Are you sure you want to completely remove SimTools and all of its components?";
            public string MsgConfirmTitle { get; set; } = "SimTools Uninstaller";
            public string MsgNoManifestText { get; set; } = "The uninstall_manifest.json file was not found. Cannot proceed with automatic uninstallation.";
            public string MsgNoManifestTitle { get; set; } = "Error";
            public string MsgSuccessText { get; set; } = "SimTools has been successfully removed from your computer.";
            public string MsgSuccessTitle { get; set; } = "Uninstall Complete";
            public string MsgErrorText { get; set; } = "An error occurred during uninstallation:\n{0}";
            public string MsgErrorTitle { get; set; } = "Uninstallation Error";
        }

        public static class LocalizationData
        {
            public static readonly Dictionary<string, LanguageStrings> Translations = new Dictionary<string, LanguageStrings>
            {
                ["en"] = new LanguageStrings(),

                ["ar"] = new LanguageStrings
                {
                    FormTitle = "برنامج إزالة التثبيت لـ SimTools",
                    Initializing = "جاري تهيئة برنامج إزالة التثبيت...",
                    RemovingPattern = "جاري إزالة: {0}",
                    MsgConfirmText = "هل أنت متأكد من أنك تريد إزالة SimTools تمامًا وجميع مكوناته؟",
                    MsgConfirmTitle = "برنامج إزالة التثبيت لـ SimTools",
                    MsgNoManifestText = "لم يتم العثور على ملف uninstall_manifest.json. لا يمكن المتابعة في إزالة التثبيت التلقائية.",
                    MsgNoManifestTitle = "خطأ",
                    MsgSuccessText = "تمت إزالة SimTools بنجاح من جهاز الكمبيوتر الخاص بك.",
                    MsgSuccessTitle = "اكتملت إزالة التثبيت",
                    MsgErrorText = "حدث خطأ أثناء إزالة التثبيت:\n{0}",
                    MsgErrorTitle = "خطأ في إزالة التثبيت"
                },

                ["de"] = new LanguageStrings
                {
                    FormTitle = "SimTools Deinstallationsprogramm",
                    Initializing = "Deinstallationsprogramm wird initialisiert...",
                    RemovingPattern = "Entfernen von: {0}",
                    MsgConfirmText = "Sind Sie sicher, dass Sie SimTools und alle seine Komponenten vollständig entfernen möchten?",
                    MsgConfirmTitle = "SimTools Deinstallationsprogramm",
                    MsgNoManifestText = "Die Datei uninstall_manifest.json wurde nicht gefunden. Die automatische Deinstallation kann nicht fortgesetzt werden.",
                    MsgNoManifestTitle = "Fehler",
                    MsgSuccessText = "SimTools wurde erfolgreich von Ihrem Computer entfernt.",
                    MsgSuccessTitle = "Deinstallation abgeschlossen",
                    MsgErrorText = "Während der Deinstallation ist ein Fehler aufgetreten:\n{0}",
                    MsgErrorTitle = "Deinstallationsfehler"
                },

                ["es"] = new LanguageStrings
                {
                    FormTitle = "Desinstalador de SimTools",
                    Initializing = "Inicializando el desinstalador...",
                    RemovingPattern = "Eliminando: {0}",
                    MsgConfirmText = "¿Está seguro de que desea eliminar por completo SimTools y todos sus componentes?",
                    MsgConfirmTitle = "Desinstalador de SimTools",
                    MsgNoManifestText = "No se encontró el archivo uninstall_manifest.json. No se puede proceder con la desinstalación automática.",
                    MsgNoManifestTitle = "Error",
                    MsgSuccessText = "SimTools se ha eliminado correctamente de su computadora.",
                    MsgSuccessTitle = "Desinstalación completada",
                    MsgErrorText = "Ocurrió un error durante la desinstalación:\n{0}",
                    MsgErrorTitle = "Error de desinstalación"
                },

                ["fr"] = new LanguageStrings
                {
                    FormTitle = "Désinstallateur SimTools",
                    Initializing = "Initialisation du désinstallateur...",
                    RemovingPattern = "Suppression de : {0}",
                    MsgConfirmText = "Êtes-vous sûr de vouloir supprimer complètement SimTools et tous ses composants ?",
                    MsgConfirmTitle = "Désinstallateur SimTools",
                    MsgNoManifestText = "Le fichier uninstall_manifest.json est introuvable. Impossible de procéder à la désinstallation automatique.",
                    MsgNoManifestTitle = "Erreur",
                    MsgSuccessText = "SimTools a été supprimé avec succès de votre ordinateur.",
                    MsgSuccessTitle = "Désinstallation terminée",
                    MsgErrorText = "Une erreur est survenue lors de la désinstallation :\n{0}",
                    MsgErrorTitle = "Erreur de désinstallation"
                },

                ["ja"] = new LanguageStrings
                {
                    FormTitle = "SimTools アンインストーラー",
                    Initializing = "アンインストーラーを初期化中...",
                    RemovingPattern = "削除中: {0}",
                    MsgConfirmText = "SimTools とそのすべてのコンポーネントを完全に削除してもよろしいですか？",
                    MsgConfirmTitle = "SimTools アンインストーラー",
                    MsgNoManifestText = "uninstall_manifest.json ファイルが見つかりませんでした。自動アンインストールを続行できません。",
                    MsgNoManifestTitle = "エラー",
                    MsgSuccessText = "SimTools はコンピューターから正常に削除されました。",
                    MsgSuccessTitle = "アンインストール完了",
                    MsgErrorText = "アンインストール中にエラーが発生しました:\n{0}",
                    MsgErrorTitle = "アンインストールエラー"
                },

                ["pt"] = new LanguageStrings
                {
                    FormTitle = "Desinstalador do SimTools",
                    Initializing = "Inicializando o desinstalador...",
                    RemovingPattern = "Removendo: {0}",
                    MsgConfirmText = "Tem certeza de que deseja remover completamente o SimTools e todos os seus componentes?",
                    MsgConfirmTitle = "Desinstalador do SimTools",
                    MsgNoManifestText = "O arquivo uninstall_manifest.json não foi encontrado. Não é possível prosseguir com a desinstalação automática.",
                    MsgNoManifestTitle = "Erro",
                    MsgSuccessText = "O SimTools foi removido com sucesso do seu computador.",
                    MsgSuccessTitle = "Desinstalação concluída",
                    MsgErrorText = "Ocorreu um erro durante a desinstalação:\n{0}",
                    MsgErrorTitle = "Erro de desinstalação"
                },

                ["ru"] = new LanguageStrings
                {
                    FormTitle = "Программа удаления SimTools",
                    Initializing = "Инициализация программы удаления...",
                    RemovingPattern = "Удаление: {0}",
                    MsgConfirmText = "Вы уверены, что хотите полностью удалить SimTools и все его компоненты?",
                    MsgConfirmTitle = "Программа удаления SimTools",
                    MsgNoManifestText = "Файл uninstall_manifest.json не найден. Невозможно продолжить автоматическое удаление.",
                    MsgNoManifestTitle = "Ошибка",
                    MsgSuccessText = "Программа SimTools была успешно удалена с вашего компьютера.",
                    MsgSuccessTitle = "Удаление завершено",
                    MsgErrorText = "Произошла ошибка при удалении:\n{0}",
                    MsgErrorTitle = "Ошибка удаления"
                },

                ["zh"] = new LanguageStrings
                {
                    FormTitle = "SimTools 卸载程序",
                    Initializing = "正在初始化卸载程序...",
                    RemovingPattern = "正在移除: {0}",
                    MsgConfirmText = "您确定要完全移除 SimTools 及其所有组件吗？",
                    MsgConfirmTitle = "SimTools 卸载程序",
                    MsgNoManifestText = "未找到 uninstall_manifest.json 文件。无法继续自动卸载。",
                    MsgNoManifestTitle = "错误",
                    MsgSuccessText = "SimTools 已成功从您的计算机中移除。",
                    MsgSuccessTitle = "卸载完成",
                    MsgErrorText = "卸载过程中发生错误:\n{0}",
                    MsgErrorTitle = "卸载错误"
                }
            };
        }
        #endregion
    }
}