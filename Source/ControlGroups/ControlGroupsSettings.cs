using System.Collections.Generic;
using UnityEngine;
using Verse;


namespace ControlGroups
{
    public class ControlGroupsSettings : ModSettings
    {
        public bool showMessages = true;

        public static Dictionary<int, KeyCode> groupKeys = new Dictionary<int, KeyCode>
        {
            {0, KeyCode.Keypad0},
            {1, KeyCode.Keypad1},
            {2, KeyCode.Keypad2},
            {3, KeyCode.Keypad3},
            {4, KeyCode.Keypad4},
            {5, KeyCode.Keypad5},
            {6, KeyCode.Keypad6},
            {7, KeyCode.Keypad7},
            {8, KeyCode.Keypad8},
            {9, KeyCode.Keypad9}
        };

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref showMessages, "showMessages", true, true);
        }

        public static void DoWindowContents(Rect canvas)
        {
            Listing_Standard listingStandard = new Listing_Standard();

            listingStandard.Begin(canvas);
            listingStandard.CheckboxLabeled("SettingShowMessages".Translate(), ref ControlGroups.settings.showMessages);
            listingStandard.End();
        }
    }
}