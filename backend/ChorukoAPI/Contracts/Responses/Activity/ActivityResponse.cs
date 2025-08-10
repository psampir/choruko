using ChorukoAPI.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ChorukoAPI.Contracts.Responses.Activity;
public class ActivityResponse
{
    public string Id { get; set; }

    public string Name { get; set; }

    public int PointsAwarded { get; set; }
}

