using Verse;

namespace RimWorld;

public class InteractionWorker_SlightMWNL : InteractionWorker
{
    private const float BaseSelectionWeight = 0f;

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        return 0f;
    }
}