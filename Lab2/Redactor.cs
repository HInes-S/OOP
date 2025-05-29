using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Redactor_ns
{
    static class AddFunc //Additional Functions
    {
        public static string SafeSubstring(string s, int start, int length)
        {
            if (s == null || start < 0 || length < 0 || start >= s.Length)
                return "";

            return s.Substring(start, Math.Min(length, s.Length - start));
        }
        public static void WriteWithHighLight(string s)
        {
            foreach (char c in s)
            {
                if (c == '\n')
                {
                    Console.ResetColor();
                    Console.Write('\n');
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;
                }
                else
                    Console.Write(c);
            }
        }
    }
    class FileDialog
    {
        string head;
        List<string> elements = new List<string>();
        int current;
        DriveInfo[] drives;
        string currentDir = "";
        int dirsCount;

        public FileDialog(string head)
        {
            this.head = head;
            this.drives = DriveInfo.GetDrives();

            ChangeDir("");
        }
        private void ChangeDir(string path)
        {
            this.current = 0;
            this.currentDir = path;
            this.elements.Clear();

            if(path == "")
            {
                foreach(DriveInfo drive in this.drives)
                {
                    this.elements.Add(drive.Name);
                }
            }
            else
            {
                this.elements.Add("..");

                var dirs = Directory.GetDirectories(path);
                
                foreach (var dir in dirs)
                {
                    var attributes = File.GetAttributes(dir);
                    if (!attributes.HasFlag(FileAttributes.Hidden) && !attributes.HasFlag(FileAttributes.System))
                        this.elements.Add(Path.GetFileName(dir));
                }
                dirsCount = this.elements.Count;

                var files = Directory.GetFiles(path);
                foreach (var file in files)
                {
                    var attributes = File.GetAttributes(file);
                    if (!attributes.HasFlag(FileAttributes.Hidden) && !attributes.HasFlag(FileAttributes.System))
                        if (new string[] {".txt", ".md", ".rtf", ".json", ".xml"}.Contains(Path.GetExtension(file).ToLower()))
                            this.elements.Add(Path.GetFileName(file));
                }
            }

            Out();
        }
        private void Out()
        {
            Console.Clear();
            Console.WriteLine(this.head);
            Console.WriteLine();

            int outTen = this.current / 10 * 10;
            int outEnd = this.elements.Count - outTen >= 10 ? outTen + 10 : this.elements.Count;

            if (outEnd == this.elements.Count && outTen != 0)
                Console.WriteLine("...");

            for (int i = outTen; i < outEnd; i++)
            {
                if(i == this.current)
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.WriteLine(this.elements[i]);
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(this.elements[i]);
                }
            }

            if (outEnd != this.elements.Count)
                Console.WriteLine("...");

            Console.WriteLine($"\n{this.current} {this.currentDir}\n{this.elements[current]}\n{outTen}, {outEnd}\n{dirsCount}");
        }
        public void Down()
        {
            if(this.current < this.elements.Count - 1)
            {
                this.current++;
                Out();
            }
        }
        public void Up()
        {
            if(this.current > 0)
            {
                this.current--;
                Out();
            }
        }
        public string Choose()
        {
            List<string> dirs;
            if (this.currentDir == "")
            {
                dirs = new List<string>();
                foreach (var drive in this.drives)
                    dirs.Add(drive.Name);
                for (int i = 0; i < dirs.Count; i++)
                    dirs[i] = dirs[i].Remove(dirs[i].Length - 1);
            }
            else
            {
                dirs = new List<string>();
                dirs.Add("..");
                dirs.AddRange(Directory.GetDirectories(this.currentDir));
            }

            if ((this.currentDir == "" && this.current > dirs.Count - 1) || (this.currentDir != "" && this.current >= dirsCount))
                return this.currentDir + "\\" + this.elements[this.current];
            else
            {
                if (this.elements[current] == "..")
                    if (Path.GetDirectoryName(this.currentDir) == null)
                        ChangeDir("");
                    else
                        ChangeDir(Path.GetDirectoryName(this.currentDir));
                else if (this.currentDir == "")
                    ChangeDir(this.elements[this.current]);
                else
                    ChangeDir(this.currentDir + "\\" + this.elements[this.current]);
                return "";
            }
        }
    }
    class Redactor
    {
        private (string, string) sceneNow = ("menu", ""); //sceneName, sceneType
        private FileDialog file_dialog;
        private Document document;
        private void Undo()
        { }
        private void Redo()
        { }
        private void TextMenu()
        { }
        private bool KeyHandler()
        {
            ConsoleKeyInfo keyInfo;
            try
            {
                keyInfo = Console.ReadKey(true);
            }
            catch
            {
                return true;
            }

            switch (keyInfo.Key)
            {
                case ConsoleKey.Escape:
                    return false;

                case ConsoleKey.Backspace:
                    if (sceneNow == ("document", "edit"))
                    {
                        document.Backspace();
                    }
                    break;

                case ConsoleKey.A:
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        if (sceneNow.Item1 == "document")
                        {
                            document.SelectAll();
                        }
                    }
                    break;

                case ConsoleKey.O:
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        if (sceneNow.Item1 == "menu")
                        {
                            file_dialog = new FileDialog("Open File");
                            sceneNow = ("filedialog", "open");
                        }
                    }
                    break;

                case ConsoleKey.S:
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
                        {
                            if (sceneNow.Item1 == "document")
                            {
                                file_dialog = new FileDialog("Save File");
                                sceneNow = ("filedialog", "save");
                            }
                        }
                        else
                        {
                            if (sceneNow.Item1 == "document")
                            {
                                file_dialog = new FileDialog("Save File As");
                                sceneNow = ("filedialog", "saveas");
                            }
                        }
                    }
                    break;

                case ConsoleKey.M:
                    if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control) &&
                        keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
                    {
                        Menu();
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if (sceneNow.Item1 == "filedialog")
                    {
                        file_dialog.Down();
                    }
                    else if (sceneNow.Item1 == "document")
                    {
                        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
                            document.isSelection = true;
                        else
                            document.isSelection = false;

                        document.CursorDown();
                    }
                    break;

                case ConsoleKey.UpArrow:
                    if (sceneNow.Item1 == "filedialog")
                    {
                        file_dialog.Up();
                    }
                    else if (sceneNow.Item1 == "document")
                    {
                        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
                            document.isSelection = true;
                        else
                            document.isSelection = false;

                        document.CursorUp();
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (sceneNow.Item1 == "document")
                    {
                        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
                            document.isSelection = true;
                        else
                            document.isSelection = false;

                        document.CursorLeft();
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (sceneNow.Item1 == "document")
                    {
                        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
                            document.isSelection = true;
                        else
                            document.isSelection = false;

                        document.CursorRight();
                    }
                    break;

                case ConsoleKey.Enter:
                    if (sceneNow.Item1 == "filedialog")
                    {
                        string choosedFile = file_dialog.Choose();
                        if (choosedFile != "")
                        {
                            OpenFile(choosedFile);
                        }
                    }
                    break;
            }

            if ((char.IsControl(keyInfo.KeyChar) ? '\0' : keyInfo.KeyChar) != '\0' && sceneNow == ("document", "edit"))
            {
                document.Write(keyInfo.KeyChar);
            }

            return true;
        }
        private void OpenFile(string path)
        {
            //TEMP
            document = new Document(path);
            sceneNow = ("document", "edit");
            //TEMPEND

            /*
            try
            {
                document = new Document(path);
                sceneNow = ("document", "edit");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }*/

        }

        private void ShowError(string error)
        {
            Console.Clear();
            Console.Write($"ERROR: {error}");
        }
        private void SaveFile()
        { }
        private void SaveFileAs()
        { }
        private void DeleteFile()
        { }
        private void CreateFile()
        { }
        private void Login()
        { }
        private void Options()
        { }
        private void Menu()
        { }
        public void Run()
        {
            bool program = true;

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
            };

            while (program)
            {
                program = KeyHandler();
            }
        }
    }
    class Document
    {
        string path;
        (int, int) cursor;
        (int, int) lastCursor;
        string content;
        List<string> windowContent;
        int headSize;
        int cw;
        int ch;
        (int begin, int end) outSize;
        (bool, bool) lastSymCheck = (false, false);
        public bool isSelection { get; set; }
        public Document(string path)
        {
            this.path = path;
            content = File.ReadAllText(this.path);
            this.cursor = (0, 0);
            this.lastCursor = (0, 0);
            isSelection = false;
            cw = Console.WindowWidth;

            content = content.Replace("\t", "    ").Replace("\r", "");

            outSize.begin = 0;

            TextHandle();
            Update();
        }
        private void Update()
        {
            if (!isSelection)
                lastCursor = cursor;

            (int x, int y) firstCursor = (cursor.Item2, cursor.Item1).CompareTo((lastCursor.Item2, lastCursor.Item1))
                < 0 ? cursor : lastCursor;
            (int x, int y) secondCursor = (cursor.Item2, cursor.Item1).CompareTo((lastCursor.Item2, lastCursor.Item1))
                > 0 ? cursor : lastCursor;

            Console.Clear();
            Console.Write($"({cursor.Item1}, {cursor.Item2}) : ({lastCursor.Item1}, {lastCursor.Item2})\n" +
                          $"({outSize.begin}, {outSize.end})\n" +
                          $"({firstCursor.x}, {firstCursor.y}), ({secondCursor.x}, {secondCursor.y})\n");
            headSize = Console.CursorTop + 1;

            cw = Console.WindowWidth;
            ch = Console.WindowHeight - headSize;

            outSize.end = Math.Min(outSize.begin + ch - 1, windowContent.Count - 1);

            if (!(cursor.Item2 >= outSize.begin && cursor.Item2 <= outSize.end))
            {
                if (cursor.Item2 < outSize.begin)
                {
                    outSize.begin = cursor.Item2;
                    outSize.end = outSize.begin + ch - 1;
                }
                else
                {
                    outSize.end = cursor.Item2;
                    outSize.begin = outSize.end - ch + 1;
                }
            }

            for (int i = outSize.begin; i <= outSize.end; i++)
            {
                Console.Write(windowContent[i]);
            }

            firstCursor = (firstCursor.y, firstCursor.x).CompareTo((outSize.begin, 0))
                > 0 ? firstCursor : (0, outSize.begin);
            firstCursor = (firstCursor.y, firstCursor.x).CompareTo((outSize.end, windowContent[outSize.end].Length - 1))
                < 0 ? firstCursor : (windowContent[outSize.end].Length - 1, outSize.end);
            secondCursor = (secondCursor.y, secondCursor.x).CompareTo((outSize.end, windowContent[outSize.end].Length - 1))
                < 0 ? secondCursor : (windowContent[outSize.end].Length - 1, outSize.end);

            string selectedText = "";

            if (firstCursor.y == secondCursor.y)
            {
                selectedText = AddFunc.SafeSubstring(windowContent[firstCursor.y],
                                                      firstCursor.x,
                                                      secondCursor.x - firstCursor.x + 1);

                if (selectedText == "\n")
                    selectedText = " \n";
            }
            else
            {
                if (windowContent[firstCursor.y] == "\n")
                    selectedText += " \n";
                else
                    selectedText += AddFunc.SafeSubstring(windowContent[firstCursor.y],
                                                          firstCursor.x,
                                                          windowContent[firstCursor.y].Length - firstCursor.x + 1);

                for (int i = firstCursor.y + 1; i < secondCursor.y; i++)
                {
                    if (windowContent[i] == "\n")
                        selectedText += " \n";
                    else
                        selectedText += windowContent[i];
                }

                if (windowContent[secondCursor.y] == "\n")
                    selectedText += " \n";
                else
                    selectedText += AddFunc.SafeSubstring(windowContent[secondCursor.y],
                                                          0,
                                                          secondCursor.x + 1);
            }

            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;

            Console.SetCursorPosition(firstCursor.x, firstCursor.y + headSize - 1 - outSize.begin);
            Console.Write(selectedText);

            Console.ResetColor();
        }
        private void TextHandle()
        {
            windowContent = new List<string>();

            var newLineSplit = content.Split("\n");

            int cw = Console.WindowWidth;

            for (int i = 0; i < newLineSplit.Length - 1; i++)
            {
                newLineSplit[i] += "\n";
                string line = newLineSplit[i];
                windowContent.AddRange(Enumerable.Range(0, (line.Length + cw - 1) / cw)
                    .Select(i => line.Substring(i * cw, Math.Min(cw, line.Length - i * cw))));
            }

            string lineLast = newLineSplit[newLineSplit.Length - 1];
            windowContent.AddRange(Enumerable.Range(0, (lineLast.Length + cw - 1) / cw)
                .Select(i => lineLast.Substring(i * cw, Math.Min(cw, lineLast.Length - i * cw))));

            //throw new Exception(); //TEMP
        }
        public void CursorLeft()
        {
            cursor.Item1--;

            if (cursor.Item1 < 0)
            {
                cursor.Item2--;
            }

            if (cursor.Item2 < 0)
            {
                cursor.Item2 = 0;
                cursor.Item1 = 0;
            }
            else if (cursor.Item1 < 0)
            {
                cursor.Item1 = windowContent[cursor.Item2].Length - 1;
            }

            if (windowContent[cursor.Item2][cursor.Item1] == '\n' && cursor.Item1 != 0)
                CursorLeft();

            if (lastSymCheck.Item1)
            {
                if (lastSymCheck.Item2 && cursor.Item2 != windowContent.Count - 1)
                {
                    windowContent.RemoveAt(windowContent.Count - 1);
                    outSize.begin--;
                    lastSymCheck = (false, false);
                }
                else if (cursor.Item1 != windowContent[cursor.Item2].Length - 1)
                {
                    windowContent[windowContent.Count - 1] = windowContent[windowContent.Count - 1].Remove(
                                                             windowContent[windowContent.Count - 1].Length - 1);
                    lastSymCheck = (false, false);
                }
            }

            Update();
        }
        public void CursorRight()
        {
            cursor.Item1++;

            if (cursor.Item1 >= windowContent[cursor.Item2].Length && cursor.Item2 < windowContent.Count - 1)
            {
                cursor.Item2++;
                cursor.Item1 = 0;
            }
            else if (cursor.Item1 >= windowContent[cursor.Item2].Length && cursor.Item2 == windowContent.Count - 1)
            {
                if (!lastSymCheck.Item1)
                {
                    if (cursor.Item1 < cw && windowContent[cursor.Item2][cursor.Item1 - 1] != '\n')
                    {
                        windowContent[cursor.Item2] += " ";
                        lastSymCheck = (true, false);
                    }
                    else
                    {
                        windowContent.Add(" ");
                        cursor.Item1 = 0;
                        cursor.Item2++;
                        lastSymCheck = (true, true);
                    }
                }
                else
                {
                    cursor.Item2++;
                    cursor.Item1 = 0;
                }
            }

            if (cursor.Item2 >= windowContent.Count)
            {
                CursorLeft();
            }

            if (windowContent[cursor.Item2][cursor.Item1] == '\n' && cursor.Item1 != 0)
                CursorRight();

            Update();
        }
        public void CursorUp()
        {
            cursor.Item2--;

            if (cursor.Item2 < 0)
                cursor.Item2 = 0;

            cursor.Item1 = Math.Min(cursor.Item1, windowContent[cursor.Item2].Length - 1);

            if (windowContent[cursor.Item2][cursor.Item1] == '\n' && cursor.Item1 != 0)
                CursorLeft();

            if (lastSymCheck.Item1)
            {
                if (lastSymCheck.Item2)
                {
                    windowContent.RemoveAt(windowContent.Count - 1);
                    outSize.begin--;
                    lastSymCheck = (false, false);
                }
                else
                {
                    windowContent[windowContent.Count - 1] = windowContent[windowContent.Count - 1].Remove(
                                                             windowContent[windowContent.Count - 1].Length - 1);
                    lastSymCheck = (false, false);
                }
            }

            Update();
        }
        public void CursorDown()
        {
            cursor.Item2++;

            if (cursor.Item2 >= windowContent.Count)
                cursor.Item2 = windowContent.Count - 1;

            cursor.Item1 = Math.Min(cursor.Item1, windowContent[cursor.Item2].Length - 1);

            if (windowContent[cursor.Item2][cursor.Item1] == '\n' && cursor.Item1 != 0)
                CursorLeft();

            Update();
        }
        public void Backspace()
        {
            if (!isSelection && cursor != (0, 0))
            {
                CursorLeft();
                windowContent[cursor.Item2] = windowContent[cursor.Item2].Remove(cursor.Item1, 1);

                if (windowContent[cursor.Item2] == "")
                {
                    windowContent.RemoveAt(cursor.Item2);
                }

                int firstBackLine = Math.Min(cursor.Item2, windowContent.Count - 1);
                while (windowContent[firstBackLine].Last() != '\n' && firstBackLine < windowContent.Count - 1)
                {
                    windowContent[firstBackLine] += windowContent[firstBackLine + 1][0].ToString();
                    windowContent[firstBackLine + 1] = windowContent[firstBackLine + 1].Remove(0, 1);
                }
            }
            else if (isSelection)
            {

            }

            Update();
        }
        public void Delete()
        { }
        public void Undo()
        { }
        public void Redo()
        { }
        public void Write(char sym)
        {
            windowContent[cursor.Item2] = windowContent[cursor.Item2].Insert(cursor.Item1, sym.ToString())
           .Replace("\t", "    ").Replace("\r", "");

            if (windowContent[cursor.Item2].Length > cw)
            {
                int excessCounter = cursor.Item2;

                while (windowContent[excessCounter].Length > cw && excessCounter < windowContent.Count - 1)
                {
                    int excessCount = windowContent[excessCounter].Length - cw;
                    string excessString = AddFunc.SafeSubstring(windowContent[excessCounter],
                                                                cw,
                                                                excessCount);
                    windowContent[excessCounter] = windowContent[excessCounter].Remove(cw);
                    windowContent[excessCounter + 1] = excessString + windowContent[excessCounter + 1];
                    excessCounter++;
                }

                if (windowContent[excessCounter].Length > cw)
                {
                    int excessCount = windowContent[excessCounter].Length - cw;
                    string excessString = AddFunc.SafeSubstring(windowContent[excessCounter],
                                                                cw,
                                                                excessCount);
                    windowContent[excessCounter] = windowContent[excessCounter].Remove(cw);
                    windowContent.Add(excessString);
                }
            }

            Update();
            CursorRight();
        }
        public void SelectAll()
        {
            isSelection = true;
            cursor = (windowContent[windowContent.Count - 1].Length - 1, windowContent.Count - 1);
            lastCursor = (0, 0);
            Update();
        }
    }
}
