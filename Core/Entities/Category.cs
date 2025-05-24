using System.ComponentModel.DataAnnotations;

namespace Artisans.Core.Entities
{
    public class Category
    {
        public int Id { get; set; } // Primary Key
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Description { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Material> Materials { get; set; } = new List<Material>();
    }
}