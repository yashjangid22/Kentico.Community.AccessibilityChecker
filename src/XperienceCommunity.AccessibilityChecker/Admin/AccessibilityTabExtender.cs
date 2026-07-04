using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;

using XperienceCommunity.AccessibilityChecker.Admin;

[assembly: UIApplication(
    identifier: "xperiencecommunity.accessibility-checker",
    type: typeof(AccessibilityCheckerApplication),
    slug: "accessibility-checker",
    name: "Accessibility checker",
    category: BaseApplicationCategories.DEVELOPMENT,
    icon: Icons.Eye,
    templateName: TemplateNames.SECTION_LAYOUT)]

[assembly: UIPage(
    parentType: typeof(AccessibilityCheckerApplication),
    slug: "checker",
    uiPageType: typeof(AccessibilityCheckerPage),
    name: "Checker",
    templateName: "@xperiencecommunity/accessibility.checker/Tab",
    order: UIPageOrder.NoOrder)]

namespace XperienceCommunity.AccessibilityChecker.Admin
{
    public class AccessibilityCheckerApplication : ApplicationPage
    {
        public const string IDENTIFIER = "xperiencecommunity.accessibility-checker";
    }

    public class AccessibilityCheckerClientProperties : TemplateClientProperties
    {
        // Empty — React component fetches its own data
    }

    public class AccessibilityCheckerPage : Page<AccessibilityCheckerClientProperties>
    {
        public override Task<AccessibilityCheckerClientProperties> ConfigureTemplateProperties(
            AccessibilityCheckerClientProperties properties) =>
            Task.FromResult(properties);
    }
}
