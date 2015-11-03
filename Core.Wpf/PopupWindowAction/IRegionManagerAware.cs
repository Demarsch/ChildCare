using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Wpf.PopupWindowActionAware
{
    public interface IRegionManagerAware
    {
        IRegionManager RegionManager { get; set; }
    }
}
