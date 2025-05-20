using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Items
{
    public static class CommodityResources
    {
        private static readonly Type[] NonCommodityResourceTypes = new Type[]
        {
            typeof(Wool),
            typeof(BaseClothMaterial),
            typeof(Flax),
            typeof(Cotton),
            typeof(BaseGranite),
            typeof(Fish),
            typeof(BigFish),
            typeof(BaseMagicFish),
            typeof(BaseFish),
            typeof(BaseOre)
        };

        private static readonly Type[] NonResourceCommodityTypes = new Type[]
        {
            typeof(Arrow),
            typeof(Bolt),
            typeof(BaseReagent),
            typeof(SpellScroll),
            typeof(BasePotion)
        };

        public static bool IsNonCommodityResource(Item item)
        {
            int nonCommodityResourceTypesLength = NonCommodityResourceTypes.Length;
            for (int i = 0; i < nonCommodityResourceTypesLength; i++)
            {
                if (NonCommodityResourceTypes[i].IsInstanceOfType(item))
                    return true;
            }
            return false;
        }

        public static bool IsNonResourceCommodity(Item item)
        {
            int nonResourceCommodityTypesLength = NonResourceCommodityTypes.Length;
            for (int i = 0; i < nonResourceCommodityTypesLength; i++)
            {
                if (NonResourceCommodityTypes[i].IsInstanceOfType(item))
                    return true;
            }
            return false;
        }
    }
}
