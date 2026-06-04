using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace Spendly.Web.Helpers
{
    /// <summary>
    /// Translates default category names using localization resources.
    /// Custom user-created categories are returned as-is.
    /// </summary>
    public static class CategoryTranslator
    {
        private static readonly Dictionary<string, string> DefaultCategoryKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Food & Dining",      "DefCat_FoodDining" },
            { "Transportation",     "DefCat_Transportation" },
            { "Entertainment",      "DefCat_Entertainment" },
            { "Shopping",           "DefCat_Shopping" },
            { "Health",             "DefCat_Health" },
            { "Education",          "DefCat_Education" },
            { "Bills & Utilities",  "DefCat_BillsUtilities" },
            { "Other",              "DefCat_Other" }
        };

        /// <summary>
        /// Returns the localized name for default categories, or the original name for custom ones.
        /// Overload for IStringLocalizer (used in most views).
        /// </summary>
        public static string Translate(string categoryName, IStringLocalizer localizer)
        {
            if (string.IsNullOrEmpty(categoryName))
                return categoryName;

            if (DefaultCategoryKeys.TryGetValue(categoryName, out var resourceKey))
            {
                var translated = localizer[resourceKey];
                return translated.ResourceNotFound ? categoryName : translated.Value;
            }

            return categoryName;
        }

        /// <summary>
        /// Returns the localized name for default categories, or the original name for custom ones.
        /// Overload for IHtmlLocalizer (used in views like Reports that inject IHtmlLocalizer).
        /// </summary>
        public static string Translate(string categoryName, IHtmlLocalizer localizer)
        {
            if (string.IsNullOrEmpty(categoryName))
                return categoryName;

            if (DefaultCategoryKeys.TryGetValue(categoryName, out var resourceKey))
            {
                var translated = localizer.GetString(resourceKey);
                return translated.ResourceNotFound ? categoryName : translated.Value;
            }

            return categoryName;
        }
    }
}
