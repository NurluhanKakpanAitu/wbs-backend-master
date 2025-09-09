using BonusSystem.Api.Infrastructure.Swagger;
using BonusSystem.Shared.Dtos;
using Microsoft.OpenApi.Models;

namespace BonusSystem.Api.Features.Auth;

public static class AuthEndpoints
{
    public static WebApplication MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("/buyer_register", AuthHandlers.BuyerRegister)
            .AllowAnonymous()
            .WithName("BuyerRegister")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Register a new user";
                operation.Description =
                    "Creates a new user account with the provided credentials and returns a JWT token.\n\n" +
                    "Request requires:\n" +
                    "- username: User display name (must be unique)\n" +
                    "- email: Email address used for login (must be unique)\n" +
                    "- password: Password (min 8 characters)\n" +
                    "Successful response contains:\n" +
                    "- userId: Unique identifier for the new user\n" +
                    "- token: JWT authentication token\n" +
                    "- role: User's role in the system";

                operation.EnsureResponse("200", "Registration successful");
                operation.EnsureResponse("400", "Registration failed");

                return operation;
            });
        group.MapPost("/admin-login", AuthHandlers.AdminLogin)
            .WithName("LoginAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Login Admin";
                operation.EnsureResponse("200", "Login successful");
                operation.EnsureResponse("400", "Login failed");
                return operation;  
            });  

        group.MapPost("/admin-verify", AuthHandlers.AdminVerify)
            .WithName("VerifyAdmin")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Verify Admin";
                operation.EnsureResponse("200", "Verification successful");
                operation.EnsureResponse("400", "Verification failed");
                return operation;
            });
        group.MapPost("/update-buyer", AuthHandlers.UpdateBuyer)
            .RequireAuthorization()
            .WithName("UpdateBuyer")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Update buyer information";
                operation.Description = "Updates the information of an existing buyer.";
                operation.EnsureResponse("200", "Update successful");
                operation.EnsureResponse("400", "Update failed");
                return operation;
            });
        group.MapPost("/create-pin-code", AuthHandlers.CreatePinCode)
            .RequireAuthorization()
            .WithName("Create Pin Code")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Create user pin code";
                operation.Description = "Create a pin code"; 
                operation.EnsureResponse("200", "Pin created");
                operation.EnsureResponse("400", "Pin failed"); 
                return operation; 
            }
            ); 
        group.MapPost("/register", AuthHandlers.Register)
            .AllowAnonymous()
            .WithName("Register")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Register a new user";
                operation.Description =
                    "Creates a new user account with the provided credentials and returns a JWT token.\n\n" +
                    "Request requires:\n" +
                    "- username: User display name (must be unique)\n" +
                    "- email: Email address used for login (must be unique)\n" +
                    "- password: Password (min 8 characters)\n" +
                    "- role: User role (0=Buyer, 1=Seller, 2=StoreAdmin, 3=SystemAdmin, 4=CompanyObserver, 5=SystemObserver)\n\n" +
                    "Successful response contains:\n" +
                    "- userId: Unique identifier for the new user\n" +
                    "- token: JWT authentication token\n" +
                    "- role: User's role in the system";

                operation.EnsureResponse("200", "Registration successful");
                operation.EnsureResponse("400", "Registration failed");

                return operation;
            });

        group.MapPost("/login", AuthHandlers.Login)
            .AllowAnonymous()
            .WithName("Login")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Authenticate user";
                operation.Description = "Authenticates a user with email and password and returns a JWT token.\n\n" +
                                        "Request requires:\n" +
                                        "- email: Email address used during registration\n" +
                                        "- password: User password\n\n" +
                                        "Successful response contains:\n" +
                                        "- userId: Unique identifier for the authenticated user\n" +
                                        "- token: JWT token to use in Authorization header\n" +
                                        "- role: User's role in the system";

                operation.EnsureResponse("200", "Authentication successful");
                operation.EnsureResponse("400", "Authentication failed");

                return operation;
            });
        group.MapPost("/login_pincode", AuthHandlers.LoginPincode)
            .WithName("Login with pin code")
            .AllowAnonymous()
            .WithOpenApi( operation =>
            {
                operation.Summary = "Pincode Login";
                operation.Description = "Login Pincode";
                operation.EnsureResponse("200", "Authentication successful");
                operation.EnsureResponse("400", "Authentication failed");
                return operation; 
            }
            ); 
        group.MapPost("/verify_email", AuthHandlers.VerifyEmail)
            .AllowAnonymous()
            .WithName("VerifyEmail")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Verify user email by code";
                operation.Description = "Verifies a user's email address using a code sent to their email.";
                operation.EnsureResponse("200", "Email verified");
                operation.EnsureResponse("400", "Invalid code or email");
                return operation;
            });

        group.MapPost("/verify_email/{email}/again", AuthHandlers.CodeAgain)
            .AllowAnonymous()
            .WithName("Send Code Again")
            .WithOpenApi( operation =>
            {
                operation.Summary = "Send code again";
                // operation.Description = "Verifies a user's email address using a code sent to their email.";
                operation.EnsureResponse("200", "Done");
                operation.EnsureResponse("400", "Invalid email");
                return operation;
            });
        
        group.MapPost("/delete_user", AuthHandlers.DeleteUser)
            .AllowAnonymous()
            .WithName("DeleteUser")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Delete a user account";
                operation.Description = "Deletes a user account with the specified user ID.";
                operation.EnsureResponse("200", "User deleted");
                operation.EnsureResponse("404", "User not found");
                return operation;
            });

        group.MapPost("/request_password_reset", AuthHandlers.RequestPasswordReset)
            .AllowAnonymous()
            .WithName("RequestPasswordReset")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Request password reset";
                operation.Description = "Requests a password reset for the user by sending a code to their email.";
                operation.EnsureResponse("200", "Password reset requested");
                operation.EnsureResponse("400", "Invalid email");
                return operation;
            });

        group.MapPost("/confirm_password_reset", AuthHandlers.ConfirmPasswordReset)
            .AllowAnonymous()
            .WithName("ConfirmPasswordReset")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Confirm password reset";
                operation.Description = "Confirms the password reset with the provided code and sets a new password.";
                operation.EnsureResponse("200", "Password reset successful");
                operation.EnsureResponse("400", "Invalid code or password");
                return operation;
            });

        return app;
    }
}