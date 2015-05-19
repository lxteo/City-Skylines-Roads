using ColossalFramework;
using ICities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ToolsAnarchy
{
    public class AnarchyLoad : LoadingExtensionBase
    {
        GameObject anarchyUpdate;
        Anarchy anarchy;
        public override void OnLevelLoaded(LoadMode mode)
        {
            anarchyUpdate = new GameObject("elevationChanger");
            anarchy = anarchyUpdate.AddComponent<Anarchy>();
            anarchy.EnableHook();

        }

        public override void OnLevelUnloading()
        {
            anarchy.DisableHook();
            anarchy = null;
            GameObject.Destroy(anarchyUpdate);
            base.OnLevelUnloading();
        }

    }

    public class Anarchy : MonoBehaviour
    {
        private bool hookEnabled = false;        
        private Dictionary<MethodInfo, RedirectCallsState> redirects = new Dictionary<MethodInfo, RedirectCallsState>();

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.L)){
                if (hookEnabled)
                {
                    DisableHook();
                }
                else
                {
                    EnableHook();
                }
            }
        }

        public void EnableHook()
        {
            if (hookEnabled)
            {
                return;
            }
            var allFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
            var method = typeof(NetTool).GetMethods(allFlags).Single(c => c.Name == "CanCreateSegment" && c.GetParameters().Length == 11);
            redirects.Add(method, RedirectionHelper.RedirectCalls(method, typeof(Anarchy).GetMethod("CanCreateSegment", allFlags)));

            method = typeof(NetTool).GetMethod("CheckNodeHeights", allFlags);
            redirects.Add(method, RedirectionHelper.RedirectCalls(method, typeof(Anarchy).GetMethod("CheckNodeHeights", allFlags)));

            method = typeof(NetTool).GetMethod("CheckCollidingSegments", allFlags);
            redirects.Add(method, RedirectionHelper.RedirectCalls(method, typeof(Anarchy).GetMethod("CheckCollidingSegments", allFlags)));

            method = typeof(BuildingTool).GetMethod("CheckCollidingBuildings", allFlags);
            redirects.Add(method, RedirectionHelper.RedirectCalls(method, typeof(Anarchy).GetMethod("CheckCollidingBuildings", allFlags)));

            method = typeof(BuildingTool).GetMethod("CheckSpace", allFlags);
            redirects.Add(method, RedirectionHelper.RedirectCalls(method, typeof(Anarchy).GetMethod("CheckSpace", allFlags)));

            method = typeof(Building).GetMethods(allFlags).Single(c => c.Name == "CheckZoning" && c.GetParameters().Length == 1);
            redirects.Add(method, RedirectionHelper.RedirectCalls(method, typeof(Anarchy).GetMethod("CheckZoning", allFlags)));

            method = typeof(NetTool).GetMethod("GetElevation", allFlags);
            redirects.Add(method, RedirectionHelper.RedirectCalls(method, typeof(Anarchy).GetMethod("GetElevation", allFlags)));


            method = typeof(RoadAI).GetMethod("GetElevationLimits", allFlags);
            redirects.Add(method, RedirectionHelper.RedirectCalls(method, typeof(Anarchy).GetMethod("GetElevationLimits", allFlags)));

            method = typeof(TrainTrackAI).GetMethod("GetElevationLimits", allFlags);
            redirects.Add(method, RedirectionHelper.RedirectCalls(method, typeof(Anarchy).GetMethod("GetElevationLimits", allFlags)));

            method = typeof(PedestrianPathAI).GetMethod("GetElevationLimits", allFlags);
            redirects.Add(method, RedirectionHelper.RedirectCalls(method, typeof(Anarchy).GetMethod("GetElevationLimits", allFlags)));

            method = typeof(NetAI).GetMethod("BuildUnderground", allFlags);
            redirects.Add(method, RedirectionHelper.RedirectCalls(method, typeof(Anarchy).GetMethod("BuildUnderground", allFlags)));


            hookEnabled = true;
            Debug.Log("Hooked.");
        }
        
        public void DisableHook()
        {
            if (!hookEnabled)
            {
                return;
            }
            foreach (var kvp in redirects)
            {
                
                RedirectionHelper.RevertRedirect(kvp.Key, kvp.Value);
            }
            redirects.Clear();
            hookEnabled = false;
        }

        private static ToolBase.ToolErrors CanCreateSegment(NetInfo segmentInfo, ushort startNode, ushort startSegment, ushort endNode, ushort endSegment, ushort upgrading, Vector3 startPos, Vector3 endPos, Vector3 startDir, Vector3 endDir, ulong[] collidingSegmentBuffer)
        {
            return ToolBase.ToolErrors.None;
        }
        private static ToolBase.ToolErrors CheckNodeHeights(NetInfo info, FastList<NetTool.NodePosition> nodeBuffer)
        {
            return ToolBase.ToolErrors.None;
        }
        public static bool CheckCollidingBuildings(ulong[] buildingMask, ulong[] segmentMask)
        {
            return false;
        }
        public static bool CheckCollidingSegments(ulong[] segmentMask, ulong[] buildingMask, ushort upgrading)
        {
            return false;
        }

        private ToolBase.ToolErrors CheckSpace(BuildingInfo info, int relocating, Vector3 pos, float minY, float maxY, float angle, int width, int length, bool test, ulong[] collidingSegmentBuffer, ulong[] collidingBuildingBuffer)
        {
            return ToolBase.ToolErrors.None;
        }

        public bool CheckZoning(ItemClass.Zone zone)
        {
            return true;
        }

        private float GetElevation(NetInfo info)
        {
            var ele = (NetTool)ToolManager.instance.m_properties.CurrentTool;
            var mi = ele.GetType().GetField("m_elevation", BindingFlags.Instance | BindingFlags.NonPublic);
            return (float)Mathf.Clamp((int)mi.GetValue(ele), -512, 1920) * 6f;
        }

        public bool BuildUnderground()
        {
            return true;
        }

        public void GetElevationLimits(out int min, out int max)
        {
            min = -10;
            max = 64;
        }

    }



}
