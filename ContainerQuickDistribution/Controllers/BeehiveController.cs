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
                ItemDrop item = Instantiate(MBComponet.m_honeyItem);
                item.m_itemData.m_stack = honeyLevel;
                if (ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, item.m_itemData))
                {
                    MBComponet.m_nview.GetZDO().Set("level", 0);
                    MBComponet.m_spawnEffect.Create(MBComponet.transform.position, Quaternion.identity);
                }
                else
                {
                    MBComponet.m_nview.GetZDO().Set("level", item.m_itemData.m_stack);
                    if (item.m_itemData.m_stack != honeyLevel)
                        MBComponet.m_spawnEffect.Create(MBComponet.transform.position, Quaternion.identity);
                }
            }
        }
    }
}