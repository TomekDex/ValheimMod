namespace TomekDexValheimMod.Controllers
{
    public class TurretController : ContainerQuickDistributionObject<Turret>
    {
        public override void UpdateOnTime()
        {
            int ammoIn = MBComponet.GetAmmo();
            if (MBComponet.m_maxAmmo > ammoIn)
                foreach (Turret.AmmoType ammo in MBComponet.m_allowedAmmo)
                    if (TryAddAmmo(ammoIn, ammo))
                        break;
        }

        private bool TryAddAmmo(int ammoIn, Turret.AmmoType ammo)
        {
            if (ammoIn != 0 && MBComponet.m_nview.GetZDO().GetString("ammoType") != ammo.m_ammo.name)
                return false;
            int ammoToAdd = ContainerQuickAccess.TryRemoveItemRegistertNearbyContainer(MBComponet.transform.position, WorkingArea, ammo.m_ammo, MBComponet.m_maxAmmo - ammoIn);
            if (ammoToAdd == 0)
                return false;
            ammoIn += ammoToAdd;
            MBComponet.m_nview.GetZDO().Set("ammo", ammoIn);
            MBComponet.m_nview.GetZDO().Set("ammoType", ammo.m_ammo.name);
            MBComponet.m_addAmmoEffect.Create(MBComponet.m_turretBody.transform.position, MBComponet.m_turretBody.transform.rotation);
            MBComponet.UpdateVisualBolt();
            if (MBComponet.m_maxAmmo <= ammoIn)
                return true;
            return false;
        }
    }
}
