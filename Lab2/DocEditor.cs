using Editor.Scenes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Editor.DocEditor
{
    public class DocEditor
    {
        //Настройка консоли для отображения ANSI (жирный, курсив, подчёркнутый)
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int nStdHandle);

        const int STD_OUTPUT_HANDLE = -11;
        const int ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        //Конец настройки

        private EditorSettings editorSettings = EditorSettings.Instance; //Настройки редактора
        private bool editorWork = true; //Пока true, редактор будет работать
        private Scene currentScene; //Текущая отображаемая сцена
        private (string login, string password, string role) currentUser; //Текущий юзер после входа
        private List<(string login, string password, string role)> accounts = new(); //Аккаунты
        private string docContent;
        private (bool isCloud, string subtype) fileManager;
        private string openedFile;
        private int openedFileType;

        private List<string> history;

        public DocEditor()
        {
            //Нажатие Ctrl + C не завершает программу
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
            };

            try
            {
                history = new List<string>(File.ReadAllLines("history.txt"));
            }
            catch
            {
                history = new();
            }

            try
            {
                accounts = new();

                foreach (var line in File.ReadAllLines("accounts.txt"))
                {
                    var parts = line.Split(';');
                    if (parts.Length == 3)
                    {
                        accounts.Add((parts[0], parts[1], parts[2]));
                    }
                }
            }
            catch
            {
                accounts = new();
            }

            //Включение режима для отображения ANSI
            IntPtr hConsole = GetStdHandle(STD_OUTPUT_HANDLE);

            GetConsoleMode(hConsole, out int mode);
            Console.WriteLine($"Текущий режим консоли: {mode}");

            bool success = SetConsoleMode(hConsole, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            Console.WriteLine(success ? "ANSI включён!" : "Ошибка включения ANSI.");
            //Конец включения

            //Файл для работы с Google Cloud
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",
            @"D:\Learning\4_sem_(2 course)\OOP\Lab2Resources\Key_for_cloudWork.json");

            currentScene = new MainMenu(editorSettings.getColorsArray());
            currentScene.Draw();

            while (editorWork)
            {
                StepHandler();
            }

            Console.Clear();
        }
        private void StepHandler()
        {
            List<string> eventMessage = currentScene.KeyHandler(Console.ReadKey(true)).Split(' ').ToList();

            switch (eventMessage[0])
            {
                case "goto":
                    switch (eventMessage[1])
                    {
                        case "mainMenu":
                            currentScene = new MainMenu(this.editorSettings.getColorsArray());
                            break;

                        case "quit":
                            File.WriteAllLines("history.txt", history);
                            
                            using (StreamWriter writer = new StreamWriter("accounts.txt"))
                            {
                                foreach (var item in accounts)
                                {
                                    writer.WriteLine($"{item.login};{item.password};{item.role}");
                                }
                            }

                            editorWork = false;
                            break;

                        case "register":
                            currentScene = new Register(this.editorSettings.getColorsArray(), this.accounts);
                            break;

                        case "login":
                            currentScene = new Login(this.editorSettings.getColorsArray(), this.accounts);
                            break;

                        case "userMenu":
                            currentUser = this.accounts.Single(account => account.login == eventMessage[2]);
                            currentScene = new UserMenu(this.editorSettings.getColorsArray(), currentUser.role, currentUser.login);
                            break;

                        case "fileDialog":
                            currentScene = new FileDialog(this.editorSettings.getColorsArray(), eventMessage[2]);
                            break;

                        case "settings":
                            currentScene = new Settings(this.editorSettings.getColorsArray(), this.editorSettings);
                            break;

                        case "cloudFileDialog":
                            currentScene = new CloudFileDialog(this.editorSettings.getColorsArray(), eventMessage[2]);
                            this.fileManager = (true, eventMessage[2]);
                            break;

                        case "localFileDialog":
                            currentScene = new LocalFileDialog(this.editorSettings.getColorsArray(), eventMessage[2]);
                            this.fileManager = (false, eventMessage[2]);
                            break;

                        case "error":
                            currentScene = new Error(this.editorSettings.getColorsArray(), currentScene.getError(),
                                                     $"{currentScene.mainType()} {currentScene.subType()}");
                            break;

                        case "usersSettings":
                            currentScene = new UsersSettings(this.editorSettings.getColorsArray(), currentUser.login, ref accounts);
                            break;

                        case "documentMode":
                            if (currentScene is CloudFileDialog cloudScene)
                            {
                                this.docContent = cloudScene.content;
                                this.openedFile = cloudScene.currentFileId;
                                this.openedFileType = cloudScene.fileType;
                            }
                            else if (currentScene is LocalFileDialog localScene)
                            {
                                this.docContent = localScene.content;
                                this.openedFile = localScene.currentFilePath;
                                this.openedFileType = localScene.fileType;
                            }
                            currentScene = new DocumentMode(this.editorSettings.getColorsArray(),
                                                            currentUser.role != "reader");
                            break;

                        case "document":
                            switch (eventMessage[2])
                            {
                                
                                case "open":
                                    currentScene = new Document(this.editorSettings.getColorsArray(),
                                                                eventMessage[4] != "0",
                                                                docContent,
                                                                eventMessage[3] == "0" ? 0 : this.openedFileType);
                                    break;

                                case "create":
                                    currentScene = new Document(this.editorSettings.getColorsArray(),
                                                                true,
                                                                "",
                                                                -1);
                                    break;
                            }
                            break;

                        case "saveAs":
                            if (currentScene is Document docSceneA)
                            {
                                docContent = docSceneA.theContent;
                            }
                            currentScene = new FileDialog(editorSettings.getColorsArray(), "save");
                            break;

                        case "history":
                            currentScene = new History(this.editorSettings.getColorsArray(), history);
                            break;

                        default:
                            break;
                    }
                    break;

                case "escape":
                    switch (currentScene.mainType())
                    {
                        case "mainMenu":
                            editorWork = false;
                            break;

                        case "register":
                            currentScene = new MainMenu(this.editorSettings.getColorsArray());
                            break;

                        case "login":
                            currentScene = new MainMenu(this.editorSettings.getColorsArray());
                            break;

                        case "userMenu":
                            currentScene = new MainMenu(this.editorSettings.getColorsArray());
                            break;

                        case "history":
                            currentScene = new MainMenu(this.editorSettings.getColorsArray());
                            break;

                        case "fileDialog":
                            currentScene = new UserMenu(this.editorSettings.getColorsArray(), currentUser.role, currentUser.login);
                            break;

                        case "settings":
                            currentScene = new UserMenu(this.editorSettings.getColorsArray(), currentUser.role, currentUser.login);
                            break;

                        case "cloudFileDialog":
                            currentScene = new FileDialog(this.editorSettings.getColorsArray(), currentScene.subType());
                            break;

                        case "localFileDialog":
                            currentScene = new FileDialog(this.editorSettings.getColorsArray(), currentScene.subType());
                            break;

                        case "usersSettings":
                            currentScene = new UserMenu(this.editorSettings.getColorsArray(), currentUser.role, currentUser.login);
                            break;

                        case "documentMode":
                            if (fileManager.isCloud)
                            {
                                currentScene = new CloudFileDialog(this.editorSettings.getColorsArray(), fileManager.subtype);
                            }
                            else
                            {
                                currentScene = new LocalFileDialog(this.editorSettings.getColorsArray(), fileManager.subtype);
                            }
                            break;

                        case "document":
                            currentScene = new DocumentMode(this.editorSettings.getColorsArray(), currentUser.role != "reader");
                            break;

                        default:
                            break;
                    }
                    break;

                case "create":
                    if (currentScene is CloudFileDialog cloudSceneA)
                    {
                        cloudSceneA.CreateFile(docContent);
                        history.Add($"User {this.currentUser.login} created file " +
                            $"{cloudSceneA.saveFileName + cloudSceneA.saveFileType.list[cloudSceneA.saveFileType.choosed]} " +
                            $"in cloud");
                    }
                    else if (currentScene is LocalFileDialog localSceneA)
                    {
                        localSceneA.CreateFile(docContent);
                        history.Add($"User {this.currentUser.login} created file " +
                            $"{localSceneA.saveChoosedPath}");
                    }
                    currentScene = new UserMenu(this.editorSettings.getColorsArray(), currentUser.role, currentUser.login);
                    break;

                case "save":
                    if (currentScene is Document docScene)
                    {
                        docContent = docScene.theContent;
                        if (this.fileManager.isCloud)
                        {
                            string[] scopes = { DriveService.Scope.Drive };
                            string applicationName = "DocEditorApp";

                            UserCredential credential;

                            using (var stream = new FileStream(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"),
                                                               FileMode.Open, FileAccess.Read))
                            {
                                string creadPath = "token.json";
                                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                                    GoogleClientSecrets.Load(stream).Secrets,
                                    scopes,
                                    "user",
                                    CancellationToken.None,
                                    new FileDataStore(creadPath, true)).Result;
                            }

                            var service = new DriveService(new Google.Apis.Services.BaseClientService.Initializer()
                            {
                                HttpClientInitializer = credential,
                                ApplicationName = applicationName
                            });

                            var requestt = service.Files.Get(openedFile);
                            requestt.Fields = "mimeType";
                            string mimeType = requestt.Execute().MimeType;

                            var fileMetadata = new Google.Apis.Drive.v3.Data.File();

                            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(docContent)))
                            {
                                var request = service.Files.Update(fileMetadata, openedFile, stream, mimeType);
                                request.Fields = "id, name";
                                var file = request.Upload();
                            }

                            string historyFileName = service.Files.Get(openedFile).Execute().Name;

                            history.Add($"User {this.currentUser.login} changed file " +
                            $"{historyFileName} " +
                            $"in cloud");
                        }
                        else
                        {
                            File.WriteAllText(openedFile, docContent);

                            history.Add($"User {this.currentUser.login} changed file " +
                            $"{openedFile}");
                        }
                    }
                    break;

                case "was":

                    switch (eventMessage[1])
                    {
                        case "delete":
                            if (currentScene is CloudFileDialog sceneCloudFD)
                            {
                                history.Add($"User {this.currentUser.login} deleted file {sceneCloudFD.deletedFile} from cloud");
                            }
                            else if (currentScene is LocalFileDialog sceneLocalFD)
                            {
                                history.Add($"User {this.currentUser.login} deleted file {sceneLocalFD.deletedFile}");
                            }
                            break;
                    }

                    break;

                default:
                    break;
            }

            if (currentScene.mainType() == "mainMenu")
            {
                this.editorSettings.ResetEditorSettings();
                currentScene.ColorsUpdate(this.editorSettings.getColorsArray());
            }

            currentScene.Draw();
        }
        
    }

    public class EditorSettings //Класс для хранения настроек редактора
    {
        //Создаём ленивую, гарантированно потокобезопасную инициализацию
        private static readonly Lazy<EditorSettings> instance = new Lazy<EditorSettings>(() => new EditorSettings());
        public static EditorSettings Instance => instance.Value;
        private List<(string colorName, ConsoleColor color, int num)> colorsList;
        private int bgClr;
        private int fontClr;
        private int selectBgClr;
        private int selectFontClr;

        private EditorSettings()
        {
            colorsList = [("black", ConsoleColor.Black, 0),
                          ("blue", ConsoleColor.Blue, 1),
                          ("cyan", ConsoleColor.Cyan, 2),
                          ("darkblue", ConsoleColor.DarkBlue, 3),
                          ("darkcyan", ConsoleColor.DarkCyan, 4),
                          ("darkgray", ConsoleColor.DarkGray, 5),
                          ("darkgreen", ConsoleColor.DarkGreen, 6),
                          ("darkmagenta", ConsoleColor.DarkMagenta, 7),
                          ("darkred", ConsoleColor.DarkRed, 8),
                          ("darkyellow", ConsoleColor.DarkYellow, 9),
                          ("gray", ConsoleColor.Gray, 10),
                          ("green", ConsoleColor.Green, 11),
                          ("magenta", ConsoleColor.Magenta, 12),
                          ("red", ConsoleColor.Red, 13),
                          ("white", ConsoleColor.White, 14),
                          ("yellow", ConsoleColor.Yellow, 15)];

            bgClr = 0;
            fontClr = 14;
            selectBgClr = 14;
            selectFontClr = 0;
        }

        public void SetColors(int bg, int font, int sbg, int sfont)
        {
            if (bg < 0 || font < 0 || sbg < 0 || sfont < 0 ||
                bg > 15 || font > 15 || sbg > 15 || sfont > 15)
                throw new Exception("Wrong color in SetColors");

            this.bgClr = bg;
            this.fontClr = font;
            this.selectBgClr = sbg;
            this.selectFontClr = sfont;
        }

        public int[] GetColors()
        {
            return [bgClr, fontClr, selectBgClr, selectFontClr];
        }

        public List<(string colorName, ConsoleColor color, int num )> getAllColorsList()
        {
            return this.colorsList;
        }

        public void ResetEditorSettings()
        {
            bgClr = 0;
            fontClr = 14;
            selectBgClr = 14;
            selectFontClr = 0;
        }

        public (string name, ConsoleColor color, int num) bgColor() { return colorsList[bgClr]; }
        public (string name, ConsoleColor color, int num) fontColor() { return colorsList[fontClr]; }
        public (string name, ConsoleColor color, int num) selectBgColor() { return colorsList[selectBgClr]; }
        public (string name, ConsoleColor color, int num) selectFontColor() { return colorsList[selectFontClr]; }

        public ConsoleColor[] getColorsArray()
        {
            return [colorsList[bgClr].color, colorsList[fontClr].color,
                    colorsList[selectBgClr].color, colorsList[selectFontClr].color];
        }
    }

    
}