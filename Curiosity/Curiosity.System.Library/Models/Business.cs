using System.ComponentModel.DataAnnotations;

namespace Curiosity.System.Library.Models
{
    public class Business
    {
        [Key]
        public string Seed { get; set; }
        public long Balance { get; set; }
        public long Registered { get; set; }
    }
}