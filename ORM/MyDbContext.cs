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
    public DbSet<Admin> Admin { get; set; }
    public DbSet<DiscountCode> DiscountCode { get; set; }
    public DbSet<Category> Category { get; set; }
    public DbSet<Feature> Feature { get; set; }
    public DbSet<FeatureValue> FeatureValue { get; set; }
    public DbSet<FeatureOptions> FeatureOptions { get; set; }
    public DbSet<ProductQuantity> ProductQuantity { get; set; }
    public DbSet<Vote> Vote { get; set; }
    public DbSet<DistrictCounty> DistrictCounty { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString = "server=localhost;port=3306;user=root;database=unnamed;password=8426";
        optionsBuilder.UseMySQL(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vote>()
            .HasKey(fv => new { fv.Review, fv.User});

        modelBuilder.Entity<DistrictCounty>()
            .HasKey(fv => new { fv.District, fv.County });
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

    [Column("instagram")]
    [StringLength(50)]
    public string Instagram { get; set; }

    [Column("facebook")]
    [StringLength(50)]
    public string Facebook { get; set; }

    [Column("county")]
    [StringLength(50)]
    public string County { get; set; }

    [Column("district")]
    [StringLength(50)]
    public string District { get; set; }

    [Column("storeIcon")]
    [StringLength(250)]
    public string Icon { get; set; }
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

    [Required]
    [Column("upvotes")]
    public int Upvotes { get; set; }

    [Required]
    [Column("downvotes")]
    public int Downvotes { get; set; }
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

public class Admin
{
    [Key]
    [Column("adminID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("adminUsername")]
    public string Username { get; set; }
}

public class DiscountCode
{
    [Key]
    [Column("discountID")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("discountCode")]
    public string Code { get; set; }

    [Required]
    [Column("discountType")]
    public string Type { get; set; }

    [Required]
    [Column("discountValue")]
    public string Value { get; set; }

    [Required]
    [Column("discountStart")]
    public DateTime Start { get; set; }

    [Required]
    [Column("discountEnd")]
    public DateTime End { get; set; }

    [Column("usageLimit")]
    public int? Limit { get; set; }

    [Required]
    [Column("usageCount")]
    public int Count { get; set; }

    [Required]
    [Column("discountActive")]
    public bool IsActive { get; set; }

    [Required]
    [Column("discountCreation")]
    public DateTime CreationDate { get; set; }

    [Required]
    [Column("discountUpdate")]
    public DateTime UpdateDate { get; set; }
}

public class Category
{
    [Key]
    [Required]
    [Column("categoryID")]
    public int Id { get; set; }

    [Required]
    [Column("categoryName")]
    public string Name { get; set; }
}

public class Feature
{
    [Key]
    [Required]
    [Column("featureID")]
    public int Id { get; set; }

    [Required]
    [Column("featureName")]
    public string Name { get; set; }

    [Required]
    [Column("categoryFK")]
    public int Category { get; set; }
}

public class FeatureValue
{
    [Key]
    [Required]
    [Column("fvID")]
    public int Id { get; set; }

    [Required]
    [Column("productFK")]
    public int Product { get; set; }

    [Required]
    [Column("featureFK")]
    public int Feature { get; set; }

    [Required]
    [Column("value")]
    public string Value { get; set; }

}

public class FeatureOptions
{
    [Key]
    [Required]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [Column("FeatureFK")]
    public int Feature { get; set; }

    [Required]
    [Column("Value")]
    public string Value { get; set; }
}

public class ProductQuantity
{
    [Key]
    [Required]
    [Column("productQuantityID")]
    public int Id { get; set; }
    
    [Required]
    [Column("productFK")]
    public int Product { get; set; }

    [Required]
    [Column("productType")]
    public string Type { get; set; }

    [Required]
    [Column("quantity")]
    public int Quantity { get; set; }
}

public class Vote
{
    [Required]
    [Column("reviewFK")]
    public int Review { get; set; }

    [Required]
    [Column("userFK")]
    public int User { get; set; }

    [Required]
    [Column("voteType")]
    public string Type { get; set; }
}

public class DistrictCounty
{
    [Required]
    [Column("district")]
    public string District;

    [Required]
    [Column("county")]
    public string County;
}