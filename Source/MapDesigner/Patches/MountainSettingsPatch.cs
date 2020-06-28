﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace MapDesigner.Patches
{

    [HarmonyPatch(typeof(RimWorld.GenStep_ElevationFertility))]
    [HarmonyPatch(nameof(RimWorld.GenStep_ElevationFertility.Generate))]
    internal static class MountainSettingsPatch
    {
        /// <summary>
        /// Mountain size and smoothness
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns></returns>
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            int hillSizeIndex = -1;
            int hillSmoothnessIndex = -1;
            float result = -1f;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R8)
                {
                    if (float.TryParse(codes[i].operand.ToString(), out result))
                    {
                        if (hillSmoothnessIndex == -1 && result == 2f)
                        {
                            hillSmoothnessIndex = i;
                        }
                        if (hillSizeIndex == -1 && result == 0.021f)
                        {
                            hillSizeIndex = i;
                        }
                    }
                }
                if (hillSizeIndex != -1 && hillSmoothnessIndex != -1)
                {
                    break;
                }
            }
            //codes[8] = new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: typeof(HelperMethods), name: nameof(HelperMethods.GetHillSize)));
            //codes[9] = new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: typeof(HelperMethods), name: nameof(HelperMethods.GetHillSmoothness)));
            codes[hillSizeIndex] = new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: typeof(HelperMethods), name: nameof(HelperMethods.GetHillSize)));
            codes[hillSmoothnessIndex] = new CodeInstruction(opcode: OpCodes.Call, operand: AccessTools.Method(type: typeof(HelperMethods), name: nameof(HelperMethods.GetHillSmoothness)));

            return codes.AsEnumerable();
        }

        /// <summary>
        /// Mountain amount
        /// Natural hill distribution
        /// </summary>
        /// <param name="map"></param>
        /// <param name="parms"></param>
        static void Postfix(Map map, GenStepParams parms)
        {
            MapDesignerSettings settings = LoadedModManager.GetMod<MapDesigner_Mod>().GetSettings<MapDesignerSettings>();
            MapGenFloatGrid elevation = MapGenerator.Elevation;

            // pushes hills away from center
            if (MapDesignerSettings.flagHillRadial)
            {
                IntVec3 center = map.Center;
                int size = map.Size.x / 2;
                float centerSize = settings.hillRadialSize * size;
                foreach (IntVec3 current in map.AllCells)
                {
                    float distance = (float)Math.Sqrt(Math.Pow(current.x - center.x, 2) + Math.Pow(current.z - center.z, 2));
                    elevation[current] += (settings.hillRadialAmt * (distance - centerSize) / size);
                }
            }

            // hills to both sides
            if (MapDesignerSettings.flagHillSplit)
            {
                float angle = settings.hillSplitDir;

                int mapSize = map.Size.x;
                float gapSize = 0.5f * mapSize * settings.hillSplitSize;
                float skew = settings.hillSplitAmt;

                ModuleBase slope = new AxisAsValueX();
                slope = new Rotate(0.0, 180.0 - angle, 0.0, slope);

                slope = new Translate(0.0 - map.Center.x, 0.0, 0.0 - map.Center.z, slope);

                float multiplier = skew / mapSize;

                foreach (IntVec3 current in map.AllCells)
                {
                    float value = slope.GetValue(current);
                    //float num = size - Math.Abs(value);
                    float num = Math.Abs(value) - gapSize;

                    //num = 1 + (skew * num / mapSize);
                    //num = 1 + num * multiplier;
                    elevation[current] += num * multiplier;
                    //elevation[current] *= num;
                    //elevation[current] += num - 1;
                }
            }

            // hills to one side
            if (MapDesignerSettings.flagHillSide)
            {
                float angle = settings.hillSideDir;
                float skew = settings.hillSideAmt;

                ModuleBase slope = new AxisAsValueX();
                slope = new Rotate(0.0, 180.0 - angle, 0.0, slope);
                slope = new Translate(0.0 - map.Center.x, 0.0, 0.0 - map.Center.z, slope);
                float multiplier = skew / map.Size.x;
                foreach (IntVec3 current in map.AllCells)
                {
                    //elevation[current] *= (1 + slope.GetValue(current) * multiplier);
                    //elevation[current] += 0.5f * slope.GetValue(current) * multiplier;
                    elevation[current] += slope.GetValue(current) * multiplier;

                }

            }


            // hill amount
            float hillAmount = settings.hillAmount;
            foreach (IntVec3 current in map.AllCells)
            {
                elevation[current] *= hillAmount;
            }


            // natural distribution
            if (MapDesignerSettings.flagHillClumping)
            {
                float hillSize = LoadedModManager.GetMod<MapDesigner_Mod>().GetSettings<MapDesignerSettings>().hillSize;

                if (hillSize > 0.022f)       // smaller than vanilla only, else skip this step
                {
                    float clumpSize = Rand.Range(0.01f, Math.Min(0.04f, hillSize));
                    float clumpStrength = Rand.Range(0.3f, 0.7f);

                    ModuleBase hillClumping = new Perlin(clumpSize, 0.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.Low);

                    foreach (IntVec3 current in map.AllCells)
                    {
                        elevation[current] += clumpStrength * hillClumping.GetValue(current);
                    }
                }
            }

        }

    }

}
