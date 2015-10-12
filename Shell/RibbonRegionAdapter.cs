using System;
using Fluent;
using Prism.Regions;

namespace Shell
{
    public class RibbonRegionAdapter : RegionAdapterBase<Ribbon>
    {
        public RibbonRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
        {
        }

        protected override void Adapt(IRegion region, Ribbon regionTarget)
        {
        }

        protected override void AttachBehaviors(IRegion region, Ribbon regionTarget)
        {
            if (region == null)
            {
                throw new ArgumentNullException("region");
            }
            // Add the behavior that syncs the items source items with the rest of the items
            region.Behaviors.Add(RibbonTabsSyncBehavior.BehaviorKey, new RibbonTabsSyncBehavior
            {
                HostControl = regionTarget
            });
            base.AttachBehaviors(region, regionTarget);
        }

        protected override IRegion CreateRegion()
        {
            return new Region();
        }
    }
}