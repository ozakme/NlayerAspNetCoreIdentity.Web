using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AspNetCoreIdentity.Web.Extensions
{
    public static class ModelStateExtensions
    {
        public static void AddModelErrorList(this ModelStateDictionary modelState, List<string> errors)
        {
            errors.ForEach(x =>
            {
                modelState.AddModelError(string.Empty, x);
            });
        }

        public static void AddModelErrorList(this ModelStateDictionary modelState, IEnumerable<IdentityError> errors)
        {
            errors.ToList().ForEach(x =>
            {
                modelState.AddModelError(string.Empty, x.Description);
            });
        }
    }
}
