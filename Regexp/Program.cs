using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Regexp
{
    // Класс для поиска текста в файлах, заданных маской
    class Search
    {
        static void Main()
        {
            Console.Write("Введите путь к каталогу: ");
            string Path = Console.ReadLine();
            Console.Write("Введите маску для файлов: ");
            string Mask = Console.ReadLine();
            Console.Write("Введите текст для поиска в файлах: ");
            string Text = Console.ReadLine();

            // Дописываем слэш (в случае его отсутствия)
            if (Path[Path.Length - 1] != '\\')
                Path += '\\';

            // Создание объекта на основе введенного пути
            DirectoryInfo di = new DirectoryInfo(Path);
            // Если путь не существует
            if (!di.Exists)
            {
                Console.WriteLine("Некорректный путь!!!");
                return;
            }

            // Преобразуем введенную маску для файлов в регулярное выражение

            // Заменяем . на \.
            Mask = Mask.Replace(".", @"\." /* (".", "\\.") */);
            // Заменяем ? на .
            Mask = Mask.Replace("?", ".");
            // Заменяем * на .*
            Mask = Mask.Replace("*", ".*");
            // Указываем, что требуется найти точное соответствие маске
            Mask = "^" + Mask + "$";

            // Создание объекта регулярного выражения на основе маски
            Regex regMask = new Regex(Mask, RegexOptions.IgnoreCase);

            // Экранируем спецсимволы во введенном тексте
            Text = Regex.Escape(Text);
            // Создание объекта регулярного выражения на основе текста
            Regex regText = new Regex(Text, RegexOptions.IgnoreCase);

            // Вызываем функцию поиска
            ulong Count = FindTextInFiles(di, regText, regMask);
            Console.WriteLine("Всего обработано файлов: {0}.", Count);
        }

        // Функция поиска
        static ulong FindTextInFiles(DirectoryInfo di, Regex regText, Regex regMask)
        {
            // Поток для чтения из файла
            StreamReader sr = null;
            // Список найденных совпадений
            MatchCollection mc = null;

            // Количество обработанных файлов
            ulong CountOfMatchFiles = 0;

            FileInfo[] fi = null;
            try
            {
                fi = di.GetFiles();// Получаем список файлов
            }
            catch
            {
                return CountOfMatchFiles;
            }

            // Перебираем список файлов
            foreach (FileInfo f in fi)
            {
                // Если файл соответствует маске
                if (regMask.IsMatch(f.Name))
                {
                    // Увеличиваем счетчик
                    ++CountOfMatchFiles;
                    Console.WriteLine("File " + f.Name + ":");

                    // Открываем файл
                    sr = new StreamReader(di.FullName + @"\" + f.Name,
                        Encoding.Default);
                    // Считываем целиком
                    string Content = sr.ReadToEnd();
                    // Закрываем файл
                    sr.Close();
                    // Ищем заданный текст
                    mc = regText.Matches(Content);
                    // Перебираем список вхождений
                    foreach (Match m in mc)
                        Console.WriteLine("Текст найден в позиции: {0}.", m.Index);

                    if (mc.Count == 0)
                        Console.WriteLine("В данном файле запрошенный текст не найден.");
                }
            }

            Console.WriteLine("Количество обработанных файлов в каталоге {0}: {1}", di.FullName, CountOfMatchFiles);

            // Получаем список подкаталогов
            DirectoryInfo[] diSub = di.GetDirectories();
            // Для каждого из них вызываем (рекурсивно) эту же функцию поиска
            foreach (DirectoryInfo diSubDir in diSub)
                CountOfMatchFiles += FindTextInFiles(diSubDir, regText, regMask);

            // Возврат количества обработанных файлов
            return CountOfMatchFiles;
        }
    }
}
