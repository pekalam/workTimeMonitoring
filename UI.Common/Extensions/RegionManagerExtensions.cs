using Prism.Regions;
using System.Linq;

namespace UI.Common.Extensions
{
    public static class RegionManagerExtensions
    {
        public static void RemoveActiveView(this IRegion region)
        {
            region.Remove(region.ActiveViews.First());
        }
    }
}