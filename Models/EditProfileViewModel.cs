using System.ComponentModel.DataAnnotations;

public class EditProfileViewModel
{
    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; }

    [Required]
    [Display(Name = "Email")]
    [EmailAddress]
    public string Email { get; set; }

    [Display(Name = "Phone")]
    [Phone]
    public string Phone { get; set; }

    [Display(Name = "County")]
    public string County { get; set; }
    
    [Display(Name = "District")]
    public string District { get; set; }

    [Display(Name = "Instagram")]
    public string Instagram { get; set; }

    [Display(Name = "Facebook")]
    public string Facebook { get; set; }

    [Required(ErrorMessage = "Please select a file")]
    [Display(Name = "Profile Photo")]
    public IFormFile Image { get; set; }
}
