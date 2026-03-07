using System.ComponentModel.DataAnnotations;

namespace TournamentManager.API.Validations
{
    public class AllowedTeamCountAttribute:ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is int teams)
            {
                int[] validBracketSizes = { 2,4,8,16,32,64};

                if (validBracketSizes.Contains(teams))
                {
                    return ValidationResult.Success;   
                }
            }

            return new ValidationResult("Invalid bracket size. Number of teams must be 2, 4, 8, 16, 32, or 64.");
        }
    }
}
