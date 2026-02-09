using System.ComponentModel.DataAnnotations;

namespace Gamestore.WebAPI.DTOs.PublisherDTO;

public class PublisherDTO
{
    [Required(ErrorMessage = "Company name name is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Company name must be between 1 and 200 characters.")]
    public string CompanyName { get; set; }

    public string? HomePage { get; set; }

    public string? Description { get; set; }
}