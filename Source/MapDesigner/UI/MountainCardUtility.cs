﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;

namespace MapDesigner.UI
{

    public static class MountainCardUtility
    {
        //public MapDesignerSettings settings = MapDesigner_Mod.GetSettings<MapDesignerSettings>();
        public static MapDesignerSettings settings = LoadedModManager.GetMod<MapDesigner_Mod>().GetSettings<MapDesignerSettings>();
        private static float viewHeight;


        public static void DrawMountainCard(Rect rect)
        {

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(rect);

            // mountains
            settings.hillAmount = InterfaceUtility.LabeledSlider(listingStandard, settings.hillAmount, 0f, 2.5f, hillAmountLabel, "ZMD_hillAmount0".Translate(), "ZMD_hillAmount5".Translate());

            // It's reversed because that's more intuitive for the user. Smaller numbers = bigger hills
            settings.hillSize = InterfaceUtility.LabeledSlider(listingStandard, settings.hillSize, 0.1f, 0.010f, hillSizeLabel, "ZMD_hillSize4".Translate(), "ZMD_hillSize0".Translate());

            settings.hillSmoothness = InterfaceUtility.LabeledSlider(listingStandard, settings.hillSmoothness, 0f, 5f, hillSmoothnessLabel, "ZMD_hillSmoothness0".Translate(), "ZMD_hillSmoothness4".Translate());


            // general mountain related
            listingStandard.CheckboxLabeled("ZMD_flagCaves".Translate(), ref MapDesignerSettings.flagCaves, "ZMD_flagCavesTooltip".Translate());

            // hill distribution
            listingStandard.GapLine();

            if (settings.hillSize > 0.022f)
            {
                listingStandard.CheckboxLabeled("ZMD_flagHillClumping".Translate(), ref MapDesignerSettings.flagHillClumping, "ZMD_flagHillClumpingTooltip".Translate());
            }

            listingStandard.CheckboxLabeled("ZMD_flagHillRadial".Translate(), ref MapDesignerSettings.flagHillRadial, "ZMD_flagHillRadialTooltip".Translate());
            if (MapDesignerSettings.flagHillRadial)
            {
                Rect hillRadialRect = listingStandard.GetRect(60f + Text.CalcHeight(hillRadialAmtLabel, listingStandard.ColumnWidth - 40f) * 2);
                hillRadialRect.xMin += 20f;
                hillRadialRect.xMax -= 20f;

                Listing_Standard hillRadialListing = new Listing_Standard();
                hillRadialListing.Begin(hillRadialRect);

                settings.hillRadialAmt = InterfaceUtility.LabeledSlider(hillRadialListing, settings.hillRadialAmt, -3.0f, 3.0f, hillRadialAmtLabel, "ZMD_center".Translate(), "ZMD_edges".Translate(), "ZMD_hillRadialAmtTooltip".Translate());

                settings.hillRadialSize = InterfaceUtility.LabeledSlider(hillRadialListing, settings.hillRadialSize, -0.2f, 1.1f, hillRadialSizeLabel, "ZMD_center".Translate(), "ZMD_edges".Translate(), "ZMD_hillRadialSizeTooltip".Translate());

                hillRadialListing.End();
            }


            // reset
            listingStandard.GapLine();
            if (listingStandard.ButtonText("ZMD_resetMountain".Translate()))
            {
                ResetMountainSettings();
            }

            listingStandard.End();
        }


        public static void ResetMountainSettings()
        {
            settings.hillAmount = 1.0f;
            settings.hillSize = 0.021f;
            settings.hillSmoothness = 2.0f;
            MapDesignerSettings.flagCaves = true;
            MapDesignerSettings.flagOneRock = false;

            MapDesignerSettings.flagHillClumping = false;

            MapDesignerSettings.flagHillRadial = false;
            settings.hillRadialAmt = 1.0f;
            settings.hillRadialSize = 0.65f;

            //settings.rockTypeRange = new IntRange(2, 3);
            //MapDesignerSettings.flagBiomeRocks = false;
        }


        private static string hillRadialAmtLabel
        {
            get
            {
                float radialAmt = settings.hillRadialAmt;
                int label = 0;
                if (radialAmt > -2.0f)    //1
                {
                    label++;
                }
                if (radialAmt > -1.0f)    //2
                {
                    label++;
                }
                if (radialAmt > -0.4f)    //no cluster = 3
                {
                    label++;
                }
                if (radialAmt > 0.4f)     //4
                {
                    label++;
                }
                if (radialAmt > 1.0f)     //5
                {
                    label++;
                }
                if (radialAmt > 2.0f)     //6
                {
                    label++;
                }
                return InterfaceUtility.FormatLabel("ZMD_hillRadialAmtLabel", "ZMD_hillRadialAmt" + label);
            }
        }


        private static string hillRadialSizeLabel
        {
            get
            {
                float radialSize = settings.hillRadialSize;
                int label = 0;
                if (radialSize > 0.35f)
                {
                    label++;
                }
                if (radialSize > 0.55f)
                {
                    label++;
                }
                if (radialSize > 0.80f)
                {
                    label++;
                }
                if (radialSize > 0.95f)
                {
                    label++;
                }
                return InterfaceUtility.FormatLabel("ZMD_hillRadialSizeLabel", "ZMD_hillRadialSize" + label);
            }
        }


        public static string hillAmountLabel
        {
            get
            {
                int label = 0;
                if (settings.hillAmount > 0.5f)
                {
                    label++;
                }
                if (settings.hillAmount > 0.8f)
                {
                    label++;
                }
                if (settings.hillAmount > 1.2f)
                {
                    label++;
                }
                if (settings.hillAmount > 1.5f)
                {
                    label++;
                }
                if (settings.hillAmount > 1.75f)
                {
                    label++;
                }
                return InterfaceUtility.FormatLabel("ZMD_hillAmountLabel", "ZMD_hillAmount" + label);
            }
        }


        private static string hillSizeLabel
        {
            get
            {
                int label = 0;                      // huge

                if (settings.hillSize > 0.013f)   // vanilla
                {
                    label++;
                }
                if (settings.hillSize > 0.033f)  // small
                {
                    label++;
                }
                if (settings.hillSize > 0.055f)    // tiny
                {
                    label++;
                }
                if (settings.hillSize > 0.085f)   // very tiny
                {
                    label++;
                }

                return InterfaceUtility.FormatLabel("ZMD_hillSizeLabel", "ZMD_hillSize" + label);
            }
        }


        private static string hillSmoothnessLabel
        {
            get
            {
                int label = 0;
                if (settings.hillSmoothness > 0.8f)
                {
                    label++;
                }
                if (settings.hillSmoothness > 1.5f)
                {
                    label++;
                }
                if (settings.hillSmoothness > 2.5f)
                {
                    label++;
                }
                if (settings.hillSmoothness > 3.8f)
                {
                    label++;
                }

                return InterfaceUtility.FormatLabel("ZMD_hillSmoothnessLabel", "ZMD_hillSmoothness" + label);
            }
        }

    }
}
