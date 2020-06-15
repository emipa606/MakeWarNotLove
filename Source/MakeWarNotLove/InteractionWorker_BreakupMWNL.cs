using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	// Token: 0x0200000B RID: 11
	public class InteractionWorker_BreakupMWNL : InteractionWorker
	{
		// Token: 0x06000013 RID: 19 RVA: 0x00002138 File Offset: 0x00000338
		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			return 0f;
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002150 File Offset: 0x00000350
		public Thought RandomBreakupReason(Pawn initiator, Pawn recipient)
		{
			List<Thought_Memory> list = (from m in initiator.needs.mood.thoughts.memories.Memories
			where m != null && m.otherPawn == recipient && m.CurStage != null && m.CurStage.baseOpinionOffset < 0f
			select m).ToList<Thought_Memory>();
			bool flag = list.Count == 0;
			Thought result;
			if (flag)
			{
				result = null;
			}
			else
			{
				float worstMemoryOpinionOffset = list.Max((Thought_Memory m) => -m.CurStage.baseOpinionOffset);
				Thought_Memory thought_Memory = null;
				(from m in list
				where -m.CurStage.baseOpinionOffset >= worstMemoryOpinionOffset / 2f
				select m).TryRandomElementByWeight((Thought_Memory m) => -m.CurStage.baseOpinionOffset, out thought_Memory);
				result = thought_Memory;
			}
			return result;
		}

		// Token: 0x06000015 RID: 21 RVA: 0x0000222C File Offset: 0x0000042C
		public virtual void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
		{
			Thought thought = this.RandomBreakupReason(initiator, recipient);
			bool flag = initiator.relations.DirectRelationExists(PawnRelationDefOf.Spouse, recipient);
			if (flag)
			{
				initiator.relations.RemoveDirectRelation(PawnRelationDefOf.Spouse, recipient);
				initiator.relations.AddDirectRelation(PawnRelationDefOf.ExSpouse, recipient);
				recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DivorcedMe, initiator);
				initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
				recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
				initiator.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HoneymoonPhase, recipient);
				recipient.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.HoneymoonPhase, initiator);
			}
			else
			{
				initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, recipient);
				initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.Fiance, recipient);
				initiator.relations.AddDirectRelation(PawnRelationDefOf.ExLover, recipient);
				recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.BrokeUpWithMe, initiator);
			}
			bool flag2 = initiator.ownership.OwnedBed != null && initiator.ownership.OwnedBed == recipient.ownership.OwnedBed;
			if (flag2)
			{
				Pawn pawn = (Rand.Value >= 0.5f) ? recipient : initiator;
				pawn.ownership.UnclaimBed();
			}
			TaleRecorder.RecordTale(TaleDefOf.Breakup, new object[]
			{
				initiator,
				recipient
			});
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("LetterNoLongerLovers".Translate(new object[]
			{
				initiator.LabelShort,
				recipient.LabelShort
			}));
			bool flag3 = thought != null;
			if (flag3)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("FinalStraw".Translate(new object[]
				{
					thought.CurStage.label
				}));
			}
			bool flag4 = PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient);
			if (flag4)
			{
				Find.LetterStack.ReceiveLetter(Translator.Translate("LetterLabelBreakup"), stringBuilder.ToString(), LetterDefOf.NegativeEvent, initiator, null);
			}
		}

		// Token: 0x04000004 RID: 4
		private const float BaseChance = 0f;

		// Token: 0x04000005 RID: 5
		private const float SpouseRelationChanceFactor = 0f;
	}
}
