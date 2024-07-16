using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProductApp.ViewModels
{
    public class ProductViewModel
    {
        public int? TenantId { get; set; }
        [Required]
        [DisplayName("Prodcut Name")]
        public string Name { get; set; }
        [Required]
        [DisplayName("Prodcut Description")]
        public string Description { get; set; }
        [DisplayName("Is available")]
        public bool IsAvailable { get; set; } = true;
        public int Id { get; set; }
    }
}
