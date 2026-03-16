using System.ComponentModel.DataAnnotations.Schema;

namespace PizzaShop.Entities
{
    [Table("Produits")]
    public class Produit
    {
        public int Id { get; init; }
        public required string Nom { get; set; }
        public decimal Prix { get; set; }
        public int Stock { get; set; }
    }
}
