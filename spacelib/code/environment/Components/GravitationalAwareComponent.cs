using System;
using System.Collections.Generic;
using Sandbox;
using Sdt.Environment;
using Sdt.Util;

namespace Sdt.environment.Components
{
	public partial class GravitationalAwareComponent: EntityComponent
	{

		[Net]
		public IList<IGravitationalEnvironment> Environments { get; set; } = new List<IGravitationalEnvironment>();

		public GravitationalAwareComponent() { }

		public void DisableGravity()
		{
			DisableEngineGravity();
			if ( Entity is GravitationalAwarePlayer { CurrentEnvironment: { } } player )
			{
				player.CurrentEnvironment = null;
				player.Gravity = Vector3.Zero;
				player.GravityDirection = Vector3.Zero;
			}
		}

		private void DisableEngineGravity()
		{
			var modelEntity = getValidEntity();
			if ( modelEntity != null )
			{
				var body = modelEntity.PhysicsBody;
				if ( body.GravityEnabled )
				{
					modelEntity.PhysicsBody.GravityScale = 0;
					modelEntity.PhysicsBody.GravityEnabled = false;
				}
			}
		}

		private ModelEntity getValidEntity()
		{
			if ( Entity is ModelEntity { PhysicsBody.BodyType: PhysicsBodyType.Dynamic } modelEntity )
			{
				return modelEntity;
			}

			return null;
		}

		public void Add( IGravitationalEnvironment environment )
		{
			Environments.Add( environment );
		}
		public void Remove( IGravitationalEnvironment environment )
		{
			Environments.Remove( environment );
		}
		
		[Event.Physics.PostStep]
		void Tick()
		{
			var active = GravitationalUtils.getActiveEnvironment( Environments, Entity );
			var modelEntity = getValidEntity();
			if ( active != null )
			{
				var applyGravity = modelEntity != null;
				var isPlayer = Entity is GravitationalAwarePlayer;
				if ( applyGravity || isPlayer )
				{
					switch ( active.Type )
					{
						case GravityType.CustomSpherical:
							var sphericalDirection =
								GravitationalUtils.CalculateSphericalGravitationalDirection( Entity, active );
							ProcessCustomGravity( active, sphericalDirection );
							break;
						case GravityType.CustomDownwards:
							var downWardsDirection =
								GravitationalUtils.CalculateDownwardsGravitationalDirection( Entity, active );
							ProcessCustomGravity( active, downWardsDirection );
							break;
						case GravityType.EngineDefault:
						default:
							EnableGravity(active,  active.GravityScale );
							break;
					}
				}
			} else {
				DisableGravity();
			}
		}
		
		public void EnableGravity(IGravitationalEnvironment targetEntity, float gravityScale )
		{
			if ( Entity is GravitationalAwarePlayer player )
			{
				player.CurrentEnvironment = targetEntity;
				player.Gravity = Game.PhysicsWorld.Gravity * gravityScale;
				player.GravityDirection = Game.PhysicsWorld.Gravity.Normal;
				return;
			}
			var modelEntity = getValidEntity();
			if ( modelEntity != null )
			{
				var body = modelEntity.PhysicsBody;
				if ( !body.GravityEnabled || Math.Abs(body.GravityScale - gravityScale) > 0.0001f )
				{
					body.GravityEnabled = true;
					body.GravityScale = gravityScale;
				}
			}
		}
		
		private void ProcessCustomGravity( IGravitationalEnvironment targetEntity, Vector3 gravitationalDirection )
		{
			DisableEngineGravity();
			var gravityVelocity = GravitationalUtils.CalculateGravity( Entity, targetEntity, gravitationalDirection );
			if ( Entity is GravitationalAwarePlayer player )
			{
				player.CurrentEnvironment = targetEntity;
				player.Gravity = gravityVelocity;
				player.GravityDirection = gravitationalDirection;
				return;
			}
			// TODO stop gravity if "hit" floor
			// Let's assume it will always be "up"
			var trace = Trace.Ray( Entity.Position, Entity.Position + (gravitationalDirection * 2) )
				.WithAnyTags( "solid" )
				.Ignore( Entity )
				.Run();
			if ( !trace.Hit )
			{
				Entity.PhysicsGroup.AddVelocity( gravityVelocity * Time.Delta );
			}
			else if (!Entity.Velocity.IsNearZeroLength)
			{
				Entity.PhysicsGroup.Sleeping = true;
				Entity.PhysicsGroup.Sleeping = false;
			}
		}

	}
}
