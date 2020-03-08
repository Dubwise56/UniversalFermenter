using System;
using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace UniversalProcessors
{

    public class JobDriver_FillProcessor : JobDriver
    {

        private const TargetIndex ProcessorInd = TargetIndex.A;
        private const TargetIndex ItemInd = TargetIndex.B;

        protected Thing Processor
        {
            get
            {
                return job.GetTarget(TargetIndex.A).Thing;
            }
        }

        protected Thing Item
        {
            get
            {
                return job.GetTarget(TargetIndex.B).Thing;
            }
        }


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve((Thing)Processor, job) && pawn.Reserve((Thing)Processor, job);
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {

            CompUniversalFermenter comp = Processor.TryGetComp<CompUniversalFermenter>();

            // Verify processor and item validity
            this.FailOn(() => comp.SpaceLeftForIngredient <= 0);
            this.FailOnDespawnedNullOrForbidden(ProcessorInd);
            this.FailOnBurningImmobile(ProcessorInd);
            this.FailOn(() => (Item.TryGetComp<CompRottable>() != null && Item.TryGetComp<CompRottable>().Stage != RotStage.Fresh));
            AddEndCondition(() => (comp.SpaceLeftForIngredient > 0) ? JobCondition.Ongoing : JobCondition.Succeeded);

            // Reserve resources
            yield return Toils_General.DoAtomic(delegate
            {
                job.count = comp.SpaceLeftForIngredient;
            });
            Toil reserveItem = Toils_Reserve.Reserve(ItemInd);
            yield return reserveItem;

            // Reserve processor
            yield return Toils_Reserve.Reserve(ProcessorInd);

            // Go to the ingredient
            yield return Toils_Goto.GotoThing(ItemInd, PathEndMode.ClosestTouch)
                .FailOnDespawnedNullOrForbidden(ItemInd)
                .FailOnSomeonePhysicallyInteracting(ItemInd)
                .FailOnDestroyedNullOrForbidden(ItemInd);

            // Haul the ingredients
            yield return Toils_Haul.StartCarryThing(ItemInd, false, true, false)
                .FailOnDestroyedNullOrForbidden(ItemInd);
            yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveItem, ItemInd, TargetIndex.None, true, null);

            // Carry ingredients to fermenter
            yield return Toils_Goto.GotoThing(ProcessorInd, PathEndMode.Touch);

            // Add delay for adding ingredients to the fermenter
            yield return Toils_General.Wait(Static.GenericWaitDuration)
                .FailOnDestroyedNullOrForbidden(ItemInd)
                .FailOnDestroyedNullOrForbidden(ProcessorInd)
                .FailOnCannotTouch(ProcessorInd, PathEndMode.Touch)
                .WithProgressBarToilDelay(ProcessorInd);

            // Use the item
            yield return new Toil()
            {
                initAction = () =>
                {
                    int amountAccepted = comp.AddIngredient(Item);
                    if (amountAccepted <= 0)
                    {
                        EndJobWith(JobCondition.Incompletable);
                    }
                    if (amountAccepted >= pawn.carryTracker.CarriedThing.stackCount)
                    {
                        pawn.carryTracker.CarriedThing.Destroy();
                    }
                    else
                    {
                        pawn.carryTracker.CarriedThing.stackCount -= amountAccepted;
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };

            // End the current job
            yield break;
        }
    }
}
