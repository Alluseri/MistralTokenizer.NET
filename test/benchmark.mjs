// jshint esversion: 11

import mistralTokenizer from './mistral-tokenizer.mjs';
import * as fs from 'node:fs';

var document = fs.readFileSync("sample_doc.txt").toString();
var samples = 250;

console.log("[MT.JS] Sample document is " + document.length + " characters long.");

console.log("[MT.JS] Warming up...");
for (var i = 0; i < 13000; i++) {
	mistralTokenizer.decode(mistralTokenizer.encode("what is a source    code?\n镇🦙Ꙋ  lol"));
}

console.log("[MT.JS] Encoding sample document " + samples + " times.");
var pne = performance.now();
for (var i = 0; i < samples; i++) {
	mistralTokenizer.encode(document);
}
pne = performance.now() - pne;
var encoded = mistralTokenizer.encode(document, true, true);

console.log("[MT.JS] Decoding sample document " + samples + " times.");
var pnd = performance.now();
for (var i = 0; i < samples; i++) {
	mistralTokenizer.decode(encoded);
}
pnd = performance.now() - pnd;
var decoded = mistralTokenizer.decode(encoded, true, true);

console.log("[MT.JS] Encoding time: " + Math.floor(pne) + " ms (~" + Math.floor(pne / samples) + " ms per run)");
console.log("[MT.JS] Decoding time: " + Math.floor(pnd) + " ms (~" + Math.floor(pnd / samples) + " ms per run)");

console.log("[MT.JS] Is tokenizer correct: " + (decoded === document));
console.log("[MT.JS] Output token count: " + encoded.length);

fs.writeFileSync("output_tokens.txt", encoded.join("\n"));