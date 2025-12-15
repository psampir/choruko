using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Settlement;

public class CreateSettlementRequest
{
    [Required(ErrorMessage = "Settlement description is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Description must be between 2 and 200 characters.")]
    public required string Description { get; set; }

    [Range(0.01, 99999999.99, ErrorMessage = "Amount must be between 0.01 and 99,999,999.99.")]
    public decimal Amount { get; set; }

    [MinLength(1, ErrorMessage = "At least one participant must be added to the settlement.")]
    public List<string>? UnsettledUsers { get; set; } = [];
}
