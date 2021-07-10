using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
    // Token: 0x0200000D RID: 13
    public class InteractionWorker_RomanceAttemptMWNL : InteractionWorker
    {
        // Token: 0x04000009 RID: 9
        private const float MinAttractionForRomanceAttempt = 0.25f;

        // Token: 0x0400000A RID: 10
        private const int MinOpinionForRomanceAttempt = 5;

        // Token: 0x0400000B RID: 11
        private const float BaseSelectionWeight = 0f;

        // Token: 0x0400000C RID: 12
        private const float BaseSuccessChance = 0f;

        // Token: 0x0600001C RID: 28 RVA: 0x00002770 File Offset: 0x00000970
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            return 0f;
        }

        // Token: 0x0600001D RID: 29 RVA: 0x00002788 File Offset: 0x00000988
        public float SuccessChance(Pawn initiator, Pawn recipient)
        {
            return 0f;
        }

        // Token: 0x0600001E RID: 30 RVA: 0x000027A0 File Offset: 0x000009A0
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

        // Token: 0x0600001F RID: 31 RVA: 0x000029FC File Offset: 0x00000BFC
        private void BreakLoverAndFianceRelations(Pawn pawn, out List<Pawn> oldLoversAndFiances)
        {
            oldLoversAndFiances = new List<Pawn>();
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

        // Token: 0x06000020 RID: 32 RVA: 0x00002AB8 File Offset: 0x00000CB8
        private void TryAddCheaterThought(Pawn pawn, Pawn cheater)
        {
            if (!pawn.Dead)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CheatedOnMe, cheater);
            }
        }

        // Token: 0x06000021 RID: 33 RVA: 0x00002AF8 File Offset: 0x00000CF8
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
}