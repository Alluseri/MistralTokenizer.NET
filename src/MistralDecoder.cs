using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Alluseri.MistralTokenizer;

/// <summary>
/// A static class that provides the Mistral decoder.
/// </summary>
public static class MistralDecoder {
	/// <summary>
	/// Decodes a sequence of Mistral token IDs into a text string.
	/// </summary>
	/// <param name="Tokens">The sequence of Mistral tokens to be decoded.</param>
	/// <param name="RemoveBosToken">Specifies whether to treat the first token as a beginning-of-sequence (BOS) token and ignore it during decoding. Default is true.</param>
	/// <param name="RemovePrecedingSpace">Specifies whether to remove the single preceding space character from the decoded text. Default is true.</param>
	/// <returns>The decoded text string. Returns an empty string if the input token list is empty or only contains a BOS token.</returns>
	public static string Decode(IList<ushort> Tokens, bool RemoveBosToken = true, bool RemovePrecedingSpace = true) {
		// GetBuffer() approach is only faster for 16K+ contexts, otherwise it is slower! Implementation rejected.
		using MemoryStream Ms = new();

		for (int i = RemoveBosToken ? 1 : 0; i < Tokens.Count; i++) {
			Ms.Write(MistralTokenizer.DecodeVocabById[Tokens[i]]);
		}

		string J = Encoding.UTF8.GetString(Ms.ToArray());

		return RemovePrecedingSpace ? J[1..] : J;
	}
}