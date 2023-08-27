using System;
using System.Collections.Generic;
using Editor;
using Sandbox;
using Sdt.Environment;
using Sdt.resources;
using GravitationalAwareComponent = Sdt.environment.Components.GravitationalAwareComponent;

namespace Sdt.environment.entities;

[Library( "sb_base_environment"  ), HammerEntity]
[Title("Basic spacebuild environment"), Category( "Spacebuild" ), Icon( "place" )]
[Tag("sb_environment")]
public partial class GravitationalEnvironment: BaseTrigger, IGravitationalEnvironment
{
	[Property( Title = "Outer Radius" )]
	public float OuterRadius { get; set; } = 1000f;
	
	[Property( Title = "Inner Radius" )]
	public float InnerRadius { get; set; } = 0f;
	
	[Property( Title = "Gravity type (experimental)" )]
	public GravityType Type { get; set; } = GravityType.EngineDefault;
	
	[Property( Title = "Gravity scale" )]
	public float GravityScale { get; set; } = 1f;
	
	[Property( Title = "Inverse gravity (experimental)" )]
	public bool Inverse { get; set; } = false;
	
	[Net, Predicted]
	public float Volume { get; set; }

	[Property( Title = "Environment preset" )]
	public EnvironmentPreset Preset { get; set; } = EnvironmentPreset.Earth;

	[Property( Title = "Environment atmosphere" )]
	[Net]
	public float Atmosphere { get; set; } = 1;

	[Property( Title = "Environment temperature (day)" )]
	[Net]
	public int Temperature { get; set; } = 275;

	[Property( Title = "Environment temperature (night)" )]
	[Net]
	public int NightTemperature { get; set; } = 275;
	
	[Net]
	public IList<NetworkedResource> Resources { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		EnableTouchPersists = true;
		// Triggers are not send by default, we need to tell it to send to the clients
		Transmit = TransmitType.Always;

		if ( Type == GravityType.CustomSpherical )
		{
			Volume = CalculateVolume( InnerRadius, OuterRadius );
		}
		else // Only 1 side
		{
			Volume = CalculateVolume( InnerRadius, OuterRadius, 2 );
		}

		switch ( Preset )
		{
			case EnvironmentPreset.Earth:
				Atmosphere = 1;
				Temperature = 295; // 20° C
				NightTemperature = 285; // 10° C
				Resources = CreateBasicResoures( 21, 0.5f, 78, 0.5f, Volume );
				break;
			default:
				Resources = CreateBasicResoures( 0, 0, 0, 0, Volume );
				break;
		}
		
	}

	private static IList<NetworkedResource> CreateBasicResoures( float o2Percentage, float co2Percentage,
		float nitrogenPercentage, float hydrogenPercentage, float volume )
	{

		var total = o2Percentage + co2Percentage + nitrogenPercentage + hydrogenPercentage;
		// TODO check if above 100%
		var oxygen = (o2Percentage / 100f) * volume;
		var carbonDioxide = (co2Percentage / 100f) * volume;
		var nitrogen =  (nitrogenPercentage / 100f) * volume;
		var hydrogen =  (hydrogenPercentage / 100f) * volume;
		var vacuum =  ((100f - total) / 100f) * volume;
		return new List<NetworkedResource>
		{
			new() {Amount = oxygen, MaxAmount = volume, ResourceName = ResourceInfo.Oxygen},
			new() {Amount = carbonDioxide, MaxAmount = volume, ResourceName = ResourceInfo.CarbonDioxide},
			new() {Amount = nitrogen, MaxAmount = volume, ResourceName = ResourceInfo.Nitrogen},
			new() {Amount = hydrogen, MaxAmount = volume, ResourceName = ResourceInfo.Hydrogen},
			new() {Amount = vacuum, MaxAmount = volume, ResourceName = ResourceInfo.Vacuum},
		};
	}

	private static float CalculateVolume( float innerRadius, float outerRadius, float divider = 1f )
	{
		var outer = (4f / 3f) * Math.PI * Math.Pow( outerRadius,  3);
		var inner = (4f / 3f) * Math.PI * Math.Pow( innerRadius,  3);
		var volume = outer - inner;
		return (float)volume / divider;
	}

	public override void StartTouch( Entity other )
	{
		base.StartTouch(other);
		var gravityComponent = other.Components.Get<GravitationalAwareComponent>();
		gravityComponent?.Add( this );
	}

	public override void EndTouch( Entity other )
	{
		base.EndTouch( other );
		var gravityComponent = other.Components.Get<GravitationalAwareComponent>();
		gravityComponent?.Remove( this );
	}
}

