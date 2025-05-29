using Editor.DocEditor;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Http;
using Google.Apis.Storage.v1.Data;
using Google.Apis.Util.Store;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Management;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Editor.Scenes
{
    public abstract class Scene
    {
        protected string maintype;
        protected string subtype;
        protected string error;
        protected (ConsoleColor bg, ConsoleColor font, ConsoleColor bgSelect, ConsoleColor fontSelect) colors { get; set; }

        public Scene(string maintype, string subtype, ConsoleColor[] clrs)
        {
            if (clrs.Length != 4)
                throw new Exception("Scene.colors size must be 4");

            this.maintype = maintype;
            this.subtype = subtype;

            this.colors = (clrs[0], clrs[1], clrs[2], clrs[3]);
        }

        public string mainType()
        {
            return this.maintype;
        }

        public string subType()
        {
            return this.subtype;
        }

        public string getError()
        {
            return this.error;
        }

        protected void ColorReset()
        {
            Console.BackgroundColor = this.colors.bg;
            Console.ForegroundColor = this.colors.font;
        }
        protected void ColorSelect()
        {
            Console.BackgroundColor = this.colors.bgSelect;
            Console.ForegroundColor = this.colors.fontSelect;
        }

        public void ColorsUpdate(ConsoleColor[] clrs)
        {
            if (clrs.Length != 4)
                throw new Exception("Scene.colors size must be 4");

            this.colors = (clrs[0], clrs[1], clrs[2], clrs[3]);
        }

        public abstract string KeyHandler(ConsoleKeyInfo key);
        public abstract void Draw();
    }

    public class MainMenu : Scene
    {
        //Кнопки меню: choosed - выбранная кнопка из массива, names - имена всех кнопок
        private (int choosed, string[] names) button = (0, ["Register", "Login", "History", "Quit"]);
        public MainMenu(ConsoleColor[] colors) : base("mainMenu", "none", colors)
        {

        }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return "escape";

                case ConsoleKey.DownArrow:
                    this.button.choosed = (this.button.choosed + 1) % this.button.names.Length;
                    break;

                case ConsoleKey.UpArrow:
                    this.button.choosed = (this.button.names.Length + this.button.choosed - 1) % this.button.names.Length;
                    break;

                case ConsoleKey.Enter:
                    return $"goto {this.button.names[this.button.choosed].ToLower()} none";
                default:
                    return "";
            }

            return "";
        }

        public override void Draw()
        {
            ColorReset();
            Console.Clear();

            int cw = Console.WindowWidth;
            int ch = Console.WindowHeight;

            int buttonY = (ch - this.button.names.Length) / 2;
            int buttonX = (cw - this.button.names.MaxBy(s => s.Length).Length) / 2;

            for (int i = 0; i < this.button.names.Length; i++)
            {
                Console.SetCursorPosition(buttonX, buttonY + i);

                if (i == this.button.choosed)
                {
                    ColorSelect();
                    Console.Write(this.button.names[i]);
                    ColorReset();
                }
                else
                {
                    Console.Write(this.button.names[i]);
                }
            }
        }
    }
    public class Register : Scene
    {
        private (int choosed, string[] names) button = (0, ["Login: ", "Password: ", "Confirm password: "]);
        private string login = "";
        private string password = "";
        private string confirm = "";
        private string error = "";
        private List<(string login, string password, string role)> accounts;
        public Register(ConsoleColor[] colors, List<(string, string, string)> accounts) : base("register", "none", colors)
        {
            this.accounts = accounts;
        }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            error = "";

            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return "escape";

                case ConsoleKey.DownArrow:
                    this.button.choosed = (this.button.choosed + 1) % this.button.names.Length;
                    break;

                case ConsoleKey.UpArrow:
                    this.button.choosed = (this.button.names.Length + this.button.choosed - 1) % this.button.names.Length;
                    break;

                case ConsoleKey.Enter:
                    if (this.password != this.confirm)
                    {
                        error = "ERROR: PASSWORD AND PASSWORD CONFIRM DON'T MATCH";
                    }
                    else if (this.login == "")
                    {
                        error = "ERROR: THE LOGIN FIELD MUST BE FILLED IN";
                    }
                    else if (this.accounts.Any(account => account.login == this.login))
                    {
                        error = "ERROR: THIS LOGIN ALREADY EXISTS";
                    }
                    else
                    {
                        this.accounts.Add((this.login, this.password, "reader"));
                        return $"goto userMenu {this.login}";
                    }
                    break;

                case ConsoleKey.Backspace:
                    if (this.button.choosed == 0)
                    {
                        try
                        {
                            this.login = this.login.Remove(this.login.Length - 1);
                        }
                        catch { }
                    }
                    else if (this.button.choosed == 1)
                    {
                        try
                        {
                            this.password = this.password.Remove(this.password.Length - 1);
                        }
                        catch { }
                    }
                    else if (this.button.choosed == 2)
                    {
                        try
                        {
                            this.confirm = this.confirm.Remove(this.confirm.Length - 1);
                        }
                        catch { }
                    }
                    break;

                default:
                    char keychar = key.KeyChar;
                    if (!char.IsControl(keychar))
                    {
                        if (button.choosed == 0 && this.login.Length < 16)
                        {
                            this.login += keychar;
                        }
                        else if (button.choosed == 1 && this.password.Length < 16)
                        {
                            this.password += keychar;
                        }
                        else if (button.choosed == 2 && this.confirm.Length < 16)
                        {
                            this.confirm += keychar;
                        }
                    }
                    return "";
            }

            return "";
        }

        public override void Draw()
        {
            ColorReset();
            Console.Clear();

            int cw = Console.WindowWidth;
            int ch = Console.WindowHeight;

            int buttonY = (ch - this.button.names.Length) / 2;
            int buttonX = (cw - this.button.names.MaxBy(s => s.Length).Length) / 2;

            if (button.choosed == 0)
                ColorSelect();

            Console.SetCursorPosition(buttonX, buttonY);
            Console.Write($"{button.names[0]}{this.login}");

            if (button.choosed == 1)
                ColorSelect();
            else
                ColorReset();

            Console.SetCursorPosition(buttonX, buttonY + 1);
            Console.Write($"{button.names[1]}{new string('*', this.password.Length)}");

            if (button.choosed == 2)
                ColorSelect();
            else
                ColorReset();

            Console.SetCursorPosition(buttonX, buttonY + 2);
            Console.Write($"{button.names[2]}{new string('*', this.confirm.Length)}");

            ColorReset();

            Console.SetCursorPosition((cw - this.error.Length) / 2, buttonY + 4);
            Console.Write(this.error);
        }
    }
    public class Login : Scene
    {
        private int choosedField = 0;
        private string login = "";
        private string password = "";
        private string error = "";
        private List<(string login, string password, string role)> accounts;

        public Login(ConsoleColor[] colors, List<(string, string, string)> accounts) : base("login", "none", colors)
        {
            this.accounts = accounts;
        }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            error = "";

            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return "escape";

                case ConsoleKey.DownArrow:
                    choosedField = choosedField == 1 ? 0 : 1;
                    break;

                case ConsoleKey.UpArrow:
                    choosedField = choosedField == 1 ? 0 : 1;
                    break;

                case ConsoleKey.Enter:
                    if (this.login == "")
                    {
                        error = "ERROR: THE LOGIN FIELD MUST BE FILLED IN";
                    }
                    else if (!this.accounts.Any(account => account.login == this.login && account.password == this.password))
                    {
                        error = "ERROR: THIS LOGIN DON'T EXISTS OR PASSWORD IS INCORRECT";
                    }
                    else
                    {
                        string username = this.accounts.SingleOrDefault(account =>
                        account.login == this.login && account.password == this.password).login;

                        return $"goto userMenu {username}";
                    }
                    break;

                case ConsoleKey.Backspace:
                    if (this.choosedField == 0)
                    {
                        try
                        {
                            this.login = this.login.Remove(this.login.Length - 1);
                        }
                        catch { }
                    }
                    else if (this.choosedField == 1)
                    {
                        try
                        {
                            this.password = this.password.Remove(this.password.Length - 1);
                        }
                        catch { }
                    }
                    break;

                default:
                    char keychar = key.KeyChar;
                    if (!char.IsControl(keychar))
                    {
                        if (choosedField == 0 && this.login.Length < 16)
                        {
                            this.login += keychar;
                        }
                        else if (choosedField == 1 && this.password.Length < 16)
                        {
                            this.password += keychar;
                        }
                    }
                    break;
            }

            return "";
        }

        public override void Draw()
        {
            ColorReset();
            Console.Clear();

            int cw = Console.WindowWidth;
            int ch = Console.WindowHeight;

            int buttonY = (ch - 2) / 2;
            int buttonX = (cw - 10) / 2;

            if (choosedField == 0)
                ColorSelect();

            Console.SetCursorPosition(buttonX, buttonY);
            Console.Write($"Login: {this.login}");

            if (choosedField == 1)
                ColorSelect();
            else
                ColorReset();

            Console.SetCursorPosition(buttonX, buttonY + 1);
            Console.Write($"Password: {new string('*', this.password.Length)}");

            ColorReset();

            Console.SetCursorPosition((cw - this.error.Length) / 2, buttonY + 3);
            Console.Write(this.error);
        }
    }
    public class UserMenu : Scene
    {
        private (int choosed, string[] names) button;
        private string username = "";

        public UserMenu(ConsoleColor[] colors, string role, string username) : base("userMenu", role, colors)
        {
            this.username = username;

            if (role == "reader")
            {
                button = (0, ["Open", "Settings", "Unlogin"]);
            }
            else if (role == "editor")
            {
                button = (0, ["Create", "Open", "Delete", "Settings", "Unlogin"]);
            }
            else if (role == "admin")
            {
                button = (0, ["Create", "Open", "Delete", "UsersSettings", "Settings", "Unlogin"]);
            }
            else
            {
                throw new Exception("Wrong user role");
            }
        }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return "escape";

                case ConsoleKey.DownArrow:
                    button.choosed = (button.choosed + 1) % button.names.Length;
                    break;

                case ConsoleKey.UpArrow:
                    button.choosed = (button.names.Length + (button.choosed - 1)) % button.names.Length;
                    break;

                case ConsoleKey.Enter:
                    switch (button.names[button.choosed])
                    {
                        case "Create":
                            return "goto document create";

                        case "Open":
                            return "goto fileDialog open";

                        case "Delete":
                            return "goto fileDialog delete";

                        case "UsersSettings":
                            return "goto usersSettings";

                        case "Settings":
                            return "goto settings";

                        case "Unlogin":
                            return "escape";
                    }
                    break;

                default:
                    break;
            }

            return "";
        }

        public override void Draw()
        {
            ColorReset();
            Console.Clear();

            int cw = Console.WindowWidth;
            int ch = Console.WindowHeight;
            int buttonX = (cw - Math.Max(10 + username.Length, 13)) / 2;
            int buttonY = (ch - this.button.names.Length - 2) / 2;

            Console.SetCursorPosition(buttonX, buttonY);
            Console.Write($"Welcome, {username}!");

            for (int i = 0; i < button.names.Length; i++)
            {
                Console.SetCursorPosition(buttonX, buttonY + i + 2);

                if (button.choosed == i)
                    ColorSelect();

                Console.Write(button.names[i]);

                ColorReset();
            }
        }
    }
    public class Settings : Scene
    {
        private EditorSettings mainSettings;
        private (int current, string[] names, int[] nums) button;
        private List<(string colorName, ConsoleColor color, int num)> colorsList;

        public Settings(ConsoleColor[] colors, EditorSettings mainSettings) : base("settings", "none", colors)
        {
            this.mainSettings = mainSettings;

            colorsList = mainSettings.getAllColorsList();

            button = (0, ["Background color: ", "Font color: ",
                          "Selection background color: ", "Selection fong color: "],
                         [mainSettings.bgColor().num, mainSettings.fontColor().num,
                          mainSettings.selectBgColor().num, mainSettings.selectFontColor().num]);
        }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return "escape";

                case ConsoleKey.R:
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                        mainSettings.ResetEditorSettings();
                    this.colors = (mainSettings.bgColor().color,
                                   mainSettings.fontColor().color,
                                   mainSettings.selectBgColor().color,
                                   mainSettings.selectFontColor().color);
                    break;

                case ConsoleKey.DownArrow:
                    button.current = (button.current + 1) % button.names.Length;
                    break;

                case ConsoleKey.UpArrow:
                    button.current = (button.names.Length + (button.current - 1)) % button.names.Length;
                    break;

                case ConsoleKey.LeftArrow:
                    if (button.nums[button.current] > 0)
                        button.nums[button.current]--;
                    break;

                case ConsoleKey.RightArrow:
                    if (button.nums[button.current] < colorsList.Count - 1)
                        button.nums[button.current]++;
                    break;

                case ConsoleKey.Enter:
                    mainSettings.SetColors(button.nums[0], button.nums[1], button.nums[2], button.nums[3]);
                    this.colors = (mainSettings.bgColor().color,
                                   mainSettings.fontColor().color,
                                   mainSettings.selectBgColor().color,
                                   mainSettings.selectFontColor().color);
                    break;

                default:
                    break;
            }

            for (int i = 0; i < button.nums.Length; i++)
            {
                if (i != button.current)
                {
                    button.nums[i] = mainSettings.GetColors()[i];
                }
            }

            return "";
        }

        public override void Draw()
        {
            ColorReset();
            Console.Clear();

            int cw = Console.WindowWidth;
            int ch = Console.WindowHeight;

            int buttonY = (ch - button.names.Length - 2) / 2;
            int buttonX = (cw - 43) / 2;

            for (int i = 0; i < button.names.Length; i++)
            {
                Console.SetCursorPosition(buttonX, buttonY + i);

                if (button.current == i)
                {
                    ColorSelect();
                }

                Console.Write($"{button.names[i]} " +
                              $"{(button.nums[i] == 0 ? ' ' : '<')} " + 
                              $"{colorsList[button.nums[i]].colorName} " +
                              $"{(button.nums[i] == colorsList.Count - 1 ? ' ' : '>')}");

                ColorReset();
            }

            Console.SetCursorPosition(buttonX, buttonY + button.names.Length + 1);
            Console.Write("Reset colors - Ctrl + R");
        }
    }
    public class FileDialog : Scene //Окно выбора между облаком и локальной памятью
    {
        private bool button = false;

        public FileDialog(ConsoleColor[] colors, string subtype) : base("fileDialog", subtype, colors) { }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return "escape";

                case ConsoleKey.DownArrow:
                    button = !button;
                    break;

                case ConsoleKey.UpArrow:
                    button = !button;
                    break;

                case ConsoleKey.Enter:
                    if (!button)
                        return $"goto localFileDialog {subtype}";
                    else
                        return $"goto cloudFileDialog {subtype}";

                default:
                    return "";
            }

            return "";
        }

        public override void Draw()
        {
            ColorReset();
            Console.Clear();

            int cw = Console.WindowWidth;
            int ch = Console.WindowHeight;

            int buttonY = (ch - 2) / 2;
            int buttonX = (cw - 12) / 2;

            Console.SetCursorPosition(buttonX, buttonY);

            if (!button)
                ColorSelect();

            Console.Write("Local Memory");

            ColorReset();
            Console.SetCursorPosition(buttonX, buttonY + 1);

            if (button)
                ColorSelect();

            Console.Write("Cloud Memory");

            ColorReset();
        }
    }
    public class CloudFileDialog : Scene
    {
        private DriveService service; //Переменная для работы с файлами облака

        private string currentDirId = "";
        private List<string> previousDirIds = new() { "root" };
        private List<(string id, string name, string mimeType)> currentFiles;
        private List<(string id, string name, string mimeType)> currentDirs;
        private List<string> currentNames;
        private int current = 0;
        private int currentTen = 0;
        public string content = "";
        private bool deleteAccept = false;
        public string currentFileId;

        public int fileType;

        public bool chooseCurrentDir = false;
        public string choosedDirId = "";
        public string saveFileName = "";
        public (int choosed, List<string> list) saveFileType = (0, new List<string> { ".txt", ".xml", ".json", ".md", ".rtf" });

        public string deletedFile;

        [Obsolete]
        public CloudFileDialog(ConsoleColor[] colors, string subtype) : base("cloudFileDialog", subtype, colors)
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

            this.service = new DriveService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName
            });

            var request = service.Files.List();
            request.Q = "'root' in parents";
            request.Fields = "files(id, name, mimeType)";
            string firstid = request.Execute().Files.ToList().Single(file => file.Name == "DocEditorStorage").Id;

            previousDirIds = new() { firstid };
            ChangeDir("..");
        }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return "escape";

                case ConsoleKey.Backspace:
                    if (saveFileName.Length != 0)
                        saveFileName = saveFileName.Remove(saveFileName.Length - 1);
                    break;

                case ConsoleKey.DownArrow:
                    if (current < currentDirs.Count + currentFiles.Count - 1)
                    {
                        current++;
                        currentTen = current / 10;
                        deleteAccept = false;
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (current > 0)
                    {
                        current--;
                        currentTen = current / 10;
                        deleteAccept = false;
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (this.subtype == "save")
                    {
                        if (key.Modifiers == ConsoleModifiers.Shift && saveFileType.choosed != 0)
                            saveFileType.choosed--;
                        else
                            this.chooseCurrentDir = false;
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (this.subtype == "save")
                    {
                        if (key.Modifiers == ConsoleModifiers.Shift && saveFileType.choosed != saveFileType.list.Count - 1)
                            saveFileType.choosed++;
                        else
                            this.chooseCurrentDir = true;
                    }
                    break;

                case ConsoleKey.Enter:
                    if (chooseCurrentDir)
                    {
                        if (saveFileName == "" ||
                            currentNames.Contains(saveFileName + saveFileType.list[saveFileType.choosed]))
                            return "";

                        choosedDirId = currentDirId;

                        return "create";
                    }
                    else if (current < currentDirs.Count)
                    {
                        ChangeDir(currentDirs[current].id);
                    }
                    else
                    {
                        if (this.subtype == "open")
                        {
                            currentFileId = currentFiles[current - currentDirs.Count].id;

                            var request = service.Files.Get(currentFileId);
                            request.Alt = DriveBaseServiceRequest<Google.Apis.Drive.v3.Data.File>.AltEnum.Media;

                            string mimeType = currentFiles[current - currentDirs.Count].mimeType;

                            if (mimeType == "text/markdown")
                            {
                                fileType = 2;
                            }
                            else if (mimeType == "application/msword")
                            {
                                fileType = 1;
                            }
                            else
                            {
                                fileType = 0;
                            }

                            using (var stream = new MemoryStream())
                            {
                                request.Download(stream);
                                content = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                            }

                            return "goto documentMode none";
                        }
                        else if (this.subtype == "delete")
                        {
                            if (deleteAccept && currentFiles.Count > 0)
                            {
                                deletedFile = currentFiles[current - currentDirs.Count].name;
                                service.Files.Delete(currentFiles[current - currentDirs.Count].id).Execute();
                                ChangeDir(currentDirId);
                                return "was delete";
                            }
                            else
                            {
                                deleteAccept = true;
                            }
                        }
                    }
                    break;
            }

            if ((char.IsControl(key.KeyChar) ? '\0' : key.KeyChar) != '\0')
            {
                if (saveFileName.Length < 30)
                    saveFileName += key.KeyChar;
            }

            return "";
        }

        public override void Draw()
        {
            ColorReset();
            Console.Clear();

            if (currentTen > 0)
            {
                Console.Write("...\n");
            }

            for (int i = currentTen * 10; i < Math.Min(currentTen * 10 + 10, currentDirs.Count); i++)
            {
                if (i == current)
                {
                    ColorSelect();
                    Console.Write($"{currentDirs[i].name}\n");
                    ColorReset();
                }
                else
                {
                    Console.Write($"{currentDirs[i].name}\n");
                }
            }

            for (int i = Math.Max(currentTen * 10, currentDirs.Count);
                 i < Math.Min(currentTen * 10 + 10, currentDirs.Count + currentFiles.Count); i++)
            {
                if (i == current)
                {
                    ColorSelect();
                    Console.Write($"{currentFiles[i - currentDirs.Count].name}\n");
                    ColorReset();
                }
                else
                {
                    Console.Write($"{currentFiles[i - currentDirs.Count].name}\n");
                }
            }

            if (currentTen * 10 + 10 < currentDirs.Count + currentFiles.Count)
            {
                Console.Write("...\n");
            }

            if (this.subtype == "delete")
            {
                Console.Write("\nDouble Enter do delete");
            }

            if (this.subtype == "save")
            {
                int ch = Console.WindowHeight;

                //Name + fileExtension + folderAction
                //Instructions

                Console.SetCursorPosition(0, ch - 2);
                Console.Write($"File name: {saveFileName + new string(' ', 30 - saveFileName.Length)} ");
                Console.Write($"File extension: {(saveFileType.choosed == 0 ? ' ' : '<')} " +
                              $"{saveFileType.list[saveFileType.choosed]} " +
                              $"{(saveFileType.choosed == saveFileType.list.Count - 1 ? ' ' : '>')} ");

                if (!chooseCurrentDir)
                    ColorSelect();
                Console.Write("Open");
                ColorReset();
                Console.Write(" ");
                if (chooseCurrentDir)
                    ColorSelect();
                Console.Write("Choose");
                ColorReset();
                Console.Write(" folder");

                Console.SetCursorPosition(0, ch - 1);
                Console.Write("L/R arrows - change folder operation, L/R arrows + shift - change file extension");
            }
        }
        
        private void ChangeDir(string nextDirId)
        {
            current = 0;

            if (nextDirId == "..")
            {
                try
                {
                    nextDirId = previousDirIds.Last();
                }
                catch
                {
                    return;
                }
            }

            var request = service.Files.List();
            request.Q = $"'{nextDirId}' in parents and (mimeType='text/plain' or " +
                                                      $"mimeType='application/msword' or " +
                                                      $"mimeType='text/xml' or " +
                                                      $"mimeType='application/json' or " +
                                                      $"mimeType='text/markdown')";
            request.Fields = "files(id, name, mimeType)";
            var FilesList = request.Execute().Files.ToList(); //Определили файлы

            currentFiles = new();
            foreach(var file in FilesList)
            {
                currentFiles.Add((file.Id, file.Name, file.MimeType));
            }

            currentNames = new();
            currentNames.AddRange(currentFiles.Select(file => file.name));

            if (this.subtype == "save")
            {
                currentFiles = new();
            }

            request = service.Files.List();
            request.Q = $"'{nextDirId}' in parents and mimeType='application/vnd.google-apps.folder'";
            request.Fields = "files(id, name, mimeType)";
            var DirsList = request.Execute().Files.ToList(); //Определили папки

            currentDirs = new();
            if (nextDirId != (previousDirIds.Count > 0 ? previousDirIds.Last() : "") && 
                nextDirId != currentDirId)
                currentDirs.Add(("..", "..", "application/vnd.google-apps.folder"));
            foreach(var dir in DirsList)
            {
                currentDirs.Add((dir.Id, dir.Name, dir.MimeType));
            }
            currentNames.AddRange(currentDirs.Select(dir => dir.name));

            if (currentDirId == nextDirId)
                return;

            if (nextDirId != (previousDirIds.Count > 0 ? previousDirIds.Last() : ""))
                previousDirIds.Add(currentDirId);
            else
                previousDirIds.RemoveAt(previousDirIds.Count - 1);

            currentDirId = nextDirId;
        }

        public void CreateFile(string fileContent)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = saveFileName + saveFileType.list[saveFileType.choosed],
                Parents = new[] { choosedDirId }
            };

            string mimeType = new List<string> { "text/plain",
                                                 "text/xml",
                                                 "application/json",
                                                 "text/markdown",
                                                 "application/msword" }[saveFileType.choosed];

            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent)))
            {
                var request = service.Files.Create(fileMetadata, stream, mimeType);
                request.Fields = "id";
                var file = request.Upload();
            }
        }
    }
    public class LocalFileDialog : Scene
    {
        private List<string> currentDirs = new();
        private List<string> currentFiles = new();
        private int current = 0;
        private List<string> currentPath = new();
        public List<string> drives = new();
        private int currentTen = 0;
        public string content = "";
        private bool deleteAccept = false;
        public string currentFilePath;

        public int fileType;

        public string saveChoosedPath = "";
        private List<string> saveCheckNames;
        public string saveFileName = "";
        private bool saveChooseCurrentDir = false;
        public (int choosed, List<string> list) saveFileType = (0, new List<string> { ".txt", ".xml", ".json", ".md", ".rtf" });

        public string deletedFile;

        public LocalFileDialog(ConsoleColor[] colors, string subtype) : base("localFileDialog", subtype, colors)
        {
            //Получение дисков в виде C:
            this.drives = DriveInfo.GetDrives().Select(d => d.Name.Remove(d.Name.Length - 1)).ToList();
            
            ChangeDir(drives[0]);
            ChangeDir("..");
        }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            switch(key.Key)
            {
                case ConsoleKey.Escape:
                    return "escape";

                case ConsoleKey.DownArrow:
                    if (current < currentDirs.Count + currentFiles.Count - 1)
                    {
                        current++;
                        currentTen = current / 10;
                        deleteAccept = false;
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (current > 0)
                    {
                        current--;
                        currentTen = current / 10;
                        deleteAccept = false;
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (this.subtype == "save")
                    {
                        if (key.Modifiers == ConsoleModifiers.Shift && saveFileType.choosed != 0)
                            saveFileType.choosed--;
                        else
                            this.saveChooseCurrentDir = false;
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (this.subtype == "save")
                    {
                        if (key.Modifiers == ConsoleModifiers.Shift && saveFileType.choosed != saveFileType.list.Count - 1)
                            saveFileType.choosed++;
                        else
                            this.saveChooseCurrentDir = true;
                    }
                    break;

                case ConsoleKey.Backspace:
                    if (this.subtype == "save" && saveFileName != "")
                    {
                        saveFileName = saveFileName.Remove(saveFileName.Length - 1);
                    }
                    break;

                case ConsoleKey.Enter:
                    if (saveChooseCurrentDir)
                    {

                        if (saveCheckNames.Contains(saveFileName + saveFileType.list[saveFileType.choosed]) ||
                            currentDirs == drives ||
                            saveFileName == "")
                        {
                            return "";
                        }

                        saveChoosedPath = string.Join("\\", currentPath) + "\\" +
                                                       saveFileName + saveFileType.list[saveFileType.choosed];

                        return "create";
                    }
                    else if (current < currentDirs.Count)
                    {
                        ChangeDir(currentDirs[current]);
                    }
                    else
                    {
                        if (this.subtype == "open")
                        {
                            content = File.ReadAllText(string.Join("\\", currentPath) + "\\" +
                                                       currentFiles[current - currentDirs.Count]);

                            currentFilePath = string.Join("\\", currentPath) + "\\" +
                                                       currentFiles[current - currentDirs.Count];

                            string ras = Path.GetExtension(currentFilePath);

                            if (ras == ".md")
                            {
                                fileType = 2;
                            }
                            else if (ras == ".rtf")
                            {
                                fileType = 1;
                            }
                            else
                            {
                                fileType = 0;
                            }

                            return "goto documentMode none";
                        }
                        else if (this.subtype == "delete")
                        {
                            if (deleteAccept && currentFiles.Count > 0)
                            {
                                deletedFile = string.Join("\\", currentPath) + "\\" +
                                            currentFiles[current - currentDirs.Count];
                                File.Delete(string.Join("\\", currentPath) + "\\" +
                                            currentFiles[current - currentDirs.Count]);
                                ChangeDir(currentPath.Last());
                                return "was delete";
                            }
                            else
                            {
                                deleteAccept = true;
                            }
                        }
                    }
                    break;
            }

            if ((char.IsControl(key.KeyChar) ? '\0' : key.KeyChar) != '\0')
            {
                if (saveFileName.Length < 30)
                    saveFileName += key.KeyChar;
            }

            return "";
        }

        public override void Draw()
        {
            ColorReset();
            Console.Clear();

            if (currentTen > 0)
                Console.Write("...\n");

            for (int i = currentTen * 10; i < Math.Min(currentTen * 10 + 10, currentDirs.Count); i++)
            {
                if (i == current)
                {
                    ColorSelect();
                    Console.Write($"{currentDirs[i]}\n");
                    ColorReset();
                }
                else
                {
                    Console.Write($"{currentDirs[i]}\n");
                }
            }

            for (int i = Math.Max(currentTen * 10, currentDirs.Count);
                 i < Math.Min(currentTen * 10 + 10, currentDirs.Count + currentFiles.Count); i++)
            {
                if (i == current)
                {
                    ColorSelect();
                    Console.Write($"{currentFiles[i - currentDirs.Count]}\n");
                    ColorReset();
                }
                else
                {
                    Console.Write($"{currentFiles[i - currentDirs.Count]}\n");
                }
            }

            if (currentTen * 10 + 10 < currentDirs.Count + currentFiles.Count)
            {
                Console.Write("...\n");
            }

            if (this.subtype == "delete")
            {
                Console.Write("\nDouble Enter do delete");
            }

            if (this.subtype == "save")
            {
                int ch = Console.WindowHeight;

                //Name + fileExtension + folderAction
                //Instructions

                Console.SetCursorPosition(0, ch - 2);
                Console.Write($"File name: {saveFileName + new string(' ', 30 - saveFileName.Length)} ");
                Console.Write($"File extension: {(saveFileType.choosed == 0 ? ' ' : '<')} " +
                              $"{saveFileType.list[saveFileType.choosed]} " +
                              $"{(saveFileType.choosed == saveFileType.list.Count - 1 ? ' ' : '>')} ");

                if (!saveChooseCurrentDir)
                    ColorSelect();
                Console.Write("Open");
                ColorReset();
                Console.Write(" ");
                if (saveChooseCurrentDir)
                    ColorSelect();
                Console.Write("Choose");
                ColorReset();
                Console.Write(" folder");

                Console.SetCursorPosition(0, ch - 1);
                Console.Write("L/R arrows - change folder operation, L/R arrows + shift - change file extension");
            }
        }

        private void ChangeDir(string dirName)
        {
            current = 0;

            if (dirName == (currentPath.Count > 0 ? currentPath.Last() : "NotExistDir:"))
            {

            }
            else if (dirName != "..")
            {
                currentPath.Add(dirName);
            }
            else
            {
                currentPath.RemoveAt(currentPath.Count - 1);
            }

            if (currentPath.Count > 0)
            {
                string[] extensions = { ".txt", ".md", ".rtf", ".json", ".xml" };
                
                string path = string.Join(@"\", currentPath);
                
                currentDirs = Directory.GetDirectories(path + "\\")
                .Where(dir => !(File.GetAttributes(dir).HasFlag(FileAttributes.Hidden) ||
                                File.GetAttributes(dir).HasFlag(FileAttributes.System)))
                .Select(dir => Path.GetFileName(dir))
                .ToList();

                currentDirs.Insert(0, "..");

                currentFiles = Directory.GetFiles(path + "\\")
                .Where(file => !File.GetAttributes(file).HasFlag(FileAttributes.Hidden) &&
                               !File.GetAttributes(file).HasFlag(FileAttributes.System) &&
                               extensions.Contains(Path.GetExtension(file).ToLower()))
                .Select(file => Path.GetFileName(file))
                .ToList();

                if (this.subtype == "save")
                {
                    saveCheckNames = new();
                    saveCheckNames.AddRange(currentDirs);
                    saveCheckNames.AddRange(currentFiles);
                    currentFiles = new();
                }
            }
            else
            {
                currentDirs = drives;
                currentFiles = new();
            }

        }

        public void CreateFile(string docContent)
        {
            File.WriteAllText(saveChoosedPath, docContent);
        }
    }
    public class UsersSettings : Scene
    {
        private string currentUser = "";
        private List<(string login, string password, string role)> accounts;
        private int current = 0;
        private int currentTen = 0;
        private List<string> roles = ["reader", "editor", "admin", "delete"];
        private List<(string login, string password, string role)> acounts;
        public UsersSettings(ConsoleColor[] colors, string currentUser, ref List<(string, string, string)> accounts) :
            base("usersSettings", "none", colors)
        {
            this.currentUser = currentUser;
            this.accounts = accounts;

            acounts = new List<(string login, string password, string role)>(accounts);
            acounts.Remove(acounts.Find(us => us.login == currentUser));
        }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    acounts.Add(accounts.Find(ac => ac.login == currentUser));
                    accounts.Clear();
                    accounts.AddRange(acounts);
                    return "escape";

                case ConsoleKey.DownArrow:
                    if (current < acounts.Count - 1)
                    {
                        current++;
                        currentTen = current / 10;
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (current > 0)
                    {
                        current--;
                        currentTen = current / 10;
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (acounts[current].role != roles[0])
                    {
                        acounts[current] = (acounts[current].login,
                                             acounts[current].password,
                                             roles[roles.IndexOf(acounts[current].role) - 1]);
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (acounts[current].role != roles[roles.Count - 1])
                        acounts[current] = (acounts[current].login,
                                             acounts[current].password,
                                             roles[roles.IndexOf(acounts[current].role) + 1]);
                    break;

                case ConsoleKey.Enter:
                    if (acounts[current].role == "delete")
                    {
                        acounts.RemoveAt(current);
                    }
                    break;
            }

            return "";
        }

        public override void Draw()
        {
            ColorReset();
            Console.Clear();

            int cw = Console.WindowWidth;
            int ch = Console.WindowHeight;

            if (acounts.Count == 0)
                return;

            int buttonX = (cw - 11 - acounts.GetRange(currentTen * 10, Math.Min(currentTen * 10 + 10,
                                                      acounts.Count - currentTen * 10))
                                            .Max(ac => ac.login.Length)) / 2;
            int buttonY = (ch - 10) / 2;

            if (currentTen > 0)
            {
                Console.SetCursorPosition(buttonX, buttonY - 1);
                Console.Write("...");
            }

            Console.SetCursorPosition(buttonX, buttonY);

            for (int i = currentTen * 10; i < Math.Min(currentTen * 10 + 10, acounts.Count); i++)
            {
                Console.CursorLeft = buttonX;

                if (current == i)
                {
                    ColorSelect();
                    Console.Write($"{acounts[i].login} " +
                                  $"{(acounts[i].role != roles[0] ? '<' : ' ')} " +
                                  $"{acounts[i].role} " +
                                  $"{(acounts[i].role != roles[roles.Count - 1] ? '>' : ' ')}\n");
                    ColorReset();
                }
                else
                {
                    Console.Write($"{acounts[i].login} " +
                                  $"{(acounts[i].role != roles[0] ? '<' : ' ')} " +
                                  $"{acounts[i].role} " +
                                  $"{(acounts[i].role != roles[roles.Count - 1] ? '>' : ' ')}\n");
                }
            }

            if (currentTen < acounts.Count / 10)
            {
                Console.CursorLeft = buttonX;
                Console.Write("...");
            }
        }
    }
    public class Error : Scene
    {
        private string texterror = "";
        private string prevScene = "";
        public Error(ConsoleColor[] colors, string error, string prevScene) : base("error", "none", colors)
        {
            this.texterror = error;
            this.prevScene = prevScene;
        }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return $"goto {prevScene}";

                case ConsoleKey.Enter:
                    return $"goto {prevScene}";
            }

            return "";
        }

        public override void Draw()
        {
            ColorReset();

            int cw = Console.WindowWidth;
            int ch = Console.WindowHeight;

            int textWidth = cw / 2 - (((cw % 2 == 0 && cw % 4 != 0) || (cw % 2 != 0 && cw % 4 == 0))? 1 : 0);
            int textHeight = texterror.Length / textWidth + (texterror.Length % textWidth == 0 ? 0 : 1);

            int x = (cw - textWidth) / 2 - 2;
            int y = (ch - textHeight) / 2 - 2;

            for (int j = 0; j < textHeight + 4; j++)
            {
                Console.SetCursorPosition(x, y + j);
                for (int i = 0; i < textWidth + 4; i++)
                {
                    if (i == 0 || j == 0 || i == textWidth + 3 || j == textHeight + 3)
                        Console.Write('#');
                    else
                        Console.Write(' ');
                }
            }

            Console.SetCursorPosition(x + 2, y + 2);

            int ii = 0, jj = 0, len = texterror.Length;

            while (jj * textWidth + ii < len && ii < textWidth && jj < textHeight)
            {
                Console.Write(texterror[jj * textWidth + ii]);

                ii++;

                if (ii == textWidth)
                {
                    jj++;
                    Console.SetCursorPosition(x + 2, y + 2 + jj);
                    ii = 0;
                }
            }
        }
    }
    public class DocumentMode : Scene
    {
        private int current;
        private List<string> mods;

        public DocumentMode(ConsoleColor[] colors, bool editor) : base("documentMode", "none", colors)
        {
            mods = ["Raw read", "Format read"];

            if (editor)
            {
                mods.Add("Raw edit");
            }
        }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return "escape";

                case ConsoleKey.DownArrow:
                    if (current < mods.Count - 1)
                        current++;
                    break;

                case ConsoleKey.UpArrow:
                    if (current > 0)
                        current--;
                    break;

                case ConsoleKey.Enter:
                    return "goto document open " +
                           $"{(mods[current].Split(' ')[0] == "Raw" ? 0 : 1)} " +
                           $"{(mods[current].Split(' ')[1] == "read" ? 0 : 1)}";
            }

            return "";
        }

        public override void Draw()
        {
            ColorReset();
            Console.Clear();

            int cw = Console.WindowWidth;
            int ch = Console.WindowHeight;

            int buttonX = (cw - mods.Max(mod => mod.Length)) / 2;
            int buttonY = (ch - mods.Count) / 2;

            for (int i = 0; i < mods.Count; i++)
            {
                Console.SetCursorPosition(buttonX, buttonY + i);

                if (i == current)
                {
                    ColorSelect();
                    Console.Write(mods[i]);
                    ColorReset();
                }
                else
                {
                    Console.Write(mods[i]);
                }
            }
        }
    }
    public class Document : Scene
    {
        private bool editable;
        private string rawcontent;
        private string content = "";
        private List<string> changes;
        private int type; //0 - (txt, xml, json)(raw), 1 - rtf, 2 - md

        private (int x, int y) cursor = (0, 0);
        private (bool Is, int x, int y) HighLight = (false, 0, 0);

        private int cw;
        private int ch;

        private int beginLine = 0;

        private List<(string text, bool enter)> lines = new();
        private List<List<(string text, bool enter)>> changeHistory = new();
        private int changeType = -1;
        private int changeCursor = 0;

        private string buffer = "";

        private bool creating = false;

        public string theContent = "";

        public Document(ConsoleColor[] colors) : base("document", "none", colors)
        {
            creating = true;

            this.editable = true;
            this.rawcontent = "";
            this.changes = new();
            this.type = 0;

            this.cw = Console.WindowWidth;
            this.ch = Console.WindowHeight;

            RawContentHandle();

            NewChange(false);
        }

        public Document(ConsoleColor[] colors, bool editable, string content, int type) : base("document", "none", colors)
        {
            this.editable = editable;
            this.rawcontent = content;
            this.changes = new();

            if (type == -1)
            {
                creating = true;
                this.type = 0;
            }
            else
                this.type = type;

            this.cw = Console.WindowWidth;
            this.ch = Console.WindowHeight;

            RawContentHandle();

            NewChange(false);
        }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            if (!editable && type != 0)
            {
                if (key.Key == ConsoleKey.Escape)
                    return "escape";
                return "";
            }

            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return "escape";

                case ConsoleKey.LeftArrow:
                    if ((key.Modifiers & ConsoleModifiers.Shift) != 0)
                    {
                        if (!HighLight.Is)
                            HighLight = (true, cursor.x, cursor.y);
                    }
                    else
                        HighLight.Is = false;
                    CursorLeft();
                    break;

                case ConsoleKey.RightArrow:
                    if ((key.Modifiers & ConsoleModifiers.Shift) != 0)
                    {
                        if (!HighLight.Is)
                            HighLight = (true, cursor.x, cursor.y);
                    }
                    else
                        HighLight.Is = false;
                    CursorRight();
                    break;

                case ConsoleKey.DownArrow:
                    if ((key.Modifiers & ConsoleModifiers.Shift) != 0)
                    {
                        if (!HighLight.Is)
                            HighLight = (true, cursor.x, cursor.y);
                    }
                    else
                        HighLight.Is = false;
                    CursorDown();
                    break;

                case ConsoleKey.UpArrow:
                    if ((key.Modifiers & ConsoleModifiers.Shift) != 0)
                    {
                        if (!HighLight.Is)
                            HighLight = (true, cursor.x, cursor.y);
                    }
                    else
                        HighLight.Is = false;
                    CursorUp();
                    break;

                case ConsoleKey.Delete:
                    if (!editable)
                        return "";
                    Delete(false);
                    break;

                case ConsoleKey.Backspace:
                    if (!editable)
                        return "";
                    BackSpace();
                    break;

                case ConsoleKey.Enter:
                    if (!editable)
                        return "";
                    Write('\n');
                    break;

                case ConsoleKey.Z:
                    if (key.Modifiers == ConsoleModifiers.Control)
                    {
                        if (!editable)
                            return "";
                        Undo();
                        return "";
                    }
                    else if (key.Modifiers.HasFlag(ConsoleModifiers.Control) &&
                             key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        if (!editable)
                            return "";
                        Redo();
                        return "";
                    }
                    break;

                case ConsoleKey.A:
                    if (key.Modifiers == ConsoleModifiers.Control)
                    {
                        SelectAll();
                        return "";
                    }
                    break;

                case ConsoleKey.C:
                    if (key.Modifiers == ConsoleModifiers.Alt)
                    {
                        Copy();
                        return "";
                    }
                    break;

                case ConsoleKey.V:
                    if (key.Modifiers == ConsoleModifiers.Alt)
                    {
                        if (!editable)
                            return "";
                        Paste();
                        return "";
                    }
                    break;

                case ConsoleKey.X:
                    if (key.Modifiers == ConsoleModifiers.Alt)
                    {
                        if (!editable)
                            return "";
                        Cut();
                        return "";
                    }
                    break;

                case ConsoleKey.S:
                    if (key.Modifiers == ConsoleModifiers.Alt)
                    {
                        if (!editable)
                            return "";
                        SetTheContent();
                        if (creating)
                        {
                            return "goto saveAs";
                        }
                        else
                        {
                            return "save none";
                        }
                    }
                    else if (key.Modifiers.HasFlag(ConsoleModifiers.Alt) &&
                             key.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        if (!editable)
                            return "";

                        SetTheContent();
                        return "goto saveAs";
                    }
                    break;
            }

            if (!editable)
                return "";

            if ((char.IsControl(key.KeyChar) ? '\0' : key.KeyChar) != '\0')
            {
                Write(key.KeyChar);
            }

            return "";
        }

        public override void Draw()
        {
            Console.Clear();
            ColorReset();

            if (type != 0)
            {
                Console.Write(content);
                return;
            }

            if (cursor.y < beginLine)
                beginLine = cursor.y;

            cw = Console.WindowWidth;
            ch = Console.WindowHeight;

            int endLine = Math.Min(beginLine + ch, lines.Count);

            for (int i = beginLine; i < endLine - 1; i++)
            {
                Console.Write($"{lines[i].text}{((lines[i].enter)? "\n" : "")}");
            }
            Console.Write($"{lines[endLine - 1].text}");

            ColorSelect();

            if (HighLight.Is)
            {
                (int x, int y) firstCursor = (cursor.y, cursor.x).CompareTo((HighLight.y, HighLight.x))
                    < 0 ? cursor : (HighLight.x, HighLight.y);

                (int x, int y) secondCursor = (cursor.y, cursor.x).CompareTo((HighLight.y, HighLight.x))
                    > 0 ? cursor : (HighLight.x, HighLight.y);

                if (firstCursor.y < beginLine)
                {
                    firstCursor = (0, beginLine);
                }

                if (secondCursor.y > beginLine + ch - 1)
                {
                    secondCursor = (lines[beginLine + ch - 1].text.Length - 1, beginLine + ch - 1);
                }

                if (firstCursor.y == secondCursor.y)
                {
                    Console.SetCursorPosition(firstCursor.x, firstCursor.y - beginLine);
                    Console.Write(lines[firstCursor.y].text.Substring(firstCursor.x, secondCursor.x - firstCursor.x + 1));
                }
                else
                {
                    Console.SetCursorPosition(firstCursor.x, firstCursor.y - beginLine);
                    Console.Write(lines[firstCursor.y].text.Substring(firstCursor.x,
                                                                      lines[firstCursor.y].text.Length - firstCursor.x));

                    Console.SetCursorPosition(0, firstCursor.y - beginLine + 1);
                    for (int i = firstCursor.y + 1; i < secondCursor.y; i++)
                    {
                        Console.Write($"{lines[i].text}{((lines[i].enter) ? "\n" : "")}");
                    }

                    Console.SetCursorPosition(0, secondCursor.y - beginLine);
                    Console.Write(lines[secondCursor.y].text.Substring(0, secondCursor.x + 1));
                }
            }
            else
            {
                Console.SetCursorPosition(cursor.x, cursor.y - beginLine);
                Console.Write(lines[cursor.y].text[cursor.x]);
            }

            ColorReset();
        }

        private void HandleLines()
        {
            lines = new();

            var enters = content.Split('\n');

            foreach (var enter in enters)
            {
                if (enter == "")
                {
                    lines.Add(("", false));
                }
                else
                {
                    lines.AddRange(Enumerable.Range(0, (enter.Length + cw - 1) / cw)
                                   .Select(i => (enter.Substring(i * cw, Math.Min(cw, enter.Length - (i * cw))), false)));
                }

                lines[lines.Count - 1] = (lines.Last().text, true);
            }

            lines[lines.Count - 1] = (lines.Last().text + " ", false);

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].enter)
                {
                    lines[i] = (lines[i].text + " ", true);
                }
            }
        }

        private void CursorLeft()
        {
            changeType = -1;

            cursor.x--;

            if (cursor.x == -1)
            {
                cursor.y--;

                if (cursor.y == -1)
                {
                    cursor = (0, 0);
                }
                else
                {
                    cursor.x = lines[cursor.y].text.Length - 1;
                }
            }

            if (cursor.y < beginLine)
            {
                beginLine--;
            }
        }

        private void CursorRight()
        {
            changeType = -1;

            cursor.x++;

            if (cursor.x == lines[cursor.y].text.Length)
            {
                cursor.y++;

                if (cursor.y > lines.Count - 1)
                {
                    cursor = (lines[lines.Count - 1].text.Length - 1, lines.Count - 1);
                }
                else
                {
                    cursor.x = 0;
                }
            }

            if (cursor.y > beginLine + ch - 1)
            {
                beginLine++;
            }
        }

        private void CursorDown()
        {
            changeType = -1;

            cursor.y++;

            if (cursor.y > lines.Count - 1)
            {
                cursor.y--;
            }
            else
            {
                cursor.x = Math.Min(cursor.x, lines[cursor.y].text.Length - 1);
            }

            if (cursor.y > beginLine + ch - 1)
            {
                beginLine++;
            }
        }

        private void CursorUp()
        {
            changeType = -1;

            cursor.y--;

            if (cursor.y < 0)
            {
                cursor.y++;
            }
            else
            {
                cursor.x = Math.Min(cursor.x, lines[cursor.y].text.Length - 1);
            }

            if (cursor.y < beginLine)
            {
                beginLine--;
            }
        }

        private void SelectAll()
        {
            cursor = (0, 0);
            HighLight = (true, lines.Last().text.Length - 1, lines.Count - 1);
        }

        private void RawContentHandle()
        {
            content = rawcontent;

            if (this.type == 1)
            {
                List<(string name, int type)> tags = [(@"\", 0),
                                              (@"{\b", 1), (@"{\i", 2), (@"{\ul", 3),
                                              (@"{\b\i", 4), (@"{\b\ul", 5), (@"{\i\ul", 6),
                                              (@"{\b\i\ul", 7), (@"}", 8)];

                List<string> repTags = ["", "\u001b[1m", "\u001b[3m", "\u001b[4m",
                                "\u001b[1m\u001b[3m", "\u001b[1m\u001b[4m", "\u001b[3m\u001b[4m",
                                "\u001b[1m\u001b[3m\u001b[4m", "\u001b[0m"];

                for (int i = tags.Count - 1; i >= 0; i--)
                {
                    var tag = tags[i];
                    string pattern = $"(?<!{Regex.Escape("\\")}){Regex.Escape(tag.name)}";

                    content = Regex.Replace(content, pattern, repTags[tag.type]);
                }
            }

            else if (this.type == 2)
            {
                List<string> replaces = ["\u001b[1m\u001b[3m", "\u001b[1m", "\u001b[3m"];

                for (int j = 0; j < 3; j++)
                {
                    string pattern = Regex.Escape(new string('*', 3 - j));
                    List<int> indexes = Regex.Matches(content, pattern).Select(match => match.Index).ToList();
                    if (indexes.Count < 2)
                        continue;
                    Console.WriteLine(string.Join(' ', indexes));
                    Console.WriteLine();
                    Console.WriteLine(content);
                    Console.WriteLine();
                    for (int i = (indexes.Count % 2 == 0 ? indexes.Count - 2 : indexes.Count - 3); i >= 0; i -= 2)
                    {
                        content = content.Remove(indexes[i + 1], 3 - j)
                                         .Insert(indexes[i + 1], "\u001b[0m")
                                         .Remove(indexes[i], 3 - j)
                                         .Insert(indexes[i], replaces[j]);
                    }
                }
            }

            content = content.Replace("\r", "").Replace("\t", "    ");

            if (type == 0)
                HandleLines();
        }

        //changeType 1
        private void BackSpace()
        {
            int remindChangeType = changeType;

            if (cursor == (0, 0))
                return;

            if (!HighLight.Is)
            {
                CursorLeft();
            }
            Delete(true);

            if (remindChangeType == 1)
                NewChange(true);
            else
                NewChange(false);

            changeType = 1;
        }

        //changeType 2
        private void Delete(bool isBackspace)
        {
            if ((lines.Count == 1 && lines[0].text == " ") ||
                ((cursor == (lines[lines.Count - 1].text.Length - 1, lines.Count - 1)) &&
                  !HighLight.Is))
                return;

            string newContent = "";

            if (HighLight.Is)
            {
                (int x, int y) firstCursor = (cursor.y, cursor.x).CompareTo((HighLight.y, HighLight.x))
                    < 0 ? cursor : (HighLight.x, HighLight.y);

                (int x, int y) secondCursor = (cursor.y, cursor.x).CompareTo((HighLight.y, HighLight.x))
                    > 0 ? cursor : (HighLight.x, HighLight.y);

                if (secondCursor == (lines.Last().text.Length - 1, lines.Count - 1))
                {
                    secondCursor.x--;
                    if (secondCursor.x == -1)
                    {
                        secondCursor.y--;
                        if (secondCursor.y == -1)
                            secondCursor = (0, 0);
                        else
                            secondCursor.x = lines[secondCursor.y].text.Length - 1;
                    }

                    if ((firstCursor.y, firstCursor.x).CompareTo((secondCursor.y, secondCursor.x)) > 0)
                        firstCursor = secondCursor;
                }

                if (firstCursor.y == secondCursor.y)
                {
                    lines[firstCursor.y] = (lines[firstCursor.y].text.Remove(firstCursor.x,
                                                                             secondCursor.x - firstCursor.x + 1),
                                            lines[firstCursor.y].text.Length - 1 != secondCursor.x);
                }
                else
                {
                    lines[firstCursor.y] = (lines[firstCursor.y].text.Remove(firstCursor.x), false);

                    for (int i = firstCursor.y + 1; i < secondCursor.y; i++)
                    {
                        lines[i] = ("", false);
                    }

                    lines[secondCursor.y] = (lines[secondCursor.y].text.Remove(0, secondCursor.x + 1),
                                             lines[secondCursor.y].text.Length - 1 != secondCursor.x);
                }

                HighLight = (false, firstCursor.x, firstCursor.y);
                cursor = (firstCursor.x == 0 ? 0 : firstCursor.x - 1, firstCursor.y);
            }
            else
            {
                lines[cursor.y] = (lines[cursor.y].text.Remove(cursor.x, 1), lines[cursor.y].enter);
            }

            foreach (var line in lines)
            {
                if (line.text == "")
                    continue;

                if (line.enter)
                {
                    if ((line.text.Last() != ' ' || line.text.Length <= cursor.x) && line == lines[cursor.y])
                        newContent += line.text;
                    else
                        newContent += line.text.Remove(line.text.Length - 1) + "\n";
                }
                else
                {
                    newContent += line.text;
                }
            }

            newContent = newContent.Length == 0 ? "" : newContent.Remove(newContent.Length - 1);

            this.content = newContent;

            HandleLines();

            if (!isBackspace)
            {
                if (changeType == 2)
                    NewChange(true);
                else
                    NewChange(false);
            }

            changeType = 2;
        }

        //changeType 3
        private void Write(char sym)
        {
            if (HighLight.Is)
            {
                Delete(true);
                CursorRight();
            }

            string newContent = "";

            lines[cursor.y] = (lines[cursor.y].text.Insert(cursor.x, sym.ToString()), lines[cursor.y].enter);

            foreach (var line in lines)
            {
                if (line.text == "")
                    continue;

                if (line.enter)
                {
                    if ((line.text.Last() != ' ' || line.text.Length <= cursor.x) && line == lines[cursor.y])
                        newContent += line.text;
                    else
                        newContent += line.text.Remove(line.text.Length - 1) + "\n";
                }
                else
                {
                    newContent += line.text;
                }
            }

            newContent = newContent.Remove(newContent.Length - 1);

            this.content = newContent;

            HandleLines();

            if (changeType == 3)
                NewChange(true);
            else
                NewChange(false);

            CursorRight();

            changeType = 3;
        }

        private void Copy()
        {
            buffer = "";

            if (HighLight.Is)
            {
                (int x, int y) firstCursor = (cursor.y, cursor.x).CompareTo((HighLight.y, HighLight.x))
                    < 0 ? cursor : (HighLight.x, HighLight.y);

                (int x, int y) secondCursor = (cursor.y, cursor.x).CompareTo((HighLight.y, HighLight.x))
                    > 0 ? cursor : (HighLight.x, HighLight.y);

                if (secondCursor == (lines.Last().text.Length - 1, lines.Count - 1))
                {
                    secondCursor.x--;
                    if (secondCursor.x == -1)
                    {
                        secondCursor.y--;
                        if (secondCursor.y == -1)
                            secondCursor = (0, 0);
                        else
                            secondCursor.x = lines[secondCursor.y].text.Length - 1;
                    }

                    if ((firstCursor.y, firstCursor.x).CompareTo((secondCursor.y, secondCursor.x)) > 0)
                        firstCursor = secondCursor;
                }

                if (firstCursor.y == secondCursor.y)
                {
                    buffer += lines[firstCursor.y].text.Substring(firstCursor.x,
                                                                secondCursor.x - firstCursor.x + 1);

                    if (lines[firstCursor.y].enter && lines[firstCursor.y].text.Length - 1 == secondCursor.x)
                    {
                        buffer = buffer.Remove(buffer.Length - 1) + "\n";
                    }
                }
                else
                {
                    buffer += lines[firstCursor.y].text.Substring(firstCursor.x);
                    if (lines[firstCursor.y].enter)
                    {
                        buffer = buffer.Remove(buffer.Length - 1) + "\n";
                    }

                    for (int i = firstCursor.y + 1; i < secondCursor.y; i++)
                    {
                        buffer += lines[i].enter ? lines[i].text.Remove(lines[i].text.Length - 1) + "\n" : lines[i].text;
                    }

                    lines[secondCursor.y] = (lines[secondCursor.y].text.Remove(0, secondCursor.x + 1),
                                             lines[secondCursor.y].text.Length - 1 != secondCursor.x);
                    if (lines[secondCursor.y].enter && lines[secondCursor.y].text.Length - 1 == secondCursor.x)
                    {
                        buffer = buffer.Remove(buffer.Length - 1) + "\n";
                    }
                }
            }
            else
            {
                buffer += lines[cursor.y].text[cursor.x];
            }
        }

        private void Paste()
        {
            if (HighLight.Is)
            {
                Delete(true);
                CursorRight();
            }

            string newContent = "";

            lines[cursor.y] = (lines[cursor.y].text.Insert(cursor.x, buffer), lines[cursor.y].enter);

            foreach (var line in lines)
            {
                if (line.text == "")
                    continue;

                if (line.enter)
                {
                    if ((line.text.Last() != ' ' || line.text.Length <= cursor.x) && line == lines[cursor.y])
                        newContent += line.text;
                    else
                        newContent += line.text.Remove(line.text.Length - 1) + "\n";
                }
                else
                {
                    newContent += line.text;
                }
            }

            newContent = newContent.Remove(newContent.Length - 1);

            this.content = newContent;

            HandleLines();

            NewChange(false);
        }

        private void Cut()
        {
            Copy();
            Delete(false);
        }

        private void Undo()
        {
            if (changeCursor != 0)
            {
                changeCursor--;
                lines = new List<(string text, bool enter)>(changeHistory[changeCursor]);
            }

            (int x, int y) tempCursor = (lines.Last().text.Length - 1, lines.Count - 1);
            cursor = (cursor.y, cursor.x).CompareTo((tempCursor.y, tempCursor.x))
                < 0 ? cursor : tempCursor;

            HighLight.Is = false;
        }

        private void Redo()
        {
            if (changeCursor != changeHistory.Count - 1)
            {
                changeCursor++;
                lines = new List<(string text, bool enter)>(changeHistory[changeCursor]);
            }

            (int x, int y) tempCursor = (lines.Last().text.Length - 1, lines.Count - 1);
            cursor = (cursor.y, cursor.x).CompareTo((tempCursor.y, tempCursor.x))
                < 0 ? cursor : tempCursor;

            HighLight.Is = false;
        }

        private void NewChange(bool isSameChange)
        {
            if (changeCursor < changeHistory.Count - 1)
                changeHistory.RemoveRange(changeCursor + 1, changeHistory.Count - changeCursor - 1);

            if (!isSameChange)
            {
                changeHistory.Add(new List<(string text, bool enter)>(lines));
                changeCursor = changeHistory.Count - 1;
            }
            else
            {
                changeHistory[changeCursor] = new List<(string text, bool enter)>(lines);
            }
        }

        private string SetTheContent()
        {
            theContent = "";

            foreach(var line in lines)
            {
                theContent += line.enter ? line.text.Remove(line.text.Length - 1) + "\n" : line.text;
            }

            theContent = theContent.Remove(theContent.Length - 1);

            return theContent;
        }
    }
    public class History : Scene
    {
        private List<string> history;

        public History(ConsoleColor[] colors, List<string> history) : base("history", "none", colors)
        {
            this.history = history;
        }

        public override string KeyHandler(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.Escape:
                    return "escape";
            }

            return "";
        }

        public override void Draw()
        {
            ColorReset();
            Console.Clear();

            foreach (var note in history)
            {
                Console.WriteLine(note);
            }
        }
    }
}
