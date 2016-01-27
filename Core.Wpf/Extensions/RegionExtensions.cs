using System;
using System.Linq;
using Prism.Regions;

namespace Core.Wpf.Extensions
{
    public static class RegionExtensions
    {
        public static void DeactivateActiveViews(this IRegion region)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            foreach (var activeView in region.ActiveViews.ToArray())
            {
                region.Deactivate(activeView);
            }
        }
    }
}
