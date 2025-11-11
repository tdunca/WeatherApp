using CvsHelper;
using CvsHelper.Configurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using WeatherAppLab3.DataAccess;
using WeatherAppLab3.Models;


namespace [WeatherAppLab3]
{
    public class Program
{
    public static void Main(string[] args)
    {
        var filePath = @"";

        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {filePath was not found. Check it and try again.");
                }


                var weatherData = ReadCsv(filePath);

                ExecuteWithDbContext(dbContext =>
            {
                dbContext.Database.EnsureDeleted();
                dbContext.Database.EnsureCreated();

                if (!dbContext.WeatherData.Any())
                {
                    dbContext.WeatherData.AddRange(weatherData);
                    dbContext.SaveChanges();
                    Console.WriteLine("Database has been filled.");
                }
                Console.WriteLine($"Total amount of rows: {dbContext.WeatherData.Count()}");
            });

                ShowMenu();

            }
                catch (Exception ex)
        {
            Console.WriteLine($"An error occured: {ex.Message}");
        }
    }

    private static void PauseBeforeReturning()
    {
        Console.WriteLine("Press any key to return to the menu...");
        Console.ReadKey();
    }

    public static void ShowMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("Pick and option:");
            Console.WriteLine("1. Outside: Display averege temperature for a date");
            Console.WriteLine("2. Outside: Sort the hottest days");
            Console.WriteLine("3. Outside: Sort the dryest days");
            Console.WriteLine("4. Display meterological autumn and winter");
            Console.WriteLine("5. Outside: Display days with the greatest risk for mold");
            Console.WriteLine("6. Inside: Show averege temperature for a date");
            Console.WriteLine("7. Inside: Sort the hottest days");
            Console.WriteLine("8. Inside: Sort the dryest days");
            Console.WriteLine("9. Inside: Display days with the greatest risk for mold");
            Console.WriteLine("10. Exit");

            var choice = GetValidatedInput("Choose an option (1-10): ", 1, 10);

            Console.Clear();

            switch (choice)
            {
                case 1:
                    ShowAveregeTemperature("");
                    break;
                case 1:
                    ShowAveregeTemperature("");
                    break;
                case 1:
                    ShowAveregeTemperature("");
                    break;
                case 1:
                    ShowAveregeTemperature("");
                    break;
                case 1:
                    ShowAveregeTemperature("");
                    break;
                case 1:
                    ShowAveregeTemperature("");
                    break;
                case 1:
                    ShowAveregeTemperature("");
                    break;
                case 1:
                    ShowAveregeTemperature("");
                    break;
                case 1:
                    ShowAveregeTemperature("");
                    break;
                case 10:
                    Console.WriteLine("Exiting program...");
                    Environment.Exit(0);
                    break;

            }
            PauseBeforeReturning();
        }
        }
    private static int GetValidatedInput(string prompt, int min, int max)
    {
        int result;
        do
        {
            Console.Write(prompt);
        } while (!int.TryParse(Console.ReadLine(), out result) || result < min || result > max);
        return result;
    }


    }
}