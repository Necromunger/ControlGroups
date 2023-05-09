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
            ControlGroups.Log("Expose Data");

            if (ControlGroups.groups == null)
                ControlGroups.groups = new Dictionary<int, List<Thing>>();

            foreach (KeyValuePair<int, KeyCode> groupKey in ControlGroupsSettings.groupKeys)
            {
                if (!ControlGroups.groups.ContainsKey(groupKey.Key))
                    ControlGroups.groups[groupKey.Key] = new List<Thing>();

                var group = ControlGroups.groups[groupKey.Key];

                Scribe_Collections.Look(ref group, "Group" + groupKey.Key, LookMode.Reference);

                ControlGroups.groups[groupKey.Key] = group;
            }

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                ControlGroups.Log("Saved Data:");

                UnwrapAndLog();
            }
            else if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ControlGroups.Log("Loaded Data;");

                UnwrapAndLog();
            }
        }

        private void UnwrapAndLog()
        {
            foreach (KeyValuePair<int, KeyCode> groupKey in ControlGroupsSettings.groupKeys)
            {
                ControlGroups.Log("Group:" + groupKey.Key);

                foreach (Thing thing in ControlGroups.groups[groupKey.Key])
                {
                    ControlGroups.Log("- " + thing.ToString());
                }
            }
        }
    }
}