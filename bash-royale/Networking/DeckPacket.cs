namespace bash_royale.Networking;
using LiteNetLib.Utils;

/// <summary>
/// Sent once, right after connecting: tells the other side which eight cards we
/// picked in the deck builder. Both machines simulate both players, so each needs
/// the other's deck before the first tick can run.
/// </summary>
public class DeckPacket : INetSerializable
{
    // One byte per CardId, in deck order.
    public byte[] Cards { get; set; } = Array.Empty<byte>();

    public void Serialize(NetDataWriter writer)
    {
        writer.Put((byte)Cards.Length);
        foreach (byte card in Cards)
            writer.Put(card);
    }

    public void Deserialize(NetDataReader reader)
    {
        int count = reader.GetByte();
        Cards = new byte[count];
        for (int i = 0; i < count; i++)
            Cards[i] = reader.GetByte();
    }
}
