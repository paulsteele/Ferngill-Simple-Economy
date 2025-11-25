namespace fse.core.multiplayer;

public class DeltaAdjustedMessage(string objectId, int amount) : IMessage
{
	public const string StaticType = "fse.delta.adjusted.message";
	public string Type => StaticType;
	
	public string ObjectId { get; } = objectId;
	public int Amount { get; } = amount;
}

