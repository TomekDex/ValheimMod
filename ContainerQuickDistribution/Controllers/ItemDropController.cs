
namespace TomekDexValheimMod.Controllers
{
    public class ItemDropController : ContainerQuickDistributionObject<ItemDrop>
    {
        public override void UpdateOnTime()
        {
            if (!MBComponet.m_nview.IsValid())
                return;
            if (!MBComponet.CanPickup())
                return;
            MBComponet.Load();
            if (ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, MBComponet.m_itemData))
                MBComponet.m_nview.Destroy();
            MBComponet.Save();
        }
    }
}
