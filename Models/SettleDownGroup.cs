using System.ComponentModel.DataAnnotations;

namespace SettleDown.Models
{
    public class SettleDownGroup
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
