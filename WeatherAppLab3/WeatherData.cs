using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WeatherAppLab3.Models
{
    [Table("Prod_WeatherData")]
    [Index(nameof(Date), nameof(Location))]
    public class WeatherRecord
    {
        public int Id { get; set; }

        [Required] 
        public DateTime Date { get; set; }

        [Required]
        [MaxLength(50)]
        public string Location { get; set; } = string.Empty;

        [Range(-50, 59)]
        public double? Temperature { get; set; }

        [Range(0, 100)]
        public double? Humidity { get; set; }

        public double MoldRisk
        {
            get
            {
                if (!Humidity.HasValue || !Temperature.HasValue)
                    return 0;

                if (Humidity.Value <= 70 || Temperature.Value < 5)
                    return 0;

                return Math.Round((Humidity.Value - 70) * Temperature.Value / 100, 2);
            }
        }
    }
}