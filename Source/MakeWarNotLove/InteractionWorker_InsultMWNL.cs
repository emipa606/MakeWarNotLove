using System;
using Verse;

namespace RimWorld
{
	// Token: 0x02000005 RID: 5
	public class InteractionWorker_InsultMWNL : InteractionWorker
	{
		// Token: 0x06000007 RID: 7 RVA: 0x000020A0 File Offset: 0x000002A0
		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			return 0f;
		}

		// Token: 0x04000003 RID: 3
		private const float BaseSelectionWeight = 0f;
	}
}
