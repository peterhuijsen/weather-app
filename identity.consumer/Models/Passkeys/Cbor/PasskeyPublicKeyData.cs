using Dahomey.Cbor.Attributes;
using Dahomey.Cbor.ObjectModel;

namespace Identity.Consumer.Models.Passkeys.Cbor;

public class PasskeyPublicKeyData
{
    [CborProperty("1")]
    public int Type { get; set; }
    [CborProperty("3")]
    public int Algorithm { get; set; }
    
    [CborProperty("-1")]
    public int Curve { get; set; }
    [CborProperty("-2")]
    
    public byte[] X { get; set; }
    [CborProperty("-3")]
    public byte[] Y { get; set; }

    public PasskeyPublicKeyData(CborObject target)
    {
        if (target.TryGetValue(1, out var type)) Type = type.Value<int>();
        if (target.TryGetValue(3, out var algorithm)) Algorithm = algorithm.Value<int>();
        if (target.TryGetValue(-1, out var curve)) Curve = curve.Value<int>();
        if (target.TryGetValue(-2, out var x)) X = x.Value<ReadOnlyMemory<byte>>().ToArray();
        if (target.TryGetValue(-3, out var y)) Y = y.Value<ReadOnlyMemory<byte>>().ToArray();
    }
}