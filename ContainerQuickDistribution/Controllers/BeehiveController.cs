using UnityEngine;

namespace TomekDexValheimMod.Controllers
{
    public class BeehiveController : ContainerQuickDistributionObject<Beehive>
    {
        public override void UpdateOnTime()
        {
            int honeyLevel = MBComponet.GetHoneyLevel();
            if (honeyLevel > 0)
            {
                ItemDrop.ItemData itemData = CreateItemData(MBComponet.m_honeyItem, honeyLevel);
                if (itemData == null)
                    return;
                if (ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, itemData))
                {
                    MBComponet.m_nview.GetZDO().Set("level", 0);
                    MBComponet.m_spawnEffect.Create(MBComponet.transform.position, Quaternion.identity);
                }
                else
                {
                    MBComponet.m_nview.GetZDO().Set("level", itemData.m_stack);
                    if (itemData.m_stack != honeyLevel)
                        MBComponet.m_spawnEffect.Create(MBComponet.transform.position, Quaternion.identity);
                }
            }
        }
    }
}