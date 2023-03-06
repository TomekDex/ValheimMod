using System.Collections.Generic;
using System.Linq;
using TomekDexValheimModHelper;
using UnityEngine;

namespace TomekDexValheimMod
{
    public static class ContainerQuickAccess
    {
        private static readonly Dictionary<ContainerIdDistance, Dictionary<string, int>> idItemsCount = new Dictionary<ContainerIdDistance, Dictionary<string, int>>();
        private static readonly Dictionary<string, Dictionary<ContainerIdDistance, int>> itemsIdCount = new Dictionary<string, Dictionary<ContainerIdDistance, int>>();
        private static readonly HashSet<ContainerIdDistance> idContaniers = new HashSet<ContainerIdDistance>();
        private static readonly HashSet<ContainerIdDistance> idContaniersCanMove = new HashSet<ContainerIdDistance>();
        private static readonly Dictionary<(Vector3 vector, int radius), SortedSet<ContainerIdDistance>> vectorRadiusId = new Dictionary<(Vector3 vector, int radius), SortedSet<ContainerIdDistance>>();
        private static readonly Dictionary<ContainerIdDistance, HashSet<(Vector3 vector, int radius)>> idVectorRadius = new Dictionary<ContainerIdDistance, HashSet<(Vector3 vector, int radius)>>();
        private static readonly Dictionary<(Vector3, Vector3), float> distance = new Dictionary<(Vector3, Vector3), float>();
        private static readonly ContainerIdDistanceComparer containerIdDistanceComparer = new ContainerIdDistanceComparer();

        #region Clear
        internal static void Clear(Container container, ContainerExtension extension)
        {
            if (extension.Mode == SortMode.Lock)
                return;
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            HashSet<int> cheked = new HashSet<int>();
            bool change = false;
            foreach (ItemDrop.ItemData item in container.GetInventory().m_inventory.ToList())
            {
                int hash = ItemsHelper.GetItemDropBySharedNameOrName(item.m_shared.m_name).name.GetStableHashCode();
                if (extension.ItemsFilter.Contains(hash) || cheked.Contains(hash))
                    continue;
                change = true;
                if (TryAddItemNearbyContainers(container.transform.position, ContainerQuickAccessConfig.WorkingArea, item, extension.Mode != SortMode.Restrictive))
                    container.GetInventory().m_inventory.Remove(item);
                else
                    cheked.Add(hash);
            }
            if (change)
                container.m_inventory.m_onChanged();
            sw.Stop();
            if (ContainerQuickAccessConfig.Logs)
                Debug.Log($"Clear {sw.Elapsed}");
        }
        #endregion Clear

        #region TryRemoveItem
        public static int TryRemoveItemRegistertNearbyContainer(Vector3 position, int workingArea, ItemDrop itemDrop, int ammound)
        {
            if (ContainerQuickAccessConfig.Logs)
                Debug.Log($"TryRemoveItemNearbyContainer {position} {workingArea} {itemDrop.name} {ammound}");
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            int removed = 0;
            foreach (ContainerIdDistance cId in GetRegistertNearbyContainerWithItem(position, workingArea, itemDrop))
            {
                if (cId.Extension.Mode == SortMode.Lock)
                    continue;
                foreach (ItemDrop.ItemData item in TryRemoveItem(cId.Container.m_inventory, itemDrop.m_itemData, ammound - removed))
                    removed += item.m_stack;
                if (ammound - removed == 0)
                    break;
            }
            sw.Stop();
            if (ContainerQuickAccessConfig.Logs)
                Debug.Log($"TryRemoveItemNearbyContainer {removed}/{ammound} {sw.Elapsed}");
            return removed;
        }

        public static IEnumerable<ItemDrop.ItemData> TryRemoveItem(Inventory inventory, ItemDrop.ItemData item, int amount)
        {
            if (ContainerQuickAccessConfig.Logs)
                Debug.Log($"TryRemoveItem {amount} {item.m_shared.m_name}");
            foreach (ItemDrop.ItemData itemIn in inventory.m_inventory.ToList())
            {
                if (itemIn.m_shared.m_name == item.m_shared.m_name && itemIn.m_quality == item.m_quality)
                {
                    if (amount >= itemIn.m_stack)
                    {
                        inventory.m_inventory.Remove(itemIn);
                        amount -= itemIn.m_stack;
                        yield return itemIn;
                    }
                    else
                    {
                        itemIn.m_stack -= amount;
                        ItemDrop newItem = Object.Instantiate(ItemsHelper.GetItemDropBySharedNameOrName(item.m_shared.m_name));
                        newItem.m_itemData = item.Clone();
                        newItem.m_itemData.m_stack = amount;
                        yield return newItem.m_itemData;
                        break;
                    }
                }
            }
            inventory.Changed();
        }
        #endregion TryRemoveItem

        #region TryAddItem
        public static bool TryAddItemNearbyContainers(Vector3 position, int workingArea, ItemDrop.ItemData item, bool contains = false)
        {
            if (ContainerQuickAccessConfig.Logs)
                Debug.Log($"TryAddItemNearbyContainers {position} {workingArea} {item.m_shared.m_name} {item.m_stack}");
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            SortedSet<ContainerIdDistance> registertNearbyContainer = GetRegistertNearbyContainer(position, workingArea);
            bool result = false;

            int itemGetStableHashCode = ItemsHelper.GetItemDropBySharedNameOrName(item.m_shared.m_name).name.GetStableHashCode();
            foreach (System.Func<ContainerIdDistance, int, string, bool> filter in GetFilters(contains))
                if (TryAddItem(registertNearbyContainer.Where(a => filter(a, itemGetStableHashCode, item.m_shared.m_name)), item))
                {
                    sw.Stop();
                    if (ContainerQuickAccessConfig.Logs)
                        Debug.Log($"TryAddItemNearbyContainers true {item.m_stack} {item.m_shared.m_name} {sw.Elapsed}");
                    return true;
                }
            sw.Stop();
            if (ContainerQuickAccessConfig.Logs)
                Debug.Log($"TryAddItemNearbyContainers false {item.m_stack} {item.m_shared.m_name} {sw.Elapsed}");
            return result;
        }

        private static bool TryAddItem(IEnumerable<ContainerIdDistance> containers, ItemDrop.ItemData item)
        {
            foreach (ContainerIdDistance cId in containers)
                if (TryAddItem(cId.Container.m_inventory, item))
                    return true;
            return false;
        }

        private static bool TryAddItem(Inventory inventory, ItemDrop.ItemData item)
        {
            if (item.m_stack <= 0)
                return true;
            while (item.m_stack > item.m_shared.m_maxStackSize)
            {
                ItemDrop itemNew = Object.Instantiate(ItemsHelper.GetItemDropBySharedNameOrName(item.m_shared.m_name));
                itemNew.m_itemData = item.Clone();
                itemNew.m_itemData.m_stack = itemNew.m_itemData.m_shared.m_maxStackSize;
                item.m_stack -= itemNew.m_itemData.m_shared.m_maxStackSize;
                if (!inventory.AddItem(itemNew.m_itemData))
                {
                    item.m_stack += itemNew.m_itemData.m_stack;
                    return false;
                }
            }
            if (!inventory.AddItem(item))
                return false;
            return true;
        }

        private static IEnumerable<System.Func<ContainerIdDistance, int, string, bool>> GetFilters(bool contains)
        {
            yield return (cid, hash, name) => cid.Extension.Mode == SortMode.Restrictive && cid.Extension.ItemsFilter.Contains(hash);
            yield return (cid, hash, name) => cid.Extension.Mode == SortMode.Free && cid.Extension.ItemsFilter.Contains(hash);
            yield return (cid, hash, name) => cid.Extension.Mode == SortMode.None && cid.Extension.ItemsFilter.Contains(hash);
            if (contains)
                yield break;
            yield return (cid, hash, name) => cid.Extension.Mode == SortMode.Free && !cid.Extension.ItemsFilter.Contains(hash) && idItemsCount[cid].ContainsKey(name);
            yield return (cid, hash, name) => cid.Extension.Mode == SortMode.None && !cid.Extension.ItemsFilter.Contains(hash) && idItemsCount[cid].ContainsKey(name);
            yield return (cid, hash, name) => cid.Extension.Mode == SortMode.Free && !cid.Extension.ItemsFilter.Contains(hash) && !idItemsCount[cid].ContainsKey(name);
            yield return (cid, hash, name) => cid.Extension.Mode == SortMode.None && !cid.Extension.ItemsFilter.Contains(hash) && !idItemsCount[cid].ContainsKey(name);
        }
        #endregion TryAddItem

        #region CountItems
        public static int CountItems(Vector3 position, int workingArea, ItemDrop item)
        {
            IEnumerable<ContainerIdDistance> contaniers = GetNearbyContainer(position, workingArea);
            return CountItems(contaniers, item);
        }

        private static int CountItems(IEnumerable<ContainerIdDistance> containers, ItemDrop item)
        {
            if (!itemsIdCount.ContainsKey(item.m_itemData.m_shared.m_name))
                return 0;
            Dictionary<ContainerIdDistance, int> idCount = itemsIdCount[item.m_itemData.m_shared.m_name];
            if (!idCount.Any())
                return 0;
            int sum = 0;
            foreach (ContainerIdDistance container in containers)
                if (idCount.TryGetValue(container, out int value))
                    sum += value;
            if (ContainerQuickAccessConfig.Logs)
                Debug.Log($"Count items {sum} {item.m_itemData.m_shared.m_name}");
            return sum;
        }
        #endregion CountItems

        #region GetContainer
        private static SortedSet<ContainerIdDistance> GetRegistertNearbyContainer(Vector3 position, int workingArea)
        {
            if (ContainerQuickAccessConfig.Logs)
                Debug.Log($"GetRegistertNearbyContainer {position} {workingArea}");
            if (!vectorRadiusId.ContainsKey((position, workingArea)))
                if (!RegistertNearbyContainer(position, workingArea))
                    return new SortedSet<ContainerIdDistance>();
            return vectorRadiusId[(position, workingArea)];
        }

        private static IEnumerable<ContainerIdDistance> GetNearbyContainer(Vector3 position, int workingArea)
        {
            foreach (ContainerIdDistance cId in idContaniers)
            {
                float distance = GetDistance((position, cId.Container.transform.position));
                if (distance <= workingArea)
                {
                    ContainerIdDistance cIdNew = new ContainerIdDistance(cId.Container, distance);
                    if (!ContainerQuickAccessConfig.UseCheckAccess || cIdNew.Container.CheckAccess(Player.m_localPlayer.GetPlayerID()))
                        yield return cIdNew;
                }
            }
        }

        public static IEnumerable<ContainerIdDistance> GetRegistertNearbyContainerWithItemWithLimit(Vector3 position, int workingArea, IEnumerable<KeyValuePair<ItemDrop, int?>> items)
        {
            List<ContainerIdDistance> nearbyContainer = GetRegistertNearbyContainer(position, workingArea).ToList();
            Dictionary<string, int> savedCount = new Dictionary<string, int>();
            foreach (ContainerIdDistance cId in nearbyContainer)
                if (cId.Extension.Mode != SortMode.Lock)
                    if (idItemsCount[cId].Any(a => IsValid(a, nearbyContainer, savedCount, items)))
                        yield return cId;
        }

        private static IEnumerable<ContainerIdDistance> GetRegistertNearbyContainerWithItem(Vector3 position, int workingArea, ItemDrop item)
        {
            if (!itemsIdCount.ContainsKey(item.m_itemData.m_shared.m_name))
                yield break;
            Dictionary<ContainerIdDistance, int> idCount = itemsIdCount[item.m_itemData.m_shared.m_name];
            foreach (ContainerIdDistance cId in GetRegistertNearbyContainer(position, workingArea))
                if (idCount.ContainsKey(cId))
                    yield return cId;
        }
        #endregion GetContainer

        #region Register
        public static bool IsAnyRegistertNearbyContainer(Vector3 position, int workingArea)
        {
            if (!vectorRadiusId.ContainsKey((position, workingArea)))
                if (!RegistertNearbyContainer(position, workingArea))
                    return false;
            return vectorRadiusId[(position, workingArea)].Any();
        }

        public static bool RegistertNearbyContainer(Vector3 position, int workingArea)
        {
            if (ContainerQuickAccessConfig.UseCheckAccess && Player.m_localPlayer == null)
                return false;
            if (!vectorRadiusId.ContainsKey((position, workingArea)))
                vectorRadiusId[(position, workingArea)] = new SortedSet<ContainerIdDistance>(containerIdDistanceComparer);
            foreach (ContainerIdDistance cId in GetNearbyContainer(position, workingArea))
                RegistertNearbyContainer(position, workingArea, cId);
            return true;
        }

        public static void UnRegistertNearbyContainer(Vector3 position, int workingArea)
        {
            if (!vectorRadiusId.TryGetValue((position, workingArea), out SortedSet<ContainerIdDistance> cIds))
                return;
            foreach (ContainerIdDistance cId in cIds)
                if (idVectorRadius.TryGetValue(cId, out HashSet<(Vector3 vector, int radius)> radiusVector))
                {
                    radiusVector.Remove((position, workingArea));
                    if (!radiusVector.Any())
                        idVectorRadius.Remove(cId);
                }
            vectorRadiusId.Remove((position, workingArea));
        }

        private static void RegistertNearbyContainer(Vector3 position, int workingArea, ContainerIdDistance cId)
        {
            vectorRadiusId[(position, workingArea)].Add(cId);
            if (!idVectorRadius.ContainsKey(cId))
                idVectorRadius[cId] = new HashSet<(Vector3 vector, int radius)>();
            idVectorRadius[cId].Add((position, workingArea));
        }
        #endregion Register

        #region Management Container
        public static void AddContainer(Container container)
        {
            if (!IsContainerToAdd(container, out bool canMove))
                return;

            ContainerIdDistance cId = new ContainerIdDistance(container);
            container.m_inventory.m_onChanged += () => RefreshContainerItems(cId);
            if (canMove)
                idContaniersCanMove.Add(cId);
            idContaniers.Add(cId);
            FillContainer(cId);
            CheekNewContainerRegistertNearbyContainer(cId);
        }

        private static void RefreshContainerItems(ContainerIdDistance cId)
        {
            RemoveItemCount(cId);
            FillContainer(cId);
        }

        internal static void RefreshContainerPosition()
        {
            if (ContainerQuickAccessConfig.Logs)
                Debug.Log($"RefreshContainerPosition");
            foreach (ContainerIdDistance cId in idContaniersCanMove)
                RefreshContainerPosition(cId);
        }

        private static void RefreshContainerPosition(ContainerIdDistance cId)
        {
            float distance = Vector3.Distance(cId.Container.transform.position, cId.LastPosition);
            if (distance < 0.5)
                return;
            RemoveContainerFromRegister(cId);
            CheekNewContainerRegistertNearbyContainer(cId);
        }

        internal static void RemoveContainer(Container container)
        {
            ContainerIdDistance cId = new ContainerIdDistance(container, toDelete: true);
            if (!idContaniers.Remove(cId))
                return;
            idContaniersCanMove.Remove(cId);
            RemoveItemCount(cId);
            idItemsCount.Remove(cId);
            RemoveContainerFromRegister(cId);
        }

        private static void RemoveContainerFromRegister(ContainerIdDistance cId)
        {
            if (idVectorRadius.ContainsKey(cId))
            {
                foreach ((Vector3 vector, int radius) vectorRadius in idVectorRadius[cId])
                {
                    vectorRadiusId[vectorRadius].Remove(cId);
                    if (!vectorRadiusId[vectorRadius].Any())
                        vectorRadiusId.Remove(vectorRadius);
                }
                idVectorRadius.Remove(cId);
            }
        }

        private static void FillContainer(ContainerIdDistance cId)
        {
            Dictionary<string, int> idItemsCountCId = new Dictionary<string, int>();
            idItemsCount[cId] = idItemsCountCId;
            foreach (ItemDrop.ItemData item in cId.Container.m_inventory.m_inventory)
            {
                if (!idItemsCountCId.ContainsKey(item.m_shared.m_name))
                    idItemsCountCId[item.m_shared.m_name] = item.m_stack;
                else
                    idItemsCountCId[item.m_shared.m_name] += item.m_stack;
            }
            foreach (KeyValuePair<string, int> idItemCount in idItemsCountCId)
            {
                if (!itemsIdCount.ContainsKey(idItemCount.Key))
                    itemsIdCount[idItemCount.Key] = new Dictionary<ContainerIdDistance, int>();
                itemsIdCount[idItemCount.Key][cId] = idItemCount.Value;
            }
        }

        private static bool IsContainerToAdd(Container container, out bool canMove)
        {
            canMove = false;
            if (container.m_inventory == null)
                return false;
            if (container.name?.StartsWith("Treasure") ?? false)
                return false;
            if (container.m_piece != null)
                return true;
            if (ContainerQuickAccessConfig.UseObliterator && container.m_name == "$piece_incinerator")
                return true;
            canMove = true;
            if (ContainerQuickAccessConfig.UseCart && container.m_wagon != null)
                return true;
            if (ContainerQuickAccessConfig.UseShip && container.GetComponentInParent<Ship>() != null)
                return true;
            return false;
        }

        private static void CheekNewContainerRegistertNearbyContainer(ContainerIdDistance cId)
        {
            foreach (KeyValuePair<(Vector3 vector, int radius), SortedSet<ContainerIdDistance>> vectorRadiusIds in vectorRadiusId)
            {
                float distance = GetDistance((vectorRadiusIds.Key.vector, cId.Container.m_nview.transform.position));
                if (distance <= vectorRadiusIds.Key.radius)
                {
                    ContainerIdDistance cIdNew = new ContainerIdDistance(cId.Container, distance);
                    RegistertNearbyContainer(vectorRadiusIds.Key.vector, vectorRadiusIds.Key.radius, cIdNew);
                }
            }
        }

        private static void RemoveItemCount(ContainerIdDistance cId)
        {
            foreach (KeyValuePair<string, int> item in idItemsCount[cId])
            {
                itemsIdCount[item.Key].Remove(cId);
                if (!itemsIdCount[item.Key].Any())
                    itemsIdCount.Remove(item.Key);
            }
        }
        #endregion Management Container

        private static bool IsValid(KeyValuePair<string, int> itemsCoun, List<ContainerIdDistance> nearbyContainer, Dictionary<string, int> savedCount, IEnumerable<KeyValuePair<ItemDrop, int?>> items)
        {
            foreach (KeyValuePair<ItemDrop, int?> item in items)
            {
                string name = item.Key.m_itemData.m_shared.m_name;
                if (itemsCoun.Key != name)
                    continue;
                if (itemsCoun.Value == 0)
                    continue;
                if (!itemsIdCount.ContainsKey(name))
                    continue;
                if (item.Value == 0)
                    continue;
                if (!savedCount.TryGetValue(name, out int count))
                    savedCount[name] = count = CountItems(nearbyContainer, item.Key);
                if (item.Value == null)
                    return true;
                if (item.Value < count)
                    return true;
            }

            return false;
        }

        private static float GetDistance((Vector3, Vector3) vectors)
        {
            if (!distance.ContainsKey(vectors))
                distance[vectors] = Vector3.Distance(vectors.Item1, vectors.Item2);
            return distance[vectors];
        }
    }
}
