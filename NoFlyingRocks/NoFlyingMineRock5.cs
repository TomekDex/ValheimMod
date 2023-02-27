using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TomekDexValheimMod
{
    [HarmonyPatch]
    public class NoFlyingMineRock5
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MineRock5), "Start")]
        public static void PrefixDamasageArea(MineRock5 __instance)
        {
            __instance.gameObject.AddComponent<HitAreaController>();
        }

        private static readonly int updateCount;

        //private static bool HaveGroundIndStick(HitAreaContener hit, HashSet<HitAreaContener> cheked)
        //{
        //    if (hit.StandingOnGround)
        //    {
        //        lightningAOE(hit.HitArea.m_collider.transform);
        //        return true;
        //    }
        //    if (hit.NoHaveGroundInSticked == updateCount)
        //        return false;
        //    if (hit.HaveGroundInSticked == updateCount)
        //        return true;
        //    foreach (HitAreaContener hited in hit.Sticks)
        //    {
        //        if (cheked.Add(hited))
        //            if (HaveGroundIndStick(hited, cheked))
        //            {
        //                hit.HaveGroundInSticked = updateCount;
        //                return true;
        //            }
        //    }
        //    hit.NoHaveGroundInSticked = updateCount;
        //    return false;
        //}

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MineRock5), "DamageArea")]
        public static bool PrefixDamageArea(MineRock5 __instance, int hitAreaIndex, HitData hit)
        {

            //HitAreaController.Proces(__instance);
            return false;
            //if (!__instance.m_haveSetupBounds)
            //{
            //    __instance.SetupColliders();
            //    __instance.m_haveSetupBounds = true;
            //}
            //MineRock5.HitArea hitArea = __instance.GetHitArea(hitAreaIndex);

            //bool iok = false;
            //float min = 10000;
            //int[] indices = hitArea.m_meshFilter.sharedMesh.GetIndices(0);
            //for (int i = 0; i < indices.Length; i++)
            //{
            //    Vector3 vertex0 = hitArea.m_meshFilter.transform.TransformPoint(hitArea.m_meshFilter.sharedMesh.vertices[indices[i]]);
            //    float groundHeight = ZoneSystem.instance.GetGroundHeight(vertex0);
            //    //Debug.Log($"groundHeight {groundHeight} {vertex0.y} {groundHeight - vertex0.y} {groundHeight >= vertex0.y} {vertex0}");
            //    if (groundHeight >= vertex0.y)
            //    {
            //        iok = true;
            //    }
            //    if (vertex0.y - groundHeight < min)
            //        min = vertex0.y - groundHeight;

            //}
            //var tempColider = new Collider[128];
            //var count = Physics.OverlapBoxNonAlloc(hitArea.m_collider.bounds.center, hitArea.m_collider.bounds.size * 0.5f, tempColider, hitArea.m_bound.m_rot, LayerMask.GetMask("terrain"));
            //for (int i = 0; i < count; i++)
            //{
            //    var a = ((MeshCollider)tempColider[i]);
            //    bool m1 = HitAreaController.MeshObjectCollisionDetection(hitArea.m_meshFilter, a);
            //    bool m2 = HitAreaController.MeshObjectCollisionDetection((Mesh)typeof(MeshCollider).GetProperty("sharedMesh").GetValue(a), a, hitArea.m_meshFilter);
            //}
            //Debug.Log($"Ray { iok}");
            //HitAreaContener hitAreaContener = HitAreaController.hitAreaConteners[__instance][hitArea.m_collider];
            //Log(hitAreaContener);

            //HashSet<HitAreaContener> cheked = new HashSet<HitAreaContener> { hitAreaContener };
            //updateCount++;
            //if (HaveGroundIndStick(hitAreaContener, cheked))
            //{

            //}
            //float groundHeight = ZoneSystem.instance.GetGroundHeight(hitArea.m_collider.bounds.center);
            //Vector3 vector = new Vector3(hitArea.m_collider.bounds.center.x, groundHeight, hitArea.m_collider.bounds.center.z);
            //var Distance = Vector3.Distance(vector, hitArea.m_collider.bounds.center);

            //Collider[] m_tempColliders = new Collider[128];
            //Debug.Log($"MineRock5.m_rayMask {MineRock5.m_rayMask}");
            //var m_rayMask = LayerMask.GetMask("piece", "Default", "static_solid", "Default_small", "terrain");
            //Debug.Log($"m_rayMask {m_rayMask}");
            //Debug.Log($"hitArea.m_bound.m_pos {hitArea.m_bound.m_pos}");
            //Debug.Log($"hitArea.m_collider.bounds.center - __instance.transform.position {hitArea.m_collider.bounds.center - __instance.transform.position}");
            //Debug.Log($"hitArea.m_bound.m_size {hitArea.m_bound.m_size}");
            //Debug.Log($"hitArea.m_bound.m_rot {hitArea.m_bound.m_rot}");
            //Debug.Log($"hitArea.m_bound.m_rot {Quaternion.identity}");
            //Debug.Log($"hitArea.m_collider.bounds.center {hitArea.m_collider.bounds.center}");
            //Debug.Log($"hitArea.m_collider.bounds.size {hitArea.m_collider.bounds.size}");
            //Debug.Log($"hitArea.m_collider.bounds.size* 0.5f {hitArea.m_collider.bounds.size * 0.5f}");
            //Debug.Log($"__instance.transform.position + hitArea.m_bound.m_pos {__instance.transform.position + hitArea.m_bound.m_pos}");
            //;
            //var a = Physics.OverlapBoxNonAlloc(__instance.transform.position + hitArea.m_bound.m_pos, hitArea.m_bound.m_size, m_tempColliders, hitArea.m_bound.m_rot, MineRock5.m_rayMask);
            //Debug.Log($"{a} groundHeight {groundHeight} {hitArea.m_collider.bounds.center} Distance {Distance}/{hitArea.m_collider.bounds.center.y - groundHeight}");
            //a = Physics.OverlapBoxNonAlloc(__instance.transform.position + hitArea.m_bound.m_pos, hitArea.m_bound.m_size, m_tempColliders, hitArea.m_bound.m_rot, m_rayMask);

            //Debug.Log($"{a} a1 {__instance.transform.position + hitArea.m_bound.m_pos} {hitArea.m_collider.bounds.center} {hitArea.m_collider.bounds.center == __instance.transform.position + hitArea.m_bound.m_pos}");
            //a = Physics.OverlapBoxNonAlloc(hitArea.m_collider.bounds.center, hitArea.m_collider.bounds.size * 0.5f, m_tempColliders, hitArea.m_bound.m_rot, m_rayMask);
            //Debug.Log($"{a} a2");
            //a = Physics.OverlapBoxNonAlloc(hitArea.m_collider.bounds.center, hitArea.m_collider.bounds.size * 0.5f, m_tempColliders, hitArea.m_bound.m_rot, LayerMask.GetMask("terrain"));
            //Debug.Log($"{a} a3");
            //a = Physics.OverlapBoxNonAlloc(__instance.transform.position + hitArea.m_bound.m_pos, hitArea.m_bound.m_size, m_tempColliders, hitArea.m_bound.m_rot, LayerMask.GetMask("terrain"));
            //Debug.Log($"{a} a4");
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MineRock5), "CheckSupport")]
        public static void PrefixIncinerate(MineRock5 __instance)
        {
            return;
            lightningAOE(__instance.transform);
            Debug.Log($"____________________________");
            Debug.Log($"____________________________");
            Debug.Log($"{MineRock5.m_rayMask}");
            int m_rayMask = LayerMask.GetMask("piece", "Default", "static_solid", "Default_small", "terrain");
            Debug.Log($"{m_rayMask}");

            for (int i = 0; i < __instance.m_hitAreas.Count; i++)
            {
                //          Collider[] m_tempColliders = new Collider[128];
                //MineRock5.HitArea hitArea = __instance.m_hitAreas[i];
                //      var a=  Physics.OverlapBoxNonAlloc(hitArea.m_bound.m_pos, hitArea.m_bound.m_size, m_tempColliders, hitArea.m_bound.m_rot, m_rayMask);
                //float groundHeight = ZoneSystem.instance.GetGroundHeight(hitArea.m_collider.bounds.center);
                //Vector3 vector = new Vector3(hitArea.m_collider.bounds.center.x, groundHeight, hitArea.m_collider.bounds.center.z);
                //var Distance =  Vector3.Distance(vector, hitArea.m_collider.bounds.center);

                //Debug.Log($"groundHeight {groundHeight} {hitArea.m_collider.bounds.center} Distance {Distance}/{hitArea.m_collider.bounds.center.y- groundHeight}");
                //Log(hitArea.m_collider);
                //if (hitArea.m_supported)
                //    {
                //        HitData hitData = new HitData();
                //        hitData.m_damage.m_damage = __instance.m_health;
                //        hitData.m_point = hitArea.m_collider.bounds.center;
                //        hitData.m_toolTier = 100;
                //        __instance.DamageArea(i, hitData);
                //    }
            }

            Log(__instance);
            //foreach (MineRock5.HitArea item in __instance.m_hitAreas)
            //{
            //    //Debug.Log($"#####");
            //    //Log(item);
            //    //Log(item.m_collider);

            //    if (!item.m_supported)
            //    {
            //        item.m_health = -1; ;
            //        foreach (GameObject drop in __instance.m_dropItems.GetDropList())
            //        {
            //            Vector3 position = UnityEngine.Random.insideUnitSphere * 0.3f;
            //            UnityEngine.Object.Instantiate(drop, position, Quaternion.identity);
            //        }
            //        if (__instance.AllDestroyed())
            //        {
            //            __instance.m_nview.Destroy();
            //        }
            //    }
            //    __instance.UpdateMesh();
            //}
            //Debug.Log($"");

            Debug.Log($"____________________________");
            Debug.Log($"____________________________");
            Debug.Log($"____________________________");
            //foreach (MineRock5.HitArea hitArea in __instance.m_hitAreas)
            //{
            //    hitArea.m_supported = false;
            //}


        }

        public static void lightningAOE(Transform t)
        {
            GameObject p = ZNetScene.instance.GetPrefab("lightningAOE");
            p = UnityEngine.Object.Instantiate(p);
            p.name = "Cloned";
            Transform childToRemove = p.transform.Find("AOE_ROD");
            if (childToRemove != null)
                childToRemove.parent = null;
            Transform childToRemove2 = p.transform.Find("AOE_AREA");
            if (childToRemove2 != null)
                childToRemove2.parent = null;
            UnityEngine.Object.Instantiate(p, t.position, t.rotation);
        }
        public static void Log(object obj)
        {
            if (obj == null)
                return;

            foreach (PropertyInfo item in obj.GetType().GetProperties())
            {
                object v = item.GetValue(obj);
                if (v != default)
                    Debug.Log($"{item.Name} {v}");
            }
            foreach (FieldInfo item in obj.GetType().GetFields())
            {
                object v = item.GetValue(obj);
                if (v != default)
                    Debug.Log($"{item.Name} {v}");
            }
        }
    }
}
