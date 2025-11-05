using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Requests.Settlement;

public class EditSettlementRequest
{
    [Required(ErrorMessage = "Settlement description is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Description must be between 2 and 200 characters.")]
    public string Description { get; set; }

    public decimal Amount { get; set; }

    [MinLength(1, ErrorMessage = "At least one participant must be added to the settlement.")]
    public List<string>? UnsettledUsers { get; set; }

}
