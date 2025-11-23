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
        var indexesOfRising = new List<int>();
        var indexesOfPositive = new List<int>();

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

        indexesOfRising.Add(0);
        for (int i = 1; i < numbers.Count(); i++)
        {
            if( numbers[i] <= numbers[i - 1])
            {
                indexesOfRising.Add(i);
            } 
        }
        indexesOfRising.Add(numbers.Count());

        var maxLengthOfRising = indexesOfRising.AsParallel()
        .Zip(indexesOfRising.Skip(1), (start, end) => end - start)
        .Max();

        Console.WriteLine($"Довжина найдовшого зростаючого підмасива {maxLengthOfRising}");

        indexesOfPositive.Add(0);
        for (int i = 1; i < numbers.Count(); i++)
        {
            if (numbers[i] < 0)
            {
                indexesOfPositive.Add(i);
            }
        }
        indexesOfPositive.Add(numbers.Count());

        var maxLengthOfPositive = indexesOfPositive.AsParallel()
        .Zip(indexesOfPositive.Skip(1), (start, end) => end - start)
        .Max();

        Console.WriteLine($"Довжина найдовшого додатнього підмасива {maxLengthOfPositive}");
    }
}