using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Diplom.Models
{
    public class TravelTask
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletionDate { get; set; }

        // Внешний ключ для ответственного
        public int? AssignedToId { get; set; }
        public TripMember AssignedTo { get; set; }

        // Внешний ключ для поездки
        public int? TripId { get; set; }
        [JsonIgnore]
        public Trip Trip { get; set; }


    }
}
