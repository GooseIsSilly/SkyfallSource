namespace TPSBR
{
	using UnityEngine;
	using Fusion;
	using Fusion.Addons.KCC;

	public class KillArea : KCCProcessor
	{
		// KCCProcessor INTERFACE

		public override void OnEnter(KCC kcc, KCCData data)
		{
			if (kcc.IsInFixedUpdate == true)
			{
				IHitTarget hitTarget = kcc.GetComponent<IHitTarget>();
				if (hitTarget != null)
				{
					HitData hitData = new HitData();
					hitData.Action           = EHitAction.Damage;
					hitData.Amount           = 99999.0f;
					hitData.IsFatal          = true;
					hitData.Position         = kcc.transform.position;
					hitData.Normal           = Vector3.up;
					hitData.InstigatorRef    = GetComponent<NetworkObject>().InputAuthority;
					hitData.Target           = hitTarget;
					hitData.HitType          = EHitType.Suicide;

					HitUtility.ProcessHit(ref hitData);
				}
			}
		}
	}
}
