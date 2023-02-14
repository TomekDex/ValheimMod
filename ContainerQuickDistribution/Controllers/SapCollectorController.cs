using UnityEngine;

namespace TomekDexValheimMod.Controllers
{
    public class SapCollectorController : ContainerQuickDistributionObject<SapCollector>
    {
        public override void UpdateOnTime()
        {
            int level = MBComponet.GetLevel();
            if (level <= 0)
                return;
            MBComponet.m_spawnEffect.Create(MBComponet.m_spawnPoint.position, Quaternion.identity);
            ItemDrop item = Instantiate(MBComponet.m_spawnItem);
            item.m_itemData.m_stack = level;
            if (ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, item))
            {
                MBComponet.ResetLevel();
                MBComponet.m_spawnEffect.Create(MBComponet.transform.position, Quaternion.identity);
            }
            else
            {
                MBComponet.m_nview.GetZDO().Set("level", item.m_itemData.m_stack);
                if (item.m_itemData.m_stack != level)
                    MBComponet.m_spawnEffect.Create(MBComponet.transform.position, Quaternion.identity);
            }
            MBComponet.UpdateEffects();
        }
    }
}