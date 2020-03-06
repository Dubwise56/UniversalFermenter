using System;

using Verse;

namespace UniversalProcessors
{

    public interface IItemProcessor
    {

        float Progress { get; set; }

        ThingRequest InputRequest { get; }

        int SpaceLeftForItem { get; }

        bool Empty { get; }

        bool Finished { get; }

        bool TemperatureAcceptable { get; }

        int EstimatedTicksLeft { get; }

        //Predicate<Thing> ItemValidator(Pawn pawn);
        ThingFilter ingredientFilter { get; }

        int AddItem(Thing item);

        bool checkCanAdd();

        void Reset();

        Thing TakeOutProduct();
    }
}
