using System;
using Verse;

namespace RimWorld
{
	// Token: 0x02000004 RID: 4
	public class InteractionWorker_SlightMWNL : InteractionWorker
	{
		// Token: 0x06000005 RID: 5 RVA: 0x00002088 File Offset: 0x00000288
		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			return 0f;
		}

		// Token: 0x04000002 RID: 2
		private const float BaseSelectionWeight = 0f;
	}
}
