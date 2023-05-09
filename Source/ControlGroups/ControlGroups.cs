using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Linq;

//TODO: save the groups ILoadReferenceable
// https://spdskatr.github.io/RWModdingResources/saving-guide
// https://github.com/pardeike/Achtung2/blob/eeb394041dc1e79ee3a1a1e3e9ef8055a84eb458/Source/ForcedWork.cs#L301
//TODO: add option to choose/override hotkeys

namespace ControlGroups
{
    public class ControlGroups : Mod
    {
        public static ControlGroupsSettings settings;

        public static Dictionary<int, List<Thing>> groups;

        public ControlGroups(ModContentPack content) : base(content)
        {
            settings = GetSettings<ControlGroupsSettings>();
        }

        public override void DoSettingsWindowContents(Rect canvas)
        {
            ControlGroupsSettings.DoWindowContents(canvas);

            base.DoSettingsWindowContents(canvas);
        }

        public override string SettingsCategory()
        {
            return "Control Groups";
        }

        public static int GroupKeyPressed()
        {
            foreach (KeyValuePair<int, KeyCode> groupKey in ControlGroupsSettings.groupKeys)
            {
                if (Input.GetKey(groupKey.Value))
                    return groupKey.Key;
            }

            return -1;
        }

        public static void AddThing(int groupID, object obj)
        {
            if (obj == null)
                return;

            if (!(obj is Thing thing))
                return;

            if (thing.Destroyed)
                return;

            if (groups[groupID].Contains(thing))
                return;

            groups[groupID].Add(thing);
        }

        public static bool ValidThing(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is Thing thing) || (thing != null && thing.Destroyed))
            {
                return false;
            }

            return true;
        }
    }

    [StaticConstructorOnStartup]
    public static class ControlGroupsLoader
    {
        static Selector selector;

        static int? currentGroup;

        // Selection from samples
        static object sample;

        static ControlGroupsLoader()
        {
            var harmony = new Harmony("com.necromunger.selectionmanager");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            ControlGroups.groups = new Dictionary<int, List<Thing>>();
        }

        [HarmonyPatch(typeof(Selector), "Select", new Type[] { typeof(object), typeof(bool), typeof(bool) })]
        class Patch_Selector_Select
        {
            private static void Postfix(Selector __instance, object obj, bool playSound = true, bool forceDesignatorDeselect = true)
            {
                if (selector == null)
                {
                    selector = __instance;
                }

                // Select more of selected type feature
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (__instance.SelectedObjects.Count == 1)
                    {
                        sample = __instance.SelectedObjects[0];
                        return;
                    }

                    if (sample != null && __instance.SelectedObjects.Count > 1)
                    {
                        var objectsToRemove = new List<object>();
                        foreach (object selectedObject in __instance.SelectedObjects)
                        {
                            if (sample.GetType() != selectedObject.GetType())
                            {
                                objectsToRemove.Add(selectedObject);
                            }
                        }

                        //todo, clear all and then select the ones that matched type
                        foreach (object removingObject in objectsToRemove)
                        {
                            selector.Deselect(removingObject);
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Selector), "Deselect", new Type[] { typeof(object) })]
        class Patch_Selector_Deselect
        {
            private static void Postfix(Selector __instance, object obj)
            {
                if (selector == null)
                {
                    selector = __instance;
                }
            }
        }

        [HarmonyPatch(typeof(Game), "UpdatePlay")]
        class Patch_Game_UpdatePlay
        {
            private static void Postfix()
            {
                if (selector == null)
                    return;

                // If user presses or releases button after binding, reset so that they may then select given group
                if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftAlt))
                     currentGroup = null;

                // check if any of the group keys were pressed
                int groupID = ControlGroups.GroupKeyPressed();
                if (groupID < 0)
                    return;

                // abort if not selecting a new group
                if (currentGroup == groupID)
                    return;

                currentGroup = groupID;

                var leftControl = Input.GetKey(KeyCode.LeftControl);
                var leftAlt = Input.GetKey(KeyCode.LeftAlt);

                // Assign currently selected objects to the numpad control group
                if (leftControl && selector.SelectedObjects.Count > 0)
                {
                    ControlGroups.groups[groupID] = new List<Thing>();

                    foreach (var obj in selector.SelectedObjects)
                        ControlGroups.AddThing(groupID, obj);

                    if (ControlGroups.settings.showMessages)
                        Messages.Message("ControlGroupSet".Translate(selector.SelectedObjects.Count.ToString(), groupID), MessageTypeDefOf.NeutralEvent, false);
                }
                // Add currently selected units to the numpad control group
                else if (leftAlt && selector.SelectedObjects.Count > 0)
                {
                    if (!ControlGroups.groups.ContainsKey(groupID) || ControlGroups.groups[groupID] == null)
                        ControlGroups.groups[groupID] = new List<Thing>();

                    foreach (var obj in selector.SelectedObjects)
                        ControlGroups.AddThing(groupID, obj);

                    if (ControlGroups.settings.showMessages)
                        Messages.Message("ControlGroupAdd".Translate(selector.SelectedObjects.Count.ToString(), groupID), MessageTypeDefOf.NeutralEvent, false);
                }
                // Selected a control group
                else if (!leftControl && !leftAlt && ControlGroups.groups.ContainsKey(groupID) && ControlGroups.groups[groupID].Count > 0)
                {
                    selector.ClearSelection();

                    Log.Message("[ControlGroups] selecting with key: " + groupID.ToString());

                    foreach (Thing thing in ControlGroups.groups[groupID])
                    {
                        Log.Message(thing.ToString());
                        selector.Select(thing);
                    }
                }
            }
        }
    }
}
