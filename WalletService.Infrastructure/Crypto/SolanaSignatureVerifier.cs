using System.Text;
using Chaos.NaCl;
using SimpleBase;

namespace WalletService.Infrastructure.Crypto;

public static class SolanaSignatureVerifier
{
    /// <summary>
    /// Verifies an Ed25519 signature for a given message using a Solana public key.
    /// - publicKeyBase58: base58-encoded 32-byte Ed25519 public key (from Phantom)
    /// - signatureBase64: base64-encoded 64-byte signature (from Phantom)
    /// - message: the exact UTF-8 string that was signed on the client
    /// </summary>
    public static bool VerifyMessage(string publicKeyBase58, string signatureBase64, string message)
    {
        if (string.IsNullOrWhiteSpace(publicKeyBase58) ||
            string.IsNullOrWhiteSpace(signatureBase64) ||
            string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        byte[] publicKeyBytes;
        byte[] signatureBytes;

        try
        {
            // Solana public keys are base58-encoded Ed25519 keys
            publicKeyBytes = Base58.Bitcoin.Decode(publicKeyBase58).ToArray();

            if (publicKeyBytes.Length != Ed25519.PublicKeySizeInBytes)
            {
                return false;
            }

            // We expect the client to send the signature as base64
            signatureBytes = Convert.FromBase64String(signatureBase64);

            if (signatureBytes.Length != Ed25519.SignatureSizeInBytes)
            {
                return false;
            }
        }
        catch
        {
            // Any decode error = invalid
            return false;
        }

        var messageBytes = Encoding.UTF8.GetBytes(message);

        // Chaos.NaCl: Ed25519.Verify(signature, message, publicKey)
        return Ed25519.Verify(signatureBytes, messageBytes, publicKeyBytes);
    }
}

