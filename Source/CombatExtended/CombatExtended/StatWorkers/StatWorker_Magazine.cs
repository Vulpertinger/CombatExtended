using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
namespace CombatExtended;
public class StatWorker_Magazine : StatWorker
{
    public CompAmmoUser compAmmo;

    private ThingDef GunDef(StatRequest req)
    {
        var def = req.Def as ThingDef;

        if (def?.building?.IsTurret ?? false)
        {
            def = def.building.turretGunDef;
        }

        return def;
    }

    public override bool ShouldShowFor(StatRequest req)
    {
        return base.ShouldShowFor(req) && (GunDef(req)?.GetCompProperties<CompProperties_AmmoUser>()?.magazineSize ?? 0) > 0;
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        compAmmo = req.Thing?.TryGetComp<CompAmmoUser>();
        if ((compAmmo?.Props?.ammoSet ?? null) != (((CompProperties_AmmoUser)req.Thing?.def?.comps?.Find(x => x is CompProperties_AmmoUser))?.ammoSet ?? null))
        {
            return compAmmo.Props.magazineSize;
        }
        float size = GunDef(req)?.GetCompProperties<CompProperties_AmmoUser>()?.magazineSize ?? 0;
        return size;
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        StringBuilder stringBuilder = new StringBuilder();
        var ammoProps = GunDef(req)?.GetCompProperties<CompProperties_AmmoUser>();
        stringBuilder.AppendLine("CE_MagazineSize".Translate() + ": " + GenText.ToStringByStyle(ammoProps.magazineSize, ToStringStyle.Integer));
        stringBuilder.AppendLine("CE_ReloadTime".Translate() + ": " + GenText.ToStringByStyle((ammoProps.reloadTime), ToStringStyle.FloatTwo) + " " + "LetterSecond".Translate());
        stringBuilder.AppendLine("This weapon reloads " + (ammoProps.reloadOneAtATime ? "one round at a time." : "the entire magazine at once."));
        return stringBuilder.ToString().TrimEndNewlines();
    }

    public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq, bool finalized = true)
    {        
        var ammoProps = GunDef(optionalReq)?.GetCompProperties<CompProperties_AmmoUser>();
        if (!optionalReq.HasThing)
        {  
            return ammoProps.magazineSize.ToString() + " / " + GenText.ToStringByStyle((ammoProps.reloadTime), ToStringStyle.FloatTwo) + " " + "LetterSecond".Translate() + (ammoProps.reloadOneAtATime ? " per" : "");
        }
        else
        {            
            return GetMagSize(optionalReq).ToString() + " / " + GenText.ToStringByStyle((ammoProps.reloadTime), ToStringStyle.FloatTwo) + " " + "LetterSecond".Translate() + (ammoProps.reloadOneAtATime ? " per" : "");
        }
    }

    private int GetMagSize(StatRequest req)
    {
        compAmmo = req.Thing?.TryGetComp<CompAmmoUser>();
        if ((compAmmo?.Props?.ammoSet ?? null) != (((CompProperties_AmmoUser)req.Thing?.def?.comps?.Find(x => x is CompProperties_AmmoUser))?.ammoSet ?? null))
        {
            return compAmmo.Props.magazineSize;
        }
        if (req.HasThing)
        {
            return (int)(compAmmo?.MagSize ?? req.Thing.GetStatValue(CE_StatDefOf.MagazineCapacity, cacheStaleAfterTicks: 250));
        }
        return GunDef(req)?.GetCompProperties<CompProperties_AmmoUser>()?.magazineSize ?? 0;
    }
}
