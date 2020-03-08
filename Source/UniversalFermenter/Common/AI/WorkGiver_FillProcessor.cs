using System;

using RimWorld;
using Verse;
using Verse.AI;

namespace UniversalProcessors
{
    public class WorkGiver_FillProcessor : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest
        {
            get
            {
                return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
            }
        }

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // If the processors isn't finished or is full, there is no job
            CompUniversalFermenter comp = t.TryGetComp<CompUniversalFermenter>();

            if (comp == null || comp.Fermented || comp.SpaceLeftForIngredient <= 0)
            {
                return false;
            }

            //if the temperature is bad
            if (!comp.TemperatureAcceptable)
            {
                JobFailReason.Is(Static.TemperatureTrans);
                return false;
            }

            //if(!workThing.checkCanAdd())
            //{
            //    // TODO figure out how to remove/abstract this
            //    //if (t is Building_Smoker smoker)
            //    //{

            //    //    if (!smoker.CanAddFood)
            //    //    {
            //    //        JobFailReason.Is(Static.SmokerLocked);
            //    //        return false;
            //    //    }
            //    //}
            //    return false;
            //}

            if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced))
            {
                return false;
            }
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            if (FindIngredient(pawn, t) == null)
            {
                JobFailReason.Is(Static.NoIngredient);
                return false;
            }
            return !t.IsBurning();
        }


        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            CompUniversalFermenter comp = t.TryGetComp<CompUniversalFermenter>();
            Thing t2 = FindIngredient(pawn, t);
            return new Job(DefDatabase<JobDef>.GetNamed("FillProcessor"), t, t2)
            {

                count = comp.SpaceLeftForIngredient
            };
        }


        private Thing FindIngredient(Pawn pawn, Thing workThing)
        {
            ThingFilter filter = workThing.TryGetComp<CompUniversalFermenter>().Product.ingredientFilter;
            Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x) && filter.Allows(x);

            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator);
        }
    }
}
