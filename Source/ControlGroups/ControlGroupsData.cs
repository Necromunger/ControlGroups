using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace ControlGroups
{
    public class ControlGroupsData : WorldComponent
    {
        public ControlGroupsData(World world) : base(world) { }

        public override void ExposeData()
        {
			/*
			Scribe_Collections.Look(ref ControlGroups.groups, "groups", LookMode.Value, LookMode.TargetInfo);


			var group1 = ControlGroups.groups[0];


			if (Scribe.mode == LoadSaveMode.Saving)
			{
				foreach (KeyCode keyCode in ControlGroups.groupKeys)
				{
					LocalTargetInfo
					Scribe_Collections.Look(ref ControlGroups.groups, "groups", LookMode.Deep);
				}
			}

			Scribe_Collections.Look(ref ControlGroups.groups, "groups", LookMode.Deep);

			if (Scribe.mode == LoadSaveMode.Saving && currentItem != null)
			{
				dummyCell = currentItem.Cell;
				dummyThing = currentItem.Thing;
			}

			Scribe_Values.Look(ref dummyCell, "current-cell", IntVec3.Invalid, false);
			Scribe_References.Look(ref dummyThing, "current-thing-ref");

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (dummyThing != null)
					currentItem = new LocalTargetInfo(dummyThing);
				else
					currentItem = new LocalTargetInfo(dummyCell);
			}*/
		}
    }
}