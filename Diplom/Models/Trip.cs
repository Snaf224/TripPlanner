using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Diplom.Models
{
    public class Trip : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Дата начала обязательна")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Дата окончания обязательна")]
        public DateTime EndDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Бюджет не может быть отрицательным")]
        public decimal Budget { get; set; }

        [JsonIgnore]
        public List<TripMember> Members { get; set; } = new();
        [JsonIgnore]
        public List<TravelTask> Tasks { get; set; } = new();
        [JsonIgnore]
        public List<Expense> Expenses { get; set; } = new();
        [JsonIgnore]
        public List<Notification> Notifications { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "Дата окончания должна быть позже даты начала.",
                    new[] { nameof(EndDate), nameof(StartDate) });
            }
        }
    }
}
