//using System;
//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO.MemoryMappedFiles;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.InteropServices;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace TomekDexValheimMod
//{
//    public class HitAreaContenerOld
//    {
//        public MineRock5.HitArea HitArea { get; }
//        public MineRock5 MineRock5 { get; }
//        public int Index { get; }
//        public int Id { get; }
//        public bool StandingOnGround { get; set; }
//        public bool StandingOnGroundColliders { get; set; }
//        public HashSet<Collider> Colliders { get; internal set; }
//        //public Dictionary<Collider, FlattenedDataCollider> FlattenedDataColliders { get; internal set; }
//        public int StandingOnGroundLastUpdate { get; internal set; }
//        public HashSet<MeshCollider> GroundColldiders { get; internal set; }
//        public Dictionary<Collider, FlattenedDataCollider> FlattenedDatGroundColldiders { get; internal set; }
//        public bool GroundChange { get; internal set; }
//        public FlattenedDataCollider FlattenedDataCollider { get; internal set; }
//        public int StandingOnGroundCollidersLastUpdate { get; internal set; }

//        public static int nextID;

//        public HitAreaContenerOld(MineRock5.HitArea hitArea, MineRock5 mineRock5, int index)
//        {
//            HitArea = hitArea;
//            MineRock5 = mineRock5;
//            Index = index;
//            Id = nextID++;
//        }
//    }

//    public class HitAreaControllerOld : MonoBehaviour
//    {
//        public static PropertyInfo meshColliderSharedMesh = typeof(MeshCollider).GetProperty("sharedMesh");
//        public MineRock5 mineRock5;
//        public static ConcurrentDictionary<MineRock5, Dictionary<Collider, HitAreaContener>> hitAreaConteners = new ConcurrentDictionary<MineRock5, Dictionary<Collider, HitAreaContener>>();

//        public static readonly ConcurrentDictionary<Collider, HitAreaContener> hitAreaContenersAll = new ConcurrentDictionary<Collider, HitAreaContener>();
//        private static readonly int layerMaskTerrain = LayerMask.GetMask("terrain");
//        private static readonly int layerMask = LayerMask.GetMask("piece", "Default", "static_solid", "Default_small");
//        private static readonly Collider[] tempColider = new Collider[128];
//        private static int updateCount;
//        private static readonly ConcurrentQueue<HitAreaContener> toDestroy = new ConcurrentQueue<HitAreaContener>();
//        private static readonly HashSet<MineRock5> priorityQueue = new HashSet<MineRock5>();
//        private static HashSet<MineRock5> queue = new HashSet<MineRock5>();
//        private static readonly ConcurrentDictionary<MineRock5, Task> fillCollidersTask = new ConcurrentDictionary<MineRock5, Task>();
//        private static readonly ConcurrentDictionary<MineRock5, Task> clearCollidersTasks = new ConcurrentDictionary<MineRock5, Task>();

//        public void Awake()
//        {
//            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

//            mineRock5 = GetComponent<MineRock5>();
//            for (int i = 0; i < mineRock5.m_hitAreas.Count; i++)
//            {
//                MineRock5.HitArea hitArea = mineRock5.m_hitAreas[i];
//                if (hitArea.m_health > 0)
//                {
//                    HitAreaContener cont = new HitAreaContener(hitArea, mineRock5, i);
//                    if (!hitAreaConteners.ContainsKey(mineRock5))
//                        hitAreaConteners[mineRock5] = new Dictionary<Collider, HitAreaContener>();
//                    hitAreaConteners[mineRock5][hitArea.m_collider] = cont;
//                    hitAreaContenersAll[hitArea.m_collider] = cont;
//                }
//            }

//            fillCollidersTask[mineRock5] = new Task(() => FillCollidersTask(mineRock5));
//            fillCollidersTask[mineRock5].Start();
//            clearCollidersTasks.TryAdd(mineRock5, null);

//            sw.Stop();
//            Debug.Log($"Awake {sw.Elapsed}");
//        }
//        //public static MineRock5 key;
//        public static void Proces(MineRock5 key)
//        {
//            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

//            ClearColdiers(hitAreaConteners[key]);
//            updateCount++;

//            HashSet<MineRock5> clearedColdiers = new HashSet<MineRock5> { key };
//            foreach (HitAreaContener hit in hitAreaConteners[key].Values)
//            {
//                HashSet<HitAreaContener> cheked = new HashSet<HitAreaContener>();
//                FindConnectedGround(hit, cheked, clearedColdiers);
//            }
//            foreach (HitAreaContener hit in hitAreaConteners[key].Values)
//                if (!hit.StandingOnGround)
//                    toDestroy.Enqueue(hit);
//            sw.Stop();
//            Debug.Log($"Proces {sw.Elapsed}");
//        }

//        public static void SaveDictionaryToSharedMemory()
//        {
//            // Utworzenie obszaru pamięci
//            using (MemoryMappedFile memoryMappedFile = MemoryMappedFile.CreateOrOpen("nazwa pliku", Marshal.SizeOf<Dictionary<MineRock5, Dictionary<Collider, HitAreaContener>>>()))
//            {
//                // Uzyskanie uchwytu do obszaru pamięci
//                using (MemoryMappedViewStream stream = memoryMappedFile.CreateViewStream())
//                {
//                    // Zapisanie referencji do obiektu Dane do obszaru pamięci
//                    IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(new[] { hitAreaConteners }, 0);
//                    byte[] bytes = BitConverter.GetBytes((long)ptr);
//                    stream.Write(bytes, 0, bytes.Length);
//                }
//            }
//        }

//        public static bool ChcekTasks(ConcurrentDictionary<MineRock5, Task> tasksDictionary, Action<MineRock5> taskAction)
//        {
//            if (!tasksDictionary.Any())
//                return false;

//            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

//            foreach (MineRock5 mineRock in tasksDictionary.Keys.ToList())
//            {
//                if (tasksDictionary[mineRock] == null)
//                {
//                    tasksDictionary[mineRock] = new Task(() => taskAction(mineRock));
//                    tasksDictionary[mineRock].Start();
//                }
//                else
//                {
//                    if (tasksDictionary.TryGetValue(mineRock, out Task task) && task?.Status == TaskStatus.Faulted)
//                    {
//                        if (hitAreaConteners.ContainsKey(mineRock))
//                        {
//                            tasksDictionary[mineRock] = new Task(() => taskAction(mineRock));
//                            tasksDictionary[mineRock].Start();
//                        }
//                        else
//                        {
//                            tasksDictionary.TryRemove(mineRock, out _);
//                        }
//                    }
//                }
//            }

//            sw.Stop();
//            Debug.Log($"ChcekTasks {sw.Elapsed}");
//            return true;
//        }

//        public static IEnumerator Coroutine()
//        {
//            while (true)
//            {
//                if (hitAreaConteners.Count == 0)
//                {
//                    Debug.Log($"hitAreaConteners.Count == 0");
//                    Debug.Log($"hfillCollidersTask{fillCollidersTask.Count}");
//                    yield return new WaitForSeconds(10f);
//                    continue;
//                }
//                if (ChcekTasks(fillCollidersTask, FillCollidersTask))
//                {
//                    Debug.Log($"hfillCollidersTask.Any() {fillCollidersTask.Count}");
//                    yield return new WaitForSeconds(5f);
//                    continue;
//                }
//                if (ChcekTasks(clearCollidersTasks, ClearCollidersTask))
//                {
//                    Debug.Log($"clearCollidersTasks.Any() {fillCollidersTask.Count}");
//                    yield return new WaitForSeconds(5f);
//                    continue;
//                }
//                MineRock5 key = GetKeyHitAreaConteners();
//                if (Player.GetClosestPlayer(key.transform.position, 20) == null)
//                {
//                    Debug.Log($"Player.GetClosestPlayer(key.transform.position, 20) == null {hitAreaConteners.Count}");
//                    yield return new WaitForSeconds(0.1f);
//                    continue;
//                }
//                Debug.Log($"FillColliders");

//                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
//                FindConnectedGroundTask(key);
//                sw.Stop();
//                Debug.Log($"FindConnectedGroundTask {sw.Elapsed}");
//                //HashSet<MineRock5> clearedColdiers = new HashSet<MineRock5> { key };
//                //foreach (HitAreaContener hit in hitAreaConteners[key].Values)
//                //{
//                //    HashSet<HitAreaContener> cheked = new HashSet<HitAreaContener>();
//                //    FindConnectedGroundTask(hit, cheked, clearedColdiers);
//                //}

//                //new Task(() =>
//                //{
//                //    FillColliders(key);
//                //    //Task task = new Task(() => FillColliders(key));
//                //    System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

//                //    sw.Restart();
//                //    HashSet<MineRock5> clearedColdiers = new HashSet<MineRock5> { key };
//                //    foreach (HitAreaContener hit in hitAreaConteners[key].Values)
//                //    {
//                //        HashSet<HitAreaContener> cheked = new HashSet<HitAreaContener>();
//                //        FindConnectedGround(hit, cheked, clearedColdiers);
//                //    }
//                //    sw.Stop();
//                //    Debug.Log($"FindConnectedGround {sw.Elapsed}");
//                //    sw.Restart();
//                //    foreach (HitAreaContener hit in hitAreaConteners[key].Values)
//                //        if (!hit.StandingOnGround || hit.StandingOnGroundCollidersLastUpdate == updateCount && hit.StandingOnGroundColliders)
//                //            toDestroy.Enqueue(hit);

//                //    sw.Stop();
//                //    Debug.Log($"toDestroy {sw.Elapsed}");
//                //}).Start();
//                //foreach (HitAreaContener hit in hitAreaConteners[key].Values)
//                //    if (!hit.StandingOnGround)
//                //        toDestroy.Enqueue(hit);
//                //Proces();
//                //sw.Stop();
//                //Debug.Log($"Proces {sw.Elapsed}");
//                yield return new WaitForSeconds(10f);

//                yield return new WaitForSeconds(0.1f);
//            }
//        }

//        private static void FindConnectedGroundTask(MineRock5 key)
//        {
//            if (!hitAreaConteners.TryGetValue(key, out Dictionary<Collider, HitAreaContener> hitAreaContener))
//                return;
//            foreach (HitAreaContener hit in hitAreaConteners[key].Values)
//            {
//                HashSet<HitAreaContener> cheked = new HashSet<HitAreaContener>();
//                if (!HasConnectionWithGround(hit, cheked))
//                    toDestroy.Enqueue(hit);
//            }
//        }

//        private static bool HasConnectionWithGround(HitAreaContener hit, HashSet<HitAreaContener> cheked)
//        {
//            if (!cheked.Add(hit))
//                return false;
//            Collider[] coliders = Physics.OverlapBox(hit.HitArea.m_collider.bounds.center, hit.HitArea.m_collider.bounds.size * 0.5f, hit.HitArea.m_bound.m_rot, layerMaskTerrain);
//            foreach (Collider colider in coliders)
//            {
//                MeshCollider meshCollider = (MeshCollider)colider;
//                FlattenedDataCollider fl = new FlattenedDataCollider(meshCollider, meshCollider.GetInstanceID());
//                if (SATCollisionDetection2.CheckCollision(fl, hit.FlattenedDataCollider))
//                    return true;

//            }
//            foreach (Collider colider in hit.Colliders)
//                if (hitAreaContenersAll.TryGetValue(colider, out HitAreaContener hitted))
//                    if (HasConnectionWithGround(hitted, cheked))
//                        return true;

//            return false;
//        }

//        private static void ClearCollidersTask(MineRock5 key)
//        {
//            if (!hitAreaConteners.TryGetValue(key, out Dictionary<Collider, HitAreaContener> hitAreaContener))
//            {
//                clearCollidersTasks.TryRemove(key, out _);
//                return;
//            }
//            foreach (HitAreaContener hit in hitAreaContener.Values.ToList())
//                foreach (Collider colider in hit.Colliders.ToList())
//                    if (hitAreaContenersAll.TryGetValue(colider, out HitAreaContener hited))
//                        if (hited.HitArea.m_health <= 0 || !SATCollisionDetection2.CheckCollision(hit.FlattenedDataCollider, hited.FlattenedDataCollider))
//                            hit.Colliders.Remove(colider);
//            clearCollidersTasks.TryRemove(key, out _);
//        }

//        private static void FillColliders(MineRock5 key)
//        {
//            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
//            ClearColdiers(hitAreaConteners[key]);
//            updateCount++;
//            HashSet<MineRock5> clearedColdiers = new HashSet<MineRock5> { key };
//            foreach (HitAreaContener hit in hitAreaConteners[key].Values)
//            {
//                HashSet<int> cheked = new HashSet<int>();
//                FillColliders(hit, cheked, clearedColdiers);
//            }
//            sw.Stop();
//            Debug.Log($"FillColliders {sw.Elapsed}");
//        }

//        private static void FillCollidersTask(MineRock5 key)
//        {
//            foreach (HitAreaContener hit in hitAreaConteners[key].Values)
//            {
//                hit.Colliders = Physics.OverlapBox(hit.HitArea.m_collider.bounds.center, hit.HitArea.m_collider.bounds.size * 0.5f, hit.HitArea.m_bound.m_rot, layerMask).Where(a => hitAreaContenersAll.ContainsKey(a)).ToHashSet();
//                hit.FlattenedDataCollider = new FlattenedDataCollider(hit.HitArea.m_collider.GetComponent<MeshCollider>(), hit.Id);
//            }
//            fillCollidersTask.TryRemove(key, out _);
//        }

//        //private static bool StandingOnGround(int id, Message msg)
//        //{
//        //    foreach (int idG in msg.Ground[id])
//        //    {
//        //        if (MeshObjectCollision.Detection(msg.VectorsStricks[id], msg.IndicesStricks[id], msg.VectorsGround[idG], msg.IndicesGround[idG]))
//        //        {
//        //            Debug.Log($"true 1");
//        //            return true;
//        //        }
//        //        if (MeshObjectCollision.Detection(msg.VectorsGround[idG], msg.IndicesGround[idG], msg.VectorsStricks[id], msg.IndicesStricks[id]))
//        //        {
//        //            Debug.Log($"true 2");
//        //            return true;
//        //        }
//        //    }
//        //    return false;
//        //}

//        //private static void FindConnectedGround(int id, Message msg, HashSet<int> cheked, Dictionary<int, bool> standingOnGround)
//        //{
//        //    if (!cheked.Add(id))
//        //        return;
//        //    standingOnGround[id] = false;
//        //    if (!standingOnGround.ContainsKey(id))
//        //        standingOnGround[id] = StandingOnGround(id, msg);
//        //    if (standingOnGround[id])
//        //        return;
//        //    foreach (int stickyId in msg.Sticky[id])
//        //    {
//        //        if (!standingOnGround.ContainsKey(stickyId))
//        //            standingOnGround[stickyId] = StandingOnGround(stickyId, msg);
//        //        if (standingOnGround[stickyId])
//        //        {
//        //            standingOnGround[id] = true;
//        //            return;
//        //        }
//        //        else
//        //        {
//        //            FindConnectedGround(stickyId, msg, cheked, standingOnGround);
//        //        }
//        //        if (standingOnGround[stickyId])
//        //            standingOnGround[id] = true;
//        //    }

//        //}

//        private static void FillStandingOnGround(HitAreaContener hit)
//        {
//            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
//            if (!hit.GroundChange)
//                return;
//            FlattenedDataCollider colliderData = hit.FlattenedDataCollider;
//            foreach (MeshCollider collider in hit.GroundColldiders)
//            {
//                FlattenedDataCollider colliderDataGround = hit.FlattenedDatGroundColldiders[collider];
//                if (SATCollisionDetection2.CheckCollision(colliderData.vertices, colliderData.indices, colliderDataGround.vertices, colliderDataGround.indices))
//                {
//                    hit.StandingOnGround = true;
//                    return;
//                }
//            }
//            sw.Stop();
//            Debug.Log($"FillStandingOnGround {sw.Elapsed}");
//        }

//        private static void FindConnectedGroundTask(HitAreaContener hit, HashSet<HitAreaContener> cheked, HashSet<MineRock5> clearedColdiers)
//        {
//            if (!cheked.Add(hit))
//                return;
//        }
//        private static void FindConnectedGround(HitAreaContener hit, HashSet<HitAreaContener> cheked, HashSet<MineRock5> clearedColdiers)
//        {
//            if (!cheked.Add(hit))
//                return;
//            if (!hitAreaConteners.ContainsKey(hit.MineRock5))
//            {
//                hit.StandingOnGroundLastUpdate = updateCount;
//                hit.StandingOnGround = false;
//                hit.StandingOnGroundCollidersLastUpdate = updateCount;
//                hit.StandingOnGroundColliders = false;
//                return;
//            }
//            if (clearedColdiers.Add(hit.MineRock5))
//                ClearColdiers(hitAreaConteners[hit.MineRock5]);
//            if (hit.StandingOnGroundLastUpdate != updateCount)
//                FillStandingOnGround(hit);
//            hit.StandingOnGroundLastUpdate = updateCount;
//            if (hit.StandingOnGround)
//                return;
//            foreach (HitAreaContener sticks in GetSticks(hit))
//            {
//                if (sticks.StandingOnGroundLastUpdate != updateCount)
//                    FillStandingOnGround(sticks);
//                if (sticks.StandingOnGround || sticks.StandingOnGroundCollidersLastUpdate == updateCount && sticks.StandingOnGroundColliders)
//                {
//                    hit.StandingOnGroundCollidersLastUpdate = updateCount;
//                    hit.StandingOnGroundColliders = true;
//                    return;
//                }
//                else
//                    FindConnectedGround(sticks, cheked, clearedColdiers);
//                if (sticks.StandingOnGround || sticks.StandingOnGroundCollidersLastUpdate == updateCount && sticks.StandingOnGroundColliders)
//                {
//                    hit.StandingOnGroundCollidersLastUpdate = updateCount;
//                    hit.StandingOnGroundColliders = true;
//                    return;
//                }
//            }
//        }



//        private static void FillGroundColldiders(HitAreaContener hit)
//        {
//            hit.StandingOnGround = false;
//            hit.StandingOnGroundLastUpdate = updateCount;
//            int count = Physics.OverlapBoxNonAlloc(hit.HitArea.m_collider.bounds.center, hit.HitArea.m_collider.bounds.size * 0.5f, tempColider, hit.HitArea.m_bound.m_rot, layerMaskTerrain);
//            HashSet<MeshCollider> tempGroundColldiders = new HashSet<MeshCollider>();
//            Dictionary<Collider, FlattenedDataCollider> tempFlattenedDatGroundColldiders = new Dictionary<Collider, FlattenedDataCollider>();
//            if (hit.GroundColldiders != null)
//            {
//                tempGroundColldiders = hit.GroundColldiders;
//                tempFlattenedDatGroundColldiders = hit.FlattenedDatGroundColldiders;
//            }
//            else
//                hit.GroundChange = true;
//            hit.GroundColldiders = new HashSet<MeshCollider>();
//            hit.FlattenedDatGroundColldiders = new Dictionary<Collider, FlattenedDataCollider>();
//            for (int i = 0; i < count; i++)
//            {
//                MeshCollider meshCollider = (MeshCollider)tempColider[i];
//                if (tempGroundColldiders.Add(meshCollider))
//                {
//                    hit.GroundColldiders.Add(meshCollider);
//                    hit.FlattenedDatGroundColldiders.Add(meshCollider, new FlattenedDataCollider(meshCollider, meshCollider.GetInstanceID()));
//                    hit.GroundChange = true;
//                }
//                else
//                {
//                    hit.GroundColldiders.Add(meshCollider);
//                    hit.FlattenedDatGroundColldiders.Add(meshCollider, tempFlattenedDatGroundColldiders[meshCollider]);
//                }
//            }
//            if (!hit.GroundChange && hit.GroundColldiders.Count != tempGroundColldiders.Count)
//                hit.GroundChange = true;
//        }

//        private static void FillColliders(HitAreaContener hit)
//        {
//            ClearColdiers(hitAreaConteners[hit.MineRock5]);
//            FillGroundColldiders(hit);
//            if (hit.Colliders == null)
//                hit.Colliders = Physics.OverlapBox(hit.HitArea.m_collider.bounds.center, hit.HitArea.m_collider.bounds.size * 0.5f, hit.HitArea.m_bound.m_rot, layerMask).Where(a => hitAreaContenersAll.ContainsKey(a)).ToHashSet();
//        }




//        private static void FillColliders(HitAreaContener hit, HashSet<int> cheked, HashSet<MineRock5> clearedColdiers)
//        {
//            if (!cheked.Add(hit.Id))
//                return;
//            if (!hitAreaConteners.ContainsKey(hit.MineRock5))
//                return;
//            if (clearedColdiers.Add(hit.MineRock5))
//                ClearColdiers(hitAreaConteners[hit.MineRock5]);

//            Debug.Log($"FillGroundColldiders {cheked.Count} {clearedColdiers.Count}");
//            FillGroundColldiders(hit);
//            if (hit.Colliders == null)
//            {
//                hit.Colliders = Physics.OverlapBox(hit.HitArea.m_collider.bounds.center, hit.HitArea.m_collider.bounds.size * 0.5f, hit.HitArea.m_bound.m_rot, layerMask).Where(a => hitAreaContenersAll.ContainsKey(a)).ToHashSet();
//                hit.FlattenedDataCollider = new FlattenedDataCollider(hit.HitArea.m_collider.GetComponent<MeshCollider>(), hit.Id);
//            }
//            foreach (Collider sticks in hit.Colliders)
//                if (hitAreaContenersAll.TryGetValue(sticks, out HitAreaContener hitStick))
//                    FillColliders(hitStick, cheked, clearedColdiers);
//        }

//        private static MineRock5 GetKeyHitAreaConteners()
//        {
//            MineRock5 key;
//            if (priorityQueue.Count > 0)
//            {
//                key = priorityQueue.First();
//                priorityQueue.Remove(key);
//                return key;
//            }
//            if (queue.Count == 0)
//                queue = hitAreaConteners.Keys.ToHashSet();
//            key = queue.First();
//            queue.Remove(key);
//            return key;
//        }

//        private static void ClearColdiers(Dictionary<Collider, HitAreaContener> hitAreaContener)
//        {
//            foreach (KeyValuePair<Collider, HitAreaContener> colider in hitAreaContener.ToList())
//            {
//                MineRock5.HitArea hitArea = hitAreaContener[colider.Key].HitArea;
//                if (hitArea.m_health <= 0)
//                {
//                    hitAreaContener.Remove(colider.Key);
//                    hitAreaContenersAll.TryRemove(colider.Key, out _);
//                    continue;
//                }
//            }
//        }


//        private static IEnumerable<HitAreaContener> GetSticks(HitAreaContener hit)
//        {
//            FlattenedDataCollider colliderData = hit.FlattenedDataCollider;
//            foreach (Collider collider in hit.Colliders.ToList())
//            {
//                FlattenedDataCollider colliderDataStick = hitAreaContenersAll[collider].FlattenedDataCollider;
//                if (SATCollisionDetection2.CheckCollision(colliderData.vertices, colliderData.indices, colliderDataStick.vertices, colliderDataStick.indices))
//                {
//                    yield return hitAreaContenersAll[collider];
//                }
//            }
//        }

//        //private static bool HaveGroundIndStick(HitAreaContener hit, HashSet<HitAreaContener> cheked)
//        //{
//        //    if (hit.StandingOnGround)
//        //        return true;
//        //    if (hit.NoHaveGroundInSticked == updateCount)
//        //        return false;
//        //    if (hit.HaveGroundInSticked == updateCount)
//        //        return true;
//        //    foreach (HitAreaContener hited in hit.Sticks)
//        //    {
//        //        Debug.Log($"b {hited?.ToString() ?? "null"} {cheked?.ToString() ?? "null"} ");
//        //        if (cheked.Add(hited))
//        //            if (HaveGroundIndStick(hited, cheked))
//        //            {
//        //                hit.HaveGroundInSticked = updateCount;
//        //                return true;
//        //            }
//        //    }
//        //    hit.NoHaveGroundInSticked = updateCount;
//        //    return false;
//        //}

//        private static void Destroy(HitAreaContener hit)
//        {
//            hit.HitArea.m_health = 0;
//            Vector3 position = hit.HitArea.m_collider.bounds.center;
//            hit.MineRock5.m_destroyedEffect.Create(position, Quaternion.identity);
//            foreach (GameObject drop in hit.MineRock5.m_dropItems.GetDropList())
//            {
//                Vector3 positionDrop = position + UnityEngine.Random.insideUnitSphere * 0.3f;
//                Instantiate(drop, positionDrop, Quaternion.identity);
//            }
//        }

//        internal static void DestroyOnTime()
//        {
//            if (toDestroy.Count == 0)
//                return;
//            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
//            int count = 0;
//            HashSet<MineRock5> touched = new HashSet<MineRock5>();
//            do
//            {
//                if (toDestroy.TryDequeue(out HitAreaContener hit) && hit != null && hit.HitArea.m_health > 0)
//                {
//                    if (touched.Add(hit.MineRock5))
//                        hit.MineRock5.LoadHealth();
//                    Destroy(hit);
//                }
//                count++;
//            } while (toDestroy.Count != 0 && sw.ElapsedMilliseconds <= 10);
//            foreach (MineRock5 touch in touched)
//            {
//                touch.SaveHealth();
//                touch.UpdateMesh();
//                if (touch.AllDestroyed())
//                    touch.m_nview.Destroy();
//            }
//            sw.Stop();
//            Debug.Log($"DestroyOnTime {sw.Elapsed} {touched.Count}/{count}");
//        }

//        public void OnDestroy()
//        {
//            foreach (Collider key in hitAreaConteners[mineRock5].Keys)
//            {
//                hitAreaContenersAll.TryRemove(key, out _);
//            }
//            hitAreaConteners.TryRemove(mineRock5, out _);
//        }
//    }
//}
