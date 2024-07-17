using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static System.Net.Mime.MediaTypeNames;
using Verse.Noise;

namespace RangedDPS
{
    public class StatWorker_OptimalDPS : StatWorker
    {
        static Dictionary<string, float> AccuracyDict = new Dictionary<string, float>();

        public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetExplanationFinalizePart(req, numberSense, finalVal));

            foreach (var AccData in AccuracyDict)
                stringBuilder.AppendLine(finalVal * AccData.Value + " (" + finalVal + "x" + AccData.Value + ") at " + AccData.Key);

            return stringBuilder.ToString();
        }

        public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
        {
            AccuracyDict.Clear();
            AccuracyDict.Add("Close", optionalReq.Thing.GetStatValue(StatDefOf.AccuracyTouch));
            AccuracyDict.Add("Short", optionalReq.Thing.GetStatValue(StatDefOf.AccuracyShort));
            AccuracyDict.Add("Medium", optionalReq.Thing.GetStatValue(StatDefOf.AccuracyMedium));
            AccuracyDict.Add("Long", optionalReq.Thing.GetStatValue(StatDefOf.AccuracyLong));

            var maxAccData = AccuracyDict.MaxBy(d => d.Value);

            return value * maxAccData.Value + " (" + value + "x" + maxAccData.Value + ") at " + maxAccData.Key;
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            if (!(req.Def is ThingDef thingDef) || !thingDef.IsRangedWeapon)
                return 0f;

            var Ranged = thingDef.Verbs.FirstOrDefault(v => v.Ranged);

            var Damage = Ranged.defaultProjectile.projectile.GetDamageAmount(req.Thing);
            var ticks_warmupTime = Ranged.warmupTime * 60;
            var ticks_Cooldown = req.Thing.GetStatValue(StatDefOf.RangedWeapon_Cooldown) * 60;
            var burstShotCount = Ranged.burstShotCount;
            var ticks_BurstShots = Ranged.ticksBetweenBurstShots;

            return Damage * burstShotCount / (ticks_Cooldown + ticks_warmupTime + ticks_BurstShots * (burstShotCount - 1)) * 60;
        }
    }
}
