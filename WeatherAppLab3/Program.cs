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
                    Console.WriteLine("Databasen har fyllts med data.");
                }
                Console.WriteLine($"Totalt antal rader: {dbContext.WeatherData.Count()}");
            });

                ShowMenu();

            }
                catch (Exception ex)
        {
            Console.WriteLine($"An error occured: {ex.Message}");
        }
    }

    private static void NAMEHERE()
    }
}