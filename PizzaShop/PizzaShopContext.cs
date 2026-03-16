using Microsoft.EntityFrameworkCore;
using PizzaShop.Entities;

namespace PizzaShop
{
    public class PizzaShopContext(DbContextOptions options): DbContext(options)
    {
        public DbSet<Produit> Products { get; set; }
    }
}
