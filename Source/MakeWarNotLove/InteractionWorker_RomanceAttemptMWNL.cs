using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class InteractionWorker_RomanceAttemptMWNL : InteractionWorker
{
    private const float MinAttractionForRomanceAttempt = 0.25f;

    private const int MinOpinionForRomanceAttempt = 5;

    private const float BaseSelectionWeight = 0f;

    private const float BaseSuccessChance = 0f;

    public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
    {
        return 0f;
    }

    public float SuccessChance(Pawn initiator, Pawn recipient)
    {
        return 0f;
    }

    public virtual void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
    {
        if (Rand.Value < SuccessChance(initiator, recipient))
        {
            BreakLoverAndFianceRelations(initiator, out var list);
            BreakLoverAndFianceRelations(recipient, out var list2);
            foreach (var pawn in list)
            {
                TryAddCheaterThought(pawn, initiator);
            }

            foreach (var pawn in list2)
            {
                TryAddCheaterThought(pawn, recipient);
            }

            initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.ExLover, recipient);
            initiator.relations.AddDirectRelation(PawnRelationDefOf.Lover, recipient);
            TaleRecorder.RecordTale(TaleDefOf.BecameLover, initiator, recipient);
            initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe,
                recipient);
            recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe,
                initiator);
            initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(
                ThoughtDefOf.FailedRomanceAttemptOnMe, recipient);
            initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(
                ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, recipient);
            recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(
                ThoughtDefOf.FailedRomanceAttemptOnMe, initiator);
            recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(
                ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, initiator);
            if (initiator.IsColonist || recipient.IsColonist)
            {
                SendNewLoversLetter(initiator, recipient, list, list2);
            }

            extraSentencePacks.Add(RulePackDefOf.Sentence_RomanceAttemptAccepted);
            LovePartnerRelationUtility.TryToShareBed(initiator, recipient);
        }
        else
        {
            initiator.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RebuffedMyRomanceAttempt, recipient);
            recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FailedRomanceAttemptOnMe, initiator);
            if (recipient.relations.OpinionOf(initiator) <= 0)
            {
                recipient.needs.mood.thoughts.memories.TryGainMemory(
                    ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, initiator);
            }

            extraSentencePacks.Add(RulePackDefOf.Sentence_RomanceAttemptRejected);
        }
    }

    private void BreakLoverAndFianceRelations(Pawn pawn, out List<Pawn> oldLoversAndFiances)
    {
        oldLoversAndFiances = [];
        for (;;)
        {
            var firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);
            if (firstDirectRelationPawn != null)
            {
                pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, firstDirectRelationPawn);
                pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn);
                oldLoversAndFiances.Add(firstDirectRelationPawn);
            }
            else
            {
                var firstDirectRelationPawn2 = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
                if (firstDirectRelationPawn2 == null)
                {
                    break;
                }

                pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, firstDirectRelationPawn2);
                pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn2);
                oldLoversAndFiances.Add(firstDirectRelationPawn2);
            }
        }
    }

    private void TryAddCheaterThought(Pawn pawn, Pawn cheater)
    {
        if (!pawn.Dead)
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CheatedOnMe, cheater);
        }
    }

    private void SendNewLoversLetter(Pawn initiator, Pawn recipient, List<Pawn> initiatorOldLoversAndFiances,
        List<Pawn> recipientOldLoversAndFiances)
    {
        var result = false;
        string text;
        LetterDef letterDef;
        Pawn t;
        if (initiator.GetFirstSpouse() != null && !initiator.GetFirstSpouse().Dead ||
            recipient.GetFirstSpouse() != null && !recipient.GetFirstSpouse().Dead)
        {
            text = "LetterLabelAffair".Translate();
            letterDef = LetterDefOf.NegativeEvent;
            if (initiator.GetFirstSpouse() != null && !initiator.GetFirstSpouse().Dead)
            {
                t = initiator;
            }
            else
            {
                t = recipient;
            }

            result = true;
        }
        else
        {
            text = "LetterLabelNewLovers".Translate();
            letterDef = LetterDefOf.PositiveEvent;
            t = initiator;
        }

        var stringBuilder = new StringBuilder();
        if (result)
        {
            if (initiator.GetFirstSpouse() != null)
            {
                stringBuilder.AppendLine("LetterAffair".Translate(initiator.LabelShort,
                    initiator.GetFirstSpouse().LabelShort, recipient.LabelShort));
            }

            if (recipient.GetFirstSpouse() != null)
            {
                if (stringBuilder.Length != 0)
                {
                    stringBuilder.AppendLine();
                }

                stringBuilder.AppendLine("LetterAffair".Translate(recipient.LabelShort,
                    recipient.GetFirstSpouse().LabelShort, initiator.LabelShort));
            }
        }
        else
        {
            stringBuilder.AppendLine("LetterNewLovers".Translate(initiator.LabelShort, recipient.LabelShort));
        }

        foreach (var pawn in initiatorOldLoversAndFiances)
        {
            if (pawn.Dead)
            {
                continue;
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("LetterNoLongerLovers".Translate(initiator.LabelShort, pawn.LabelShort));
        }

        foreach (var pawn in recipientOldLoversAndFiances)
        {
            if (pawn.Dead)
            {
                continue;
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("LetterNoLongerLovers".Translate(recipient.LabelShort, pawn.LabelShort));
        }

        Find.LetterStack.ReceiveLetter(text, stringBuilder.ToString().TrimEndNewlines(), letterDef, t);
    }
}