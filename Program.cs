using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Localization
{
    internal class Program
    {
        // E:\Projects\ZabotaAnswers\Zabota-Answers
        static Regex regex1 = new Regex(@"\W(ToolTip=""[^""\\{]*(?:\\.[^""\\]*)*""|Content=""[^""\\{]*(?:\\.[^""\\]*)*""|Header=""[^""\\{]*(?:\\.[^""\\]*)*""|Text=""[^""\\{]*(?:\\.[^""\\]*)*"")\W", RegexOptions.Compiled);
        static Regex regex2 = new Regex("\"([^\"]*)\"", RegexOptions.Compiled);
        static Dictionary<string, string> dict1 = new Dictionary<string, string>();
        static Dictionary<string, string> dict2 = new Dictionary<string, string>();


        static void Main(string[] args)
        {
            ReadFromFile();
            WalkDirectoryTree(new DirectoryInfo(Environment.GetCommandLineArgs()[1]), "xaml");
            //WalkDirectoryTree(new DirectoryInfo(Environment.GetCommandLineArgs()[1]), "xaml", true);
            //string keys = "";
            //List<string> Branches = new List<string>();
            foreach (var item in dict2.Keys)
            {
                Console.WriteLine(item);
                //Branches.Add(item);
                //keys += item + Environment.NewLine;
            }
        }
        static void WalkDirectoryTree(DirectoryInfo root, string ext, bool insert = false)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            // Сначала обработайте все файлы непосредственно в этой папке
            try
            {
                files = root.GetFiles($"*.{ext}");
                foreach (var file in files)
                {
                    if (!insert)
                        ReadKeysFromFile(file);
                    else
                        InsertKeysToFile(file);
                }
            }
            // Это происходит, если хотя бы для одного из файлов требуются
            // разрешения, превышающие предоставляемые приложением.
            catch (UnauthorizedAccessException e)
            {
                // Этот код просто записывает сообщение и продолжает рекурсию.
                // Возможно, вы решите сделать здесь что-то другое. Например,
                // вы можете попытаться повысить свои привилегии и снова получить доступ к файлу.
                //log.Add(e.Message);
            }

            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (FileInfo fi in files)
                {
                    // В этом примере мы получаем доступ только к существующему
                    // объекту FileInfo. Если мы хотим открыть, удалить или изменить файл,
                    // то здесь требуется блок try-catch для обработки случая, когда файл
                    // был удален с момента вызова функции TraverseTree().
                    Console.WriteLine(fi.FullName);
                }

                // Теперь найдите все подкаталоги в этом каталоге.
                subDirs = root.GetDirectories();

                foreach (DirectoryInfo dirInfo in subDirs)
                {
                    // Рекурсивный вызов для каждого подкаталога.
                    WalkDirectoryTree(dirInfo, ext, insert);
                }
            }
        }
        public static void ReadFromFile()
        {
            using (TextFieldParser parser = new TextFieldParser(@"g:\11.csv"))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                bool first = true;
                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    if (first)
                    {
                        first = false;
                        continue;
                    }
                    //if (fields[2] == "Пришло отзывов по сравнению предыдущим аналогичным периодом")
                    //{ var t = 1; }

                    if (!dict1.ContainsKey(fields[2]) && fields[0] != "")
                    {
                        dict1.Add(fields[2], fields[0]);
                    }
                }
            }
        }
        public static void ReadKeysFromFile(FileInfo file)
        {
            string text = File.ReadAllText(file.FullName);

            MatchCollection matches = regex1.Matches(text);

            foreach(Match match in matches)
                if (Regex.IsMatch(regex2.Matches(match.Value)[0].Value, @"\p{IsCyrillic}"))
                {
                    string word = regex2.Matches(match.Value)[0].Value.Trim('"');
                    //if (word == "Пришло отзывов по сравнению предыдущим аналогичным периодом")
                    //    { var t = 1; }
                    if (!dict1.ContainsKey(word) && !dict2.ContainsKey(word))
                        dict2.Add(word, ""); 
                }
        }
        public static void InsertKeysToFile(FileInfo file)
        {
            string text = File.ReadAllText(file.FullName);
            foreach (var item in dict1)
            {
                //if (item.Key == "Промоутеров за выбранный период")
                //    if (text.Contains($"\"{item.Key}\""))
                //        { var t = 1; }
                text = text.Replace($"\"{item.Key}\"", $"\"{{Loc {item.Value}}}\"");
                //if (text.Contains($"\"{item.Key}\""))
                //    { var t = 1; }
            }
            File.WriteAllText(file.FullName, text);
        }
    }
}
