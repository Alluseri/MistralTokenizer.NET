using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Alluseri.MistralTokenizer;

internal static class MistralTokenizer {
	public static Dictionary<string, int> Merges;
	public static string[] EncodeVocabById;
	public static byte[][] DecodeVocabById;
	public static Dictionary<string, ushort> VocabByString;
	public static Dictionary<byte, ushort> VocabByByte;
	public static Dictionary<Rune, ushort> VocabByRune;

	public static string GetMergeIdentifierString(ushort ID1, ushort ID2) => $"{EncodeVocabById[ID1]} {EncodeVocabById[ID2]}";

	private static Stream GetResourceStream(string Res) {
		Assembly Ex = Assembly.GetExecutingAssembly();
		return Ex.GetManifestResourceStream(Ex.GetManifestResourceNames().Single(str => str.EndsWith(Res))) ?? throw new FileNotFoundException($"No such resource: {Res}");
	}

	private static string GetTokens() {
		using (StreamReader Reader = new(GetResourceStream("tokens.b64"))) {
			return Encoding.UTF8.GetString(Convert.FromBase64String(Reader.ReadToEnd()));
		}
	}

	private static byte[] GetMerges() {
		using (CryptoStream Cs = new(GetResourceStream("merges.b64"), new FromBase64Transform(FromBase64TransformMode.DoNotIgnoreWhiteSpaces), CryptoStreamMode.Read)) {
			using MemoryStream Ms = new();
			Cs.CopyTo(Ms);
			return Ms.ToArray();
		}
	}

	static MistralTokenizer() {
#if MT_NET_PROFILE_INIT
		Stopwatch Sw = Stopwatch.StartNew();
#endif
		EncodeVocabById = GetTokens().Split("\n", StringSplitOptions.None);
		VocabByString = EncodeVocabById.Select((S, I) => new KeyValuePair<string, ushort>(S, (ushort) I)).ToDictionary();
		VocabByRune = VocabByString.Where(K => Encoding.UTF32.GetByteCount(K.Key) == 4).Select(K => new KeyValuePair<Rune, ushort>(Rune.GetRuneAt(K.Key, 0), K.Value)).ToDictionary();
		VocabByByte = Enumerable.Range(0, 256).Select(X => new KeyValuePair<byte, ushort>((byte) X, VocabByString[$"<0x{X:X2}>"])).ToDictionary();
		// GOD I HOPE GC COLLECTS EVERY ARRAY MADE HERE BY GETBYTES BECAUSE I CANT DELINQ THIS STATEMENT
		DecodeVocabById = EncodeVocabById.Select(S => S.StartsWith("<0x") && S.EndsWith('>') ? new byte[1] { byte.Parse($"{S[3..^1]}", NumberStyles.HexNumber) } : Encoding.UTF8.GetBytes(S.Replace(MistralTokenizer.EncodeVocabById[28705][0], ' '))).ToArray();

		Merges = new();

		byte[] MergeBytes = GetMerges();
		ushort[] TokenIds = new ushort[(MergeBytes.Length + 1) / 2];

		int j = 0;
		for (int i = 0; i < MergeBytes.Length; i += 2) {
			TokenIds[j++] = (ushort) (MergeBytes[i] + (MergeBytes[i + 1] << 8));
		}

		for (int i = 0; i < TokenIds.Length; i += 2) {
			ushort Id1 = TokenIds[i];
			ushort Id2 = TokenIds[i + 1];

			string mergeIdentifierString = GetMergeIdentifierString(Id1, Id2);
			Merges[mergeIdentifierString] = i + 1;
		}
#if MT_NET_PROFILE_INIT
		Console.WriteLine($"[MT.NET PROFILE_INIT] Spent {Sw.ElapsedMilliseconds} ms initializing MistralTokenizer.");
#endif
	}
}