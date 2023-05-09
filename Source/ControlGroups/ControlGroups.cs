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

        public static KeyCode[] groupKeys = {
            KeyCode.Keypad0,
            KeyCode.Keypad1,
            KeyCode.Keypad2,
            KeyCode.Keypad3,
            KeyCode.Keypad4,
            KeyCode.Keypad5,
            KeyCode.Keypad6,
            KeyCode.Keypad7,
            KeyCode.Keypad8,
            KeyCode.Keypad9,
        };

        public static Dictionary<KeyCode, List<object>> groups;

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

        public static KeyCode GroupKeyPressed()
        {
            foreach (KeyCode code in groupKeys)
            {
                if (Input.GetKey(code))
                {
                    return code;
                }
            }

            return KeyCode.None;
        }

        public static bool ValidObject(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is Thing thing && thing.Destroyed)
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

        static KeyCode currentKeyCode;

        // Selection from samples
        static object sample;

        static ControlGroupsLoader()
        {
            var harmony = new Harmony("com.necromunger.selectionmanager");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            ControlGroups.groups = new Dictionary<KeyCode, List<object>>();
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

        [HarmonyPatch(typeof(Selector), "ClearSelection")]
        class Patch_Selector_ClearSelection
        {
            private static void Postfix(Selector __instance)
            {
                // If selection was cleared, reset group selection
                currentKeyCode = KeyCode.None;
            }
        }

        [HarmonyPatch(typeof(Game), "UpdatePlay")]
        class Patch_Game_UpdatePlay
        {
            private static void Postfix()
            {
                if (selector == null)
                {
                    return;
                }

                // If user releases button after binding, reset so that they may then select given group
                if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.LeftAlt))
                {
                    currentKeyCode = KeyCode.None;
                }

                // check if any of the group keys were pressed
                KeyCode groupKey = ControlGroups.GroupKeyPressed();
                if (groupKey == KeyCode.None)
                {
                    return;
                }

                // abort if not selecting a new group
                if (currentKeyCode == groupKey)
                {
                    return;
                }

                currentKeyCode = groupKey;

                // Assign currently selected objects to the numpad control group
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    if (selector.SelectedObjects.Count > 0)
                    {
                        ControlGroups.groups[groupKey] = new List<object>();
                        ControlGroups.groups[groupKey].AddRange(selector.SelectedObjects);

                        if (ControlGroups.settings.showMessages)
                        {
                            Messages.Message("ControlGroupSet".Translate(selector.SelectedObjects.Count.ToString(), groupKey.ToString().Last()), MessageTypeDefOf.NeutralEvent, false);
                        }
                    }
                }
                // Add currently selected units to the numpad control group
                else if (Input.GetKey(KeyCode.LeftAlt))
                {
                    if (selector.SelectedObjects.Count > 0)
                    {
                        if (ControlGroups.groups[groupKey] == null)
                            ControlGroups.groups[groupKey] = new List<object>();

                        ControlGroups.groups[groupKey].AddRange(selector.SelectedObjects.Except(ControlGroups.groups[groupKey]));

                        if (ControlGroups.settings.showMessages)
                        {
                            Messages.Message("ControlGroupAdd".Translate(selector.SelectedObjects.Count.ToString(), groupKey.ToString().Last()), MessageTypeDefOf.NeutralEvent, false);
                        }
                    }
                }
                // Selected a control group
                else
                {
                    if (ControlGroups.groups.ContainsKey(groupKey) && ControlGroups.groups[groupKey].Count > 0)
                    {
                        selector.ClearSelection();

                        ControlGroups.groups[groupKey].RemoveAll(item => !ControlGroups.ValidObject(item));

                        foreach (object obj in ControlGroups.groups[groupKey])
                        {
                            selector.Select(obj);
                        }
                    }
                }
            }
        }
    }
}
