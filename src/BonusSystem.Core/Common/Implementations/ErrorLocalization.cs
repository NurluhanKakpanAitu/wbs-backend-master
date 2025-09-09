using BonusSystem.Core.Common.Interfaces; 
namespace BonusSystem.Core.Common.Implementations; 

public class ErrorLocalizationService : IErrorLocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _errors = new()
    {
        ["ru"] = new()
        {
            ["UserNotFound"] = "Пользователь не найден",
            ["InvalidPassword"] = "Неверный пароль"
        },
        ["en"] = new()
        {
            ["UserNotFound"] = "User not found",
            ["InvalidPassword"] = "Invalid password"
        }
    };

    public string GetErrorMessage(string key, string language)
    {
        if (_errors.TryGetValue(language, out var dict) && dict.TryGetValue(key, out var msg))
            return msg;
        // fallback
        return _errors["en"].TryGetValue(key, out var enMsg) ? enMsg : key;
    }
}