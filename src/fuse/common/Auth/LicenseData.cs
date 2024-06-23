using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Outracks.Fuse.Auth.Utilities;

namespace Outracks.Fuse.Auth
{
	public class LicenseData
	{
		public string License;
		public string Name;
		public string Email;
		public string Company;
		public string Device;
		public string Authority;
		public DateTime UtcIssued;
		public DateTime UtcExpires;

		public static LicenseData Null => new LicenseData();

		public override string ToString()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		public static LicenseData Decode(string data)
		{
			return string.IsNullOrEmpty(data) ? Null : JsonConvert.DeserializeObject<LicenseData>(VerifyString(data, PUBLIC_KEY));
		}

		static string VerifyString(string data, string key)
		{
			var stack = Convert.FromBase64String(data).DecompressGZip();
			var sign = new byte[128];
			var hash = GetSha256(stack, sign.Length, stack.Length - sign.Length);
			Buffer.BlockCopy(stack, 0, sign, 0, sign.Length);

			if (!ParsePem(key).VerifyHash(hash, sign, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
				throw new InvalidOperationException("Invalid digital signature.");

			return Encoding.UTF8.GetString(stack, sign.Length, stack.Length - sign.Length);
		}

		static byte[] GetSha256(byte[] bytes, int offset, int count)
		{
			using (var sha256 = SHA256.Create())
				return sha256.ComputeHash(bytes, offset, count);
		}

		static RSACryptoServiceProvider ParsePem(string input)
		{
			var rsa = new RSACryptoServiceProvider();
			var pem = new PemReader(new StringReader(input));
			var obj = pem.ReadObject();

			if (obj is RsaKeyParameters pk)
				rsa.ImportParameters(DotNetUtilities.ToRSAParameters(pk));
			else
				throw new InvalidOperationException();

			return rsa;
		}

		const string PUBLIC_KEY = @"-----BEGIN PUBLIC KEY-----
MIGeMA0GCSqGSIb3DQEBAQUAA4GMADCBiAKBgH8u56PcaQji0QtMQuBzpC4rIrsS
hQqMh64/SdmVj+rLwNgYqet0PjYMDKdyGmPxG5+Y26aJ68i2HYl6Q88ExGd8C9qF
8ybzhmmxQ2R7vm0ltPd10GdMWoi3w3oeDqiRxHf9TW4xJZLILRcngPD7wkWAL6/c
fBQu3nHhh4d7W0E1AgMBAAE=
-----END PUBLIC KEY-----";
	}
}
