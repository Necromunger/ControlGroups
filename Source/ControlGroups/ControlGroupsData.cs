using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ControlGroups
{
    public class ControlGroupsData : WorldComponent
    {
        public ControlGroupsData(World world) : base(world) { }

        public override void ExposeData()
        {
            Log.Message("[ControlGroups] Expose Data");

            foreach (KeyValuePair<int, KeyCode> groupKey in ControlGroupsSettings.groupKeys)
            {
                if (ControlGroups.groups == null)
                    ControlGroups.groups = new Dictionary<int, List<Thing>>();

                var group = ControlGroups.groups[groupKey.Key];
                if (group == null)
                    group = new List<Thing>();

                Scribe_Collections.Look(ref group, "Group" + groupKey.Key, LookMode.Reference);

                ControlGroups.groups[groupKey.Key] = group;
            }
        }
    }
}