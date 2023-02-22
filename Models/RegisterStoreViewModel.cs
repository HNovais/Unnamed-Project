using System.ComponentModel.DataAnnotations;

public class RegisterStoreViewModel
{
    [Required]
    [Display(Name = "Name")]
    [NoNumbers]
    public string Name { get; set; }

    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; }

    [Required]
    [Display(Name = "Email")]
    [EmailAddress]
    public string Email { get; set; }

    [Display(Name = "Phone")]
    [Phone]
    public string Phone { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    [MinLength(8)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
}


