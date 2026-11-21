using System;
using Blueprinter.shippack1;
using BoscaliOceanTrainingExercise;
using UnityEngine;

namespace Bote2082Compat;

public class AircraftGroundEffectPart : AircraftShipPart
{
	public float buoyancyHeight;
	public float liftShare;
	public AnimationCurve liftBySpeed = new AnimationCurve(new Keyframe(0, 0), new Keyframe(30, 0.1f), new Keyframe(90, 0.8f), new Keyframe(130, 1));
	public AnimationCurve liftByHeight = new AnimationCurve(new Keyframe(0, 1), new Keyframe(4, 0.75f), new Keyframe(8, 0.35f), new Keyframe(14, 0));
	public Transform liftPoint;
	public float airDrag;
	public float sideDrag;
	public Transform[] extraFloats;
	public float extraFloatDisplacement;
	public float extraFloatHeight = 2.5f;
	public float extraFloatMass;
	public Vector3 extraFloatDrag = new Vector3(1f, 1f, 0.005f);
	
	[NonSerialized] public float lastHeight, lastLift;

	public override void Awake()
	{
		base.Awake();
		if (buoyancyHeight > 0f)
		{
			base.height = buoyancyHeight;
		}
		
		enabled = liftShare > 0f || airDrag > 0f || sideDrag > 0f || (extraFloats != null && extraFloats.Length > 0f);
	}

	private void FixedUpdate()
	{
		if (rb == null || rb.isKinematic || IsDetached() || parentUnit == null || !parentUnit.LocalSim) return;
		Transform root = parentUnit.transform;
		Vector3 p = liftPoint != null ? liftPoint.position : transform.position;
		Vector3 v = rb.GetPointVelocity(p);
		Vector3 force = Vector3.zero;

		float h = p.y - Datum.LocalSeaY;
		lastHeight = h; lastLift = 0f;
		float fwd = Vector3.Dot(rb.velocity, root.forward);
		if (liftShare > 0f && fwd > 0f && h > 0f)
		{
			float lift = liftShare * rb.mass * Physics.gravity.magnitude * Mathf.Max(liftBySpeed.Evaluate(fwd), 0f) * Mathf.Max(liftByHeight.Evaluate(h), 0f);
			force += transform.up * lift; lastLift = lift;
		}
		if (airDrag > 0f) force -= v * v.magnitude * airDrag;
		if (sideDrag > 0f)
		{
			float vs = Vector3.Dot(v, root.right);
			force -= root.right * (vs * Mathf.Abs(vs) * sideDrag);
		}
		if (force.sqrMagnitude > 0f && !float.IsNaN(force.x)) rb.AddForceAtPosition(force, p);
		if (extraFloats == null) return;
		foreach (Transform t in extraFloats)
		{
			if (t == null) continue;
			Vector3 fp = t.position;
			float s = Mathf.Clamp01((extraFloatHeight * 0.5f - (fp.y - Datum.LocalSeaY)) / extraFloatHeight);
			if (s <= 0f) continue;
			Vector3 fv = rb.GetPointVelocity(fp);
			float u = Vector3.Dot(fv, t.forward), r = Vector3.Dot(fv, t.right), w = Vector3.Dot(fv, t.up);
			Vector3 drag = -(t.forward * u * Mathf.Abs(u) * extraFloatDrag.z + t.right * r * Mathf.Abs(r) * extraFloatDrag.x + t.up * w * Mathf.Abs(w) * extraFloatDrag.y) * extraFloatMass * s;
			drag = Vector3.ClampMagnitude(drag, extraFloatMass * 500f);
			Vector3 f = Vector3.up * (Mathf.Lerp(1.2f, 1000f, s) * 9.81f * extraFloatDisplacement) + drag;
			if (!float.IsNaN(f.x)) rb.AddForceAtPosition(f, fp);
		}
	}
}