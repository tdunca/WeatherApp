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
        private const string OutsideLocation = "Ute";
        private const string InsideLocation = "Inne";

        public static void Main(string[] args)
        {
            // Absolut sökväg till CSV-filen som innehåller väderdata
            var filePath = @"C:\Users\there\Desktop\TempFuktData.csv";

            try
            {
                InitializeDatabase(filePath);

                RunApplication();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Ett oväntat fel uppstod i applikationen:");
                Console.WriteLine(ex.ToString());
                Console.WriteLine();
                Console.WriteLine("Tryck på valfri tangent för att avsluta...");
                Console.ReadKey();
            }
        }


        // Läser CSV-fil, skapar databasen (Code First) och fyller den med data om den är tom.
        private static void InitializeDatabase(string filePath)
        {
            Console.WriteLine("Startar WeatherAppLab3...");
            Console.WriteLine($"Försöker läsa CSV från: {filePath}");
            Console.WriteLine();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("CSV-fil kunde inte hittas via den angivna sökvägen.");
                Console.WriteLine("Kontrollera sökvägen i Program.Main och försök igen.");
                return;
            }

            // Läser alla rader från CSV och mappar dem till WeatherRecord-objekt via CsvHelper
            var weatherData = ReadCsv(filePath);
            Console.WriteLine($"CSV laddad. Antal rader: {weatherData.Count}");

            ExecuteWithDbContext(dbContext =>
            {
                Console.WriteLine("Säkerställer att databas existerar (EnsureCreated)...");
                // Skapar databasen och tabellen om de inte finns
                dbContext.Database.EnsureCreated();

                // Om tabellen är tom, fyll den med CSV-data
                if (!dbContext.WeatherData.Any())
                {
                    Console.WriteLine("Databasen är tom – fyller med CSV-data...");
                    dbContext.WeatherData.AddRange(weatherData);
                    dbContext.SaveChanges();
                    Console.WriteLine("Databasen har blivit fylld från CSV.");
                }
                else
                {
                    Console.WriteLine("Databasen innehåller redan data – hoppar över import.");
                }

                Console.WriteLine($"Rader i databasen: {dbContext.WeatherData.Count()}");
            });

            Console.WriteLine();
            Console.WriteLine("Initial setup klar.");
            Console.WriteLine();
        }

        private static void RunApplication()
        {
            Console.WriteLine("Startar meny...");
            PauseBeforeReturning();
            ShowMenu();
        }

        private static void PauseBeforeReturning()
        {
            Console.WriteLine("Tryck på valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        public static void ShowMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Välj ett alternativ:");
                Console.WriteLine($" 1. {OutsideLocation}: Visa medeltemperatur för valt datum");
                Console.WriteLine($" 2. {OutsideLocation}: Sortera varmaste dagarna (medeltemperatur per dag)");
                Console.WriteLine($" 3. {OutsideLocation}: Sortera torraste till fuktigaste dagarna");
                Console.WriteLine(" 4. Visa meteorologisk höst och vinter (utomhus)");
                Console.WriteLine($" 5. {OutsideLocation}: Sortera dagar efter minst till störst risk för mögel");
                Console.WriteLine($" 6. {InsideLocation}: Visa medeltemperatur för valt datum");
                Console.WriteLine($" 7. {InsideLocation}: Sortera varmaste dagarna (medeltemperatur per dag)");
                Console.WriteLine($" 8. {InsideLocation}: Sortera torraste till fuktigaste dagarna");
                Console.WriteLine($" 9. {InsideLocation}: Sortera dagar efter minst till störst risk för mögel");
                Console.WriteLine("10. Sortera dagar efter uppskattad tid balkongdörren varit öppen");
                Console.WriteLine("11. Sortera dagar där inne- och utetemperatur skiljt sig minst");
                Console.WriteLine("12. Sortera dagar där inne- och utetemperatur skiljt sig mest");
                Console.WriteLine("13. Avsluta");

                var choice = GetValidatedInput("Välj ett alternativ (1-13): ", 1, 13);

                Console.Clear();

                switch (choice)
                {
                    case 1:
                        ShowAverageTemperature(OutsideLocation);
                        break;
                    case 2:
                        ShowSortedTemperatures(OutsideLocation);
                        break;
                    case 3:
                        ShowSortedHumidity(OutsideLocation);
                        break;
                    case 4:
                        ShowMeteorologicalSeasons();
                        break;
                    case 5:
                        ShowMoldRisk(OutsideLocation);
                        break;
                    case 6:
                        ShowAverageTemperature(InsideLocation);
                        break;
                    case 7:
                        ShowSortedTemperatures(InsideLocation);
                        break;
                    case 8:
                        ShowSortedHumidity(InsideLocation);
                        break;
                    case 9:
                        ShowMoldRisk(InsideLocation);
                        break;
                    case 10:
                        ShowEstimatedBalconyDoorOpenTime();
                        break;
                    case 11:
                        ShowTemperatureDifferenceDays(smallestDifferenceFirst: true);
                        break;
                    case 12:
                        ShowTemperatureDifferenceDays(smallestDifferenceFirst: false);
                        break;
                    case 13:
                        Console.WriteLine("Avslutar program...");
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
            var format = "yyyy-MM-dd";

            // Kräver specifikt datumformat för att undvika kulturberoende tolkningar
            while (true)
            {
                Console.Write($"{prompt} ({format}): ");
                var input = Console.ReadLine();

                if (DateTime.TryParseExact(
                        input,
                        format,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out date))
                {
                    return date;
                }

                Console.WriteLine("Ogiltigt datum-format. Försök igen.");
            }
        }

        private static List<WeatherRecord> ReadCsv(string filePath)
        {
            using var reader = new StreamReader(filePath);
            using var cvs = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ","
            });

            // Kopplar CSV-kolumnerna till WeatherRecord via WeatherDataMap
            cvs.Context.RegisterClassMap<WeatherDataMap>();
            return cvs.GetRecords<WeatherRecord>().ToList();
        }

        private static void ExecuteWithDbContext(Action<WeatherDBContext> action)
        {
            // Skapar en ny DbContext per anrop och ser till att den stängs/disposas korrekt
            using var dbContext = new WeatherDBContext();
            action(dbContext);
        }
        public static void ShowAverageTemperature(string location)
        {
            var dateInput = GetValidatedDate("Ange ett datum");

            ExecuteWithDbContext(dbContext =>
            {
                // Filtrerar på datum + plats + att vi har ett temperaturvärde
                var query = dbContext.WeatherData
                    .Where(w => w.Date.Date == dateInput.Date &&
                                w.Location == location &&
                                w.Temperature.HasValue);

                if (!query.Any())
                {
                    Console.WriteLine($"Inga data hittades för {location} den {dateInput:yyyy-MM-dd}.");
                    return;
                }

                // LINQ beräknar medeltemperatur
                var avgTemp = query.Average(w => w.Temperature!.Value);

                Console.WriteLine($"Medeltemperatur för {location} den {dateInput:yyyy-MM-dd} är {avgTemp:F2}°C");
            });
        }

        public static void ShowSortedTemperatures(string location)
        {
            var count = GetValidatedInput("Hur många dagar vill du visa? ", 1, int.MaxValue);

            ExecuteWithDbContext(dbContext =>
            {
                // Gruppar upp mätningarna per dag och plats och tar medeltemperatur per dag
                var sortedDays = dbContext.WeatherData
                    .Where(w => w.Location == location && w.Temperature.HasValue)
                    .GroupBy(w => w.Date.Date)
                    .Select(g => new { Date = g.Key, AvgTemp = g.Average(w => w.Temperature!.Value) })
                    .OrderByDescending(d => d.AvgTemp) // varmaste först
                    .Take(count)
                    .ToList();

                if (!sortedDays.Any())
                {
                    Console.WriteLine($"Inga data hittades för {location}.");
                    return;
                }

                Console.WriteLine($"Varmaste dagar för {location}:");
                foreach (var day in sortedDays)
                {
                    Console.WriteLine($"{day.Date:yyyy-MM-dd}: {day.AvgTemp:F2}°C");
                }
            });
        }

        public static void ShowSortedHumidity(string location)
        {
            var count = GetValidatedInput("Hur många dagar vill du visa? ", 1, int.MaxValue);

            ExecuteWithDbContext(dbContext =>
            {
                var sortedDays = dbContext.WeatherData
                    .Where(w => w.Location == location && w.Humidity.HasValue)
                    .GroupBy(w => w.Date.Date)
                    .Select(g => new { Date = g.Key, AvgHumidity = g.Average(w => w.Humidity!.Value) })
                    .OrderBy(d => d.AvgHumidity) // torrast först
                    .Take(count)
                    .ToList();

                if (!sortedDays.Any())
                {
                    Console.WriteLine($"Inga data hittades för {location}.");
                    return;
                }

                Console.WriteLine($"Torraste till fuktigaste dagarna för {location}:");
                foreach (var day in sortedDays)
                {
                    Console.WriteLine($"{day.Date:yyyy-MM-dd}: {day.AvgHumidity:F2}%");
                }
            });
        }

        public static void ShowMeteorologicalSeasons()
        {
            ExecuteWithDbContext(dbContext =>
            {
                // Tar bara med utomhusdata ("Ute") med giltig temperatur
                var outsideData = dbContext.WeatherData
                    .Where(w => w.Location == OutsideLocation && w.Temperature.HasValue);

                if (!outsideData.Any())
                {
                    Console.WriteLine($"Inga data hittades för '{OutsideLocation}'.");
                    return;
                }

                // Höst: första dagen där daglig medeltemp < 10°C
                var autumnStart = outsideData
                    .GroupBy(w => w.Date.Date)
                    .Where(g => g.Average(w => w.Temperature!.Value) < 10)
                    .Select(g => g.Key)
                    .OrderBy(date => date)
                    .FirstOrDefault();

                // Vinter: första dagen där daglig medeltemp < 0°C
                var winterStart = outsideData
                    .GroupBy(w => w.Date.Date)
                    .Where(g => g.Average(w => w.Temperature!.Value) < 0)
                    .Select(g => g.Key)
                    .OrderBy(date => date)
                    .FirstOrDefault();

                Console.WriteLine($"Meteorologisk höst börjar: {autumnStart:yyyy-MM-dd}");
                Console.WriteLine($"Meteorologisk vinter börjar: {winterStart:yyyy-MM-dd}");
            });
        }

        public static void ShowMoldRisk(string location)
        {
            var count = GetValidatedInput("Hur många dagar vill du visa? ", 1, int.MaxValue);

            ExecuteWithDbContext(dbContext =>
            {
                // MoldRisk är en beräknad property i WeatherRecord, så vi jobbar mot den direkt
                var data = dbContext.WeatherData
                    .Where(w => w.Location == location)
                    .AsEnumerable() // kör resten i minnet pga beräknad property
                    .GroupBy(w => w.Date.Date)
                    .Select(g => new { Date = g.Key, AvgMoldRisk = g.Average(w => w.MoldRisk) })
                    .OrderBy(d => d.AvgMoldRisk) // minst → störst risk
                    .Take(count)
                    .ToList();

                if (!data.Any())
                {
                    Console.WriteLine($"Inga data hittades för {location}.");
                    return;
                }

                Console.WriteLine($"Dagar sorterade efter minst till störst risk för mögel ({location}):");
                foreach (var day in data)
                {
                    Console.WriteLine($"{day.Date:yyyy-MM-dd}: {day.AvgMoldRisk:F2}");
                }
            });
        }

        /// Uppskatta hur länge balkongdörren varit öppen per dag.
        /// Antagande:
        /// - När balkongdörren öppnas minskar skillnaden mellan inne- och utetemperatur
        ///   tydligt jämfört med dagens genomsnittliga skillnad.
        /// - Vi tolkar "öppna dörr"-tillfällen som mätningar där skillnaden är minst 1 grad
        ///   lägre än dagens medelskillnad.
        public static void ShowEstimatedBalconyDoorOpenTime()
        {
            var count = GetValidatedInput("Hur många dagar vill du visa (längst öppettid först)? ", 1, int.MaxValue);

            ExecuteWithDbContext(dbContext =>
            {
                // 1. Hämta parvisa mätningar där vi har både Inne och Ute vid exakt samma tidpunkt
                var pairs = (from inne in dbContext.WeatherData
                             where inne.Location == InsideLocation && inne.Temperature.HasValue
                             join ute in dbContext.WeatherData
                                on inne.Date equals ute.Date
                             where ute.Location == OutsideLocation && ute.Temperature.HasValue
                             select new
                             {
                                 inne.Date,
                                 inneTemp = inne.Temperature!.Value,
                                 uteTemp = ute.Temperature!.Value
                             })
                             .AsEnumerable()
                             .ToList();

                if (!pairs.Any())
                {
                    Console.WriteLine("Inga parvisa mätningar hittades för Inne/Ute.");
                    return;
                }

                // 2. Grupp per dag och räkna:
                //    - genomsnittlig skillnad inne-ute
                //    - hur många mätningar som har lägre skillnad än (medel - 1°C)
                var perDay = pairs
                    .GroupBy(p => p.Date.Date)
                    .Select(g =>
                    {
                        var diffs = g.Select(x => x.inneTemp - x.uteTemp).ToList();
                        var avgDiff = diffs.Average();

                        // "Öppen dörr"-tillfällen: när skillnaden är synligt lägre än dagens medel
                        var openCount = g.Count(x => (x.inneTemp - x.uteTemp) < avgDiff - 1.0);

                        return new
                        {
                            Date = g.Key,
                            EstimatedOpenEvents = openCount
                        };
                    })
                    // Ta bara med dagar där vi faktiskt hittat minst ett "öppet" tillfälle
                    .Where(d => d.EstimatedOpenEvents > 0)
                    .OrderByDescending(d => d.EstimatedOpenEvents) // längst öppet först
                    .Take(count)
                    .ToList();

                if (!perDay.Any())
                {
                    Console.WriteLine("Inga dagar med uppskattad öppen balkongdörr hittades med den valda modellen.");
                    return;
                }

                Console.WriteLine("Dagar sorterade efter uppskattad tid balkongdörren varit öppen (flest 'öppna'-tillfällen först):");
                foreach (var day in perDay)
                {
                    Console.WriteLine($"{day.Date:yyyy-MM-dd}: uppskattat antal 'öppna dörr'-tillfällen = {day.EstimatedOpenEvents}");
                }
            });
        }

        /// Sorterar dagar där inne- och utetemperaturen skiljt sig minst eller mest.
        /// Vi räknar:
        /// - medeltemperatur per dag för Inne
        /// - medeltemperatur per dag för Ute
        /// - beräknar absolut skillnad mellan dessa.
        public static void ShowTemperatureDifferenceDays(bool smallestDifferenceFirst)
        {
            var count = GetValidatedInput("Hur många dagar vill du visa? ", 1, int.MaxValue);

            ExecuteWithDbContext(dbContext =>
            {
                // 1. Beräkna daglig medeltemperatur per plats (Inne/Ute)
                var daily = dbContext.WeatherData
                    .Where(w => w.Temperature.HasValue &&
                                (w.Location == InsideLocation || w.Location == OutsideLocation))
                    .GroupBy(w => new { Date = w.Date.Date, w.Location })
                    .Select(g => new
                    {
                        Day = g.Key.Date,
                        Location = g.Key.Location,
                        AvgTemp = g.Average(w => w.Temperature!.Value)
                    })
                    .ToList();

                // 2. Para ihop Inne- och Ute-värden per dag
                var joined = (from inne in daily
                              where inne.Location == InsideLocation
                              join ute in daily
                                on inne.Day equals ute.Day
                              where ute.Location == OutsideLocation
                              select new
                              {
                                  Date = inne.Day,
                                  InsideTemp = inne.AvgTemp,
                                  OutsideTemp = ute.AvgTemp,
                                  Diff = Math.Abs(inne.AvgTemp - ute.AvgTemp)
                              })
                              .ToList();

                if (!joined.Any())
                {
                    Console.WriteLine("Inga dagar med både inne- och utetemperatur hittades.");
                    return;
                }

                // 3. Sortera efter skillnad – minst först eller störst först beroende på parameter
                var ordered = (smallestDifferenceFirst
                    ? joined.OrderBy(d => d.Diff)
                    : joined.OrderByDescending(d => d.Diff))
                    .Take(count)
                    .ToList();

                if (smallestDifferenceFirst)
                {
                    Console.WriteLine("Dagar där inne- och utetemperaturen skiljt sig minst:");
                }
                else
                {
                    Console.WriteLine("Dagar där inne- och utetemperaturen skiljt sig mest:");
                }

                foreach (var day in ordered)
                {
                    Console.WriteLine($"{day.Date:yyyy-MM-dd}: Inne={day.InsideTemp:F2}°C, Ute={day.OutsideTemp:F2}°C, Skillnad={day.Diff:F2}°C");
                }
            });
        }
    }
}
