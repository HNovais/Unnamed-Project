using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

public class MyDbContext : DbContext
{
    public DbSet<User> User { get; set; }
    public DbSet<Store> Store { get; set; }
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