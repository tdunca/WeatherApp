using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using WeatherAppLab3.DataAccess;
using WeatherAppLab3.Models;


namespace WeatherAppLab3
{
    public class Program
{
    public static void Main(string[] args)
    {
        var filePath = @""; //insert csvfile

        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {filePath} was not found. Check path and try again.");
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
                    ShowAveregeTemperature("Outside");
                    break;
                case 2:
                    ShowSortedTemperatures("Outside");
                    break;
                case 3:
                    ShowSortedHumidity("Outside");
                    break;
                case 4:
                    ShowMeterologicalSeasons();
                    break;
                case 5:
                    ShowMoldRisk("Outside");
                    break;
                case 6:
                    ShowAveregeTemperature("Inside");
                    break;
                case 7:
                    ShowSortedTemperatures("Inside");
                    break;
                case 8:
                    ShowSortedHumidity("Inside");
                    break;
                case 9:
                    ShowMoldRisk("Inside");
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

    private static DateTime GetValidatedDate(string prompt)
    {
        DateTime date;
        do
        {
            Console.Write(prompt);
        } while (!DateTime.TryParse(Console.ReadLine(), out date));
        return (date);
    }

    private static List<WeatherRecord> ReadCsv(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var cvs = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ","
        });

        cvs.Context.RegisterClassMap<WeatherDataMap>();
        return cvs.GetRecords<WeatherRecord>().ToList();
    }
    private static void ExecuteWithDbContext(Action<WeatherDBContext> action)
    {
        using var dbContext = new WeatherDBContext();
        action(dbContext);
    }

    public static void ShowAveregeTemperature(string location)
    {
        var dateInput = GetValidatedDate("Enter a date (YYYY-MM-DD): ");

        ExecuteWithDbContext(dbContext =>
        {
            var avgTemp = dbContext.WeatherData
                .Where(w => w.Date.Date == dateInput.Date && w.Location == location)
                .Average(w => w.Temperature);

            Console.WriteLine($"Average temperature for {location} on {dateInput:yyyy-MM-dd} is {avgTemp:F2}°C");
        });

    }

    public static void ShowSortedTemperatures(string location)
    {
        var count = GetValidatedInput("How many days would you like to view?", 1, int.MaxValue);

        ExecuteWithDbContext(dbContext =>
        {
            var sortedDays = dbContext.WeatherData
            .Where(w => w.Location == location)
            .GroupBy(w => w.Date.Date)
            .Select(g => new { Date = g.Key, AvgTemp = g.Average(w => w.Temperature) })
            .OrderByDescending(d => d.AvgTemp)
            .Take(count);

            foreach (var day in sortedDays)
            {
                Console.WriteLine($"{day.Date:yyyy-MM-dd}: {day.AvgTemp:F2}°C");
            }
        });

    }
    public static void ShowSortedHumidity(string location)
    {
        var count = GetValidatedInput("How many days would you like to view?", 1, int.MaxValue);
        ExecuteWithDbContext(dbContext =>
        {
            var sortedDays = dbContext.WeatherData
            .Where(w => w.Location == location)
            .GroupBy(w => w.Date.Date)
            .Select(g => new { Date = g.Key, AvgHumidity = g.Average(w => w.Humidity) })
            .OrderBy(d => d.AvgHumidity)
            .Take(count);
           
            foreach (var day in sortedDays)
            {
                Console.WriteLine($"{day.Date:yyyy-MM-dd}: {day.AvgHumidity:F2}%");
            }
        });
    }

    public static void ShowMeterologicalSeasons()
    {
        ExecuteWithDbContext(dbContext =>
        {
            var autumnStart = dbContext.WeatherData
            .Where(w => w.Location == "Outside")
            .GroupBy(w => w.Date.Date)
            .Where(g => g.Average(w => w.Temperature) < 10)
            .Select(g => g.Key)
            .OrderBy(date => date)
            .FirstOrDefault();

            var winterStart = dbContext.WeatherData
            .Where(w => w.Location == "Outside")
            .GroupBy(w => w.Date.Date)
            .Where(g => g.Average(w => w.Temperature) < 0)
            .Select(g => g.Key)
            .OrderBy(date => date)
            .FirstOrDefault();

            Console.WriteLine($"Meterological autumn begins: {autumnStart:yyyy-MM-dd}");
            Console.WriteLine($"Meterological winter begins: {winterStart:yyyy-MM-dd}");
        });
    }
    public static void ShowMoldRisk(string location)
    {
        var count = GetValidatedInput("How many days would you like to show?", 1, int.MaxValue);

        ExecuteWithDbContext(dbContext =>
        {
            var data = dbContext.WeatherData
            .Where(w => w.Location == location)
            .AsEnumerable()
            .GroupBy(w => w.Date.Date)
            .Select(g => new { Date = g.Key, AvgMoldRisk = g.Average(w => w.MoldRisk) })
            .OrderByDescending(d => d.AvgMoldRisk)
            .Take(count);

            Console.WriteLine($"Days with highest risk for mold ({location}): ");
            foreach (var day in data)
            {
                Console.WriteLine($"{day.Date:yyyy-MM-dd}: {day.AvgMoldRisk:F2}");
            }
        });

        }
    }
}