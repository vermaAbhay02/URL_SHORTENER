using System.ComponentModel.DataAnnotations;

namespace URL_Shortener.DTOs.Auth;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; } = string.Empty;
    public string SecurityQuestion {get;set;}=string.Empty;
    public string SecurityAnswer {get;set;}=string.Empty;
    
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RefreshDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = string.Empty;
    public string Plan { get; set; } = string.Empty;
    public IList<string> Role { get; set; } = new List<string>();
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
}


public class ResetPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    public string SecurityAnswer {get;set;}=string.Empty;
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string NewPassword { get; set; } = string.Empty;
}