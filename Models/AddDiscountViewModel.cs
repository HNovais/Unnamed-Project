using System.ComponentModel.DataAnnotations;

public class AddDiscountViewModel
{ 
    public int? Id { get; set; }
    public string Code { get; set; }
    public string Type { get; set; }
    public string? Value { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public int? Limit { get; set; }
    public bool isActive { get; set; }
    public DateTime Creation { get; set; }
    public DateTime Updated { get; set; }
}
