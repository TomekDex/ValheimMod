using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TomekDexValheimMod
{
    public class NoFlyingRocksProcess
    {
        private static readonly int layerMaskTerrain = LayerMask.GetMask("terrain");
        private static readonly int layerMaskDefault = LayerMask.GetMask("Default");
        private static readonly int layerMask = LayerMask.GetMask("piece", "static_solid", "Default_small", "Default");
        private static readonly ConcurrentDictionary<MineRock5, Dictionary<Collider, HitAreaContener>> hitAreaConteners = new ConcurrentDictionary<MineRock5, Dictionary<Collider, HitAreaContener>>();
        private static readonly ConcurrentDictionary<Collider, HitAreaContener> hitAreaContenersAll = new ConcurrentDictionary<Collider, HitAreaContener>();
        private static readonly ConcurrentBag<string> logs = new ConcurrentBag<string>();
        private static readonly ConcurrentQueue<HitAreaContener> toDestroy = new ConcurrentQueue<HitAreaContener>();
        private static readonly HashSet<MineRock5> priorityQueue = new HashSet<MineRock5>();
        private static HashSet<MineRock5> queue = new HashSet<MineRock5>();
        private static readonly ConcurrentDictionary<MineRock5, Task> fillCollidersTask = new ConcurrentDictionary<MineRock5, Task>();
        private static readonly ConcurrentDictionary<MineRock5, Task> clearCollidersTasks = new ConcurrentDictionary<MineRock5, Task>();
        private static readonly ConcurrentDictionary<MineRock5, Task> calculateMeshAdjacentTask = new ConcurrentDictionary<MineRock5, Task>();
        private static readonly ConcurrentDictionary<MineRock5, Task> findNotConnectedGroundTask = new ConcurrentDictionary<MineRock5, Task>();
        private static readonly ConcurrentDictionary<MineRock5, Task> validationTask = new ConcurrentDictionary<MineRock5, Task>();
        private static readonly ConcurrentDictionary<Collider, ConcurrentDictionary<Collider, object>> groundColliders = new ConcurrentDictionary<Collider, ConcurrentDictionary<Collider, object>>();
        private static readonly ConcurrentDictionary<Collider, object> refreshGroundCollider = new ConcurrentDictionary<Collider, object>();
        private static Task refreshGroundColliderTask;

        public static void Process()
        {
            while (logs.TryTake(out string log))
                Debug.Log(log);
            if (hitAreaConteners.Count == 0 || Player.m_localPlayer == null)
                return;
            if (Player.s_players.Any(a => a.IsTeleporting()))
                return;
            if (ChcekTasks(fillCollidersTask, FillCollidersTask, clearCollidersTasks, 20))
            {
                Debug.Log($"Preliminary checking of the borders {fillCollidersTask.Count}");
                return;
            }
            if (ChcekTasks(clearCollidersTasks, FillGroundTask, calculateMeshAdjacentTask, 20))
            {
                Debug.Log($"Thorough search for the ground {clearCollidersTasks.Count}");
                return;
            }
            if (ChcekTasks(calculateMeshAdjacentTask, CalculateMeshAdjacentTask, validationTask, 5))
            {
                Debug.Log($"Thorough checking of the borders {calculateMeshAdjacentTask.Count}");
                return;
            }
            if (refreshGroundCollider.Any() && (refreshGroundColliderTask == null || refreshGroundColliderTask.IsCompleted))
            {
                Debug.Log($"Ground level to recalculate {refreshGroundCollider.Count}");
                refreshGroundColliderTask = new Task(RefreshGroundColliderTask);
                refreshGroundColliderTask.Start();
            }
            if (ChcekTasks(findNotConnectedGroundTask, FindNotConnectedGroundTask, null, 1))
                return;
            MineRock5 key;
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                if (sw.ElapsedMilliseconds > 10)
                    return;
                key = GetKeyHitAreaConteners();
            } while (key == null || Player.GetClosestPlayer(key.transform.position, 20) == null || !validationTask.ContainsKey(key));
            findNotConnectedGroundTask.TryAdd(key, null);
            ChcekTasks(findNotConnectedGroundTask, FindNotConnectedGroundTask, null, 1);
            MeshObjectCollision.ClearCache();
        }

        private static void RefreshGroundColliderTask()
        {
            try
            {
                foreach (Collider collider in refreshGroundCollider.Keys.ToList())
                {
                    if (!hitAreaContenersAll.TryGetValue(collider, out HitAreaContener hit))
                        continue;
                    UpdateGroundColldiders(collider, hit);
                    refreshGroundCollider.TryRemove(collider, out _);
                }
            }
            catch (Exception ex)
            {
                logs.Add($"Error in RefreshGroundColliderTask\r\n{ex.ToString()}");
            }
        }

        private static void FindNotConnectedGroundTask(Dictionary<Collider, HitAreaContener> mineRock5Data)
        {
            foreach (HitAreaContener hit in mineRock5Data.Values)
            {
                HashSet<HitAreaContener> cheked = new HashSet<HitAreaContener>();
                if (hit.HitArea.m_health > 0 && !HasConnectionWithGround(hit, cheked))
                    toDestroy.Enqueue(hit);
            }
        }

        private static bool HasConnectionWithGround(HitAreaContener hit, HashSet<HitAreaContener> cheked)
        {
            if (!cheked.Add(hit))
                return false;
            if (hit.GroundColldiders.Any())
                return true;
            if (hit.CollidersAdjacentDefault.Any())
                return true;
            foreach (Collider collider in hit.CollidersAdjacent)
                if (hitAreaContenersAll.TryGetValue(collider, out HitAreaContener hitted) && hitted.HitArea.m_health > 0)
                    if (HasConnectionWithGround(hitted, cheked))
                        return true;
            return false;
        }

        private static void FillCollidersTask(Dictionary<Collider, HitAreaContener> mineRock5Data)
        {
            foreach (HitAreaContener hit in mineRock5Data.Values)
            {
                hit.CollidersAdjacent = Physics.OverlapBox(hit.HitArea.m_collider.bounds.center, hit.HitArea.m_collider.bounds.size * 0.5f, hit.HitArea.m_bound.m_rot, layerMask).Where(a => hitAreaContenersAll.ContainsKey(a)).ToHashSet();
                hit.CollidersAdjacentDefault = Physics.OverlapBox(hit.HitArea.m_collider.bounds.center, hit.HitArea.m_collider.bounds.size * 0.5f, hit.HitArea.m_bound.m_rot, layerMaskDefault).Where(a => !hitAreaContenersAll.ContainsKey(a)).ToHashSet();
            }
        }

        private static void FillGroundTask(Dictionary<Collider, HitAreaContener> mineRock5Data)
        {
            foreach (KeyValuePair<Collider, HitAreaContener> hit in mineRock5Data.ToList())
                UpdateGroundColldiders(hit.Key, hit.Value);
        }

        private static void UpdateGroundColldiders(Collider collider, HitAreaContener hit)
        {
            ConcurrentDictionary<Collider, object> tempGroundColldiders = new ConcurrentDictionary<Collider, object>();
            foreach (Collider colliderGround in Physics.OverlapBox(collider.bounds.center, collider.bounds.size * 0.5f, hit.HitArea.m_bound.m_rot, layerMaskTerrain))
            {
                if (MeshObjectCollision.InGroundRaycast((MeshCollider)colliderGround, collider.GetComponent<MeshCollider>()))
                {
                    tempGroundColldiders.TryAdd(colliderGround, null);
                    if (!groundColliders.ContainsKey(colliderGround))
                        groundColliders.TryAdd(colliderGround, new ConcurrentDictionary<Collider, object>());
                    groundColliders[colliderGround].TryAdd(collider, null);
                }
            }
            hit.GroundColldiders = tempGroundColldiders;
        }

        private static void CalculateMeshAdjacentTask(Dictionary<Collider, HitAreaContener> mineRock5Data)
        {
            foreach (KeyValuePair<Collider, HitAreaContener> col in mineRock5Data.ToList())
            {
                MeshCollider meshCollider = col.Key.GetComponent<MeshCollider>();
                foreach (Collider collider in col.Value.CollidersAdjacent.ToList())
                    if (!MeshObjectCollision.DetectionSAT(collider.GetComponent<MeshCollider>(), meshCollider))
                        col.Value.CollidersAdjacent.Remove(collider);
                if (!col.Value.CollidersAdjacentDefault.Any(c => c.GetType() == typeof(BoxCollider) && MeshObjectCollision.DetectionSAT(meshCollider, (BoxCollider)c)))
                    col.Value.CollidersAdjacentDefault.Clear();
            }
        }

        private static bool ChcekTasks(ConcurrentDictionary<MineRock5, Task> tasksDictionary, Action<Dictionary<Collider, HitAreaContener>> taskAction, ConcurrentDictionary<MineRock5, Task> next, int maxStarted)
        {
            if (!tasksDictionary.Any())
                return false;
            int started = 0;
            foreach (MineRock5 mineRock in tasksDictionary.Keys.ToList())
            {
                if (mineRock == null)
                {
                    tasksDictionary.TryRemove(mineRock, out _);
                    continue;
                }
                started++;
                if (tasksDictionary[mineRock] == null)
                {
                    tasksDictionary[mineRock] = new Task(() => ProcessTaskAction(taskAction, mineRock, tasksDictionary, next));
                    tasksDictionary[mineRock].Start();
                    if (started >= maxStarted)
                        return true;
                }
                else
                {
                    if (tasksDictionary.TryGetValue(mineRock, out Task task) && task?.Status == TaskStatus.Faulted)
                    {
                        if (hitAreaConteners.ContainsKey(mineRock))
                        {
                            tasksDictionary[mineRock] = new Task(() => ProcessTaskAction(taskAction, mineRock, tasksDictionary, next));
                            tasksDictionary[mineRock].Start();
                            if (started >= maxStarted)
                                return true;
                        }
                        else
                        {
                            tasksDictionary.TryRemove(mineRock, out _);
                        }
                    }
                }
            }
            return true;
        }

        private static void ProcessTaskAction(Action<Dictionary<Collider, HitAreaContener>> taskAction, MineRock5 mineRock, ConcurrentDictionary<MineRock5, Task> current, ConcurrentDictionary<MineRock5, Task> next)
        {
            try
            {
                if (!hitAreaConteners.TryGetValue(mineRock, out Dictionary<Collider, HitAreaContener> mineRock5Data))
                {
                    current.TryRemove(mineRock, out _);
                    return;
                }
                if (mineRock5Data == null)
                    current.TryRemove(mineRock, out _);
                taskAction(mineRock5Data);
                current.TryRemove(mineRock, out _);
                next?.TryAdd(mineRock, null);
            }
            catch (Exception ex)
            {
                current.TryRemove(mineRock, out _);
                if (mineRock != null)
                    fillCollidersTask.TryAdd(mineRock, null);
                logs.Add($"Error in ProcessTaskAction\r\n{ex.ToString()}");
            }
        }

        private static MineRock5 GetKeyHitAreaConteners()
        {
            MineRock5 key;
            if (priorityQueue.Count > 0)
            {
                key = priorityQueue.First();
                priorityQueue.Remove(key);
                return key;
            }
            if (queue.Count == 0)
                queue = hitAreaConteners.Keys.ToHashSet();
            key = queue.First();
            queue.Remove(key);
            return key;
        }

        private static void Destroy(HitAreaContener hit)
        {
            hit.HitArea.m_health = 0;
            Vector3 position = hit.HitArea.m_collider.bounds.center;
            hit.MineRock5.m_destroyedEffect.Create(position, Quaternion.identity);
            foreach (GameObject drop in hit.MineRock5.m_dropItems.GetDropList())
            {
                Vector3 positionDrop = position + UnityEngine.Random.insideUnitSphere * 0.3f;
                UnityEngine.Object.Instantiate(drop, positionDrop, Quaternion.identity);
            }
        }

        public static void DestroyOnTime()
        {
            if (toDestroy.Count == 0)
                return;
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            int count = 0;
            HashSet<MineRock5> touched = new HashSet<MineRock5>();
            do
            {
                if (toDestroy.TryDequeue(out HitAreaContener hit) && hit != null && hit.HitArea.m_health > 0)
                {
                    if (touched.Add(hit.MineRock5))
                        hit.MineRock5.LoadHealth();
                    Destroy(hit);
                }
                count++;
            } while (toDestroy.Count != 0 && sw.ElapsedMilliseconds <= 10);
            foreach (MineRock5 touch in touched)
            {
                touch.SaveHealth();
                touch.UpdateMesh();
                if (touch.AllDestroyed())
                    touch.m_nview.Destroy();
            }
            sw.Stop();
            Debug.Log($"Destroyed elements {count}");
        }

        internal static void AddMineRock5(MineRock5 mineRock5)
        {
            for (int i = 0; i < mineRock5.m_hitAreas.Count; i++)
            {
                MineRock5.HitArea hitArea = mineRock5.m_hitAreas[i];
                if (hitArea.m_health > 0)
                {
                    HitAreaContener cont = new HitAreaContener(hitArea, mineRock5);
                    if (!hitAreaConteners.ContainsKey(mineRock5))
                        hitAreaConteners[mineRock5] = new Dictionary<Collider, HitAreaContener>();
                    hitAreaConteners[mineRock5][hitArea.m_collider] = cont;
                    hitAreaContenersAll[hitArea.m_collider] = cont;
                }
            }
            fillCollidersTask.TryAdd(mineRock5, null);
        }

        internal static void AddToPriorityQueue(MineRock5 mineRock5)
        {
            if (validationTask.ContainsKey(mineRock5))
                priorityQueue.Add(mineRock5);
        }

        internal static void RemoveMineRock5(MineRock5 mineRock5)
        {
            if (fillCollidersTask.TryRemove(mineRock5, out Task task))
                task?.Wait();
            if (clearCollidersTasks.TryRemove(mineRock5, out task))
                task?.Wait();
            if (calculateMeshAdjacentTask.TryRemove(mineRock5, out task))
                task?.Wait();
            if (validationTask.TryRemove(mineRock5, out task))
                task?.Wait();
            refreshGroundColliderTask?.Wait();
            if (hitAreaConteners.TryGetValue(mineRock5, out Dictionary<Collider, HitAreaContener> contener))
                foreach (Collider collider in contener.Keys)
                {
                    hitAreaContenersAll.TryRemove(collider, out HitAreaContener hit);
                    if (hit.GroundColldiders != null)
                        foreach (Collider groundColldider in hit.GroundColldiders.Keys)
                            if (groundColliders.TryGetValue(groundColldider, out ConcurrentDictionary<Collider, object> colliders))
                                colliders.TryRemove(collider, out _);
                }
            hitAreaConteners.TryRemove(mineRock5, out _);
        }

        internal static void PokeGroundCollider(MeshCollider groundCollider)
        {
            if (groundColliders.TryGetValue(groundCollider, out ConcurrentDictionary<Collider, object> colliders))
                foreach (Collider collider in colliders.Keys.ToList())
                    refreshGroundCollider.TryAdd(collider, null);
        }
    }
}
