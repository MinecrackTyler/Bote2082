using UnityEngine;

namespace Bote2082Compat;

public class ShipStabilizer : MonoBehaviour
{
	[SerializeField] private Unit unit;

	[Header("Axes")]
	[SerializeField] private bool pitch = true;
	[SerializeField] private bool roll = true;

	[Header("Stabilization Tuning")]
	[SerializeField] private float uprightTorque = 50f;
	[SerializeField] private float damping = 10f;

	private void FixedUpdate()
	{
		if (unit == null || unit.rb == null)
		{
			return;
		}

		Rigidbody rb = unit.rb;
		
		Quaternion rotation = Quaternion.FromToRotation(transform.up, Vector3.up);
		rotation.ToAngleAxis(out float angleDeg, out Vector3 worldAxis);

		if (angleDeg > 180f)
		{
			angleDeg -= 360f;
		}

		float angleRad = angleDeg * Mathf.Deg2Rad;
		Vector3 localAxis = transform.InverseTransformDirection(worldAxis);
		
		Vector3 localSpringTorque = new Vector3(
			pitch ? localAxis.x * angleRad * uprightTorque : 0f,
			0f,
			roll ? localAxis.z * angleRad * uprightTorque : 0f
		);
		
		Vector3 localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);
		Vector3 localDampingTorque = new Vector3(
			pitch ? -localAngularVelocity.x * damping : 0f,
			0f,
			roll ? -localAngularVelocity.z * damping : 0f
		);

		Vector3 worldTorque = transform.TransformDirection(localSpringTorque + localDampingTorque);
		rb.AddTorque(worldTorque, ForceMode.Force);
	}
}