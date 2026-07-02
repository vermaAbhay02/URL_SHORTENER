﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using URL_Shortener.Models.Data;
using URL_Shortener.Models;
using URL_Shortener.DTOs.Auth;
using URL_Shortener.Services;

namespace URL_Shortener.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;

    public AuthController(UserManager<User> userManager, AppDbContext db, 
                            TokenService tokenService)
    {
        _userManager = userManager;
        _db = db;
        _tokenService = tokenService;
    }

    private static readonly List<string> ValidQuestions=new List<string>
    {
        "What was your first pet's name?",
        "What city were you born in?",
        "What is your mother's maiden name?",
        "What was your childhood nickname?",
        "What was the name of your first school?"
    };

    // ── Register ──────────────────────────────────────────────
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return Conflict(new { message = "Email already in use" });
        if(!ValidQuestions.Contains(dto.SecurityQuestion)){
            return BadRequest(new { message = "Invalid security question" });
        }    
        var passwordHasher=new PasswordHasher<User>();
    
        var user = new User
        {
            Email = dto.Email,
            UserName = dto.Email,
            Plan = Plan.Free,
            SecurityQuestion=dto.SecurityQuestion,
        };
        user.SecurityAnswerHash=passwordHasher.HashPassword(user,dto.SecurityAnswer.ToLower().Trim());

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        await _userManager.AddToRoleAsync(user,"User");
        return Ok(new { message = "Registered successfully" });
    }

    // ── Login ─────────────────────────────────────────────────
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Invalid email or password" });

        var roles=await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user,roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);

        return Ok( new AuthResponseDto {
            
                Id=user.Id,
                Email=user.Email!,
                Role=roles,
                Plan=user.Plan.ToString(),
                AccessToken = accessToken,
                RefreshToken = refreshToken,
        });
    }

    // ── Refresh Token ─────────────────────────────────────────
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByIdAsync(dto.UserId);

        if (user == null)
            return Unauthorized(new { message = "User not found" });

        if (user.RefreshToken != dto.RefreshToken)
            return Unauthorized(new { message = "Invalid refresh token" });

        if (user.RefreshTokenExpiry < DateTime.UtcNow)
            return Unauthorized(new { message = "Refresh token expired, please login again" });

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _tokenService.GenerateAccessToken(user,roles);

        return Ok(new RefreshResponseDto { 
            AccessToken = newAccessToken
        });
    }

    // ── Logout ────────────────────────────────────────────────
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return NotFound(new { message = "User not found" });

        // Invalidate refresh token
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Logged out successfully" });
    }

    // ── Get Current User ──────────────────────────────────────
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User ID not found in token" });

        var user = await _userManager.FindByIdAsync(userId!);

        if (user == null)
            return Unauthorized(new { message = "Session expired or user not found. Please login again." });
        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Id,
            user.Email,
            Role=roles,
            Plan = user.Plan.ToString(),
            user.CreatedAt
        });
    }

[Authorize]
[HttpPost("upgrade-to-pro")]
public async Task<IActionResult> UpgradeToPro()
{
    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var user = await _userManager.FindByIdAsync(userId!);

    if (user == null)
        return NotFound(new { message = "User not found" });

    if (user.Plan == Plan.Pro)
        return BadRequest(new { message = "You are already on Pro plan" });

    user.Plan = Plan.Pro;
    await _userManager.UpdateAsync(user);

    return Ok(new { message = "Upgraded to Pro successfully" });
}

 
[HttpGet("security-questions")]
public  IActionResult GetSecurityQuestions()
{
   return Ok(ValidQuestions);
}

[HttpGet("security-question/{email}")]
public async Task<IActionResult> GetSecuityQuestionByEmail(string email){
    var user = await _userManager.FindByEmailAsync(email);
    if(user==null || string.IsNullOrEmpty(user.SecurityQuestion)){
        return NotFound(new { message = "Security question not found" });
    }
    return Ok(new {question=user.SecurityQuestion});
}

[Authorize]
[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto){
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var user = await _userManager.FindByEmailAsync(dto.Email);
    if(user==null){
        return BadRequest(new { message = "Invalid email or answer" });
    }
    
    var passwordHasher=new PasswordHasher<User>();
    var result=passwordHasher.VerifyHashedPassword(user,user.SecurityAnswerHash,
                                                    dto.SecurityAnswer.ToLower().Trim());
    if(result==PasswordVerificationResult.Failed){
        return BadRequest(new { message = "Invalid email or answer" });
    }  
    var resetToken=await _userManager.GeneratePasswordResetTokenAsync(user);
    var resetResult=await _userManager.ResetPasswordAsync(user,resetToken,dto.NewPassword);     
    if(!resetResult.Succeeded){
        return BadRequest(new { errors = resetResult.Errors.Select(e => e.Description) });
    }
   return Ok(new {message="Password reset successfully"});                                         

}

}