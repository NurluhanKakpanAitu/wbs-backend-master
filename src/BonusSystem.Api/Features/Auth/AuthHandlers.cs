using BonusSystem.Api.Helpers;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using BonusSystem.Localization;

namespace BonusSystem.Api.Features.Auth;

public static class HttpContextExtensions
{
    public static Guid? GetUserIdFromToken(this HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
public static class AuthHandlers
{
    public static async Task<IResult> UpdateBuyer(
        IAuthenticationService authService,
        BuyerUpdateDto buyerUpdate,
        HttpContext httpContext)
    {
        try
        {
            var userId = httpContext.GetUserIdFromToken();
            if (userId == null)
                return Results.Unauthorized();
            if (userId.Value == null)
                return Results.BadRequest(new { error = Res.Get("Error.UserNotFound") });
            var result = await authService.UpdateBuyerAsync(userId.Value, buyerUpdate);
            if (!result.Success)
                return Results.BadRequest(new { error = result.ErrorMessage });
            return RequestHelper.CreateSuccessResponse(new { message = Res.Get("Success.BuyerUpdated") });
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.UpdatingBuyer"));
        }
    }
    public static async Task<IResult> LoginPincode(
        IAuthenticationService authService,
        HttpContext httpContext,
        string pincode
    )
    {
        try
        {
            var userId = httpContext.GetUserIdFromToken();
            if (userId == null)
                return Results.Unauthorized(); 
            if (userId.Value == null)
                return Results.BadRequest(new { error = Res.Get("Error.UserNotFound") });
            var result = await authService.PinCodeLogin(userId.Value, pincode);
            if (!result.Success)
                if (result.ErrorMessage == Res.Get("Error.PincodeInvalid"))
                    return Results.BadRequest(new { errorr = Res.Get("Error.PincodeInvalid") });
                else if (result.ErrorMessage == Res.Get("Error.UserNotFound") )
                    return Results.BadRequest(new { error = Res.Get("Error.UserNotFound") });
            return RequestHelper.CreateSuccessResponse(new { message = Res.Get("Success.Login") }); 
        }
        catch (Exception e)
        {
            return RequestHelper.HandleExceptionResponse(e, Res.Get("Error.LoginGeneric"));
        }
    }
    public static async Task<IResult> AdminLogin(
        IAuthenticationService authService,
        AdminLoginDto adminLogin)
    {
        try
        {
            var result = await authService.AdminSignInAsync(adminLogin);
            if (!result.Success)
                return Results.BadRequest(new { error = result.ErrorMessage });
            return RequestHelper.CreateSuccessResponse(new { message = Res.Get("Success.AdminLoggedIn") });
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.AdminLogin"));
        }
    }
    public static async Task<IResult> AdminVerify(
        IAuthenticationService authService,
        EmailVerificationDto verificationDto)
    {
        try
        {
            var result = await authService.VerifyAdminAsync(verificationDto);
            if (!result.Success)
                return Results.BadRequest(new { error = result.ErrorMessage });
            return RequestHelper.CreateSuccessResponse(new
            {
                userId = result.UserId,
                token = result.Token,
                role = result.Role
            });
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.AdminVerification"));
        }
    }
    public static async Task<IResult> BuyerRegister(
        IAuthenticationService authService,
        BuyerRegistrationDto buyerRegistration)
    {
        try
        {
            var registration = new UserRegistrationDto
            {
                Username = buyerRegistration.UserName,
                Email = buyerRegistration.Email,
                Role = UserRole.Buyer
            };

            var result = await authService.SignUpAsync(registration);

            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.ErrorMessage });
            }

            return RequestHelper.CreateSuccessResponse(new
            {
                userId = result.UserId,
                token = result.Token,
                role = result.Role
            });
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.BuyerRegistration"));
        }
    }
    public static async Task<IResult> CodeAgain(
        string email,
        IAuthenticationService authService
    )
    {
        try
        {
            var result = await authService.SendCodeAgain(email);
            if (!result.Success)
                return Results.BadRequest(new { error = result.ErrorMessage });
            return RequestHelper.CreateSuccessResponse(new { message = Res.Get("Success.CodeSent") });
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.SendCodeAgain"));
        }
    }
    public static async Task<IResult> DeleteUser(
        IAuthenticationService authService,
        Guid userId)
    {
        try
        {
            var result = await authService.DeleteUserAsync(userId);
            if (!result.Success)
                return Results.BadRequest(new { error = result.ErrorMessage });
            return RequestHelper.CreateSuccessResponse(new { message = Res.Get("Success.UserDeleted") });
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.DeletingUser"));
        }
    }
    public static async Task<IResult> Register(
        IAuthenticationService authService,
        UserRegistrationDto registration)
    {
        try
        {
            var result = await authService.SignUpAsync(registration);

            if (!result.Success)
            {
                return Results.BadRequest(new { error = result.ErrorMessage });
            }

            return RequestHelper.CreateSuccessResponse(new
            {
                message = Res.Get("Success.UserRegistered"),
            });
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.UserRegistration"));
        }
    }

    public static async Task<IResult> Login(
    IAuthenticationService authService,
    UserLoginDto login)
    {
        try
        {
            var result = await authService.SignInAsync(login);

            if (!result.Success)
            {
                var errors = new Dictionary<string, string>();

                if (result.ErrorMessage == Res.Get("Error.InvalidEmail"))
                {
                    errors["email"] = Res.Get("Error.UserNotFound");
                }

                return Results.BadRequest(new { errors });
            }

            return RequestHelper.CreateSuccessResponse(new
            {
                message = Res.Get("Success.UserLoggedIn"),
                // token = result.Token,
                // userid = result.UserId,
                // role = result.Role
            });
        }
        catch (Exception ex)
        {
            return RequestHelper.HandleExceptionResponse(ex, Res.Get("Error.LoginGeneric"));
        }
    }

    public static async Task<IResult> VerifyEmail(
        IAuthenticationService authService,
        EmailVerificationDto verificationDto)
    {
        var result = await authService.VerifyEmailAsync(verificationDto);
        if (!result.Success)
            return Results.BadRequest(new { error = result.ErrorMessage });
        return RequestHelper.CreateSuccessResponse(new { userId = result.UserId, message = Res.Get("Success.EmailVerified"), token = result.Token, role = result.Role });
    }

    public static async Task<IResult> RequestPasswordReset(
        IAuthenticationService authService,
        PasswordResetRequestDto dto)
    {
        var result = await authService.RequestPasswordResetAsync(dto);
        if (!result.Success)
            return Results.BadRequest(new { error = result.ErrorMessage });
        return RequestHelper.CreateSuccessResponse(new { message = Res.Get("Success.ResetCodeSent") });
    }
    public static async Task<IResult> CreatePinCode(
        IAuthenticationService authService,
        HttpContext httpContext,
        string pincode)
    {
        var user = httpContext.GetUserIdFromToken();
        var result = await authService.CreatePinCodeAsync(user.Value, pincode);
        if (!result.Success)
            return Results.BadRequest(new { error = result.ErrorMessage });
        return RequestHelper.CreateSuccessResponse(new { message = Res.Get("Success.PincodeCreated") });
    }
    public static async Task<IResult> ConfirmPasswordReset(
        IAuthenticationService authService,
        PasswordResetConfirmDto dto)
    {
        var result = await authService.ConfirmPasswordResetAsync(dto);
        if (!result.Success)
        {
            var errors = new Dictionary<string, string>();

            if (result.ErrorMessage == Res.Get("Error.InvalidEmail"))
            {
                errors["email"] = Res.Get("Error.UserNotFound");
            }
            else if (result.ErrorMessage == Res.Get("Error.CodeInvalid"))
            {
                errors["password"] = Res.Get("Error.InvalidCode");
            }
            else
            {
                errors["email"] = Res.Get("Error.UserNotFound");
                errors["password"] = Res.Get("Error.InvalidCode");
            }

            return Results.BadRequest(new { errors });
        }
        return RequestHelper.CreateSuccessResponse(new { message = Res.Get("Success.PasswordChanged") });
    }
}