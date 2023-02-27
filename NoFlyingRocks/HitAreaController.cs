using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

namespace TomekDexValheimMod
{
    public class HitAreaContener
    {
        public MineRock5.HitArea HitArea { get; }
        public MineRock5 MineRock5 { get; }
        //public int Index { get; }
        public int Id { get; }
        //public bool StandingOnGround { get; set; }
        //public bool StandingOnGroundColliders { get; set; }
        public HashSet<Collider> Colliders { get; internal set; }
        //public int StandingOnGroundLastUpdate { get; internal set; }
        //public HashSet<MeshCollider> GroundColldiders { get; internal set; }
        public Dictionary<Collider, bool> HaveGroundGroundColldiders { get; internal set; }
        //public bool GroundChange { get; internal set; }
        public FlattenedDataCollider FlattenedDataCollider { get; internal set; }
        public List<FlattenedDataCollider> GroundFlattenedDataCollider { get; internal set; }

        //public int StandingOnGroundCollidersLastUpdate { get; internal set; }

        public static int nextID;

        public HitAreaContener(MineRock5.HitArea hitArea, MineRock5 mineRock5, int index)
        {
            HitArea = hitArea;
            MineRock5 = mineRock5;
            //Index = index;
            Id = nextID++;
        }
    }

    public class HitAreaController : MonoBehaviour
    {
        public static PropertyInfo meshColliderSharedMesh = typeof(MeshCollider).GetProperty("sharedMesh");
        public MineRock5 mineRock5;
        public static ConcurrentDictionary<MineRock5, Dictionary<Collider, HitAreaContener>> hitAreaConteners = new ConcurrentDictionary<MineRock5, Dictionary<Collider, HitAreaContener>>();
        public static readonly ConcurrentDictionary<Collider, HitAreaContener> hitAreaContenersAll = new ConcurrentDictionary<Collider, HitAreaContener>();
        private static readonly int layerMaskTerrain = LayerMask.GetMask("terrain");
        private static readonly int layerMask = LayerMask.GetMask("piece", "Default", "static_solid", "Default_small");
        private static readonly ConcurrentQueue<HitAreaContener> toDestroy = new ConcurrentQueue<HitAreaContener>();
        private static readonly HashSet<MineRock5> priorityQueue = new HashSet<MineRock5>();
        private static HashSet<MineRock5> queue = new HashSet<MineRock5>();
        private static readonly ConcurrentDictionary<MineRock5, Task> fillCollidersTask = new ConcurrentDictionary<MineRock5, Task>();
        private static readonly ConcurrentDictionary<MineRock5, Task> clearCollidersTasks = new ConcurrentDictionary<MineRock5, Task>();
        private static readonly ConcurrentDictionary<Collider, FlattenedDataCollider> groundFlattenedDataCollider = new ConcurrentDictionary<Collider, FlattenedDataCollider>();

        public void Awake()
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            mineRock5 = GetComponent<MineRock5>();
            for (int i = 0; i < mineRock5.m_hitAreas.Count; i++)
            {
                MineRock5.HitArea hitArea = mineRock5.m_hitAreas[i];
                if (hitArea.m_health > 0)
                {
                    HitAreaContener cont = new HitAreaContener(hitArea, mineRock5, i);
                    if (!hitAreaConteners.ContainsKey(mineRock5))
                        hitAreaConteners[mineRock5] = new Dictionary<Collider, HitAreaContener>();
                    hitAreaConteners[mineRock5][hitArea.m_collider] = cont;
                    hitAreaContenersAll[hitArea.m_collider] = cont;
                }
            }

            //fillCollidersTask[mineRock5] = new Task(() => FillCollidersTask(mineRock5));
            //fillCollidersTask[mineRock5].Start();
            fillCollidersTask.TryAdd(mineRock5, null);
            clearCollidersTasks.TryAdd(mineRock5, null);

            sw.Stop();
            Debug.Log($"Awake {sw.Elapsed}");
        }


        public static IEnumerator Coroutine()
        {
            while (true)
            {
                if (hitAreaConteners.Count == 0)
                {
                    Debug.Log($"hitAreaConteners.Count == 0");
                    Debug.Log($"hfillCollidersTask{fillCollidersTask.Count}");
                    yield return new WaitForSeconds(10f);
                    continue;
                }
                if (ChcekTasks(fillCollidersTask, FillCollidersTask))
                {
                    Debug.Log($"hfillCollidersTask.Any() {fillCollidersTask.Count}");
                    yield return new WaitForSeconds(5f);
                    continue;
                }
                if (ChcekTasks(clearCollidersTasks, ClearCollidersTask))
                {
                    Debug.Log($"clearCollidersTasks.Any() {clearCollidersTasks.Count}");
                    yield return new WaitForSeconds(5f);
                    continue;
                }
                MineRock5 key = GetKeyHitAreaConteners();
                if (key == null || Player.GetClosestPlayer(key.transform.position, 20) == null)
                {
                    Debug.Log($"Player.GetClosestPlayer(key.transform.position, 20) == null {hitAreaConteners.Count}");
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }
                Debug.Log($"FillColliders");

                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                if (hitAreaConteners.TryGetValue(key, out Dictionary<Collider, HitAreaContener> data))
                {
                    string tempFilePath;
                    FlattenedDataMineRock5 flattenedDataMineRock5 = new FlattenedDataMineRock5(data.Values.ToArray(), hitAreaContenersAll);

                    tempFilePath = Path.GetTempFileName();
                    Debug.Log(tempFilePath);
                    using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(fileStream, flattenedDataMineRock5);
                    }
                    System.Diagnostics.Process.Start(@"D:\Steam\steamapps\common\Valheim\BepInEx\plugins\NoFlyingRocksCalculator.exe");
                    Debug.Log($"{File.Exists(".\\BepInEx\\plugins\\NoFlyingRocksCalculator.exe")} {".\\BepInEx\\plugins\\NoFlyingRocksCalculator.exe"}");
                    System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    startInfo.FileName = ".\\BepInEx\\plugins\\NoFlyingRocksCalculator.exe";
                    startInfo.Arguments = tempFilePath;
                    startInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                    Debug.Log(Directory.GetCurrentDirectory());
                    startInfo.RedirectStandardOutput = true;
                    startInfo.UseShellExecute = true;

                    System.Diagnostics.Process process = new System.Diagnostics.Process();
                    process.StartInfo = startInfo;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    Debug.Log(output);
                }




                //using (MemoryMappedFile mmf = MemoryMappedFile.CreateNew("sharedMemory", flattenedDataMineRock5.GetCapacity()))
                //{
                //    using (MemoryMappedViewStream stream = mmf.CreateViewStream())
                //    {
                //        BinaryFormatter formatter = new BinaryFormatter();
                //        formatter.Serialize(stream, flattenedDataMineRock5);
                //    }
                //}
                //FindConnectedGroundTask(key);
                sw.Stop();
                Debug.Log($"FindConnectedGroundTask {sw.Elapsed}");
                yield return new WaitForSeconds(10f);
            }
        }

        private static void FindConnectedGroundTask(MineRock5 key)
        {
            if (!hitAreaConteners.TryGetValue(key, out Dictionary<Collider, HitAreaContener> hitAreaContener))
                return;
            foreach (HitAreaContener hit in hitAreaConteners[key].Values)
            {
                HashSet<HitAreaContener> cheked = new HashSet<HitAreaContener>();
                if (!HasConnectionWithGround(hit, cheked))
                    toDestroy.Enqueue(hit);
            }
        }

        private static bool HasConnectionWithGround(HitAreaContener hit, HashSet<HitAreaContener> cheked)
        {
            if (!cheked.Add(hit))
                return false;

            if (hit.HaveGroundGroundColldiders.Any(a => a.Value))
                return true;
            foreach (Collider colider in hit.Colliders)
                if (hitAreaContenersAll.TryGetValue(colider, out HitAreaContener hitted))
                    if (HasConnectionWithGround(hitted, cheked))
                        return true;

            return false;
        }

        private static void FillConnectionWithGround(HitAreaContener hit)
        {
            Collider[] coliders = Physics.OverlapBox(hit.HitArea.m_collider.bounds.center, hit.HitArea.m_collider.bounds.size * 0.5f, hit.HitArea.m_bound.m_rot, layerMaskTerrain);
            hit.HaveGroundGroundColldiders = new Dictionary<Collider, bool>();
            hit.GroundFlattenedDataCollider = new List<FlattenedDataCollider>();
            foreach (Collider colider in coliders)
            {
                MeshCollider meshCollider = (MeshCollider)colider;
                if (!groundFlattenedDataCollider.TryGetValue(colider, out FlattenedDataCollider fl))
                {
                    fl = new FlattenedDataCollider(meshCollider, meshCollider.GetInstanceID());
                    groundFlattenedDataCollider[colider] = fl;
                }
                hit.GroundFlattenedDataCollider.Add(fl);
            }
        }


        private static void ChekConnectionWithGround(HitAreaContener hit)
        {
            Collider[] coliders = Physics.OverlapBox(hit.HitArea.m_collider.bounds.center, hit.HitArea.m_collider.bounds.size * 0.5f, hit.HitArea.m_bound.m_rot, layerMaskTerrain);
            hit.HaveGroundGroundColldiders = new Dictionary<Collider, bool>();
            foreach (Collider colider in coliders)
            {
                MeshCollider meshCollider = (MeshCollider)colider;
                if (!groundFlattenedDataCollider.TryGetValue(colider, out FlattenedDataCollider fl))
                {
                    fl = new FlattenedDataCollider(meshCollider, meshCollider.GetInstanceID());
                    groundFlattenedDataCollider[colider] = fl;
                }
                hit.HaveGroundGroundColldiders[colider] = SATCollisionDetection2.CheckCollision(fl, hit.FlattenedDataCollider);
            }
        }

        private static void FillCollidersTask(MineRock5 key)
        {
            if (!hitAreaConteners.TryGetValue(key, out Dictionary<Collider, HitAreaContener> mineRock5Data))
                return;
            foreach (HitAreaContener hit in mineRock5Data.Values)
            {
                hit.Colliders = Physics.OverlapBox(hit.HitArea.m_collider.bounds.center, hit.HitArea.m_collider.bounds.size * 0.5f, hit.HitArea.m_bound.m_rot, layerMask).Where(a => hitAreaContenersAll.ContainsKey(a)).ToHashSet();
                hit.FlattenedDataCollider = new FlattenedDataCollider(hit.HitArea.m_collider.GetComponent<MeshCollider>(), hit.Id);
            }
            fillCollidersTask.TryRemove(key, out _);
        }

        private static void ClearCollidersTask(MineRock5 key)
        {
            if (!hitAreaConteners.TryGetValue(key, out Dictionary<Collider, HitAreaContener> hitAreaContener))
            {
                clearCollidersTasks.TryRemove(key, out _);
                return;
            }
            foreach (HitAreaContener hit in hitAreaContener.Values.ToList())
            {
                foreach (Collider colider in hit.Colliders.ToList())
                    if (hitAreaContenersAll.TryGetValue(colider, out HitAreaContener hited))
                        if (hited.HitArea.m_health <= 0 || !SATCollisionDetection2.CheckCollision(hit.FlattenedDataCollider, hited.FlattenedDataCollider))
                            hit.Colliders.Remove(colider);

                FillConnectionWithGround(hit);
            }

            clearCollidersTasks.TryRemove(key, out _);
        }

        public static bool ChcekTasks(ConcurrentDictionary<MineRock5, Task> tasksDictionary, Action<MineRock5> taskAction)
        {
            if (!tasksDictionary.Any())
                return false;

            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            foreach (MineRock5 mineRock in tasksDictionary.Keys.ToList())
            {
                if (tasksDictionary[mineRock] == null)
                {
                    tasksDictionary[mineRock] = new Task(() => taskAction(mineRock));
                    tasksDictionary[mineRock].Start();
                }
                else
                {
                    if (tasksDictionary.TryGetValue(mineRock, out Task task) && task?.Status == TaskStatus.Faulted)
                    {
                        if (hitAreaConteners.ContainsKey(mineRock))
                        {
                            tasksDictionary[mineRock] = new Task(() => taskAction(mineRock));
                            tasksDictionary[mineRock].Start();
                        }
                        else
                        {
                            tasksDictionary.TryRemove(mineRock, out _);
                        }
                    }
                }
            }

            sw.Stop();
            Debug.Log($"ChcekTasks {sw.Elapsed}");
            return true;
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
                Instantiate(drop, positionDrop, Quaternion.identity);
            }
        }

        internal static void DestroyOnTime()
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
            Debug.Log($"DestroyOnTime {sw.Elapsed} {touched.Count}/{count}");
        }

        public void OnDestroy()
        {
            Debug.Log("OnDestroy");
            foreach (Collider key in hitAreaConteners[mineRock5].Keys)
            {
                hitAreaContenersAll.TryRemove(key, out _);
            }
            hitAreaConteners.TryRemove(mineRock5, out _);
        }
    }
}
