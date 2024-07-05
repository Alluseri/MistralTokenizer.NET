# MistralTokenizer.NET
MistralTokenizer.NET is a complete standalone port of the JavaScript library [mistral-tokenizer](https://github.com/imoneoi/mistral-tokenizer) in C#.

## Usage
```cs
ushort[] Tokens = MistralEncoder.Encode("Hello, world!");
string Text = MistralDecoder.Decode(Tokens);
Console.WriteLine($"\"{Text}\" is {Tokens.Length} tokens long.");
```

## Performance
The main premise of this library is to **be fast**. Following this principle, the encoder is **up to 50% faster** and the decoder is **97% faster** than the respective JavaScript implementations. Consider the following benchmark:

```
[MT.JS] Sample document is 192168 characters long.
[MT.JS] Warming up...
[MT.JS] Encoding sample document 250 times.
[MT.JS] Decoding sample document 5000 times.
[MT.JS] Encoding time: 95655 ms (~382 ms per run)
[MT.JS] Decoding time: 224469 ms (~44 ms per run)
[MT.JS] Is tokenizer correct: true
[MT.JS] Output token count: 101167

[MT.NET PROFILE_INIT] Spent 75 ms initializing MistralTokenizer.
[MT.NET] Sample document is 192168 characters long.
[MT.NET] Warming up...
[MT.NET] Encoding sample document 250 times.
[MT.NET] Decoding sample document 5000 times.
[MT.NET] Encoding time: 48952 ms (~195 ms per run)
[MT.NET] Decoding time: 6287 ms (~1 ms per run)
[MT.NET] Is tokenizer correct: True
[MT.NET] Output token count: 101172
```

Though it would be completely reasonable to use BenchmarkDotNet here, I have to be able to compare the performance to the JS library, which, quite obviously, BenchmarkDotNet does not support, hence the low quality benchmarking setup.

In rare cases, as seen here for example, MT.JS and MT.NET may have slight discrepancies in token outputs, which result in the same output when decoded through the same library. It's not immediately obvious as to why, perhaps it's a mistake in either of the libraries' priority queues or something more complicated. I haven't performed cross-decoding.

<!-- TODO: BenchmarkDotNet various distributions for encoder and decoder alike -->

## Compatibility
Out of the box, this library provides the `v1` tokenizer, used in Mistral 7B and 8x7B.

Mixtral 8x22B officially uses the `v3` tokenizer, but [MLX community provides the v1 tokenizer for WizardLM-2-8x22B, too](https://huggingface.co/mlx-community/WizardLM-2-8x22B-4bit/blob/main/tokenizer.json). I verified that by comparing CRC32s of MT.NET's token db and the provided token db using `7z h`. Both returned `57FC7F86`.

It is very important to note that I couldn't align MT.JS and MT.NET with **Mistral's official tokenizer** or **[Lunary's Mistral tokenizer](https://lunary.ai/mistral-tokenizer)**, so keep that in mind.

Mistral tokenization seems to be very niche with most people opting to (mistakenly) use Tiktoken or LLaMA's tokenizer, so it's difficult to find an actually 100% correct tokenizer for the purpose.

### OpenRouter

MT.NET produces the results closest in terms of token count to OpenRouter's post-completion lookup API when queried against WizardLM-2-8x22B, here's a sample:
```
Tiktoken: 330
Llama Tokenizer: 414
Lunary Mistral Tokenizer: 426
MT.NET: 435
OpenRouter(DeepInfra): 442
```

The response field `native_tokens_prompt` was used and checked against the cost of the request to verify its validity. `tokens_prompt` is always tokenized with Tiktoken, but it is **not** used for billing purposes.

### Why not port Mistral's official tokenizer instead?
Because Python is the worst thing I have ever witnessed.

## Legal
MistralTokenizer.NET is licensed under the [CC-BY 4.0 license](https://github.com/Alluseri/MistralTokenizer.NET/blob/master/LICENSE).

[mistral-tokenizer](https://github.com/imoneoi/mistral-tokenizer) is licensed under the [MIT license](https://github.com/Alluseri/MistralTokenizer.NET/blob/master/mistral-tokenizer-js.LICENSE.md).