using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace Droweroid
{
    public class Drawer
    {
        public static bool Proga { get; set; }
        public static Canvas Canvas { get; set; }
        public static string Comand { get; set; }
        public static List<Canvas> SaverList { get; set; }
        public static int SaverPointer { get; set; }

        public static void DoComand(string comand)
        {
            List<string> keyWords = comand.Split(' ').ToList();

            string error = "";
            int cnt = keyWords.Count();
            bool change = false;

            switch (keyWords[0])
            {
                case "":
                    break;

                case "exit":
                    if (cnt == 1)
                    {
                        Proga = false;
                        return;
                    }
                    else
                        error = "Error: Command 'exit' have no arguments";
                    break;

                case "clear":
                    if (cnt == 1)
                    {
                        Canvas.Figures.Clear();
                        Canvas.Update(Canvas.Figures);
                        change = true;
                    }
                    else
                        error = "Error: Command 'clear' have no arguments";
                    break;

                case "canvas":
                    if (cnt == 1)
                    {
                        error = $"Canvas: size({Canvas.Width}, {Canvas.Height}), color({Canvas.BackgroundColor})";
                        break;
                    }
                    switch (keyWords[1])
                    {
                        case "color":
                            if (cnt == 3 && (keyWords[2].Length == 1 || keyWords[2] == "space"))
                            {
                                if (keyWords[2].Length != 1)
                                {
                                    Canvas.Update(' ');
                                    change = true;
                                    break;
                                }
                                Canvas.Update(Char.Parse(keyWords[2]));
                                change = true;
                            }
                            else
                            {
                                error = "Error: Wrong arguments";
                            }
                            break;

                        case "size":
                            int w, h;
                            if (cnt == 4 && int.TryParse(keyWords[2], out w) && int.TryParse(keyWords[3], out h))
                            {
                                if (w >= 1 && h >= 1)
                                {
                                    try
                                    {
                                        Canvas.Update(w, h);
                                        change = true;
                                    }
                                    catch (OutOfMemoryException ex)
                                    {
                                        error = "Error: Out of memory";
                                    }
                                    break;
                                }
                            }
                            error = "Error: Incorrect use of 'canvas size'";
                            break;

                        default:
                            if (keyWords[1] == "")
                                error = "Error: Comand 'canvas' have more arguments";
                            else
                                error = $"Error: Command 'canvas' have no argument '{keyWords[1]}'";
                            break;

                    }
                    break;

                case "save":
                    if (cnt == 2)
                    {
                        error = Canvas.Save(keyWords[1]);
                        break;
                    }
                    error = "Error: Wrong count of arguments";
                    break;

                case "load":
                    if (cnt == 2)
                    {
                        error = Canvas.Load(keyWords[1]);
                        break;
                    }
                    error = "Error: Wrong count of arguments";
                    break;

                case "draw":
                    if (cnt == 10)
                    {
                        if (Canvas.Figures.Find(f => f != null && f.Name == keyWords[2]) != null)
                        {
                            error = "Error: Figure with this Name already exists";
                            break;
                        }

                        if (keyWords[2] == "")
                        {
                            error = "Error: Wrong Name";
                            break;
                        }

                        if (!int.TryParse(keyWords[3], out var tempX) || !int.TryParse(keyWords[4], out var tempY))
                        {
                            error = "Error: Wrong Coordinates";
                            break;
                        }
                        else if (tempX < 0 || tempY < 0)
                        {
                            error = "Error: Wrong Coordinates";
                            break;
                        }

                        if (!int.TryParse(keyWords[5], out var tempW) || !int.TryParse(keyWords[6], out var tempH))
                        {
                            error = "Error: Wrong Size";
                            break;
                        }
                        else if (tempW < 1 || tempH < 1)
                        {
                            error = "Error: Wrong Size";
                            break;
                        }

                        //border, background, layer
                        if (!char.TryParse(keyWords[7], out var tempBorder) && keyWords[7] != "empty" && keyWords[7] != "space")
                        {
                            error = "Error: Wrong BorderColor";
                            break;
                        }

                        if (!char.TryParse(keyWords[8], out var tempBackgound) && keyWords[8] != "empty" && keyWords[8] != "space")
                        {
                            error = "Error: Wrong BackgroundColor";
                            break;
                        }

                        if (!int.TryParse(keyWords[9], out var tempLayer))
                        {
                            error = "Error: Wrong Layer";
                            break;
                        }

                        if (keyWords[7] == "empty")
                            tempBorder = '\n';
                        if (keyWords[8] == "empty")
                            tempBackgound = '\n';
                        if (keyWords[7] == "space")
                            tempBorder = ' ';
                        if (keyWords[8] == "space")
                            tempBackgound = ' ';

                        switch (keyWords[1])
                        {
                            case "ellipse":
                                Canvas.Figures.Add(new Ellipse(tempX, tempY, tempW, tempH, tempBorder, tempBackgound, tempLayer, keyWords[2]));
                                Canvas.Update(Canvas.Figures);
                                change = true;
                                break;

                            case "rect":
                                Canvas.Figures.Add(new Rectangle(tempX, tempY, tempW, tempH, tempBorder, tempBackgound, tempLayer, keyWords[2]));
                                Canvas.Update(Canvas.Figures);
                                change = true;
                                break;

                            default:
                                error = "Error: wrong figure type";
                                break;
                        }
                    }
                    else if (cnt == 12)
                    {
                        if (Canvas.Figures.Find(f => f != null && f.Name == keyWords[2]) != null)
                        {
                            error = "Error: Figure with this Name already exists";
                            break;
                        }

                        if (keyWords[2] == "")
                        {
                            error = "Error: Wrong Name";
                            break;
                        }

                        if (!int.TryParse(keyWords[3], out var tempX1) ||
                            !int.TryParse(keyWords[4], out var tempY1) ||
                            !int.TryParse(keyWords[5], out var tempX2) ||
                            !int.TryParse(keyWords[6], out var tempY2) ||
                            !int.TryParse(keyWords[7], out var tempX3) ||
                            !int.TryParse(keyWords[8], out var tempY3))
                        {
                            error = "Error: Wrong Coordinates";
                            break;
                        }
                        else if (tempX1 < 0 || tempY1 < 0 || tempX2 < 0 || tempY2 < 0 || tempX3 < 0 || tempY3 < 0)
                        {
                            error = "Error: Wrong Coordinates";
                            break;
                        }

                        //border, background, layer
                        if (!char.TryParse(keyWords[9], out var tempBorder) && keyWords[9] != "empty" && keyWords[9] != "space")
                        {
                            error = "Error: Wrong BorderColor";
                            break;
                        }

                        if (!char.TryParse(keyWords[10], out var tempBackgound) && keyWords[10] != "empty" && keyWords[10] != "space")
                        {
                            error = "Error: Wrong BackgroundColor";
                            break;
                        }

                        if (!int.TryParse(keyWords[11], out var tempLayer))
                        {
                            error = "Error: Wrong Layer";
                            break;
                        }

                        if (keyWords[9] == "empty")
                            tempBorder = '\n';
                        if (keyWords[10] == "empty")
                            tempBackgound = '\n';
                        if (keyWords[9] == "space")
                            tempBorder = ' ';
                        if (keyWords[10] == "space")
                            tempBackgound = ' ';

                        if (keyWords[1] == "triangle")
                        {
                            Canvas.Figures.Add(new Triangle((tempX1, tempY1, tempX2, tempY2, tempX3, tempY3), 
                                               tempBorder, tempBackgound, tempLayer, keyWords[2]));
                            Canvas.Update(Canvas.Figures);
                            change = true;
                        }
                        else
                        {
                            error = "Error: Wrong figure type";
                        }
                    }
                    else
                        error = "Error: Wrong count of arguments";
                    break;

                case "delete":
                    if (cnt != 2)
                    {
                        error = "Error: Wrong count of arguments";
                        break;
                    }
                    else
                    {
                        if (Canvas.Figures.Find(f => f != null && f.Name == keyWords[1]) != null)
                        {
                            var tempFigure = Canvas.Figures.Find(f => f != null && f.Name == keyWords[1]);
                            Canvas.Figures.Remove(tempFigure);
                            Canvas.Update(Canvas.Figures);
                            change = true;
                        }
                        else
                        {
                            error = "Error: figure with this Name doesn't exist";
                        }
                    }
                    break;

                case "figure":
                    if (cnt > 1)
                    {
                        if (keyWords[1] == "names")
                        {
                            if (cnt != 2)
                            {
                                error = "Error: Wrong count of arguments";
                                break;
                            }
                            error = "";
                            foreach (var f in Canvas.Figures)
                            {
                                error = error + f.Name + ' ';
                            }
                        }
                        else
                        {
                            if (Canvas.Figures.Find(f => f != null && f.Name == keyWords[1]) != null)
                            {
                                Figure tempf = Canvas.Figures.Find(f => f != null && f.Name == keyWords[1]);

                                if (cnt == 2)
                                {
                                    error = $"{tempf.Name}: pos({tempf.x}, {tempf.y}), size({tempf.Width}, {tempf.Height}), layer({tempf.Layer})," +
                                            $" borderColor({tempf.BorderColor}), bgColor({tempf.BackgroundColor}), type(";

                                    if (tempf.GetType() == typeof(Ellipse))
                                        error = error + "ellipse)";
                                    else if (tempf.GetType() == typeof(Rectangle))
                                        error = error + "rectangle)";
                                    else if (tempf.GetType() == typeof(Triangle))
                                        error = error + "triangle)";
                                    else
                                        error = error + "unknown)";
                                }
                                else
                                {
                                    switch (keyWords[2])
                                    {
                                        case "pos":
                                            if (cnt == 5)
                                            {
                                                if (int.TryParse(keyWords[3], out var tempx) && int.TryParse(keyWords[4], out var tempy))
                                                {
                                                    if (tempx >= 0 && tempx + tempf.Width <= Canvas.Width &&
                                                       tempy >= 0 && tempy + tempf.Height <= Canvas.Height)
                                                    {
                                                        tempf.x = tempx;
                                                        tempf.y = tempy;
                                                    }
                                                    else
                                                    {
                                                        error = "Error: Wrong coordinates";
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    error = "Error: Wrong coordinates";
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                error = "Error: Wrong count of arguments";
                                            }
                                            break;

                                        case "color":
                                            if (cnt == 5)
                                            {
                                                if ((char.TryParse(keyWords[3], out var tempbc) || keyWords[3] == "empty" || keyWords[3] == "space") &&
                                                    (char.TryParse(keyWords[4], out var tempbgc) || keyWords[4] == "empty" || keyWords[4] == "space"))
                                                {
                                                    if (tempbc == '\0')
                                                        if (keyWords[3] == "empty")
                                                            tempf.BorderColor = '\n';
                                                        else
                                                            tempf.BorderColor = ' ';
                                                    else
                                                        tempf.BorderColor = tempbc;

                                                    if (tempbgc == '\0')
                                                        if (keyWords[4] == "empty")
                                                            tempf.BackgroundColor = '\n';
                                                        else
                                                            tempf.BackgroundColor = ' ';
                                                    else
                                                        tempf.BackgroundColor = tempbgc;
                                                }
                                                else
                                                {
                                                    error = "Error: Wrong colors";
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                error = "Error: Wrong count of arguments";
                                            }
                                            break;

                                        case "layer":
                                            if (cnt == 4)
                                            {
                                                if (int.TryParse(keyWords[3], out var tempLayer))
                                                {
                                                    tempf.Layer = tempLayer;
                                                }
                                                else
                                                {
                                                    error = "Error: Wrong Layer";
                                                }
                                            }
                                            else
                                            {
                                                error = "Error: Wrong count of arguments";
                                            }
                                            break;

                                        case "size":
                                            if (cnt == 5)
                                            {
                                                if (keyWords[2].GetType() == typeof(Triangle))
                                                {
                                                    error = "Error: Can not change size of triangle";
                                                    break;
                                                }
                                                else if (int.TryParse(keyWords[3], out var tempw) && int.TryParse(keyWords[4], out var temph))
                                                {
                                                    if (tempw > 0 && tempw + tempf.x <= Canvas.Width &&
                                                       temph > 0 && temph + tempf.y <= Canvas.Height)
                                                    {
                                                        tempf.Width = tempw;
                                                        tempf.Height = temph;
                                                    }
                                                    else
                                                    {
                                                        error = "Error: Wrong size parameters";
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    error = "Error: Wrong size parameeters";
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                error = "Error: Wrong count of arguments";
                                            }
                                            break;

                                        default:
                                            error = "Error: Wrong option";
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                error = "Error: Figure with this Name doesn't exists";
                                break;
                            }
                        }
                    }
                    else
                    {
                        error = "Error: Wrong count of arguments";
                    }
                    if (error == "")
                    {
                        Canvas.Update(Canvas.Figures);
                        change = true;
                    }
                    break;

                case "undo":
                    if (cnt == 1)
                    {
                        if (SaverPointer > 0)
                            SaverPointer--;
                        else
                            error = "Error: The maximum cancellation of changes has been reached";
                    }
                    else
                    {
                        error = "Error: Wrong count of arguments";
                    }
                    break;

                case "redo":
                    if (cnt == 1)
                    {
                        if (SaverPointer < SaverList.Count - 1)
                            SaverPointer++;
                        else
                            error = "Error: The maximum repeat of changes has been reached";
                    }
                    else
                    {
                        error = "Error: Wrong count of arguments";
                    }
                    break;

                case "help":
                    if (cnt == 1)
                    {
                        error = "draw, clear, save, load, canvas, delete, figure, undo, redo, exit, help";
                    }
                    else if (cnt == 2)
                    {
                        switch (keyWords[1])
                        {
                            case "draw":
                                error = "draw elipse <Name> <x> <y> <w> <h> <BorderColor> <BackgroundColor> <Layer>\n" +
                                        "draw rect <Name> <x> <y> <w> <h> <BorderColor> <BackgroundColor> <Layer>\n" +
                                        "draw triangle <Name> <x1> <y1> <x2> <y2> <x3> <y3> <BorderColor> <BackgroundColor> <Layer>";
                                break;

                            case "clear":
                                error = "clear";
                                break;

                            case "save":
                                error = "save <filepath>";
                                break;

                            case "load":
                                error = "load <filepath>";
                                break;

                            case "canvas":
                                error = "canvas color <Color> | canvas size <w> <h>";
                                break;

                            case "delete":
                                error = "delete <Name>";
                                break;

                            case "figure":
                                error = "figure <Name> pos <x> <y> | figure <Name> color <BorderColor> <BackgroundColor> | figure <Name> layer <Layer> | figure <Name> size <w> <h> | figure <Name> | figure names";
                                break;

                            case "undo":
                                error = "undo";
                                break;

                            case "redo":
                                error = "redo";
                                break;

                            case "exit":
                                error = "exit";
                                break;

                            case "help":
                                error = "help | help <command>";
                                break;

                            default:
                                error = $"Error: Unknown command '{keyWords[1]}'";
                                break;
                        }
                    }
                    else
                    {
                        error = "Wrong count of arguments";
                    }
                    break;

                default:
                    error = "Error: Unknown command";
                    break;
            }

            if (change)
            {
                Canvas temp = Canvas.DeepClone();
                if (SaverPointer == SaverList.Count - 1)
                    SaverList.Add(temp);
                else
                    SaverList[SaverPointer + 1] = temp;
                SaverPointer++;
                SaverList.RemoveRange((SaverPointer + 1) % SaverList.Count, SaverList.Count - SaverPointer - 1);
            }

            if (SaverPointer > 9)
            {
                SaverPointer = 9;
                SaverList.RemoveRange(0, SaverList.Count - 10);
            }

            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            Console.Write(error/* + new string(' ', Console.WindowWidth - error.Length)*/);
            if (error != "")
                Console.ReadKey(true);

            ReDraw(SaverList[SaverPointer]);

            Canvas = SaverList[SaverPointer].DeepClone();
        }
        public static void ReDraw(Canvas canvas)
        {
            lock (Console.Out)
            {
                int h = canvas.h();
                int w = canvas.w();
                char[,] img = canvas.img();

                Console.Clear();
                Console.SetCursorPosition(0, 0);
                for (int i = 0; i < h; i++)
                {
                    if (i >= Console.WindowHeight - 2)
                        break;
                    for (int j = 0; j < w; j++)
                    {
                        if (j >= Console.WindowWidth)
                            break;
                        Console.Write(img[i, j]);
                    }
                    Console.Write('\n');
                }

                Console.SetCursorPosition(0, Console.WindowHeight - 2);
                Console.Write('>');
            }
        }
    }
    public class Filler
    {
        public static int[,] Fill(int[,] matrixx, int ex, int ey, int color)
        {
            var matrix = matrixx;
            var beginColor = matrix[ex, ey];

            if (beginColor == color)
                return matrix;

            matrix[ex, ey] = color;
            int cx = 0, cy = 0;

            List<(int, int)> chckList = new();
            List<(int, int)> nextList = new();
            chckList.Add((ex, ey));

            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);

            do
            {
                nextList.Clear();

                foreach (var l in chckList)
                {
                    cx = l.Item1 - 1; cy = l.Item2;
                    if(cx >= 0 && cy >= 0 && cx < w && cy < h)
                    {
                        if (matrix[cx, cy] == beginColor)
                        {
                            matrix[cx, cy] = color;
                            nextList.Add((cx, cy));
                        }
                    }

                    cx = l.Item1 + 1;
                    if (cx >= 0 && cy >= 0 && cx < w && cy < h)
                    {
                        if (matrix[cx, cy] == beginColor)
                        {
                            matrix[cx, cy] = color;
                            nextList.Add((cx, cy));
                        }
                    }

                    cy = l.Item2 - 1; cx = l.Item1;
                    if (cx >= 0 && cy >= 0 && cx < w && cy < h)
                    {
                        if (matrix[cx, cy] == beginColor)
                        {
                            matrix[cx, cy] = color;
                            nextList.Add((cx, cy));
                        }
                    }

                    cy = l.Item2 + 1;
                    if (cx >= 0 && cy >= 0 && cx < w && cy < h)
                    {
                        if (matrix[cx, cy] == beginColor)
                        {
                            matrix[cx, cy] = color;
                            nextList.Add((cx, cy));
                        }
                    }
                }

                chckList = nextList.ToList();

            } while (chckList.Count != 0);

            return matrix;
        }
    }
    public abstract class Figure
    {
        public abstract int x { get; set; }
        public abstract int y { get; set; }
        public abstract int Width { get; set; }
        public abstract int Height { get; set; }
        public abstract char BorderColor { get; set; }
        public abstract char BackgroundColor { get; set; }
        public abstract int Layer { get; set; }
        public abstract string Name { get; set; }
        public virtual char[,] GetImage() { return new char[0, 0]; }
        public abstract Figure DeepClone();
        
    }
    public class Ellipse : Figure
    {
        public override int x { get; set; }
        public override int y { get; set; }
        public override int Width { get; set; }
        public override int Height { get; set; }
        public override char BorderColor { get; set; }
        public override char BackgroundColor { get; set; }
        public override int Layer { get; set; }
        public override string? Name { get; set; }

        public Ellipse()
        {
            x = 0;
            y = 0;
            Width = 1;
            Height = 1;
            BorderColor = ' ';
            BackgroundColor = ' ';
            Layer = 0;
        }

        public Ellipse(int x, int y, int width, int height, char borderColor, char backgroundColor, int Layer, string name)
        {
            this.x = x;
            this.y = y;
            Width = width;
            Height = height;
            BorderColor = borderColor;
            BackgroundColor = backgroundColor;
            this.Layer = Layer;
            Name = name;
        }

        public override Figure DeepClone()
        {
            return new Ellipse
            {
                x = this.x,
                y = this.y,
                Width = this.Width,
                Height = this.Height,
                BorderColor = this.BorderColor,
                BackgroundColor = this.BackgroundColor,
                Layer = this.Layer,
                Name = this.Name
            };
        }

        public int[,] GetForm()
        {
            int w = Width, h = Height;
            double rx = w / 2 - (w % 2 == 0 ? 1 : 0), ry = h / 2 - (h % 2 == 0 ? 1 : 0);
            var ans = new int[w, h];
            double dx, dy, d1, d2, x, y;

            x = 0;
            y = ry;

            int ex, ey;

            d1 = (ry * ry) - (rx * rx * ry) + (0.25f * rx * rx);
            dx = 2 * ry * ry * x;
            dy = 2 * rx * rx * y;

            while (dx < dy)
            {
                ex = -(int)x + (int)rx; ey = -(int)y + (int)ry;
                ans[ex, ey] = 1;
                ans[w - 1 - ex, h - 1 - ey] = 1;
                ans[ex, h - 1 - ey] = 1;
                ans[w - 1 - ex, ey] = 1;

                if (d1 < 0)
                {
                    x++;
                    dx = dx + (2 * ry * ry);
                    d1 = d1 + dx + (ry * ry);
                }
                else
                {
                    x++;
                    y--;
                    dx = dx + (2 * ry * ry);
                    dy = dy - (2 * rx * rx);
                    d1 = d1 + dx - dy + (ry * ry);
                }
            }

            d2 = ((ry * ry) * ((x + 0.5f) * (x + 0.5f)))
                + ((rx * rx) * ((y - 1) * (y - 1)))
                - (rx * rx * ry * ry);

            while (y >= 0)
            {
                ex = -(int)x + (int)rx; ey = -(int)y + (int)ry;
                ans[ex, ey] = 1;
                ans[w - 1 - ex, h - 1 - ey] = 1;
                ans[ex, h - 1 - ey] = 1;
                ans[w - 1 - ex, ey] = 1;

                if (d2 > 0)
                {
                    y--;
                    dy = dy - (2 * rx * rx);
                    d2 = d2 + (rx * rx) - dy;
                }
                else
                {
                    y--;
                    x++;
                    dx = dx + (2 * ry * ry);
                    dy = dy - (2 * rx * rx);
                    d2 = d2 + dx - dy + (rx * rx);
                }
            }

            if (w >= 3 && h >= 3)
                return Filler.Fill(ans, w / 2, h / 2, 2);
            else return ans;
        }

        public override char[,] GetImage()
        {
            var form = GetForm();
            var w = form.GetLength(0);
            var h = form.GetLength(1);
            var ans = new char[w, h];

            for(int i = 0; i < w; i++)
            {
                for(int j = 0; j < h; j++)
                {
                    if (form[i, j] == 0)
                        ans[i, j] = '\n';
                    else if (form[i, j] == 1)
                        ans[i, j] = BorderColor;
                    else
                        ans[i, j] = BackgroundColor;
                }
            }

            return ans;
        }
    }
    public class Rectangle : Figure
    {
        public override int x { get; set; }
        public override int y { get; set; }
        public override int Width { get; set; }
        public override int Height { get; set; }
        public override char BorderColor { get; set; }
        public override char BackgroundColor { get; set; }
        public override int Layer { get; set; }
        public override string? Name { get; set; }

        public Rectangle()
        {
            x = 0;
            y = 0;
            Width = 1;
            Height = 1;
            BorderColor = ' ';
            BackgroundColor = ' ';
            Layer = 0;
        }

        public Rectangle(int x, int y, int width, int height, char borderColor, char backgroundColor, int Layer, string name)
        {
            this.x = x;
            this.y = y;
            Width = width;
            Height = height;
            BorderColor = borderColor;
            BackgroundColor = backgroundColor;
            this.Layer = Layer;
            Name = name;
        }

        public override Figure DeepClone()
        {
            return new Rectangle
            {
                x = this.x,
                y = this.y,
                Width = this.Width,
                Height = this.Height,
                BorderColor = this.BorderColor,
                BackgroundColor = this.BackgroundColor,
                Layer = this.Layer,
                Name = this.Name
            };
        }

        public override char[,] GetImage()
        {
            var ans = new char[Width, Height];

            for(int i = 0; i < Width; i++)
            {
                for(int j = 0; j < Height; j++)
                {
                    ans[i, j] = BorderColor;
                }
            }

            for(int i = 1; i < Width - 1; i++)
            {
                for(int j = 1; j < Height - 1; j++)
                {
                    ans[i, j] = BackgroundColor;
                }
            }

            return ans;
        }
    }
    public class Triangle : Figure
    {
        public override int x { get; set; }
        public override int y { get; set; }
        public override int Width { get; set; }
        public override int Height { get; set; }
        public override char BorderColor { get; set; }
        public override char BackgroundColor { get; set; }
        public override int Layer { get; set; }
        public override string Name { get; set; }
        public (int, int, int, int, int, int) Cords { get; set; }

        public Triangle()
        {
            x = 0;
            y = 0;
            Width = 0;
            Height = 0;
            Cords = (0, 0, 0, 0, 0, 0);
            BorderColor = ' ';
            BackgroundColor = ' ';
            Layer = 0;
            Name = "";
        }

        public Triangle((int, int, int, int, int, int) cords, char borderColor, char backgroundColor, int layer, string name)
        {
            this.x = Math.Min(cords.Item1, Math.Min(cords.Item3, cords.Item5));
            this.y = Math.Min(cords.Item2, Math.Min(cords.Item4, cords.Item6));
            Width = Math.Max(cords.Item1, Math.Max(cords.Item3, cords.Item5)) - x + 1;
            Height = Math.Max(cords.Item2, Math.Max(cords.Item4, cords.Item6)) - y + 1;
            Cords = cords;
            BorderColor = borderColor;
            BackgroundColor = backgroundColor;
            Layer = layer;
            Name = name;
        }

        public override Figure DeepClone()
        {
            return new Triangle
            {
                x = this.x,
                y = this.y,
                Width = this.Width,
                Height = this.Height,
                BackgroundColor = this.BackgroundColor,
                BorderColor = this.BorderColor,
                Layer = this.Layer,
                Name = this.Name,
                Cords = this.Cords
            };
        }

        static List<(int, int)> GetLine(int x1, int y1, int x2, int y2)
        {
            var ans = new List<(int, int)>();

            var steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);
            if (steep)
            {
                int temp = x1; x1 = y1; y1 = temp;
                temp = x2; x2 = y2; y2 = temp;
            }

            bool switched = false;
            if (x1 > x2)
            {
                switched = true;
                int temp = x1; x1 = x2; x2 = temp;
                temp = y1; y1 = y2; y2 = temp;
            }

            int ystep;
            if (y1 < y2)
                ystep = 1;
            else
                ystep = -1;

            int deltax = x2 - x1, deltay = Math.Abs(y2 - y1), error = -deltax / 2, y = y1;

            for (int x = x1; (x2 > x1 ? x <= x2 : x >= x2); x += x2 > x1 ? 1 : -1)
            {
                if (steep)
                    ans.Add((y, x));
                else
                    ans.Add((x, y));

                error += deltay;
                if (error > 0)
                {
                    y += ystep;
                    error -= deltax;
                }
            }

            if (switched)
                ans.Reverse();

            return ans;
        }

        private int[,] GetBorder()
        {
            int x1 = Cords.Item1, x2 = Cords.Item3, x3 = Cords.Item5,
                y1 = Cords.Item2, y2 = Cords.Item4, y3 = Cords.Item6;

            if (x1 < 0 || x2 < 0 || x3 < 0 || y1 < 0 || y2 < 0 || y3 < 0)
                throw new Exception("Triangle negative point coordinates");

            int minx = Math.Min(x1, Math.Min(x2, x3)), miny = Math.Min(y1, Math.Min(y2, y3));

            x1 -= minx; x2 -= minx; x3 -= minx;
            y1 -= miny; y2 -= miny; y3 -= miny;

            var ans = new int[Math.Abs(Math.Min(x1, Math.Min(x2, x3)) - Math.Max(x1, Math.Max(x2, x3))) + 1,
                              Math.Abs(Math.Min(y1, Math.Min(y2, y3)) - Math.Max(y1, Math.Max(y2, y3))) + 1];

            var line = GetLine(x1, y1, x2, y2);
            foreach (var p in line)
                ans[p.Item1, p.Item2] = 1;

            line = GetLine(x3, y3, x2, y2);
            foreach (var p in line)
                ans[p.Item1, p.Item2] = 1;

            line = GetLine(x1, y1, x3, y3);
            foreach (var p in line)
                ans[p.Item1, p.Item2] = 1;

            return ans;
        }

        public override char[,] GetImage()
        {
            var ansi = GetBorder();

            var ansiw = ansi.GetLength(0);
            var ansih = ansi.GetLength(1);

            if (ansi[0, 0] == 0)
                ansi = Filler.Fill(ansi, 0, 0, 3);
            if (ansi[0, ansih - 1] == 0)
                ansi = Filler.Fill(ansi, 0, ansih - 1, 3);
            if (ansi[ansiw - 1, 0] == 0)
                ansi = Filler.Fill(ansi, ansiw - 1, 0, 3);
            if (ansi[ansiw - 1, ansih - 1] == 0)
                ansi = Filler.Fill(ansi, ansiw - 1, ansih - 1, 3);

            var ans = new char[ansiw, ansih];

            for(int i = 0; i < ansiw; i++)
            {
                for(int j = 0; j < ansih; j++)
                {
                    if (ansi[i, j] == 1)
                        ans[i, j] = BorderColor;
                    else if (ansi[i, j] == 0)
                        ans[i, j] = BackgroundColor;
                    else
                        ans[i, j] = '\n';
                }
            }

            return ans;
        }
    }

    [XmlInclude(typeof(Ellipse))]
    [XmlInclude(typeof(Rectangle))]
    [XmlInclude(typeof(Triangle))]
    public class Canvas
    {
        public int Width { get; set; }
        public int Height { get; set; }
        char[,] Image { get; set; }
        public char BackgroundColor { get; set; }
        public List<Figure> Figures { get; set; }

        public Canvas DeepClone()
        {
            var clonedFigures = new List<Figure>();
            foreach(var f in this.Figures)
            {
                clonedFigures.Add(f.DeepClone());
            }

            var clonedImage = new char[this.Height, this.Width];
            for(int i = 0; i < Height; i++)
            {
                for(int j = 0; j < Width; j++)
                {
                    clonedImage[i, j] = this.Image[i, j];
                }
            }

            return new Canvas
            {
                Width = this.Width,
                Height = this.Height,
                Image = clonedImage,
                BackgroundColor = this.BackgroundColor,
                Figures = clonedFigures
            };
        }
        public Canvas()
        {
            Width = 1;
            Height = 1;
            BackgroundColor = ' ';
            Image = SetImage(BackgroundColor);
            Figures = new List<Figure>();
        }

        public Canvas(int w, int h, char BackgroundColor)
        {
            Width = w;
            Height = h;
            this.BackgroundColor = BackgroundColor;
            Image = SetImage(BackgroundColor);
            Figures = new List<Figure>();
        }

        public Canvas(int w, int h, char BackgroundColor, List<Figure> figures)
        {
            Width = w;
            Height = h;
            this.BackgroundColor = BackgroundColor;
            Figures = figures;

            Image = SetImage(figures, BackgroundColor);
        }

        public string Save(string filepath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Canvas));
                using (FileStream fs = new FileStream(filepath, FileMode.Create))
                {
                    serializer.Serialize(fs, this);
                }

                return "";
            }
            catch(Exception ex)
            {
                return "Error: Can not create file";
            }
        }

        public string Load(string filepath)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Canvas));
                using(FileStream fs = new FileStream(filepath, FileMode.Open))
                {
                    Canvas? temp = (Canvas?)serializer.Deserialize(fs);

                    Width = temp.Width;
                    Height = temp.Height;
                    BackgroundColor = temp.BackgroundColor;
                    Figures = temp.Figures;
                    Image = SetImage(Figures, BackgroundColor);
                }

                return "";
            }
            catch(Exception ex)
            {
                return "Error: Can not read file";
            }
        }

        public int w()
        {
            return Width;
        }

        public int h()
        {
            return Height;
        }

        public char[,] img()
        {
            return Image;
        }

        public void Update(List<Figure> figures)
        {
            Figures = figures;
            Image = SetImage(figures, BackgroundColor);
        }

        public void Update(int w, int h)
        {
            Width = w;
            Height = h;

            if (Figures == null)
                Image = SetImage(BackgroundColor);
            else
                Image = SetImage(Figures, BackgroundColor);
        }

        public void Update(char backgroundcolor)
        {
            BackgroundColor = backgroundcolor;

            if (Figures == null)
                Image = SetImage(BackgroundColor);
            else
                Image = SetImage(Figures, BackgroundColor);
        }

        private char[,] SetImage(char BackgroundColor)
        {
            var ans = new char[Height, Width];

            for(int i = 0; i < Height; i++)
            {
                for(int j = 0; j < Width; j++)
                {
                    ans[i, j] = BackgroundColor;
                }
            }

            return ans;
        }

        private char[,] SetImage(List<Figure> figuress, char BackgroundColor)
        {
            var ans = SetImage(BackgroundColor);

            var figures = figuress.OrderBy(f => f.Layer).ToList();

            foreach(var f in figures)
            {
                var image = f.GetImage();

                int ew = image.GetLength(0);
                int eh = image.GetLength(1);
                int ex = f.x;
                int ey = f.y;
                int dx, dy;

                for (int i = 0; i < eh; i++)
                {
                    if (ey + i >= Height)
                        break;
                    for (int j = 0; j < ew; j++)
                    {
                        if (ex + j >= Width)
                            break;
                        dx = j + ex; dy = i + ey;
                        if (image[j, i] == '\n')
                            continue;
                        else
                            ans[dy, dx] = image[j, i];
                    }
                }
            }

            return ans;
        }
    }
}
