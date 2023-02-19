using System.ComponentModel.DataAnnotations;

public class NoNumbersAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value != null)
        {
            string name = (string)value;

            if (name.Any(char.IsDigit))
            {
                return new ValidationResult("Name should not contain numbers.");
            }
        }

        return ValidationResult.Success;
    }
}

public class RegisterViewModel
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

