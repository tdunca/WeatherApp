using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System.Globalization;
using WeatherAppLab3.Models;

namespace WeatherAppLab3
{
    public class WeatherDataMap : ClassMap<WeatherRecord>
    {
        public WeatherDataMap()
        {
            Map(m => m.Date).Name("Date").TypeConverterOption.Format("yyyy-MM-dd H:mm");
            Map(m => m.Location).Name("Location");
            Map(m => m.Temperature).Name("Temp").TypeConverter<NullableDoubleConverter>();
            Map(m => m.Humidity).Name("Humidity").TypeConverter<NullableDoubleConverter>();
        }
    }
    public class NullableDoubleConverter : DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, CsvHelper.IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? (double?)result : null;
        }
    }
}
