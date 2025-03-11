/*
 * Нарисовать фигуру с настраиваемыми параметрами
 * Стереть объект
 * Переместить объект
 * Добавить фон к рисунку
 * Сохранить холст в виде файла
 * Загрузить из файла
 * Отменить/повторить действие
 * Классы: фигура
 * █▓▒░ 
*/

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Droweroid;

/*
 - draw <figure> <Name> <x> <y> <w> <h> <BorderColor> <BackgroundColor> <Layer>
 - clear
 - save <filepath>
 - load <filepath>
 - canvas color <Color>
 - canvas size <w> <h>
 - delete <Name>
 - figure <Name> pos <x> <y>
 - figure <Name> color <BorderColor> <BackgroundColor>
 - figure <Name> layer <Layer>
 - figure <Name> size <w> <h>
 - figure <Name>
 - figure names
 - undo
 - redo 
 - exit
 - help
 - help <command>
 */
namespace Program
{
    class Program
    {
        static void Main()
        {
            Drawer.Proga = true;
            Drawer.Canvas = new Canvas(Console.WindowWidth, Console.WindowHeight, ' ');

            Drawer.SaverList = new List<Canvas>();
            Drawer.SaverList.Add(Drawer.Canvas.DeepClone());
            Drawer.SaverPointer = 0;

            Drawer.ReDraw(Drawer.Canvas);
            while (Drawer.Proga)
            {
                Drawer.Comand = Console.ReadLine();
                Drawer.DoComand(Drawer.Comand);
            }
        }
    }
}


