using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ResumeAnalyzerConsole
{
    public class Resume
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string City { get; set; }
        public int YearsExperience { get; set; }
        public decimal SalaryRequirement { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public override string ToString() =>
            $"{FullName}, {Age} р., {City}, досвід: {YearsExperience}, зарплата: {SalaryRequirement:N0} грн";
    }

    class Program
    {
        private static readonly List<Resume> Resumes = new();

        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Аналізатор резюме (PLINQ + паралельне завантаження)");
            Console.ResetColor();

            while (true)
            {
                Console.WriteLine("\nМеню:");
                Console.WriteLine("1 - Завантажити одне резюме");
                Console.WriteLine("2 - Завантажити кілька файлів");
                Console.WriteLine("3 - Завантажити всі .txt з папки (паралельно)");
                Console.WriteLine("4 - Показати звіти (PLINQ)");
                Console.WriteLine("5 - Вихід");
                Console.Write("Вибір: ");

                switch (Console.ReadLine())
                {
                    case "1": LoadSingle(); break;
                    case "2": LoadMultiple(); break;
                    case "3": LoadFromFolderParallel(); break;
                    case "4": GenerateReportsWithPlinq(); break;
                    case "5": return;
                    default: Console.WriteLine("Невірно!"); break;
                }
            }
        }

        static void LoadSingle()
        {
            Console.Write("Шлях до файлу: ");
            string path = Console.ReadLine().Trim('"');
            if (File.Exists(path))
            {
                Resumes.Clear();
                Resumes.Add(ParseResume(path));
                Console.WriteLine("Завантажено 1 резюме.");
            }
            else Console.WriteLine("Файл не знайдено.");
        }

        static void LoadMultiple()
        {
            Console.WriteLine("Введіть шляхи через пробіл або кому:");
            var paths = Console.ReadLine()
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim('"'))
                .Where(File.Exists);

            Resumes.Clear();
            Resumes.AddRange(paths.Select(ParseResume));
            Console.WriteLine($"Завантажено {Resumes.Count} резюме.");
        }

        static void LoadFromFolderParallel()
        {
            Console.Write("Шлях до папки: ");
            string folder = Console.ReadLine().Trim('"');

            if (!Directory.Exists(folder))
            {
                Console.WriteLine("Папка не існує!");
                return;
            }

            var files = Directory.GetFiles(folder, "*.txt");
            if (!files.Any())
            {
                Console.WriteLine("Немає .txt файлів.");
                return;
            }

            Console.WriteLine($"Завантаження {files.Length} файлів на всіх ядрах...");

            var bag = new ConcurrentBag<Resume>();

            Parallel.ForEach(files, file =>
            {
                var resume = ParseResume(file);
                if (resume != null)
                    bag.Add(resume);
            });

            Resumes.Clear();
            Resumes.AddRange(bag);

            Console.WriteLine($"Успішно завантажено {Resumes.Count} резюме.");
        }

        static Resume ParseResume(string path)
        {
            try
            {
                var resume = new Resume();
                foreach (var line in File.ReadLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length < 2) continue;

                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    switch (key)
                    {
                        case "FirstName": resume.FirstName = value; break;
                        case "LastName": resume.LastName = value; break;
                        case "Age": resume.Age = int.Parse(value); break;
                        case "City": resume.City = value; break;
                        case "YearsExperience": resume.YearsExperience = int.Parse(value); break;
                        case "SalaryRequirement": resume.SalaryRequirement = decimal.Parse(value); break;
                    }
                }
                return resume.FirstName != null ? resume : null;
            }
            catch
            {
                Console.WriteLine($"Помилка у файлі: {Path.GetFileName(path)}");
                return null;
            }
        }

        static void GenerateReportsWithPlinq()
        {
            if (!Resumes.Any())
            {
                Console.WriteLine("Немає даних!");
                return;
            }

            Console.WriteLine("\nГенерація звітів\n");

            var mostExp = Resumes.AsParallel().OrderByDescending(r => r.YearsExperience).First();
            var leastExp = Resumes.AsParallel().OrderBy(r => r.YearsExperience).First();
            var lowestSalary = Resumes.AsParallel().OrderBy(r => r.SalaryRequirement).First();
            var highestSalary = Resumes.AsParallel().OrderByDescending(r => r.SalaryRequirement).First();

            var byCity = Resumes.AsParallel()
                .GroupBy(r => r.City)
                .Where(g => g.Count() > 1)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key);

            Console.WriteLine($"Найдосвідченіший: {mostExp} ({mostExp.YearsExperience} років)");
            Console.WriteLine($"Найменш досвідчений: {leastExp} ({leastExp.YearsExperience} років)");
            Console.WriteLine($"Найнижча зарплата: {lowestSalary} ({lowestSalary.SalaryRequirement:N0} грн)");
            Console.WriteLine($"Найвища зарплата: {highestSalary} ({highestSalary.SalaryRequirement:N0} грн)");

            Console.WriteLine("\nКандидати з одного міста:");
            if (byCity.Any())
                foreach (var g in byCity)
                    Console.WriteLine($"  {g.Key}: {string.Join(", ", g.Select(r => r.FullName))} ({g.Count()} чол.)");
            else
                Console.WriteLine("  Усі з різних міст.");

            Console.WriteLine($"\nВсього проаналізовано резюме: {Resumes.Count}");
        }
    }
}