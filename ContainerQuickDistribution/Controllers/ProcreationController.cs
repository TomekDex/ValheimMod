using System.Collections.Generic;
using TomekDexValheimModHelper;
using UnityEngine;


namespace TomekDexValheimMod.Controllers
{
    public class ProcreationController : ContainerQuickDistributionObject<Procreation>
    {
        private Vector3 lastPosition;

        public override void UpdateOnTime()
        {
            if (!MBComponet.m_nview.IsValid() || !MBComponet.m_nview.IsOwner() || !MBComponet.m_character.IsTamed())
                return;
            GetNrOfInstances(out int peers, out int offspring, out int partners);
            if (peers + offspring <= MBComponet.m_maxCreatures)
                return;
            if (!MBComponet.m_noPartnerOffspring && (!MBComponet.m_seperatePartner && partners < 2 || MBComponet.m_seperatePartner && partners < 1))
                return;
            if (lastPosition == default)
                lastPosition = new Vector3(MBComponet.transform.position.x, MBComponet.transform.position.y, MBComponet.transform.position.z);
            else
            {
                float distance = Vector3.Distance(lastPosition, MBComponet.transform.position);
                if (distance >= 4)
                {
                    ContainerQuickAccess.UnRegistertNearbyContainer(lastPosition, WorkingArea);
                    lastPosition = new Vector3(MBComponet.transform.position.x, MBComponet.transform.position.y, MBComponet.transform.position.z);
                }
            }
            if (ContainerQuickDistributionConfig.Logs)
                Debug.Log($"Found Peers {peers}, offsping {offspring}, partners {partners}, max peers and offsping is {MBComponet.m_maxCreatures} {MBComponet.name}");
            if (!ContainerQuickAccess.IsAnyRegistertNearbyContainer(MBComponet.transform.position, WorkingArea))
                return;
            Kill();
        }

        private void GetNrOfInstances(out int peers, out int offspring, out int partners)
        {
            if (MBComponet.m_offspringPrefab == null)
            {
                string prefabName = Utils.GetPrefabName(MBComponet.m_offspring);
                MBComponet.m_offspringPrefab = ZNetScene.instance.GetPrefab(prefabName);
                int prefab = MBComponet.m_nview.GetZDO().GetPrefab();
                MBComponet.m_myPrefab = ZNetScene.instance.GetPrefab(prefab);
            }
            peers = SpawnSystem.GetNrOfInstances(MBComponet.m_myPrefab, MBComponet.transform.position, MBComponet.m_totalCheckRange);
            offspring = SpawnSystem.GetNrOfInstances(MBComponet.m_offspringPrefab, MBComponet.transform.position, MBComponet.m_totalCheckRange);
            partners = SpawnSystem.GetNrOfInstances(MBComponet.m_seperatePartner ? MBComponet.m_seperatePartner : MBComponet.m_myPrefab, MBComponet.transform.position, MBComponet.m_partnerCheckRange, eventCreaturesOnly: false, procreationOnly: true);
        }

        private void Kill()
        {
            MBComponet.m_character.m_deathEffects.Create(MBComponet.m_character.transform.position, MBComponet.m_character.transform.rotation, MBComponet.m_character.transform);
            CharacterDrop characterDrop = MBComponet.m_character.GetComponent<CharacterDrop>();
            foreach (KeyValuePair<GameObject, int> items in characterDrop.GenerateDropList())
            {
                GameObject prefab = Instantiate(items.Key);
                ItemDrop item = prefab.GetComponent<ItemDrop>();
                item.m_itemData.m_stack = items.Value;
                if (!ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, item))
                    ItemsHelper.Drop(item, MBComponet.transform.position);
                MBComponet.m_character.m_nview.Destroy();
            }
        }
    }
}
