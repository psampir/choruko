using ChorukoAPI.Contracts.Responses;
using Microsoft.AspNetCore.Http;

namespace ChorukoAPI.Services.Abstraction;

public interface IImageProcessor
{
    Task<ServiceResponse<string>> ProcessAsync(
        IFormFile? file,
        string subfolder,
        CancellationToken cancellationToken = default);

    void Delete(string? relativePath);
}
