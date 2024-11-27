using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Shop.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdProduct { get; set; }

        [Required]
        [StringLength(255)]
        public string NameProduct { get; set; }

        [StringLength(int.MaxValue)]
        public string DescriptionProduct { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PriceProduct { get; set; }

        [StringLength(255)]
        public string ImageUrlProduct { get; set; }
    }
}
