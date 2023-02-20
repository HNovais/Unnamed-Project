using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Username or E-mail")]
    public string UsernameOrEmail { get; set; }

    [Required]
    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}
