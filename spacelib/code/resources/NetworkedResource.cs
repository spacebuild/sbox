using Sandbox;

namespace Sdt.resources;

public partial class NetworkedResource: BaseNetworkable 
{
	[Net] public string ResourceName { get; set; }
	[Net] public float Amount { get; set; }
	[Net] public float MaxAmount { get; set; }
	
	public ResourceInfo ResourceInfo { get => ResourceInfo.Get( ResourceName ); }
}
