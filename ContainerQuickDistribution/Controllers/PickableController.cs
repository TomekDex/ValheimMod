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
            // create ItemData clone from prefab without instantiating
            ItemDrop.ItemData itemData = CreateItemData(MBComponet.m_itemPrefab, MBComponet.m_amount);
            if (itemData == null)
                return;
            int originalAmount = itemData.m_stack;
            if (!ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, itemData))
            {
                int added = originalAmount - itemData.m_stack;
                if (added > 0)
                {
                    // rollback partially added items
                    ContainerQuickAccess.TryRemoveItemRegistertNearbyContainer(MBComponet.transform.position, WorkingArea, itemData, added);
                }
                return;
            }
            if (!MBComponet.m_extraDrops.IsEmpty())
            {
                int offsetBase = 1;
                foreach (ItemDrop.ItemData dropListItem in MBComponet.m_extraDrops.GetDropListItems())
                {
                    ItemDrop.ItemData extraData = CreateItemData(dropListItem.m_dropPrefab, dropListItem.m_stack);
                    if (extraData == null)
                        continue;
                    int offset = offsetBase;
                    if (!ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, extraData))
                        MBComponet.Drop(dropListItem.m_dropPrefab, offset++, extraData.m_stack);
                }
            }
            MBComponet.m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetPicked", true);
        }
    }
}