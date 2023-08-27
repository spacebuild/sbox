using System.Collections.Generic;

namespace Sdt.resources;

public readonly struct ResourceInfo
{
	public const string Vacuum = "vacuum";
	public const string Electricity = "electricity";
	public const string Oxygen = "oxygen";
	public const string Water = "water";
	public const string Hydrogen = "hydrogen";
	public const string Nitrogen = "nitrogen";
	public const string CarbonDioxide = "carbon dioxide";
	public const string Steam = "steam";
	public const string HeavyWater = "heavy water";
	public const string LiquidNitrogen = "liquid nitrogen";

	public static Dictionary<string, ResourceInfo> Registry { get; } = new()
	{
		{Vacuum, new ResourceInfo(Vacuum, "Vacuum", ResourceType.Unknown, new List<ResourceAttribute>())},
		{Electricity, new ResourceInfo(Electricity, "Electricity", ResourceType.Energy, new List<ResourceAttribute>())},
		{Oxygen, new ResourceInfo(Oxygen, "Oxygen", ResourceType.Gas, new List<ResourceAttribute>())},
		{Water, new ResourceInfo(Water, "Water", ResourceType.Liquid, new List<ResourceAttribute> {new(ResourceAttribute.Coolant)})},
		{Hydrogen, new ResourceInfo(Hydrogen, "Hydrogen", ResourceType.Gas, new List<ResourceAttribute> {new(ResourceAttribute.Flammable)})},
		{Nitrogen, new ResourceInfo(Nitrogen, "Nitrogen", ResourceType.Gas, new List<ResourceAttribute>())},
		{CarbonDioxide, new ResourceInfo(CarbonDioxide, "Carbon Dioxide", ResourceType.Gas, new List<ResourceAttribute>())},
		{Steam, new ResourceInfo(Steam, "Steam", ResourceType.Gas, new List<ResourceAttribute>())},
		{HeavyWater, new ResourceInfo(HeavyWater, "Heavy Water", ResourceType.Liquid, new List<ResourceAttribute>())},
		{LiquidNitrogen, new ResourceInfo(LiquidNitrogen, "Liquid Nitrogen", ResourceType.Liquid, new List<ResourceAttribute> {new(ResourceAttribute.Coolant, 2)})}
	};

	public static ResourceInfo Unknown { get; } =
		new ResourceInfo( "unknown", "Unknown", ResourceType.Unknown, new List<ResourceAttribute>() );

	public static ResourceInfo Add( string id, string name, ResourceType type = ResourceType.Unknown,
		List<ResourceAttribute> attributes = null )
	{
		if ( Registry.TryGetValue( id, out var value ) )
		{
			Log.Warning($"Resource has already been registered! {value}");
			return value;
		}
		var resourceInfo = new ResourceInfo(id, name, type, attributes ?? new List<ResourceAttribute>());
		Registry.Add(id, resourceInfo);
		return resourceInfo;
	}

	public string Id { get; }
	public string Name { get; }
	
	public ResourceType Type { get; }

	public List<ResourceAttribute> Attributes { get; }

	private ResourceInfo( string id, string name, ResourceType type, List<ResourceAttribute> attributes )
	{
		Id = id;
		Name = name;
		Type = type;
		Attributes = attributes;
	}

	public override string ToString()
	{
		return $"Resource {Id} ({Name}) with type={Type} and attributes={string.Join(",", Attributes)})";
	}

	public static ResourceInfo Get( string resourceName )
	{
		return Registry.TryGetValue( resourceName, out var value ) ? value : Unknown;
	}
}
