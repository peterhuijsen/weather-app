using Org.BouncyCastle.Asn1;

namespace Identity.Consumer.Helpers;

public static class ASN1
{
    public static byte[] ConvertToRS(byte[] asn1)
    {
        var seq = Asn1Sequence.GetInstance(asn1);
        var rs = new[]
        {
            DerInteger.GetInstance(seq[0]).PositiveValue,
            DerInteger.GetInstance(seq[1]).PositiveValue,
        };
                    
        // Get the r and s bytes of the asn.1 encoded signature.
        var (rBytes, sBytes) = (rs[0].ToByteArrayUnsigned(), rs[1].ToByteArrayUnsigned());
        
        // Pad both the r and s bytes.
        var (rBytes32, sBytes32) = (new byte[32 - rBytes.Length].Concat(rBytes).ToArray(), new byte[32 - sBytes.Length].Concat(sBytes).ToArray());
                   
        // Concat them to get the r|s bytes.
        var rsBytes = rBytes32.Concat(sBytes32).ToArray();
        return rsBytes;
    }
}