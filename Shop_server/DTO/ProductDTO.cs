using System.ComponentModel.DataAnnotations;

namespace Shop_server.DTO
{
    public class ProductDTO
    {
        [Required]
        [MinLength(2)]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "A termék árának megadása kötelező.")]
        [Range(typeof(decimal),"10.00","999999999",ParseLimitsInInvariantCulture = true,
                ErrorMessage = "Az ár minimum 10 Ft'")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; } = 1;

        [Range(0, int.MaxValue)]
        public int DepartmentId { get; set; }
    }
}
