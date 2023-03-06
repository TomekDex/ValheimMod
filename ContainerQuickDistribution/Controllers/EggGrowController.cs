namespace TomekDexValheimMod.Controllers
{
    public class EggGroweController : ContainerQuickDistributionObject<EggGrow>
    {
        public override void UpdateOnTime()
        {
            if (MBComponet.CanGrow())
                return;
            if (ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, MBComponet.m_item.m_itemData))
                MBComponet.m_nview.Destroy();
        }
    }
}