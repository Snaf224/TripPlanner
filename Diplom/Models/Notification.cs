using System.Data.Common;
using System.Text.Json.Serialization;

namespace Diplom.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime SendDate { get; set; }
        public bool IsRead { get; set; }

        // Внешний ключ для получателя
        public int? ReceiverId { get; set; }
        public TripMember Receiver { get; set; }

        // Внешний ключ для поездки
        public int? TripId { get; set; }
        [JsonIgnore]
        public Trip Trip { get; set; }
    }
}
