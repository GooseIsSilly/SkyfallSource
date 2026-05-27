namespace TPSBR
{
	using System;
	using UnityEngine;

	[Serializable]
	public sealed class ProjectileDamage
	{
		public float Damage             = 10f;
		public float MaxDistance        = 300f;
		public float FullDamageDistance = 80f;

		public float GetDamage(float distance)
		{
			if (distance < FullDamageDistance)
				return Damage;

			if (FullDamageDistance >= MaxDistance)
				return Damage;

			return Mathf.Lerp(Damage, 0f, (distance - FullDamageDistance) / (MaxDistance - FullDamageDistance));
		}
	}
}
