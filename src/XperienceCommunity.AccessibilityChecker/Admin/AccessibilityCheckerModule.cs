using CMS;
using CMS.Core;

using Kentico.Xperience.Admin.Base;

using XperienceCommunity.AccessibilityChecker.Admin;

[assembly: AssemblyDiscoverable]
[assembly: RegisterModule(typeof(AccessibilityCheckerModule))]

namespace XperienceCommunity.AccessibilityChecker.Admin
{
    public class AccessibilityCheckerModule : AdminModule
    {
        public AccessibilityCheckerModule()
            : base("XperienceCommunity.AccessibilityChecker")
        {
        }

        protected override void OnInit(ModuleInitParameters parameters)
        {
            base.OnInit(parameters);

            RegisterClientModule("xperiencecommunity", "accessibility.checker");

        }
    }
}
