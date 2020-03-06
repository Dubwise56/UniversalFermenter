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
            if (!(t is IItemProcessor workThing) || workThing.Finished || workThing.SpaceLeftForItem <= 0)
            {
                return false;
            }

            //if the temperature is bad
            if (!workThing.TemperatureAcceptable)
            {
                JobFailReason.Is(Static.TemperatureTrans);
                return false;
            }

            if(!workThing.checkCanAdd())
            {
                // TODO figure out how to remove/abstract this
                //if (t is Building_Smoker smoker)
                //{

                //    if (!smoker.CanAddFood)
                //    {
                //        JobFailReason.Is(Static.SmokerLocked);
                //        return false;
                //    }
                //}
                return false;
            }

            if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced))
            {
                return false;
            }
            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }
            if (FindIngredient(pawn, workThing) == null)
            {
                JobFailReason.Is(Static.NoIngredient);
                return false;
            }
            return !t.IsBurning();
        }


        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            IItemProcessor workThing = t as IItemProcessor;
            Thing t2 = FindIngredient(pawn, workThing);
            return new Job(SrvDefOf.SRV_FillProcessor, t, t2)
            {
                count = workThing.SpaceLeftForItem
            };
        }


        private Thing FindIngredient(Pawn pawn, IItemProcessor workThing)
        {
            ThingFilter filter = workThing.ingredientFilter;
            Predicate<Thing> validator = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x) && filter.Allows(x);

            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, workThing.InputRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator);
        }
    }
}
