using Dahomey.Cbor.Attributes;

namespace Identity.Consumer.Models.Passkeys.Attestation;

public class PackedAttestationStatement
{
    [CborProperty("alg")]
    public int Algorithm { get; set; }

    [CborProperty("sig")]
    public byte[] Signature { get; set; }
}