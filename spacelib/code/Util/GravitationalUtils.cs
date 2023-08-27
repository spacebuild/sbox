using System.Collections.Generic;
using Sandbox;
using Sdt.Environment;

namespace Sdt.Util;

public static class GravitationalUtils
{
	
	private const float Gravity = 800; // TODO get it out of settings! (is a vector!)
	
	public static IGravitationalEnvironment getActiveEnvironment( IList<IGravitationalEnvironment> environments, IEntity target )
	{
		//TODO what are the rules?
		return environments.Count > 0 ? environments[0] : null;
	}
	
	public static Vector3 CalculateSphericalGravitationalDirection( IEntity component, IGravitationalEnvironment targetEntity )
	{
		var direction = (component.Transform.Position - targetEntity.Position).Normal; // Or center of mass?
		return targetEntity.Inverse ? direction : -direction;
	}
	
	public static Vector3 CalculateDownwardsGravitationalDirection( IEntity component, IGravitationalEnvironment targetEntity )
	{
		var rotation = targetEntity.Rotation;
		return targetEntity.Inverse ? rotation.Up : rotation.Down;
	}
	
	public static Vector3 CalculateGravity( IEntity component, IGravitationalEnvironment targetEntity, Vector3 gravitationalDirection )
	{
		var speed = CalculateGravityPull( component, targetEntity );
		return gravitationalDirection * speed;
	}
	
	private static float CalculateGravityPull(IEntity component, IGravitationalEnvironment targetEntity)
	{
		return Gravity * targetEntity.GravityScale;
	}
	
}
