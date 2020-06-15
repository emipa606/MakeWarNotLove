using System;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	// Token: 0x0200000D RID: 13
	public class InteractionWorker_RomanceAttemptMWNL : InteractionWorker
	{
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
			bool flag = Rand.Value < this.SuccessChance(initiator, recipient);
			if (flag)
			{
				List<Pawn> list;
				this.BreakLoverAndFianceRelations(initiator, out list);
				List<Pawn> list2;
				this.BreakLoverAndFianceRelations(recipient, out list2);
				for (int i = 0; i < list.Count; i++)
				{
					this.TryAddCheaterThought(list[i], initiator);
				}
				for (int j = 0; j < list2.Count; j++)
				{
					this.TryAddCheaterThought(list2[j], recipient);
				}
				initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.ExLover, recipient);
				initiator.relations.AddDirectRelation(PawnRelationDefOf.Lover, recipient);
				TaleRecorder.RecordTale(TaleDefOf.BecameLover, new object[]
				{
					initiator,
					recipient
				});
				initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, recipient);
				recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, initiator);
				initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMe, recipient);
				initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, recipient);
				recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMe, initiator);
				recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, initiator);
				bool flag2 = initiator.IsColonist || recipient.IsColonist;
				if (flag2)
				{
					this.SendNewLoversLetter(initiator, recipient, list, list2);
				}
				extraSentencePacks.Add(RulePackDefOf.Sentence_RomanceAttemptAccepted);
				LovePartnerRelationUtility.TryToShareBed(initiator, recipient);
			}
			else
			{
				initiator.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RebuffedMyRomanceAttempt, recipient);
				recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FailedRomanceAttemptOnMe, initiator);
				bool flag3 = recipient.relations.OpinionOf(initiator) <= 0;
				if (flag3)
				{
					recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, initiator);
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
				Pawn firstDirectRelationPawn = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, null);
				bool flag = firstDirectRelationPawn != null;
				if (flag)
				{
					pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, firstDirectRelationPawn);
					pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, firstDirectRelationPawn);
					oldLoversAndFiances.Add(firstDirectRelationPawn);
				}
				else
				{
					Pawn firstDirectRelationPawn2 = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, null);
					bool flag2 = firstDirectRelationPawn2 == null;
					if (flag2)
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
			bool flag = !pawn.Dead;
			if (flag)
			{
				pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CheatedOnMe, cheater);
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002AF8 File Offset: 0x00000CF8
		private void SendNewLoversLetter(Pawn initiator, Pawn recipient, List<Pawn> initiatorOldLoversAndFiances, List<Pawn> recipientOldLoversAndFiances)
		{
			bool flag = false;
			bool flag2 = (initiator.GetSpouse() != null && !initiator.GetSpouse().Dead) || (recipient.GetSpouse() != null && !recipient.GetSpouse().Dead);
			string text;
			LetterDef letterDef;
			Pawn t;
			if (flag2)
			{
				text = Translator.Translate("LetterLabelAffair");
				letterDef = LetterDefOf.NegativeEvent;
				bool flag3 = initiator.GetSpouse() != null && !initiator.GetSpouse().Dead;
				if (flag3)
				{
					t = initiator;
				}
				else
				{
					t = recipient;
				}
				flag = true;
			}
			else
			{
				text = Translator.Translate("LetterLabelNewLovers");
				letterDef = LetterDefOf.PositiveEvent;
				t = initiator;
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag4 = flag;
			if (flag4)
			{
				bool flag5 = initiator.GetSpouse() != null;
				if (flag5)
				{
					stringBuilder.AppendLine("LetterAffair".Translate(new object[]
					{
						initiator.LabelShort,
						initiator.GetSpouse().LabelShort,
						recipient.LabelShort
					}));
				}
				bool flag6 = recipient.GetSpouse() != null;
				if (flag6)
				{
					bool flag7 = stringBuilder.Length != 0;
					if (flag7)
					{
						stringBuilder.AppendLine();
					}
					stringBuilder.AppendLine("LetterAffair".Translate(new object[]
					{
						recipient.LabelShort,
						recipient.GetSpouse().LabelShort,
						initiator.LabelShort
					}));
				}
			}
			else
			{
				stringBuilder.AppendLine("LetterNewLovers".Translate(new object[]
				{
					initiator.LabelShort,
					recipient.LabelShort
				}));
			}
			for (int i = 0; i < initiatorOldLoversAndFiances.Count; i++)
			{
				bool flag8 = !initiatorOldLoversAndFiances[i].Dead;
				if (flag8)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("LetterNoLongerLovers".Translate(new object[]
					{
						initiator.LabelShort,
						initiatorOldLoversAndFiances[i].LabelShort
					}));
				}
			}
			for (int j = 0; j < recipientOldLoversAndFiances.Count; j++)
			{
				bool flag9 = !recipientOldLoversAndFiances[j].Dead;
				if (flag9)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("LetterNoLongerLovers".Translate(new object[]
					{
						recipient.LabelShort,
						recipientOldLoversAndFiances[j].LabelShort
					}));
				}
			}
			Find.LetterStack.ReceiveLetter(text, stringBuilder.ToString().TrimEndNewlines(), letterDef, t, null);
		}

		// Token: 0x04000009 RID: 9
		private const float MinAttractionForRomanceAttempt = 0.25f;

		// Token: 0x0400000A RID: 10
		private const int MinOpinionForRomanceAttempt = 5;

		// Token: 0x0400000B RID: 11
		private const float BaseSelectionWeight = 0f;

		// Token: 0x0400000C RID: 12
		private const float BaseSuccessChance = 0f;
	}
}
