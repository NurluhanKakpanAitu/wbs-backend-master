namespace BonusSystem.Core.Common.Interfaces; 
public interface IErrorLocalizationService
{
    string GetErrorMessage(string key, string language);
}