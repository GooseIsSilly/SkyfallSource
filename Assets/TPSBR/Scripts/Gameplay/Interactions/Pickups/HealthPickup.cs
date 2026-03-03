using UnityEngine;

namespace TPSBR
{
	public class HealthPickup : StaticPickup
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private int _amount = 10;
		[SerializeField]
		private EHitAction _actionType;

		// StaticPickup INTERFACE

		protected override bool Consume(GameObject instigator, out string result)
		{
			if (instigator.TryGetComponent(out Health health) == false)
			{
				result = "Not applicable";
				return false;
			}

			var hitData = new HitData
			{
				Action        = _actionType,
				Amount        = _amount,
				InstigatorRef = Object.InputAuthority,
				Target        = health,
				HitType       = EHitType.Heal,
			};

			HitUtility.ProcessHit(ref hitData);

		if (hitData.Amount > 0f && _actionType == EHitAction.Heal)
		{
			var agent = instigator.GetComponent<Agent>();
			if (agent != null)
			{
				GameplayQuestHooks.NotifyHealingUsed(agent);
			}
		}


			result = hitData.Amount > 0f ? string.Empty : (_actionType == EHitAction.Shield ? "Shield full" : "Health full");

			return hitData.Amount > 0f;
		}
	}
}
