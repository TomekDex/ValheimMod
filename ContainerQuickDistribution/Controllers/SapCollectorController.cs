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
            ItemDrop.ItemData itemData = CreateItemData(MBComponet.m_spawnItem, level);
            if (itemData == null)
                return;
            if (ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, itemData))
            {
                MBComponet.ResetLevel();
                MBComponet.m_spawnEffect.Create(MBComponet.transform.position, Quaternion.identity);
            }
            else
            {
                MBComponet.m_nview.GetZDO().Set("level", itemData.m_stack);
                if (itemData.m_stack != level)
                    MBComponet.m_spawnEffect.Create(MBComponet.transform.position, Quaternion.identity);
            }
            MBComponet.UpdateEffects();
        }
    }
}