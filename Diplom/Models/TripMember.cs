using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Diplom.Models
{
    public class TripMember
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Вклад обязателен")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Вклад должен быть положительным числом")]
        public decimal Contribution { get; set; }

        public int? TripId { get; set; } // Nullable, чтобы Trip был необязательным
        [JsonIgnore]
        public Trip Trip { get; set; }
    }
}