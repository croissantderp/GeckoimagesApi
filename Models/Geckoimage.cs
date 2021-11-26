using System.ComponentModel.DataAnnotations;

namespace GeckoimagesApi.Models
{
    public class Geckoimage
    {
        [Key]
        public string? number { get; set; }
        public string? name { get; set; }
        public string? author { get; set; }
        public DateTime created { get; set; }
        public string? url { get; set; }
        public string? driveUrl { get; set; }

    }
}
