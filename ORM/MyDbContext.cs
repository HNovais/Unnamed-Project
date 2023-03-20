using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Security.Policy;

public class MyDbContext : DbContext
{
    public DbSet<User> User { get; set; }
    public DbSet<Store> Store { get; set; }
    public DbSet<Review> Review { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<Cart> Cart { get; set; }
    public DbSet<CartProduct> CartProduct { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "server=localhost;port=3306;user=root;database=unnamed;password=8426";
        optionsBuilder.UseMySQL(connectionString);
    }
}
public class User
{
    [Key]
    [Column("userID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    [StringLength(50)]
    public string Name { get; set; }

    [Required]
    [Column("username")]
    [StringLength(50)]
    public string Username { get; set; }

    [Required]
    [Column("userEmail")]
    [StringLength(50)]
    public string Email { get; set; }

    [Required]
    [Column("userPassword")]
    [StringLength(128)]
    public string Password { get; set; }

    [Required]
    [Column("userSalt")]
    [StringLength(128)]
    public string Salt { get; set; }
}

public class Store
{
    [Key]
    [Column("storeID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("storeName")]
    [StringLength(50)]
    public string Name { get; set; }

    [Required]
    [Column("storeUsername")]
    [StringLength(50)]
    public string Username { get; set; }

    [Required]
    [Column("storePassword")]
    [StringLength(255)]
    public string Password { get; set; }

    [Required]
    [Column("storeSalt")]
    [StringLength(255)]
    public string Salt { get; set; }

    [Required]
    [Column("storeEmail")]
    [StringLength(50)]
    public string Email { get; set; }

    [Required]
    [Column("storePhone")]
    [StringLength(50)]
    public string Phone { get; set; }
}

public class Review
{
    [Key]
    [Column("reviewID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("storeFK")]
    public int Store { get; set; }

    [Required]
    [Column("reviewer")]
    public string Reviewer { get; set; }

    [Required]
    [Column("rating")]
    public int Rating { get; set; }

    [Required]
    [Column("comment")]
    public string? Comment { get; set; }

    [Required]
    [Column("reviewDate")]
    public DateTime ReviewDate { get; set; }
}

public class Product
{
    [Key]
    [Column("productID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("productName")]
    public string Name { get; set; }

    [Required]
    [Column("productDescription")]
    public string Description { get; set; }

    [Required]
    [Column("productCategory")]
    public string Category { get; set; }

    [Required]
    [Column("productPrice")]
    public float Price { get; set; }

    [Required]
    [Column("productQuantity")]
    public int Quantity { get; set; }

    [Required]
    [Column("productIcon")]
    public string Icon { get; set; }

    [Required]
    [Column("productImages")]
    public string Images { get; set; }

    [Required]
    [Column("seller")]
    public int Store { get; set; }
}

public class Cart
{
    [Key]
    [Column("cartID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("userCart")]
    public string User { get; set; }

    [Required]
    [Column("lastUpdate")]
    public DateTime Time { get; set; }
}

public class CartProduct
{
    [Key]
    [Column("cpID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("cartFK")]
    public int Cart { get; set; }

    [Required]
    [Column("productFK")]
    public int Product { get; set; }

    [Required]
    [Column("cpQuantity")]
    public int Quantity { get; set; }
}