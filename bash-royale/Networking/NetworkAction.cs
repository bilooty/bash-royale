namespace bash_royale.Networking;
using LiteNetLib.Utils;

public enum ActionType : byte
{
    NoAction = 0,
    DeployCard = 1,
    Emote = 2,
}
public class NetworkAction : INetSerializable
{
    public int Tick { get; set; }
    public byte PlayerId { get; set; }
    public byte EmoteId { get; set; }
    public ActionType Action { get; set; }

    public byte CardIdx { get; set; }
    public byte X { get; set; }
    public byte Y { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Tick);
        writer.Put(PlayerId);
        writer.Put((byte)Action);

        if (Action == ActionType.DeployCard)
        {
            writer.Put(CardIdx);
            writer.Put(X);
            writer.Put(Y);
        }
        else if (Action == ActionType.Emote)
        {
            writer.Put(EmoteId);
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        Tick = reader.GetInt();
        PlayerId = reader.GetByte();
        Action = (ActionType)reader.GetByte();
        if (Action == ActionType.DeployCard)
        {
            CardIdx = reader.GetByte();
            X = reader.GetByte();
            Y = reader.GetByte();
        }
        else if (Action == ActionType.Emote)
        {
            EmoteId = reader.GetByte();
        }
    }
}