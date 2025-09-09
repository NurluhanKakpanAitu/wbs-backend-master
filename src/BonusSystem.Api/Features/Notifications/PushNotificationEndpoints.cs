using BonusSystem.Api.Infrastructure.Swagger;
using BonusSystem.Api.Helpers;
using BonusSystem.Core.Services.Interfaces;
using BonusSystem.Shared.Dtos;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using BonusSystem.Localization;

namespace BonusSystem.Api.Features.Notifications;

public static class PushNotificationEndpoints
{
    public static void MapPushNotificationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/push-notification")
            .WithTags("Push Notifications")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapPost("/send-to-user", async (
            PushNotificationToUserRequest request,
            IFirebasePushNotificationService pushNotificationService) =>
        {
            var result = await pushNotificationService.SendToUserAsync(request);
            return Results.Ok(result);
        })
        .WithName("SendPushNotificationToUser")
        .Produces<PushNotificationResponse>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized)
        .Produces<object>(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Send push notification to a specific user";
            operation.Description = "Sends a push notification to a specific user identified by their user ID.\n\n" +
                "Request requires:\n" +
                "- userId: The unique identifier of the target user\n" +
                "- title: Notification title\n" +
                "- body: Notification message body\n" +
                "- data: Optional additional data to send with the notification\n\n" +
                "Successful response contains the notification delivery status.";
            operation.EnsureResponse("200", "Notification sent successfully");
            operation.EnsureResponse("400", "Invalid request or user not found");
            operation.EnsureResponse("401", "Unauthorized");
            operation.EnsureResponse("500", "Internal server error");
            return operation;
        });

        group.MapPost("/send-to-multiple-users", async (
            PushNotificationToMultipleUsersRequest request,
            IFirebasePushNotificationService pushNotificationService) =>
        {
            var result = await pushNotificationService.SendToMultipleUsersAsync(request);
            return Results.Ok(result);
        })
        .WithName("SendPushNotificationToMultipleUsers")
        .Produces<PushNotificationResponse>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized)
        .Produces<object>(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Send push notification to multiple users";
            operation.Description = "Sends a push notification to multiple users identified by their user IDs.\n\n" +
                "Request requires:\n" +
                "- userIds: Array of unique identifiers for target users\n" +
                "- title: Notification title\n" +
                "- body: Notification message body\n" +
                "- data: Optional additional data to send with the notification\n\n" +
                "Successful response contains the notification delivery status for each user.";
            operation.EnsureResponse("200", "Notifications sent successfully");
            operation.EnsureResponse("400", "Invalid request or users not found");
            operation.EnsureResponse("401", "Unauthorized");
            operation.EnsureResponse("500", "Internal server error");
            return operation;
        });

        group.MapPost("/send-to-role", async (
            PushNotificationToRoleRequest request,
            IFirebasePushNotificationService pushNotificationService) =>
        {
            var result = await pushNotificationService.SendToRoleAsync(request);
            return Results.Ok(result);
        })
        .WithName("SendPushNotificationToRole")
        .Produces<PushNotificationResponse>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized)
        .Produces<object>(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Send push notification to all users with a specific role";
            operation.Description = "Sends a push notification to all users who have a specific role in the system.\n\n" +
                "Request requires:\n" +
                "- role: The role identifier (0=Buyer, 1=Seller, 2=StoreAdmin, 3=SystemAdmin, 4=CompanyObserver, 5=SystemObserver)\n" +
                "- title: Notification title\n" +
                "- body: Notification message body\n" +
                "- data: Optional additional data to send with the notification\n\n" +
                "Successful response contains the notification delivery status for all users with the specified role.";
            operation.EnsureResponse("200", "Notifications sent successfully");
            operation.EnsureResponse("400", "Invalid request or role not found");
            operation.EnsureResponse("401", "Unauthorized");
            operation.EnsureResponse("500", "Internal server error");
            return operation;
        });

        group.MapPut("/device-token", async (
            HttpContext httpContext,
            DeviceTokenUpdateRequestWithoutUserId request,
            IFirebasePushNotificationService pushNotificationService) =>
        {
            return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
            {
                var result = await pushNotificationService.UpdateDeviceTokenAsync(userId, request.DeviceToken);
                if (result)
                {
                    return Results.Ok(new MessageResponseDto { Message = "Device token updated successfully" });
                }
                return Results.BadRequest(new MessageResponseDto { Message = "Failed to update device token" });
            }, "Error updating device token");
        })
        .WithName("UpdateDeviceToken")
        .Produces<MessageResponseDto>(StatusCodes.Status200OK)
        .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized)
        .Produces<object>(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Update current user's device token";
            operation.Description = "Updates the Firebase device token for the currently authenticated user to enable push notifications.\n\n" +
                "Request requires:\n" +
                "- deviceToken: The Firebase FCM device token for the user's device\n\n" +
                "The user ID is automatically extracted from the JWT authentication token.\n" +
                "Successful response contains a confirmation message.";
            operation.EnsureResponse("200", "Device token updated successfully");
            operation.EnsureResponse("400", "Invalid request or user not found");
            operation.EnsureResponse("401", "Unauthorized");
            operation.EnsureResponse("500", "Internal server error");
            return operation;
        });

        group.MapDelete("/device-token", async (
            HttpContext httpContext,
            IFirebasePushNotificationService pushNotificationService) =>
        {
            return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
            {
                var result = await pushNotificationService.RemoveDeviceTokenAsync(userId);
                if (result)
                {
                    return Results.Ok(new MessageResponseDto { Message = "Device token removed successfully" });
                }
                return Results.BadRequest(new MessageResponseDto { Message = "Failed to remove device token" });
            }, "Error removing device token");
        })
        .WithName("RemoveDeviceToken")
        .Produces<MessageResponseDto>(StatusCodes.Status200OK)
        .Produces<MessageResponseDto>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized)
        .Produces<object>(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Remove current user's device token";
            operation.Description = "Removes the Firebase device token for the currently authenticated user, disabling push notifications.\n\n" +
                "No parameters required. The user ID is automatically extracted from the JWT authentication token.\n\n" +
                "Successful response contains a confirmation message.";
            operation.EnsureResponse("200", "Device token removed successfully");
            operation.EnsureResponse("400", "Invalid request or user not found");
            operation.EnsureResponse("401", "Unauthorized");
            operation.EnsureResponse("500", "Internal server error");
            return operation;
        });

        group.MapPost("/test", async (
            string deviceToken,
            string title,
            string body,
            IFirebasePushNotificationService pushNotificationService) =>
        {
            var result = await pushNotificationService.SendTestNotificationAsync(deviceToken, title, body);
            return Results.Ok(result);
        })
        .WithName("TestPushNotification")
        .Produces<PushNotificationResponse>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized)
        .Produces<object>(StatusCodes.Status500InternalServerError);

        group.MapPost("/send-data-only", async (
            HttpContext httpContext,
            Dictionary<string, string> data,
            IFirebasePushNotificationService pushNotificationService) =>
        {
            return await RequestHelper.ProcessAuthenticatedRequest(httpContext, async userId =>
            {
                // Get user's device token from the database
                var user = await pushNotificationService.GetUserDeviceTokenAsync(userId);
                if (string.IsNullOrEmpty(user))
                {
                    return Results.BadRequest(new MessageResponseDto { Message = "User has no device token registered" });
                }

                var result = await pushNotificationService.SendDataOnlyMessageAsync(user, data);
                return Results.Ok(result);
            }, "Error sending data-only message");
        })
        .WithName("SendDataOnlyMessage")
        .Produces<PushNotificationResponse>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized)
        .Produces<object>(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
                         operation.Summary = "Send data-only message to current user (silent notification)";
             operation.Description = "Sends a data-only message to the currently authenticated user's device.\n\n" +
                 "Request body:\n" +
                 "- data: Custom key-value pairs to send with the message (required)\n\n" +
                 "The device token is automatically extracted from the user's profile.\n" +
                 "Example data: {\"action\": \"sync\", \"timestamp\": \"2024-01-01T00:00:00Z\", \"userId\": \"123\"}";
            operation.EnsureResponse("200", "Data message sent successfully");
            operation.EnsureResponse("400", "Invalid request or device token");
            operation.EnsureResponse("401", "Unauthorized");
            operation.EnsureResponse("500", "Internal server error");
            return operation;
        });

        group.MapPost("/send-data-only-to-user", async (
            Guid targetUserId,
            Dictionary<string, string> data,
            IFirebasePushNotificationService pushNotificationService) =>
        {
            // Get target user's device token from the database
            var deviceToken = await pushNotificationService.GetUserDeviceTokenAsync(targetUserId);
            if (string.IsNullOrEmpty(deviceToken))
            {
                return Results.BadRequest(new MessageResponseDto { Message = Res.Get("PushNotification.TargetUserHasNoDeviceToken") });
            }

            var result = await pushNotificationService.SendDataOnlyMessageAsync(deviceToken, data);
            return Results.Ok(result);
        })
        .WithName("SendDataOnlyMessageToUser")
        .Produces<PushNotificationResponse>(StatusCodes.Status200OK)
        .Produces<object>(StatusCodes.Status400BadRequest)
        .Produces<object>(StatusCodes.Status401Unauthorized)
        .Produces<object>(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Send data-only message to specific user by ID (silent notification)";
            operation.Description = "Sends a data-only message to a specific user identified by their user ID.\n\n" +
                "Request parameters:\n" +
                "- targetUserId: The unique identifier of the target user (required)\n" +
                "- data: Custom key-value pairs to send with the message (required)\n\n" +
                "Example data: {\"action\": \"sync\", \"timestamp\": \"2024-01-01T00:00:00Z\", \"userId\": \"123\"}";
            operation.EnsureResponse("200", "Data message sent successfully");
            operation.EnsureResponse("400", "Invalid request or user has no device token");
            operation.EnsureResponse("401", "Unauthorized");
            operation.EnsureResponse("500", "Internal server error");
            return operation;
        });

        // Firebase test page endpoint - serves HTML page for FCM token generation
        group.MapGet("/test-page", () =>
        {
            var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Firebase FCM Token Test</title>
    <script src=""https://www.gstatic.com/firebasejs/10.7.1/firebase-app-compat.js""></script>
    <script src=""https://www.gstatic.com/firebasejs/10.7.1/firebase-messaging-compat.js""></script>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        button { margin: 10px; padding: 10px 20px; font-size: 16px; }
        #tokenDisplay { margin: 20px; padding: 15px; background: #f5f5f5; border: 1px solid #ddd; border-radius: 5px; word-break: break-all; }
        .success { color: green; }
        .error { color: red; }
        .info { color: blue; }
    </style>
</head>
<body>
    <h1>Firebase FCM Token Test</h1>
    <p>This page will help you get a real FCM token for testing push notifications.</p>
    
    <div>
        <button onclick=""requestPermission()"">Request Notification Permission</button>
        <button onclick=""getToken()"">Get FCM Token</button>
        <button onclick=""copyToken()"">Copy Token</button>
        <button onclick=""testNotification()"">Test Notification</button>
    </div>
    
    <div id=""status""></div>
    <div id=""tokenDisplay""></div>
    
    <script>
        // Firebase config - you'll need to update these values
        const firebaseConfig = {
            apiKey: ""AIzaSyBRAi0H5lKxqPYStWrQPCIovBHPNytD3n4"",
            authDomain: ""world-bonus-system-6d14b.firebaseapp.com"",
            projectId: ""world-bonus-system-6d14b"",
            storageBucket: ""world-bonus-system-6d14b.firebasestorage.app"",
            messagingSenderId: ""1018515867174"",
            appId: ""1:1018515867174:web:0f9a22eb59f232050508e7"",
            measurementId: ""G-BZ4NMPGK5Q""
        };

        let currentToken = '';
        let messaging;

        // Initialize Firebase
        try {
            firebase.initializeApp(firebaseConfig);
            messaging = firebase.messaging();
            
            // Register service worker for FCM
            if ('serviceWorker' in navigator) {
                navigator.serviceWorker.register('/firebase-messaging-sw.js')
                    .then((registration) => {
                        updateStatus('Service worker registered successfully', 'success');
                    })
                    .catch((error) => {
                        updateStatus('Service worker registration failed: ' + error.message, 'error');
                    });
            }
            
            updateStatus('Firebase initialized successfully', 'success');
        } catch (error) {
            updateStatus('Firebase initialization failed: ' + error.message, 'error');
        }

        function updateStatus(message, type = 'info') {
            const statusDiv = document.getElementById('status');
            statusDiv.innerHTML = `<div class=""${type}"">${message}</div>`;
        }

        async function requestPermission() {
            try {
                updateStatus('Requesting notification permission...', 'info');
                const permission = await Notification.requestPermission();
                if (permission === 'granted') {
                    updateStatus('Notification permission granted!', 'success');
                } else {
                    updateStatus('Notification permission denied!', 'error');
                }
            } catch (error) {
                updateStatus('Error requesting permission: ' + error.message, 'error');
            }
        }

        async function getToken() {
            try {
                updateStatus('Getting FCM token...', 'info');
                currentToken = await messaging.getToken();
                if (currentToken) {
                    document.getElementById('tokenDisplay').innerHTML = 
                        '<strong>FCM Token:</strong><br>' + currentToken;
                    updateStatus('FCM token received successfully!', 'success');
                } else {
                    updateStatus('No token received', 'error');
                }
            } catch (error) {
                updateStatus('Error getting token: ' + error.message, 'error');
            }
        }

        function copyToken() {
            if (currentToken) {
                navigator.clipboard.writeText(currentToken);
                updateStatus('Token copied to clipboard!', 'success');
            } else {
                updateStatus('No token to copy! Get token first.', 'error');
            }
        }

        function testNotification() {
            if (currentToken) {
                updateStatus('Testing notification...', 'info');
                // Send test notification to your API
                fetch('/api/push-notification/test', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        deviceToken: currentToken,
                        title: 'Test from Browser',
                        body: 'This is a test notification from the browser!'
                    })
                })
                .then(response => response.json())
                .then(data => {
                    updateStatus('Test notification sent: ' + JSON.stringify(data), 'success');
                })
                .catch(error => {
                    updateStatus('Test failed: ' + error.message, 'error');
                });
            } else {
                updateStatus('No token available! Get token first.', 'error');
            }
        }

        // Handle incoming messages
        if (messaging) {
            messaging.onMessage((payload) => {
                console.log('Message received:', payload);
                updateStatus('📨 Notification received: ' + payload.notification.title, 'success');
                
                // Show browser notification
                if (Notification.permission === 'granted') {
                    new Notification(payload.notification.title, {
                        body: payload.notification.body,
                        icon: payload.notification.icon || '/favicon.ico'
                    });
                }
            });
        }
    </script>
</body>
</html>";

            return Results.Content(html, "text/html");
        })
        .WithName("FirebaseTestPage")
        .Produces<string>(StatusCodes.Status200OK)
        .WithOpenApi(operation =>
        {
            operation.Summary = "Get Firebase FCM token test page";
            operation.Description = "Returns an HTML page that allows you to generate Firebase FCM tokens in your browser for testing push notifications.\n\n" +
                "This page includes:\n" +
                "- Firebase Web SDK integration\n" +
                "- Notification permission request\n" +
                "- FCM token generation\n" +
                "- Token copying functionality\n" +
                "- Test notification sending\n\n" +
                "The page is served anonymously and doesn't require authentication.";
            operation.EnsureResponse("200", "HTML test page");
            return operation;
        })
        .AllowAnonymous();
    }
}
