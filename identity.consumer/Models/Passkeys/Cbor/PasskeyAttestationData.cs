using Dahomey.Cbor.Attributes;
using Dahomey.Cbor.ObjectModel;

namespace Identity.Consumer.Models.Passkeys.Cbor;

public class PasskeyAttestationData
{
    [CborProperty("fmt")]
    public string Format { get; set; }

    [CborProperty("attStmt")]
    public CborObject Statement { get; set; }

    [CborProperty("authData")]
    public byte[] Data { get; set; }
}