using Verse;

namespace RimWorld
{
    // Token: 0x02000003 RID: 3
    public class InteractionWorker_DeepTalkMWNL : InteractionWorker
    {
        // Token: 0x04000001 RID: 1
        private const float BaseSelectionWeight = 0f;

        // Token: 0x06000003 RID: 3 RVA: 0x00002070 File Offset: 0x00000270
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            return 0f;
        }
    }
}