using System.Collections.Generic;
using Sandbox;
using Sandbox.Internal;
using Sdt;
using Sdt.resources;

namespace Sdt.Environment;

public interface IGravitationalEnvironment: IEntity, INetworkTable
{
	
	string Name { get; }
	
	float OuterRadius { get; }
	
	float InnerRadius { get; }
	
	GravityType Type { get; }
	
	float GravityScale { get; }
	
	bool Inverse { get; }
	
	float Volume { get; }

	EnvironmentPreset Preset { get; }
	
	float Atmosphere { get; }
	
	int Temperature { get; }
	
	int NightTemperature { get; }

	IList<NetworkedResource> Resources { get; }

}
