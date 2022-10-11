using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld;

public class InteractionWorker_BreakupMWNL : InteractionWorker
{
    private const float BaseChance = 0f;

    private const float SpouseRelationChanceFactor = 0f;

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        return 0f;
    }

    public Thought RandomBreakupReason(Pawn initiator, Pawn recipient)
    {
        var list = (from m in initiator.needs.mood.thoughts.memories.Memories
            where m != null && m.otherPawn == recipient && m.CurStage != null && m.CurStage.baseOpinionOffset < 0f
            select m).ToList();
        Thought result;
        if (list.Count == 0)
        {
            result = null;
        }
        else
        {
            var worstMemoryOpinionOffset = list.Max(m => -m.CurStage.baseOpinionOffset);
            (from m in list
                where -m.CurStage.baseOpinionOffset >= worstMemoryOpinionOffset / 2f
                select m).TryRandomElementByWeight(m => -m.CurStage.baseOpinionOffset, out var thought_Memory);
            result = thought_Memory;
        }

        return result;
    }

    public virtual void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
    {
        var thought = RandomBreakupReason(initiator, recipient);
        if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient))
        {
            initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, recipient);
            initiator.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, recipient);
            recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DivorcedMe, initiator);
            initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
            recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
            initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HoneymoonPhase,
                recipient);
            recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HoneymoonPhase,
                initiator);
        }
        else
        {
            initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
            initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, recipient);
            initiator.relations.AddDirectRelation(PawnRelationDefOf.ExLover, recipient);
            recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BrokeUpWithMe, initiator);
        }

        if (initiator.ownership.OwnedBed != null &&
            initiator.ownership.OwnedBed == recipient.ownership.OwnedBed)
        {
            var pawn = Rand.Value >= 0.5f ? recipient : initiator;
            pawn.ownership.UnclaimBed();
        }

        TaleRecorder.RecordTale(TaleDefOf.Breakup, initiator, recipient);
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("LetterNoLongerLovers".Translate(initiator.LabelShort, recipient.LabelShort));
        if (thought != null)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("FinalStraw".Translate(thought.CurStage.label));
        }

        if (PawnUtility.ShouldSendNotificationAbout(initiator) ||
            PawnUtility.ShouldSendNotificationAbout(recipient))
        {
            Find.LetterStack.ReceiveLetter("LetterLabelBreakup".Translate(), stringBuilder.ToString(),
                LetterDefOf.NegativeEvent, initiator);
        }
    }
}