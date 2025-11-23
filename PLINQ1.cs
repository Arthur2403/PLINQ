using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        string filePath = "test.txt";

        var numbers = new List<int>();

        try
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                if (int.TryParse(line, out int number))
                {
                    numbers.Add(number);
                }
                else
                {
                    Console.WriteLine($"Помилка: Не вдалося перетворити рядок '{line}' на число.");
                }
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"Помилка: Файл '{filePath}' не знайдено.");
        }

        var uniqueCount = numbers
        .AsParallel()
        .Distinct()
        .Count();

        Console.WriteLine($"Кількість унікальних елементів {uniqueCount}");
    }
}