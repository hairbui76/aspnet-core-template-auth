using _Paseto = Paseto;
using Paseto.Builder;
using System.Text;

namespace CSBackend.Configs;

public class Paseto
{
	private static PasetoBuilder Builder() => new PasetoBuilder().UseV4(_Paseto.Purpose.Local);
	public static string Encode(object payload, string secret)
	{
		if (Config.TOKEN.SECRET == null)
			throw new NullReferenceException("Token secret is required");
		var encryptKey = Encoding.ASCII.GetBytes(secret);
		return Builder().WithKey(encryptKey, _Paseto.Encryption.SymmetricKey).AddClaim("value", payload).Encode();
	}

	public static _Paseto.PasetoTokenValidationResult Decode(string token, string secret)
	{
		if (Config.TOKEN.SECRET == null)
			throw new NullReferenceException("Token secret is required");
		var decryptKey = Encoding.ASCII.GetBytes(secret);
		return Builder().WithKey(decryptKey, _Paseto.Encryption.SymmetricKey).Decode(token);
	}
}