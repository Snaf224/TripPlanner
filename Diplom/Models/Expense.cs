using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Diplom.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "Сумма обязательна")]
        [Range(0, 9999999999999999.99, ErrorMessage = "Сумма не может быть отрицательной")]
        public decimal Amount { get; set; }

        // Внешний ключ для того, кто оплатил
        public int? PaidById { get; set; }
        public TripMember PaidBy { get; set; }

        // Внешний ключ для поездки
        public int? TripId { get; set; }
        [JsonIgnore]
        public Trip? Trip { get; set; }
    }
}
