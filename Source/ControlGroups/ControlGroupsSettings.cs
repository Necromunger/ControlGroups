using UnityEngine;
using Verse;


namespace ControlGroups
{
    public class ControlGroupsSettings : ModSettings
    {
        public bool showMessages = true;

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