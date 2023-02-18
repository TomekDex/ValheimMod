namespace TomekDexValheimMod.Controllers
{
    public class PickableController : ContainerQuickDistributionObject<Pickable>
    {
        public override void UpdateOnTime()
        {
            if (MBComponet.m_picked)
                return;
            if (!MBComponet.m_nview.IsOwner())
                return;
            if (MBComponet.m_tarPreventsPicking)
            {
                if (MBComponet.m_floating == null)
                    MBComponet.m_floating = GetComponent<Floating>();
                if (MBComponet.m_floating && MBComponet.m_floating.IsInTar())
                    return;
            }
            ItemDrop item = Instantiate(MBComponet.m_itemPrefab).GetComponent<ItemDrop>();
            item.m_itemData.m_stack = MBComponet.m_amount;
            if (!ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, item))
            {
                ContainerQuickAccess.TryRemoveItemRegistertNearbyContainer(MBComponet.transform.position, WorkingArea, item, MBComponet.m_amount - item.m_itemData.m_stack);
                return;
            }
            if (!MBComponet.m_extraDrops.IsEmpty())
            {
                foreach (ItemDrop.ItemData dropListItem in MBComponet.m_extraDrops.GetDropListItems())
                {
                    item = Instantiate(dropListItem.m_dropPrefab).GetComponent<ItemDrop>();
                    item.m_itemData.m_stack = dropListItem.m_stack;
                    int offset = 1;
                    if (!ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, item))
                        MBComponet.Drop(dropListItem.m_dropPrefab, offset++, item.m_itemData.m_stack);
                }
            }
            MBComponet.m_nview.InvokeRPC(ZNetView.Everybody, "SetPicked", true);
        }
    }
}