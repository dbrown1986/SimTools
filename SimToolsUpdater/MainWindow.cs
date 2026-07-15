// SimTools
// Updater
// Main Window Code-Behind
// (C) Archeon Industries, LLC. 2024 - 2026, All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Diagnostics;
using System.Linq;

namespace SimToolsUpdater
{
    public partial class MainWindow : Form
    {
        private const string ManifestBaseUrl = "https://us1-repo.simtools-app.com/App";
        private const string TargetExe = "SimTools.exe";

        private bool _is64BitBinary = false;

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

            // 1. First wake up and populate all design panels so they exist in memory
            InitializeComponent();

            // 2. Map the active culture strings onto the controls before rendering
            InitializeLocalization();
        }

        private async void BtnNext_Click(object sender, EventArgs e)
        {
            // Transition: Welcome -> Update
            if (WelcomeScreen.Visible)
            {
                WelcomeScreen.Visible = false;
                UpdateScreen.Visible = true;
                BtnNext.Enabled = false; // Disable during update processing

                bool verificationPassed = await RunUpdateEngineAsync();

                if (verificationPassed)
                {
                    // Transition: Update -> Complete (Only on complete success)
                    UpdateScreen.Visible = false;
                    CompleteScreen.Visible = true;
                    BtnNext.Text = _currentLanguage.BtnFinish;
                    BtnNext.Enabled = true;
                }
                else
                {
                    // The engine encountered an issue (Target missing, network down, bad manifest)
                    // Re-enable the next button so they aren't completely soft-locked if they want to retry
                    BtnNext.Enabled = true;
                    UpdateScreen.Visible = false;
                    WelcomeScreen.Visible = true;
                }
            }
            // Transition: Complete -> Exit
            else if (CompleteScreen.Visible)
            {
                FinalizeAndExit();
            }
        }

        private async Task<bool> RunUpdateEngineAsync()
        {
            try
            {
                // 1. Verify existence of TargetExe alongside updater
                string executionDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? AppDomain.CurrentDomain.BaseDirectory;
                string localTargetExePath = Path.Combine(executionDirectory, TargetExe);

                if (!File.Exists(localTargetExePath))
                {
                    ShowError(_currentLanguage.ErrTargetMissingTitle, string.Format(_currentLanguage.ErrTargetMissingText, TargetExe));
                    Application.Exit();
                    return false;
                }

                // 2. Read TargetExe PE configuration structure matching x86/x64 streams
                _is64BitBinary = Is64BitImage(localTargetExePath);
                string platformFolder = _is64BitBinary ? "x64" : "x86";

                // 3. Fetch update remote deployment manifest xml package mapping
                string xmlManifestUrl = $"{ManifestBaseUrl}/SimTools-Update-{platformFolder}.xml";
                string manifestData = await SimTools.SecureWebClient.GetStringAsync(xmlManifestUrl);

                XDocument doc = XDocument.Parse(manifestData);
                var elements = doc.Descendants("File").ToList();
                int totalFiles = elements.Count;
                int processedFiles = 0;

                // 4. Update deployment synchronization execution routine loop
                foreach (var fileElement in elements)
                {
                    string relativePath = fileElement.Attribute("name")?.Value ?? "";
                    string remoteFileHash = fileElement.Attribute("hash")?.Value?.ToLowerInvariant() ?? "";
                    string fileUrl = fileElement.Attribute("url")?.Value ?? "";

                    if (string.IsNullOrEmpty(relativePath)) continue;

                    if (string.IsNullOrEmpty(fileUrl))
                    {
                        fileUrl = $"{ManifestBaseUrl}/{platformFolder}/{relativePath.Replace('\\', '/')}";
                    }

                    // STRIP PLATFORM PREFIXES FOR LOCAL PERSISTENCE
                    if (relativePath.StartsWith("x64/", StringComparison.OrdinalIgnoreCase) ||
                        relativePath.StartsWith("x64\\", StringComparison.OrdinalIgnoreCase))
                    {
                        relativePath = relativePath.Substring(4);
                    }
                    else if (relativePath.StartsWith("x86/", StringComparison.OrdinalIgnoreCase) ||
                             relativePath.StartsWith("x86\\", StringComparison.OrdinalIgnoreCase))
                    {
                        relativePath = relativePath.Substring(4);
                    }

                    string targetFileName = Path.Combine(executionDirectory, relativePath);

                    if (File.Exists(targetFileName))
                    {
                        string currentLocalHash = GetFileHash(targetFileName);
                        if (currentLocalHash == remoteFileHash)
                        {
                            processedFiles++;
                            UpdateOverallProgress(processedFiles, totalFiles);
                            continue;
                        }
                    }

                    await DownloadFileWithRetryAsync(fileUrl, targetFileName);
                    processedFiles++;
                    UpdateOverallProgress(processedFiles, totalFiles);
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowError(_currentLanguage.ErrUpdateFailedTitle, string.Format(_currentLanguage.ErrUpdateFailedText, ex.Message));
                this.Close();
                return false;
            }
        }

        private async Task DownloadFileWithRetryAsync(string url, string destination)
        {
            int maxAttempts = 4;
            int delaySeconds = 2;

            string? directoryPath = Path.GetDirectoryName(destination);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    // Use BouncyCastle-powered secure download with real-time progress
                    await SimTools.SecureWebClient.DownloadFileWithProgressAsync(
                        url,
                        destination,
                        onProgress: (pct) => UpdateFileProgress(pct),
                        onIndeterminate: () => UpdateFileProgress(0),
                        onHeadersParsed: (lastModified) => { /* Optional: handle lastModified if needed */ }
                    );
                    return;
                }
                catch (IOException) when (attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
                catch (Exception) when (attempt < maxAttempts)
                {
                    // Catch socket/TLS errors and retry
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }
            }
        }

        private void UpdateFileProgress(int percentage)
        {
            if (FileProgressBar.InvokeRequired)
            {
                FileProgressBar.BeginInvoke(new Action(() => FileProgressBar.Value = Math.Clamp(percentage, 0, 100)));
            }
            else
            {
                FileProgressBar.Value = Math.Clamp(percentage, 0, 100);
            }
        }

        private void UpdateOverallProgress(int current, int total)
        {
            int overallPercentage = (int)((current * 100) / total);
            if (OverallProgressBar.InvokeRequired)
            {
                OverallProgressBar.BeginInvoke(new Action(() => OverallProgressBar.Value = Math.Clamp(overallPercentage, 0, 100)));
            }
            else
            {
                OverallProgressBar.Value = Math.Clamp(overallPercentage, 0, 100);
            }
        }

        private void FinalizeAndExit()
        {
            if (ChkLaunch.Checked)
            {
                string executionDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? AppDomain.CurrentDomain.BaseDirectory;
                string targetExePath = Path.Combine(executionDirectory, TargetExe);

                if (File.Exists(targetExePath))
                {
                    Process.Start(new ProcessStartInfo(targetExePath)
                    {
                        UseShellExecute = true,
                        WorkingDirectory = executionDirectory
                    });
                }
                else
                {
                    ShowError(_currentLanguage.ErrLaunchFailedTitle, string.Format(_currentLanguage.ErrLaunchFailedText, targetExePath));
                }
            }
            Application.Exit();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                _currentLanguage.MsgCancelQuestion,
                _currentLanguage.MsgCancelTitle,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        #region Helper Functions (PE Architecture & Hashing Verification)
        private static bool Is64BitImage(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(stream);

            stream.Seek(0x3c, SeekOrigin.Begin);
            int peOffset = reader.ReadInt32();
            stream.Seek(peOffset, SeekOrigin.Begin);

            uint peHead = reader.ReadUInt32();
            if (peHead != 0x00004550) throw new Exception("Invalid PE Header format.");

            ushort machine = reader.ReadUInt16();
            return machine == 0x8664;
        }

        private static string GetFileHash(string filename)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            using var stream = File.OpenRead(filename);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion

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

            // Global Form Properties
            this.Text = _currentLanguage.FormTitle;

            // Welcome Screen
            lblWelcomeTitle.Text = _currentLanguage.WelcomeTitle;
            lblWelcomeBody.Text = _currentLanguage.WelcomeBody;

            // Update Screen
            lblUpdatingTitle.Text = _currentLanguage.UpdatingTitle;
            lblFileProgress.Text = _currentLanguage.FileProgressLabel;
            lblOverallProgress.Text = _currentLanguage.OverallProgressLabel;

            // Complete Screen
            lblCompleteTitle.Text = _currentLanguage.CompleteTitle;
            lblCompleteBody.Text = _currentLanguage.CompleteBody;
            ChkLaunch.Text = _currentLanguage.ChkLaunchText;

            // Persistent Navigation Controls
            BtnNext.Text = _currentLanguage.BtnNext;
            BtnCancel.Text = _currentLanguage.BtnCancel;
        }

        public class LanguageStrings
        {
            public string FormTitle { get; set; } = "SimTools Update Wizard";
            public string BtnNext { get; set; } = "Next >";
            public string BtnCancel { get; set; } = "Cancel";
            public string BtnFinish { get; set; } = "Finish";

            public string WelcomeTitle { get; set; } = "Welcome to the SimTools Update Wizard";
            public string WelcomeBody { get; set; } = "The Update Wizard will update the existing version of SimTools on your computer to the latest available release.\r\n\r\nClick Next to continue or Cancel to exit the Setup Wizard.";

            public string UpdatingTitle { get; set; } = "Updating...";
            public string FileProgressLabel { get; set; } = "File Progress:";
            public string OverallProgressLabel { get; set; } = "Overall Progress:";

            public string CompleteTitle { get; set; } = "Completing the SimTools Setup Wizard";
            public string CompleteBody { get; set; } = "SimTools has been successfully updated on your computer.";
            public string ChkLaunchText { get; set; } = "Launch SimTools now";

            public string MsgCancelQuestion { get; set; } = "Are you sure you want to cancel the SimTools update verification cycle?";
            public string MsgCancelTitle { get; set; } = "Cancel Setup";

            public string ErrTargetMissingTitle { get; set; } = "Target Missing";
            public string ErrTargetMissingText { get; set; } = "Could not locate {0} inside the application folder. Please make sure the updater is located in the main directory.";
            public string ErrUpdateFailedTitle { get; set; } = "Update Failed";
            public string ErrUpdateFailedText { get; set; } = "An unexpected error occurred during the update cycle:\n\n{0}\n\nPlease check your internet connection and try running the updater as an Administrator.";
            public string ErrLaunchFailedTitle { get; set; } = "Launch Failed";
            public string ErrLaunchFailedText { get; set; } = "Could not find SimTools.exe at:\n{0}";
        }

        public static class LocalizationData
        {
            public static readonly Dictionary<string, LanguageStrings> Translations = new Dictionary<string, LanguageStrings>
            {
                ["en"] = new LanguageStrings(),

                ["ar"] = new LanguageStrings
                {
                    FormTitle = "معالج تحديث SimTools",
                    BtnNext = "التالي <",
                    BtnCancel = "إلغاء الأمر",
                    BtnFinish = "إنهاء",
                    WelcomeTitle = "مرحبًا بك في معالج تحديث SimTools",
                    WelcomeBody = "سيقوم معالج التحديث بتحديث الإصدار الحالي من SimTools على جهاز الكمبيوتر الخاص بك إلى أحدث إصدار متاح.\n\nانقر فوق التالي للمتابعة أو إلغاء الأمر للخروج من المعالج.",
                    UpdatingTitle = "جاري التحديث...",
                    FileProgressLabel = "تقدم الملف:",
                    OverallProgressLabel = "التقدم العام:",
                    CompleteTitle = "إكمال معالج إعداد SimTools",
                    CompleteBody = "تم تحديث SimTools بنجاح على جهاز الكمبيوتر الخاص بك.",
                    ChkLaunchText = "تشغيل SimTools الآن",
                    MsgCancelQuestion = "هل أنت متأكد من أنك تريد إلغاء دورة التحقق من تحديث SimTools؟",
                    MsgCancelTitle = "إلغاء الإعداد",
                    ErrTargetMissingTitle = "الهدف مفقود",
                    ErrTargetMissingText = "تعذر تحديد موقع {0} داخل مجلد التطبيق. يرجى التأكد من وجود أداة التحديث في الدليل الرئيسي.",
                    ErrUpdateFailedTitle = "فشل التحديث",
                    ErrUpdateFailedText = "حدث خطأ غير متوقع أثناء دورة التحديث:\n\n{0}\n\nيرجى التحقق من اتصال الإنترنت الخاص بك ومحاولة تشغيل أداة التحديث كمسؤول.",
                    ErrLaunchFailedTitle = "فشل التشغيل",
                    ErrLaunchFailedText = "تعذر العثور على الملف التنفيذي في:\n{0}"
                },

                ["de"] = new LanguageStrings
                {
                    FormTitle = "SimTools Update-Assistent",
                    BtnNext = "Weiter >",
                    BtnCancel = "Abbrechen",
                    BtnFinish = "Fertigstellen",
                    WelcomeTitle = "Willkommen beim SimTools Update-Assistenten",
                    WelcomeBody = "Der Update-Assistent aktualisiert die vorhandene Version von SimTools auf Ihrem Computer auf die neueste verfügbare Version.\n\nKlicken Sie auf Weiter, um fortzufahren, oder auf Abbrechen, um den Assistenten zu beenden.",
                    UpdatingTitle = "Aktualisierung läuft...",
                    FileProgressLabel = "Datei-Fortschritt:",
                    OverallProgressLabel = "Gesamtfortschritt:",
                    CompleteTitle = "Fertigstellen des SimTools Installations-Assistenten",
                    CompleteBody = "SimTools wurde erfolgreich auf Ihrem Computer aktualisiert.",
                    ChkLaunchText = "SimTools jetzt starten",
                    MsgCancelQuestion = "Sind Sie sicher, dass Sie die Überprüfung des SimTools-Updates abbrechen möchten?",
                    MsgCancelTitle = "Setup abbrechen",
                    ErrTargetMissingTitle = "Ziel fehlt",
                    ErrTargetMissingText = "{0} konnte im Anwendungsordner nicht gefunden werden. Bitte stellen Sie sicher, dass sich der Updater im Hauptverzeichnis befindet.",
                    ErrUpdateFailedTitle = "Update fehlgeschlagen",
                    ErrUpdateFailedText = "Während des Update-Zyklus ist unvorhergesehen ein Fehler aufgetreten:\n\n{0}\n\nBitte überprüfen Sie Ihre Internetverbindung und versuchen Sie, den Updater als Administrator auszuführen.",
                    ErrLaunchFailedTitle = "Start fehlgeschlagen",
                    ErrLaunchFailedText = "SimTools.exe konnte nicht gefunden werden unter:\n{0}"
                },

                ["es"] = new LanguageStrings
                {
                    FormTitle = "Asistente de Actualización de SimTools",
                    BtnNext = "Siguiente >",
                    BtnCancel = "Cancelar",
                    BtnFinish = "Finalizar",
                    WelcomeTitle = "Bienvenido al Asistente de Actualización de SimTools",
                    WelcomeBody = "El Asistente de Actualización actualizará la versión existente de SimTools en su computadora a la última versión disponible.\n\nHaga clic en Siguiente para continuar o en Cancelar para salir del asistente.",
                    UpdatingTitle = "Actualizando...",
                    FileProgressLabel = "Progreso del Archivo:",
                    OverallProgressLabel = "Progreso General:",
                    CompleteTitle = "Completando el Asistente de Instalación de SimTools",
                    CompleteBody = "SimTools se ha actualizado correctamente en su computadora.",
                    ChkLaunchText = "Ejecutar SimTools ahora",
                    MsgCancelQuestion = "¿Está seguro de que desea cancelar el ciclo de verificación de la actualización de SimTools?",
                    MsgCancelTitle = "Cancelar Configuración",
                    ErrTargetMissingTitle = "Objetivo Faltante",
                    ErrTargetMissingText = "No se pudo localizar {0} dentro de la carpeta de la aplicación. Asegúrese de que el actualizador se encuentre en el directorio principal.",
                    ErrUpdateFailedTitle = "Actualización Fallida",
                    ErrUpdateFailedText = "Ocurrió un error inesperado durante el ciclo de actualización:\n\n{0}\n\nCompruebe su conexión a Internet e intente ejecutar el actualizador como Administrador.",
                    ErrLaunchFailedTitle = "Error de Lanzamiento",
                    ErrLaunchFailedText = "No se pudo encontrar SimTools.exe en:\n{0}"
                },

                ["fr"] = new LanguageStrings
                {
                    FormTitle = "Assistant de Mise à Jour SimTools",
                    BtnNext = "Suivant >",
                    BtnCancel = "Annuler",
                    BtnFinish = "Terminer",
                    WelcomeTitle = "Bienvenido dans l'assistant de mise à jour SimTools",
                    WelcomeBody = "L'assistant va mettre à jour la version existante de SimTools sur votre ordinateur vers la dernière version disponible.\n\nCliquez sur Suivant pour continuer ou sur Annuler para quitter l'assistant.",
                    UpdatingTitle = "Mise à jour en cours...",
                    FileProgressLabel = "Progression du fichier :",
                    OverallProgressLabel = "Progression générale :",
                    CompleteTitle = "Fin de l'installation de SimTools",
                    CompleteBody = "SimTools a été mis à jour avec succès sur votre ordinateur.",
                    ChkLaunchText = "Lancer SimTools maintenant",
                    MsgCancelQuestion = "Êtes-vous sûr de vouloir annuler le cycle de vérification de mise à jour de SimTools ?",
                    MsgCancelTitle = "Annuler la configuration",
                    ErrTargetMissingTitle = "Cible manquante",
                    ErrTargetMissingText = "Impossible de localiser {0} dans le dossier de l'application. Veuillez vous assurer que le programme de mise à jour se trouve dans le répertoire principal.",
                    ErrUpdateFailedTitle = "Échec de la mise à jour",
                    ErrUpdateFailedText = "Une erreur inattendue est survenue pendant le cycle de mise à jour :\n\n{0}\n\nVeuillez vérifier votre connexion Internet et essayer d'exécuter le programme en tant qu'Administrateur.",
                    ErrLaunchFailedTitle = "Échec du lancement",
                    ErrLaunchFailedText = "SimTools.exe est introuvable à l'emplacement :\n{0}"
                },

                ["ja"] = new LanguageStrings
                {
                    FormTitle = "SimTools アップデートウィザード",
                    BtnNext = "次へ >",
                    BtnCancel = "キャンセル",
                    BtnFinish = "完了",
                    WelcomeTitle = "SimTools アップデートウィザードへようこそ",
                    WelcomeBody = "アップデートウィザードは、コンピューター上の既存の SimTools を最新のリリースに更新します。\n\n続行するには「次へ」、終了するには「キャンセル」をクリックしてください。",
                    UpdatingTitle = "更新中...",
                    FileProgressLabel = "ファイルの進行状況:",
                    OverallProgressLabel = "全体の進行状況:",
                    CompleteTitle = "SimTools セットアップウィザードの完了",
                    CompleteBody = "SimTools はコンピューター上で正常にアップデートされました。",
                    ChkLaunchText = "今すぐ SimTools を起動する",
                    MsgCancelQuestion = "SimTools のアップデート確認処理をキャンセルしてもよろしいですか？",
                    MsgCancelTitle = "セットアップのキャンセル",
                    ErrTargetMissingTitle = "ターゲットが見つかりません",
                    ErrTargetMissingText = "アプリケーションフォルダ内に {0} が見つかりませんでした。アップデートプログラムがメインディレクトリに配置されているか確認してください。",
                    ErrUpdateFailedTitle = "アップデート失敗",
                    ErrUpdateFailedText = "アップデートサイクル中に予期しないエラーが発生しました:\n\n{0}\n\nインターネット接続を確認し、管理者として実行してください。",
                    ErrLaunchFailedTitle = "起動失敗",
                    ErrLaunchFailedText = "次のパスに SimTools.exe が見つかりませんでした:\n{0}"
                },

                ["pt"] = new LanguageStrings
                {
                    FormTitle = "Assistente de Atualização do SimTools",
                    BtnNext = "Avançar >",
                    BtnCancel = "Cancelar",
                    BtnFinish = "Concluir",
                    WelcomeTitle = "Bem-vindo ao Assistente de Atualização do SimTools",
                    WelcomeBody = "O Assistente de Atualização irá atualizar a versão existente do SimTools no seu computador para a versão mais recente disponível.\n\nClique em Avançar para continuar ou Cancelar para sair do assistente.",
                    UpdatingTitle = "Atualizando...",
                    FileProgressLabel = "Progresso do Arquivo:",
                    OverallProgressLabel = "Progresso Geral:",
                    CompleteTitle = "Concluindo o Assistente de Instalação do SimTools",
                    CompleteBody = "O SimTools foi atualizado com sucesso no seu computador.",
                    ChkLaunchText = "Iniciar o SimTools agora",
                    MsgCancelQuestion = "Tem certeza de que deseja cancelar o ciclo de verificação de atualização do SimTools?",
                    MsgCancelTitle = "Cancelar Configuração",
                    ErrTargetMissingTitle = "Destino Ausente",
                    ErrTargetMissingText = "Não foi possível localizar o arquivo {0} dentro da pasta do aplicativo. Certifique-se de que o atualizador está no diretório principal.",
                    ErrUpdateFailedTitle = "Falha na Atualização",
                    ErrUpdateFailedText = "Ocorreu um erro inesperado durante o ciclo de atualização:\n\n{0}\n\nPor favor, verifique sua conexão com a internet e tente executar o atualizador como Administrador.",
                    ErrLaunchFailedTitle = "Falha ao Iniciar",
                    ErrLaunchFailedText = "Não foi possível encontrar SimTools.exe em:\n{0}"
                },

                ["ru"] = new LanguageStrings
                {
                    FormTitle = "Мастер обновления SimTools",
                    BtnNext = "Далее >",
                    BtnCancel = "Отмена",
                    BtnFinish = "Завершить",
                    WelcomeTitle = "Вас приветствует мастер обновления SimTools",
                    WelcomeBody = "Мастер обновления обновит существующую версию SimTools на вашем компьютере до последнего доступного выпуска.\n\nНажмите «Далее» для продолжения или «Отмена» для выхода из мастера.",
                    UpdatingTitle = "Обновление...",
                    FileProgressLabel = "Прогресс файла:",
                    OverallProgressLabel = "Общий прогресс:",
                    CompleteTitle = "Завершение работы мастера установки SimTools",
                    CompleteBody = "Программа SimTools была успешно обновлена на вашем компьютере.",
                    ChkLaunchText = "Запустить SimTools сейчас",
                    MsgCancelQuestion = "Вы уверены, что хотите отменить цикл проверки обновлений SimTools?",
                    MsgCancelTitle = "Отмена установки",
                    ErrTargetMissingTitle = "Целевой файл отсутствует",
                    ErrTargetMissingText = "Не удалось найти {0} в папке приложения. Пожалуйста, убедитесь, что программа обновления находится в основном каталоге.",
                    ErrUpdateFailedTitle = "Обновление не удалось",
                    ErrUpdateFailedText = "Во время цикла обновления произошла непредвиденная ошибка:\n\n{0}\n\nПожалуйста, проверьте интернет-соединение и попробуйте запустить программу от имени Администратора.",
                    ErrLaunchFailedTitle = "Ошибка запуска",
                    ErrLaunchFailedText = "Не удалось найти SimTools.exe по пути:\n{0}"
                },

                ["zh"] = new LanguageStrings
                {
                    FormTitle = "SimTools 更新向导",
                    BtnNext = "下一步 >",
                    BtnCancel = "取消",
                    BtnFinish = "完成",
                    WelcomeTitle = "欢迎使用 SimTools 更新向导",
                    WelcomeBody = "更新向导将把您计算机上现有的 SimTools 版本升级到最新的可用版本。\n\n点击“下一步”继续，或点击“取消”退出更新向导。",
                    UpdatingTitle = "正在更新...",
                    FileProgressLabel = "文件进度:",
                    OverallProgressLabel = "总体进度:",
                    CompleteTitle = "正在完成 SimTools 安装向导",
                    CompleteBody = "SimTools 已成功在您的计算机上完成更新升级。",
                    ChkLaunchText = "立即运行 SimTools",
                    MsgCancelQuestion = "您确定要取消当前的 SimTools 更新验证进程吗？",
                    MsgCancelTitle = "取消安装",
                    ErrTargetMissingTitle = "目标文件丢失",
                    ErrTargetMissingText = "无法在应用程序文件夹内找到 {0}。请确保更新程序位于软件主运行目录下。",
                    ErrUpdateFailedTitle = "更新失败",
                    ErrUpdateFailedText = "更新循环过程中发生了未预期的错误:\n\n{0}\n\n请检查您的网络连接并尝试以管理员权限重新运行此更新程序。",
                    ErrLaunchFailedTitle = "运行失败",
                    ErrLaunchFailedText = "无法在以下路径找到 SimTools.exe:\n{0}"
                }
            };
        }
        #endregion
    }
}