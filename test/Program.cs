using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Alluseri.MistralTokenizer;

public static class Program {
	public static void TestCase(string Input, ushort[] Expected) {
		ushort[] ActualTokens = MistralEncoder.Encode(Input, true, true);

		if (!ActualTokens.SequenceEqual(Expected)) {
			Console.WriteLine($"Invalid MistralEncoder response for {Input}:");
			Console.WriteLine("Expected the sequence: " + string.Join(' ', Expected.Select(Y => $"{Y:X4}")));
			Console.WriteLine("Got this sequence tho: " + string.Join(' ', ActualTokens.Select(Y => $"{Y:X4}")));
		}
		string K = MistralDecoder.Decode(ActualTokens);
		if (Input != K) {
			Console.WriteLine($"Invalid MistralDecoder response for {Input}");
			Console.WriteLine("Expected the output: " + Input);
			Console.WriteLine("Got this output tho: " + K);
		}
	}

	public static void RunTests() {
		Console.WriteLine("Begin MistralTokenizer.NET adapted test...");
		Console.OutputEncoding = Encoding.UTF8;

		Console.WriteLine("Test 1:");
		TestCase("grabbed", new ushort[] { 1, 13300 });
		Console.WriteLine("Test 2:");
		TestCase(" grabbed", new ushort[] { 1, 28705, 13300 });
		Console.WriteLine("Test 3:");
		TestCase("           grabbed", new ushort[] { 1, 17422, 13300 });
		Console.WriteLine("Test 4:");
		TestCase("\n", new ushort[] { 1, 28705, 13 });
		Console.WriteLine("Test 5:");
		TestCase(" \n", new ushort[] { 1, 259, 13 });
		Console.WriteLine("Test 6:");
		TestCase("	tabs				out here", new ushort[] { 1, 28705, 12, 24856, 12, 12, 12, 12, 406, 1236 });
		Console.WriteLine("Test 7:");
		TestCase("ax\n####\nboo", new ushort[] { 1, 6056, 13, 2000, 13, 1798, 28709 });
		Console.WriteLine("Test 8:");
		TestCase("镇", new ushort[] { 1, 28705, 29780 });
		Console.WriteLine("Test 9:");
		TestCase("🦙", new ushort[] { 1, 28705, 243, 162, 169, 156 });
		Console.WriteLine("Test 10:");
		TestCase("🦙Ꙋ", new ushort[] { 1, 28705, 243, 162, 169, 156, 237, 156, 141 });
		Console.WriteLine("Test 11:");
		TestCase("Ꙋ🦙", new ushort[] { 1, 28705, 237, 156, 141, 243, 162, 169, 156 });
		Console.WriteLine("Test 12:");
		TestCase("The llama (/ˈlɑːmə/; 🦙Spanish pronunciation: [ˈʎama]) (Lama glama) is a domesticated South American camelid, widely used as a meat and pack animal by Andean cultures since the Pre-Columbian era. Llamas are social animals and live with others as a herd. Their wool is soft and contains only a small amount of lanolin.[2] Llamas can learn simple tasks after a few repetitions. When using a pack, they can carry about 25 to 30% of their body weight for 8 to 13 km (5–8 miles).[3] The name llama (in the past also spelled \"lama\" or \"glama\") was adopted by European settlers from native Peruvians.[4] The ancestors of llamas are thought to have originated from the Great Plains of North America about 40 million years ago, and subsequently migrated to South America about three million years ago during the Great American Interchange. By the end of the last ice age (10,000–12,000 years ago), camelids were extinct in North America.[3] As of 2007, there were over seven million llamas and alpacas in South America and over 158,000 llamas and 100,000Ꙋ🦙 alpacas, descended from progenitors imported late in the 20th century, in the United States and Canada.[5] In Aymara mythology, llamas are important beings. The Heavenly Llama is said to drink water from the ocean and urinates as it rains.[6] According to Aymara eschatology, llamas will return to the water springs and lagoons where they come from at the end of time.[6]", new ushort[] { 1, 415, 8814, 2786, 325, 28748, 29097, 28714, 29813, 29240, 28719, 29108, 28748, 28745, 28705, 243, 162, 169, 156, 13116, 789, 12704, 14281, 352, 28747, 733, 29097, 205, 145, 2786, 2803, 325, 28758, 2786, 1272, 2786, 28731, 349, 264, 2853, 374, 6899, 3658, 2556, 3730, 301, 313, 28725, 12575, 1307, 390, 264, 10228, 304, 2163, 8527, 486, 1015, 28706, 276, 19826, 1854, 272, 4258, 28733, 1577, 2915, 753, 4204, 28723, 393, 5989, 293, 460, 2809, 8222, 304, 2943, 395, 2663, 390, 264, 559, 28715, 28723, 6723, 25943, 349, 2664, 304, 5876, 865, 264, 1741, 3558, 302, 26573, 27545, 20011, 28750, 28793, 393, 5989, 293, 541, 2822, 3588, 9796, 1024, 264, 1664, 21435, 2065, 28723, 1684, 1413, 264, 2163, 28725, 590, 541, 7096, 684, 28705, 28750, 28782, 298, 28705, 28770, 28734, 28823, 302, 652, 2187, 4336, 354, 28705, 28783, 298, 28705, 28740, 28770, 3535, 325, 28782, 28816, 28783, 6052, 609, 28792, 28770, 28793, 415, 1141, 8814, 2786, 325, 262, 272, 2609, 835, 668, 6099, 345, 28714, 2786, 28739, 442, 345, 1727, 2786, 1243, 403, 13424, 486, 6392, 4641, 8531, 477, 8271, 2744, 5388, 3693, 20011, 28781, 28793, 415, 25427, 302, 17620, 293, 460, 1654, 298, 506, 5016, 601, 477, 272, 6043, 1641, 1606, 302, 3964, 4352, 684, 28705, 28781, 28734, 3841, 1267, 3584, 28725, 304, 18410, 11205, 601, 298, 3658, 4352, 684, 1712, 3841, 1267, 3584, 1938, 272, 6043, 2556, 4287, 4078, 28723, 2463, 272, 948, 302, 272, 1432, 7515, 3595, 325, 28740, 28734, 28725, 28734, 28734, 28734, 28816, 28740, 28750, 28725, 28734, 28734, 28734, 1267, 3584, 557, 3730, 301, 2298, 654, 1568, 5654, 297, 3964, 4352, 20011, 28770, 28793, 1136, 302, 28705, 28750, 28734, 28734, 28787, 28725, 736, 654, 754, 6671, 3841, 17620, 293, 304, 389, 28720, 323, 293, 297, 3658, 4352, 304, 754, 28705, 28740, 28782, 28783, 28725, 28734, 28734, 28734, 17620, 293, 304, 28705, 28740, 28734, 28734, 28725, 28734, 28734, 28734, 237, 156, 141, 243, 162, 169, 156, 389, 28720, 323, 293, 28725, 2283, 2508, 477, 430, 2383, 9058, 26659, 3909, 297, 272, 28705, 28750, 28734, 362, 5445, 28725, 297, 272, 2969, 3543, 304, 6082, 20011, 28782, 28793, 560, 330, 1082, 2923, 13345, 2161, 28725, 17620, 293, 460, 2278, 16905, 28723, 415, 22830, 346, 393, 28714, 2786, 349, 773, 298, 4663, 2130, 477, 272, 13993, 304, 4273, 10218, 390, 378, 408, 1606, 20011, 28784, 28793, 6586, 298, 330, 1082, 2923, 1037, 12932, 2161, 28725, 17620, 293, 622, 604, 298, 272, 2130, 7474, 28713, 304, 305, 4567, 1053, 970, 590, 1567, 477, 438, 272, 948, 302, 727, 20011, 28784, 28793 });

		Console.WriteLine("MistralTokenizer.NET test complete! If no errors are logged above this message, the library is 100% production ready.");
	}

	public static void Main(string[] Args) {
		RunTests();

		Console.WriteLine(MistralTokenizer.EncodeVocabById[28705]);

		string Document = File.ReadAllText("test/sample_doc.txt");

		int SamplesE = 250;
		int SamplesD = 5000;

		Console.WriteLine("[MT.NET] Sample document is " + Document.Length + " characters long.");

		Console.WriteLine("[MT.NET] Warming up...");
		for (int i = 0; i < 13000; i++) {
			MistralDecoder.Decode(MistralEncoder.Encode("what is a source    code?\n镇🦙Ꙋ  lol"));
		}

		Console.WriteLine("[MT.NET] Encoding sample document " + SamplesE + " times.");
		Stopwatch Swe = Stopwatch.StartNew();
		for (int i = 0; i < SamplesE; i++) {
			MistralEncoder.Encode(Document);
		}
		Swe.Stop();
		ushort[] Encoded = MistralEncoder.Encode(Document, true, true);

		Console.WriteLine("[MT.NET] Decoding sample document " + SamplesD + " times.");
		Stopwatch Swd = Stopwatch.StartNew();
		for (int i = 0; i < SamplesD; i++) {
			MistralDecoder.Decode(Encoded);
		}
		Swd.Stop();
		string Decoded = MistralDecoder.Decode(Encoded, true, true);

		Console.WriteLine("[MT.NET] Encoding time: " + Swe.ElapsedMilliseconds + " ms (~" + (Swe.ElapsedMilliseconds / SamplesE) + " ms per run)");
		Console.WriteLine("[MT.NET] Decoding time: " + Swd.ElapsedMilliseconds + " ms (~" + (Swd.ElapsedMilliseconds / SamplesD) + " ms per run)");

		Console.WriteLine("[MT.NET] Is tokenizer correct: " + (Decoded == Document));
		Console.WriteLine("[MT.NET] Output token count: " + Encoded.Length);

		File.WriteAllText("output_tokens.txt", string.Join('\n', Encoded));
	}
}