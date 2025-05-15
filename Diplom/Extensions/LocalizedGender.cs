using Diplom.Models;

namespace Diplom.Extensions
{
    public static class LocalizedGender
    {
        public static string GetLocalizedGender(this ApplicationUser user)
        {
            return user.Gender switch
            {
                "Male" => "Мужской",
                "Female" => "Женский",
                _ => "Не указан"
            };
        }
    } 
}
