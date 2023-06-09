using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Models;
public class Transaction
{
    [Key]
    public int TransactionId { get; set; }
    [Range(1, int.MaxValue,ErrorMessage ="please choose a category.")]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Amount should be greater than zero.")]
    public int Amount { get; set; }
    [Column(TypeName = "nvarchar(75)")]
    public string? Note { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;

    [NotMapped]
    public string? CategoryTitleWithIcon
    {
        get
        {
            return Category == null ? "" : $"{Category.Icon} {Category.Title}";
        }
    }
    public string? FormattedAmount
    {
        get
        {
            return ((Category==null || Category.Type=="Expense") ? "- ":"+ ") + Amount.ToString("C0");
        }
    }
}