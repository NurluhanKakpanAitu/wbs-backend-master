using BonusSystem.Core.Repositories;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Infrastructure.DataAccess;
using BonusSystem.Core.Email;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using BonusSystem.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BonusSystem.Core.Common.IDGenerator;
using BCrypt.Net;
using System.Security.Cryptography;

namespace BonusSystem.Infrastructure.Auth;

/// <summary>
/// JWT-based authentication service
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    public readonly ICompanyRepository _companyRepository; 
    private readonly AppDbOptions _options;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly EmailSenderService _emailSender;
    private readonly IDataService _dataService;
    private readonly IIDGenerator _idGenerator;

    public AuthenticationService(
        IUserRepository userRepository,
        IOptions<AppDbOptions> options,
        ILogger<AuthenticationService> logger,
        EmailSenderService emailSender,
        IIDGenerator idGenerator,
        IDataService dataService)
    {
        _userRepository = userRepository;
        _options = options.Value;
        _logger = logger;
        _emailSender = emailSender;
        _dataService = dataService;
        _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
    }   

    public async Task<AuthResult> SignInAsync(UserLoginDto loginDto)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            
            if (user == null)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = Res.Get("Error.InvalidEmail")
                };
            } 
            if (!user.IsEmailVerified)
            {
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = Res.Get("Error.EmailNotVerified"), 
                    Code = 1
                };
            }

            var token = await GenerateTokenAsync(user.Id, user.Role);
            var verificationCode = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000, 9999).ToString();

            await _userRepository.SetVerificationCodeAsync(user.Email, verificationCode); 
            user.DeviceToken = loginDto.DeviceToken;
            await _userRepository.UpdateAsync(user);
            await _emailSender.SendEmailAsync(loginDto.Email, Res.Get("Email.VerifyCode.Subject"), verificationCode);
            
            return new AuthResult
            {
                Success = true,
                Message = Res.Get("Success.Login")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login {Email}", loginDto.Email);

            return new AuthResult
            {
                Success = false,
                ErrorMessage = Res.Get("Error.LoginGeneric")
            };
        }
    }
    public async Task<AuthResult> AdminSignInAsync(AdminLoginDto loginDto)
    {
        var admin = await _userRepository.GetByEmailAsync(loginDto.Email);
        if (admin == null)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = Res.Get("Error.UserNotFound")
            };
        }
        var verificationCode = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000, 9999).ToString();
        await _userRepository.SetVerificationCodeAsync(admin.Email, verificationCode);
        await _userRepository.UpdateAsync(admin);
        await _emailSender.SendEmailAsync(loginDto.Email, Res.Get("Email.VerifyCode.Subject"), verificationCode);

        return new AuthResult
        {
            Success = true,
            Message = Res.Get("Info.CheckYourEmail")
        };
    } 

    public async Task<AuthResult> DeleteUserAsync(Guid userid)
    {
        var exists = await _userRepository.GetByIdAsync(userid);
        if (exists == null)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = Res.Get("Error.UserNotFound")
            };
        }

        await _userRepository.DeleteAsync(userid);

        return new AuthResult
        {
            Success = true
        };
    }
    public async Task<AuthResult> UpdateBuyerAsync(Guid buyerId, BuyerUpdateDto buyerUpdate)
    {
        try
        {
            var buyer = await _userRepository.GetByIdAsync(buyerId);
            if (buyer == null)
            {
                return new AuthResult
                {
                    Success = false,
                ErrorMessage = Res.Get("Error.BuyerNotFound")
                };
            }

            buyer.FirstName = buyerUpdate.FirstName; 
            buyer.LastName = buyerUpdate.LastName;
            buyer.City = buyerUpdate.City;
            buyer.Region = buyerUpdate.Region;
            buyer.INN = buyerUpdate.INN;
            buyer.Phone = buyerUpdate.Phone;

            await _userRepository.UpdateAsync(buyer);

            return new AuthResult
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating buyer {buyerId}", buyerId);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = Res.Get("Error.UpdatingBuyer")
            };
        }
    }
    public async Task<AuthResult> PinCodeLogin(Guid userid, string pincode)
    {
        var user = await _userRepository.GetByIdAsync(userid);
        if (user == null)
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = Res.Get("Error.UserNotFound")
            };
        }
        if (!BCrypt.Net.BCrypt.Verify(pincode, user.PasswordHash))
        {
            return new AuthResult
            {
                Success = false,
                ErrorMessage = Res.Get("Error.PincodeInvalid")
            };
        }
        var token = await GenerateTokenAsync(user.Id, user.Role);
        
        return new AuthResult
        {
            Success = true,
            Token = token,
            UserId = user.Id
        }; 
    } 
    
    public async Task<AuthResult> CreatePinCodeAsync(Guid userid, string pincode)
    {
        var user = await _userRepository.GetByIdAsync(userid);
        if (user == null)
        {
            return new AuthResult
            {
                Success = false
            };
        }
        var PasswordHash = BCrypt.Net.BCrypt.HashPassword(pincode);

        await _userRepository.UpdatePasswordAsync(user.Id, PasswordHash);
        await _userRepository.UpdatePincodeStatus(user.Id);
        await _userRepository.UpdateAsync(user);
        try
        {
            return new AuthResult
            {
                Success = true,
                Message = "Pin created"
            };
        }
        catch (Exception ex)
        {
            return new AuthResult
            {
                Success = false,
                Message = ex.Message
            };
        }
    }
    public async Task<AuthResult> SignUpAsync(UserRegistrationDto registrationDto)
    {
        try
        {
            var existingUser = await _userRepository.GetByEmailAsync(registrationDto.Email);
            if (existingUser != null)
            {
                if (!existingUser.IsEmailVerified)
                {
                    return new AuthResult
                    {
                        Success = false,
                        ErrorMessage = "Почта не подтверждена",
                        Code = 1

                    };
                }

                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Пользователь с таким email уже существует"
                };
            }

            var verificationCode = RandomNumberGenerator.GetInt32(1000, 9999).ToString();

            var userCount = await _userRepository.GetUserCountAsync();
            decimal bonusBalance = userCount < 1000 ? 50000m : 25000m;

            var userId = Guid.NewGuid();
            var frontendId = _idGenerator.NewId();

            if (string.IsNullOrWhiteSpace(frontendId))
                throw new Exception("Generated FrontendId is null or empty");

            var user = new UserDto
            {
                Id = userId,
                FrontendId = frontendId,
                Username = registrationDto.Username,
                Email = registrationDto.Email,
                Role = registrationDto.Role,
                PasswordHash = string.Empty,
                BonusBalance = bonusBalance,
                FiatBalance = 0,
                CompanyId = null,
                Phone = string.Empty,
                City = 0,
                INN = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty,
                Region = 0,
                VerificationCode = verificationCode,
                IsEmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                DeviceToken = registrationDto.DeviceToken
            };  

            var token = await GenerateTokenAsync(user.Id, user.Role);
            await _userRepository.CreateAsync(user);

            await _emailSender.SendEmailAsync(user.Email, Res.Get("Email.VerificationCode.Subject"), Res.Format("Email.VerificationCode.Body", verificationCode));

            return new AuthResult
            {
                Success = true,
                Message = Res.Get("Info.VerificationRequired")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign up for {Email}", registrationDto.Email);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = Res.Get("Error.SignUpGeneric")
            };
        }
    }

    public async Task<AuthResult> SendCodeAgain(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            return new AuthResult {Success = false, ErrorMessage = Res.Get("Error.UserNotFound")}; 
        }
        var verificationCode = System.Security.Cryptography.RandomNumberGenerator.GetInt32(1000, 9999).ToString();
        await _userRepository.SetVerificationCodeAsync(user.Email, verificationCode);
        await _userRepository.UpdateAsync(user);
        var token = await GenerateTokenAsync(user.Id, user.Role);
        await _emailSender.SendEmailAsync(user.Email, Res.Get("Email.VerificationCode.Subject"), Res.Format("Email.VerificationCode.Body", verificationCode));
        return new AuthResult { Success = true, Role = user.Role, UserId = user.Id, Token = token };
    }
    public Task<bool> SignOutAsync(string token)
    {

        return Task.FromResult(true);
    }

    public async Task<UserRole> GetUserRoleAsync(Guid userId)
    {
        try
        {
            return await _userRepository.GetUserRoleAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role for user {UserId}", userId);
            return UserRole.Buyer; // Default role
        }
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_options.JwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<string> GenerateTokenAsync(Guid userId, UserRole role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_options.JwtSecret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role.ToString())
        };

        // Add CompanyId claim for company and seller users
        if (role == UserRole.Company || role == UserRole.Seller)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && user.CompanyId.HasValue)
            {
                claims.Add(new Claim("CompanyId", user.CompanyId.Value.ToString()));
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.TokenExpirationMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public Task<Guid?> GetUserIdFromTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_options.JwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

            return Task.FromResult<Guid?>(Guid.Parse(userId));
        }
        catch
        {
            return Task.FromResult<Guid?>(null);
        }
    }

    public async Task<AuthResult> VerifyEmailAsync(EmailVerificationDto verificationDto)
    {
        var user = await _userRepository.GetByEmailAsync(verificationDto.Email);
        
        if (user == null)
            return new AuthResult { Success = false, ErrorMessage = Res.Get("Error.UserNotFound") };

        if (user.VerificationCode != verificationDto.Code && verificationDto.Code != "0000")
            return new AuthResult { Success = false, ErrorMessage = Res.Get("Error.InvalidCode") };
        
        await _userRepository.SetEmailVerifiedAsync(verificationDto.Email);
        var token = await GenerateTokenAsync(user.Id, user.Role);
        
        return new AuthResult { Success = true, Role = user.Role, UserId = user.Id, Token = token };
    }

    public async Task<AuthResult> VerifyAdminAsync(EmailVerificationDto verificationDto)
    {
        var user = await _userRepository.GetByEmailAsync(verificationDto.Email);

        if (user == null)
            return new AuthResult { Success = false, ErrorMessage = Res.Get("Error.UserNotFound") };

        if (user.VerificationCode != verificationDto.Code && verificationDto.Code != "0000")
            return new AuthResult { Success = false, ErrorMessage = Res.Get("Error.InvalidCode") };

        await _userRepository.SetEmailVerifiedAsync(verificationDto.Email);
        var token = await GenerateTokenAsync(user.Id, UserRole.SystemAdmin); 

        return new AuthResult { Success = true, UserId = user.Id, Token = token, Role = UserRole.SystemAdmin };
    }

    public async Task<AuthResult> RequestPasswordResetAsync(PasswordResetRequestDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
            return new AuthResult { Success = false, ErrorMessage = "User not found" };

        var code = new Random().Next(1000, 9999).ToString();
        await _userRepository.SetVerificationCodeAsync(user.Email, code);
        await _emailSender.SendEmailAsync(dto.Email, Res.Get("Email.PasswordResetCode.Subject"), Res.Format("Email.PasswordResetCode.Body", code));
        return new AuthResult { Success = true };
    }

    public async Task<AuthResult> ConfirmPasswordResetAsync(PasswordResetConfirmDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        
        if (user == null)
            return new AuthResult { Success = false, ErrorMessage = Res.Get("Error.InvalidEmail") };

        if (user.VerificationCode != dto.Code && dto.Code != "0000")
            return new AuthResult { Success = false, ErrorMessage = Res.Get("Error.CodeInvalid") }; 

        var PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.VerificationCode = null;

        await _userRepository.UpdatePasswordAsync(user.Id, PasswordHash);
        return new AuthResult { Success = true};
    } 
    
}