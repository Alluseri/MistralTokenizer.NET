using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Alluseri.MistralTokenizer;

/// <summary>
/// A static class that provides the Mistral encoder.
/// </summary>
public static class MistralEncoder {
	private static string UTF8ByteToHex(byte Byte) => $"<0x{Byte:X2}>";

	private static List<ushort> MapCharactersToTokenIds(string Prompt, bool AddBosToken = true, bool AddPrecedingSpace = true) {
		List<ushort> TokenIds = new();

		if (AddBosToken)
			TokenIds.Add(1);

		if (AddPrecedingSpace)
			Prompt = " " + Prompt;

		Span<byte> Decoded = stackalloc byte[4];

		foreach (Rune CodePoint in Prompt.Replace(' ', MistralTokenizer.EncodeVocabById[28705][0]).EnumerateRunes()) {
			if (MistralTokenizer.VocabByRune.TryGetValue(CodePoint, out ushort Token)) {
				TokenIds.Add(Token);
			} else if (CodePoint.IsAscii) {
				TokenIds.Add(MistralTokenizer.VocabByByte[(byte) CodePoint.Value]);
			} else {
				int Len = CodePoint.EncodeToUtf8(Decoded);
				for (int i = 0; i < Len; i++)
					TokenIds.Add(MistralTokenizer.VocabByByte[Decoded[i]]);
			}
		}

		return TokenIds;
	}

	private class LinkedNode {
		public int OrigPos { get; set; }
		public ushort TokenId { get; set; }
		public LinkedNode? Prev { get; set; }
		public LinkedNode? Next { get; set; }
		public string MergeToString { get; set; }
		public bool Deleted { get; set; }

		public LinkedNode() {
			Deleted = false;
			MergeToString = "";
		}
	}

	private static void AddToMergeQueue(string Prompt, PriorityQueue<LinkedNode, int> MergeQueue, LinkedNode LeftNode) {
		string MStrId = MistralTokenizer.GetMergeIdentifierString(LeftNode.TokenId, LeftNode.Next!.TokenId);

		if (MistralTokenizer.Merges.TryGetValue(MStrId, out int Merge)) {
			LeftNode.MergeToString = MStrId.Replace(" ", "");
			MergeQueue.Enqueue(LeftNode, Merge + (LeftNode.OrigPos / Prompt.Length));
		}
	}

	/// <summary>
	/// Encodes the given text prompt into a sequence of Mistral token IDs.
	/// </summary>
	/// <param name="Prompt">The text prompt to be encoded. Must not be null or empty.</param>
	/// <param name="AddBosToken">Specifies whether to add a beginning-of-sequence (BOS) token at the start of the encoded sequence. Default is true.</param>
	/// <param name="AddPrecedingSpace">Specifies whether to add a preceding space character before the encoded sequence. Default is true.</param>
	/// <returns>An array of token IDs representing the input sequence. Returns an empty array if the input prompt is empty.</returns>
	public static ushort[] Encode(string Prompt, bool AddBosToken = true, bool AddPrecedingSpace = true) {
		if (Prompt.Length == 0)
			return Array.Empty<ushort>();

		List<ushort> TokenIds = MapCharactersToTokenIds(Prompt, AddBosToken, AddPrecedingSpace);

		PriorityQueue<LinkedNode, int> MergeQueue = new();

		LinkedNode FirstTokenNode = new() {
			OrigPos = 0,
			TokenId = TokenIds[0],
			Prev = null,
			Next = null
		};
		LinkedNode PrevTokenNode = FirstTokenNode;

		for (int i = 1; i < TokenIds.Count; i++) {
			LinkedNode CurrentTokenNode = new() {
				OrigPos = i,
				TokenId = TokenIds[i],
				Prev = PrevTokenNode,
				Next = null
			};

			PrevTokenNode.Next = CurrentTokenNode;
			AddToMergeQueue(Prompt, MergeQueue, PrevTokenNode);
			PrevTokenNode = CurrentTokenNode;
		}

		while (MergeQueue.Count != 0) {
			LinkedNode LeftOfMerge = MergeQueue.Dequeue();

			if (LeftOfMerge.Deleted)
				continue;

			if (LeftOfMerge.Next == null)
				continue;

			if (LeftOfMerge.Next.Deleted)
				continue;

			LeftOfMerge.Deleted = true;
			LeftOfMerge.Next.Deleted = true;

			if (LeftOfMerge.Prev != null) {
				LinkedNode OldPrev = LeftOfMerge.Prev;
				OldPrev.Deleted = true;
				LinkedNode NewPrev = new() {
					OrigPos = OldPrev.OrigPos,
					TokenId = OldPrev.TokenId,
					Prev = OldPrev.Prev,
					Next = OldPrev.Next
				};

				LeftOfMerge.Prev = NewPrev;

				if (NewPrev.Prev != null) {
					NewPrev.Prev.Next = NewPrev;
				} else {
					FirstTokenNode = NewPrev;
				}
			}

			LinkedNode ResultOfMerge = new() {
				OrigPos = LeftOfMerge.OrigPos,
				TokenId = MistralTokenizer.VocabByString[LeftOfMerge.MergeToString], // TODO: Or GetValueByDefault? :/
				Prev = LeftOfMerge.Prev,
				Next = LeftOfMerge.Next.Next
			};

			if (ResultOfMerge.Prev != null) {
				ResultOfMerge.Prev.Next = ResultOfMerge;
				AddToMergeQueue(Prompt, MergeQueue, ResultOfMerge.Prev);
			} else {
				FirstTokenNode = ResultOfMerge;
			}

			if (ResultOfMerge.Next != null) {
				ResultOfMerge.Next.Prev = ResultOfMerge;

				AddToMergeQueue(Prompt, MergeQueue, ResultOfMerge);
			}
		}

		List<ushort> MergedTokenIds = new();

		for (LinkedNode? CurrentTokenNode = FirstTokenNode;
	CurrentTokenNode != null;
	CurrentTokenNode = CurrentTokenNode.Next) {
			MergedTokenIds.Add(CurrentTokenNode.TokenId);
		}

		return MergedTokenIds.ToArray();
	}
}