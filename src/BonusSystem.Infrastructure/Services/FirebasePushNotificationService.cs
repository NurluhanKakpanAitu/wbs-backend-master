using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Infrastructure.DataAccess.Options;
using BonusSystem.Shared.Dtos;
using BonusSystem.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using BonusSystem.Localization;

namespace BonusSystem.Infrastructure.Services;

public class FirebasePushNotificationService : IFirebasePushNotificationService
{
    private readonly IDataService _dataService;
    private readonly ILogger<FirebasePushNotificationService> _logger;
    private readonly FirebaseOptions _firebaseOptions;

    public FirebasePushNotificationService(
        IDataService dataService,
        ILogger<FirebasePushNotificationService> logger,
        IOptions<FirebaseOptions> firebaseOptions,
        HttpClient httpClient)
    {
        _dataService = dataService;
        _logger = logger;
        _firebaseOptions = firebaseOptions.Value;
    }

    public async Task<PushNotificationResponse> SendToUserAsync(PushNotificationToUserRequest request)
    {
        try
        {
            var user = await _dataService.Users.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return new PushNotificationResponse
                {
                    Success = false,
                    Message = Res.Get("PushNotification.UserNotFound"),
                    FailureCount = 1
                };
            }

            if (string.IsNullOrEmpty(user.DeviceToken))
            {
                return new PushNotificationResponse
                {
                    Success = false,
                    Message = Res.Get("PushNotification.UserHasNoDeviceToken"),
                    FailureCount = 1
                };
            }

            var response = await SendFirebaseNotificationAsync(user.DeviceToken, request);
            return new PushNotificationResponse
            {
                Success = response,
                Message = response ? Res.Get("PushNotification.NotificationSentSuccessfully") : Res.Get("PushNotification.FailedToSendNotification"),
                SuccessCount = response ? 1 : 0,
                FailureCount = response ? 0 : 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to user {UserId}", request.UserId);
            return new PushNotificationResponse
            {
                Success = false,
                Message = Res.Get("PushNotification.InternalServerError"),
                FailureCount = 1,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<PushNotificationResponse> SendToMultipleUsersAsync(PushNotificationToMultipleUsersRequest request)
    {
        var response = new PushNotificationResponse();
        var errors = new List<string>();

        foreach (var userId in request.UserIds)
        {
            try
            {
                var userRequest = new PushNotificationToUserRequest
                {
                    UserId = userId,
                    Title = request.Title,
                    Body = request.Body,
                    ImageUrl = request.ImageUrl,
                    Data = request.Data,
                    Type = request.Type
                };

                var result = await SendToUserAsync(userRequest);
                if (result.Success)
                {
                    response.SuccessCount++;
                }
                else
                {
                    response.FailureCount++;
                    if (!string.IsNullOrEmpty(result.Message))
                        errors.Add($"User {userId}: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                response.FailureCount++;
                errors.Add($"User {userId}: {ex.Message}");
                _logger.LogError(ex, "Error sending push notification to user {UserId}", userId);
            }
        }

        response.Success = response.SuccessCount > 0;
        response.Message = Res.Format("PushNotification.SentToUsersSummary", response.SuccessCount, response.FailureCount);
        response.Errors = errors;

        return response;
    }

    public async Task<PushNotificationResponse> SendToRoleAsync(PushNotificationToRoleRequest request)
    {
        try
        {
            var users = await _dataService.Users.GetUsersByRoleAsync(request.Role);
            var userIds = users.Where(u => !string.IsNullOrEmpty(u.DeviceToken)).Select(u => u.Id).ToList();

            if (!userIds.Any())
            {
                return new PushNotificationResponse
                {
                    Success = false,
                    Message = Res.Format("PushNotification.NoUsersWithRoleHaveDeviceTokens", request.Role),
                    FailureCount = 0
                };
            }

            var bulkRequest = new PushNotificationToMultipleUsersRequest
            {
                UserIds = userIds,
                Title = request.Title,
                Body = request.Body,
                ImageUrl = request.ImageUrl,
                Data = request.Data,
                Type = request.Type
            };

            return await SendToMultipleUsersAsync(bulkRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to role {Role}", request.Role);
            return new PushNotificationResponse
            {
                Success = false,
                Message = Res.Get("PushNotification.InternalServerError"),
                FailureCount = 1,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<bool> UpdateDeviceTokenAsync(Guid userId, string deviceToken)
    {
        try
        {
            var user = await _dataService.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.DeviceToken = deviceToken;
            await _dataService.Users.UpdateAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating device token for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> RemoveDeviceTokenAsync(Guid userId)
    {
        try
        {
            var user = await _dataService.Users.GetByIdAsync(userId);
            if (user == null)
                return false;

            user.DeviceToken = null;
            await _dataService.Users.UpdateAsync(user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing device token for user {UserId}", userId);
            return false;
        }
    }

    public Task<PushNotificationResponse> SendWithDataAsync(PushNotificationRequest request, Dictionary<string, string> data)
    {
        var response = new PushNotificationResponse
        {
            Success = false,
            Message = Res.Get("PushNotification.MethodNotImplementedYet"),
            FailureCount = 1
        };
        
        return Task.FromResult(response);
    }

    public async Task<PushNotificationResponse> SendTestNotificationAsync(string deviceToken, string title, string body)
    {
        try
        {
            var testRequest = new PushNotificationRequest
            {
                Title = title ?? "Test Notification",
                Body = body ?? "This is a test notification",
                Type = NotificationType.System
            };

            var result = await SendFirebaseNotificationAsync(deviceToken, testRequest);
            return new PushNotificationResponse
            {
                Success = result,
                Message = result ? Res.Get("PushNotification.TestNotificationSentSuccessfully") : Res.Get("PushNotification.FailedToSendTestNotification"),
                SuccessCount = result ? 1 : 0,
                FailureCount = result ? 0 : 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test notification to device token");
            return new PushNotificationResponse
            {
                Success = false,
                Message = Res.Format("PushNotification.TestFailed", ex.Message),
                FailureCount = 1,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<PushNotificationResponse> SendDataOnlyMessageAsync(string deviceToken, Dictionary<string, string> data)
    {
        try
        {
            var result = await SendFirebaseDataOnlyMessageAsync(deviceToken, data);
            return new PushNotificationResponse
            {
                Success = result,
                Message = result ? Res.Get("PushNotification.DataMessageSentSuccessfully") : Res.Get("PushNotification.FailedToSendDataMessage"),
                SuccessCount = result ? 1 : 0,
                FailureCount = result ? 0 : 1
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending data-only message to device token");
            return new PushNotificationResponse
            {
                Success = false,
                Message = Res.Format("PushNotification.DataMessageFailed", ex.Message),
                FailureCount = 1,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<string?> GetUserDeviceTokenAsync(Guid userId)
    {
        try
        {
            var user = await _dataService.Users.GetByIdAsync(userId);
            return user?.DeviceToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device token for user {UserId}", userId);
            return null;
        }
    }

    private async Task<bool> SendFirebaseNotificationAsync(string deviceToken, PushNotificationRequest request)
    {
        try
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var credential = GoogleCredential.FromJson(CreateServiceAccountJson());
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = _firebaseOptions.ProjectId
                });
            }

            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification
                {
                    Title = request.Title,
                    Body = request.Body,
                    ImageUrl = request.ImageUrl
                },
                Data = request.Data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        ContentAvailable = true
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation(Res.Format("PushNotification.FirebaseMessageSentSuccessfully", response), response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Firebase notification: {Error}", ex.Message);
            return false;
        }
    }

    private async Task<bool> SendFirebaseDataOnlyMessageAsync(string deviceToken, Dictionary<string, string> data)
    {
        try
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                var credential = GoogleCredential.FromJson(CreateServiceAccountJson());
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = _firebaseOptions.ProjectId
                });
            }

            var message = new Message
            {
                Token = deviceToken,
                Data = data?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        ContentAvailable = true,
                        MutableContent = true
                    }
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation(Res.Format("PushNotification.FirebaseDataMessageSentSuccessfully", response), response);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Firebase data-only message: {Error}", ex.Message);
            return false;
        }
    }

    private string CreateServiceAccountJson()
    {
        var serviceAccount = new
        {
            type = "service_account",
            project_id = _firebaseOptions.ProjectId,
            private_key_id = _firebaseOptions.PrivateKeyId,
            private_key = _firebaseOptions.PrivateKey,
            client_email = _firebaseOptions.ClientEmail,
            client_id = _firebaseOptions.ClientId,
            auth_uri = _firebaseOptions.AuthUri,
            token_uri = _firebaseOptions.TokenUri,
            auth_provider_x509_cert_url = _firebaseOptions.AuthProviderX509CertUrl,
            client_x509_cert_url = _firebaseOptions.ClientX509CertUrl
        };

        return JsonSerializer.Serialize(serviceAccount);
    }
}
