using UnityEngine;

namespace TomekDexValheimMod.Controllers
{
    public class FireplaceController : ContainerQuickDistributionObject<Fireplace>
    {
        public override void UpdateOnTime()
        {
            float maxFuel = MBComponet.m_maxFuel;
            float fuel = MBComponet.m_nview.GetZDO().GetFloat("fuel");
            if (Mathf.CeilToInt(fuel) >= maxFuel)
                return;
            fuel += ContainerQuickAccess.TryRemoveItemRegistertNearbyContainer(MBComponet.transform.position, WorkingArea, MBComponet.m_fuelItem, Mathf.FloorToInt(maxFuel - fuel));
            MBComponet.m_nview.GetZDO().Set("fuel", fuel);
            MBComponet.m_fuelAddedEffects.Create(MBComponet.transform.position, MBComponet.transform.rotation);
            MBComponet.UpdateState();
        }
    }
}