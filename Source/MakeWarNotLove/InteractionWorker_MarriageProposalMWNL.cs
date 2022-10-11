using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class InteractionWorker_MarriageProposalMWNL : InteractionWorker
{
    private const float BaseSelectionWeight = 0f;

    private const float BaseAcceptanceChance = 0f;

    private const float BreakupChanceOnRejection = 0f;

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        return 0f;
    }

    public virtual void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
    {
        var num = AcceptanceChance(initiator, recipient);
        var brokeUp = false;
        if (Rand.Value < num)
        {
            initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
            initiator.relations.AddDirectRelation(PawnRelationDefOf.Fiance, recipient);
            initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(
                ThoughtDefOf.RejectedMyProposal, recipient);
            recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(
                ThoughtDefOf.RejectedMyProposal, initiator);
            initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(
                ThoughtDefOf.RejectedMyProposalMood, recipient);
            recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(
                ThoughtDefOf.RejectedMyProposalMood, initiator);
            initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(
                ThoughtDefOf.IRejectedTheirProposal, recipient);
            recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(
                ThoughtDefOf.IRejectedTheirProposal, initiator);
            extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalAccepted);
        }
        else
        {
            initiator.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RejectedMyProposal, recipient);
            recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.IRejectedTheirProposal, initiator);
            extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalRejected);
            if (Rand.Value < 0.4f)
            {
                initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
                initiator.relations.AddDirectRelation(PawnRelationDefOf.ExLover, recipient);
                brokeUp = true;
                extraSentencePacks.Add(RulePackDefOf.Sentence_MarriageProposalRejectedBrokeUp);
            }
        }

        if (initiator.IsColonist || recipient.IsColonist)
        {
            SendLetter(initiator, recipient, Rand.Value < num, brokeUp);
        }
    }

    public float AcceptanceChance(Pawn initiator, Pawn recipient)
    {
        return 0f;
    }

    private void SendLetter(Pawn initiator, Pawn recipient, bool accepted, bool brokeUp)
    {
        var stringBuilder = new StringBuilder();
        string text;
        LetterDef letterDef;
        if (accepted)
        {
            text = "LetterLabelAcceptedProposal".Translate();
            letterDef = LetterDefOf.PositiveEvent;
            stringBuilder.AppendLine("LetterAcceptedProposal".Translate(initiator, recipient));
        }
        else
        {
            text = "LetterLabelRejectedProposal".Translate();
            letterDef = LetterDefOf.NegativeEvent;
            stringBuilder.AppendLine("LetterRejectedProposal".Translate(initiator, recipient));
            if (brokeUp)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("LetterNoLongerLovers".Translate(initiator, recipient));
            }
        }

        Find.LetterStack.ReceiveLetter(text, stringBuilder.ToString().TrimEndNewlines(), letterDef, initiator);
    }
}