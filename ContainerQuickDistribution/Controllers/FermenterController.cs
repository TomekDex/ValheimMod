using TomekDexValheimModHelper;
using UnityEngine;
using static Fermenter;

namespace TomekDexValheimMod.Controllers
{
    public class FermenterController : ContainerQuickDistributionObject<Fermenter>
    {
        public override void UpdateOnTime()
        {
            Status status = MBComponet.GetStatus();
            if (Status.Ready == status)
            {
                Teak();
                Add();
            }
            else if (Status.Empty == status)
                Add();
        }

        private void Teak()
        {
            ItemConversion itemConversion = MBComponet.GetItemConversion(MBComponet.GetContent());
            if (itemConversion == null)
                return;
            ItemDrop item = Instantiate(itemConversion.m_to);
            item.m_itemData.m_stack = itemConversion.m_producedItems;
            if (!ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, item.m_itemData))
            {
                Vector3 position = MBComponet.m_outputPoint.position + Vector3.up * 0.3f;
                ItemsHelper.Drop(item, position);
            }
            MBComponet.m_tapEffects.Create(MBComponet.transform.position, MBComponet.transform.rotation);
            MBComponet.m_nview.GetZDO().Set("Content", "");
            MBComponet.m_nview.GetZDO().Set("StartTime", 0);
        }

        private void Add()
        {
            foreach (ItemConversion item in MBComponet.m_conversion)
                if (ContainerQuickAccess.TryRemoveItemRegistertNearbyContainer(MBComponet.transform.position, WorkingArea, item.m_from, 1) == 1)
                {
                    MBComponet.m_addedEffects.Create(MBComponet.transform.position, MBComponet.transform.rotation);
                    MBComponet.m_nview.GetZDO().Set("Content", item.m_from.name);
                    MBComponet.m_nview.GetZDO().Set("StartTime", ZNet.instance.GetTime().Ticks);
                    return;
                }
        }
    }
}