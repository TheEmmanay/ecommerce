using System;

namespace ecommerce.DTOs
{
    // Request to create a user (password in plain text; will be hashed by the service)
    public record CreateUserRequest(string FullName, string Email, string Password, string Role = "User");

    // Response DTO that excludes sensitive fields like PasswordHash
    public record UserResponse(int Id, string FullName, string Email, string Role, DateTime CreatedAt);

    // Request to update a user (Password optional; if provided will replace the hash)
    public record UpdateUserRequest(string FullName, string Email, string? Password, string Role);
}
