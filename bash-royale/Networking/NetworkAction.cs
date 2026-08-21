namespace bash_royale.Networking;
using LiteNetLib.Utils;

public enum ActionType : byte
{
    NoAction = 0,
    DeployCard = 1,
}
public class NetworkAction : INetSerializable
{
    public int Tick;
    public byte PlayerId;
    public ActionType Action;

    public byte CardType;
    public byte X;
    public byte Y;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Tick);
        writer.Put(PlayerId);
        writer.Put((byte)Action);

        if (Action == ActionType.DeployCard)
        {
            writer.Put(CardType);
            writer.Put(X);
            writer.Put(Y);
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        Tick = reader.GetInt();
        PlayerId = reader.GetByte();
        Action = (ActionType)reader.GetByte();
        if (Action == ActionType.DeployCard)
        {
            CardType = reader.GetByte();
            X = reader.GetByte();
            Y = reader.GetByte();
        }
    }
}