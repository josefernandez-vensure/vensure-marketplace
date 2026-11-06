# Law 3.2 ledger: the .NET law rewritten as short instructions

Law 3.2 is `rancho-dotnet` 2.2.0. The core law (`rancho` 3.0.0) and `rancho-react` 2.0.0 are unchanged. Every sentence of `plugins/rancho-dotnet/skills/law/SKILL.md` was rewritten for structure and length: long paragraphs became lists, multi-part rules became sub-bullets, and each Testing entry states its own subject and id. No rule was meant to change.

A sentence-by-sentence old/new list would be most of the file twice. This ledger instead proves the rewrite against the old law's obligations:

1. **Checklist.** An agent that saw only the 3.1 text (`law-3.1`, commit e597ab1) listed every obligation, condition, exception, named tool, ⚙ mark, Testing entry, table row and definition as one item each: 924 items, O-1 to O-924 (below, *Checklist*).
2. **Check.** Three agents, each given the old text, the rewrite (commit c90a813) and a third of the checklist, gave every item a verdict - KEPT, WEAKENED, CHANGED or MISSING - and flagged new obligations and ambiguous wording (below, *Check results*; line numbers refer to c90a813).
3. **Fix.** Every non-KEPT item and every ambiguity note was fixed, mostly by restoring the old wording (commit b07663c and the commit after it).
4. **Re-check.** A fourth agent confirmed each finding resolved against the old text, and checked every line the fixes touched for lost or moved obligations and marks (below, *Re-check*).

Load per .NET stream (`python -I tools/check-law.py`): ~19.4k tokens before, ~19.3k after (.NET law 9325 to 9240 words). The rewrite is for clarity, not size: the .NET law's length is its rules, conditions and named tools, which the check keeps.

`python -I tools/law-ledger.py law-3.1` lists 62 normative sentences of 3.1 that are no longer stated verbatim: the rewordings the check covers. Against `main` the list also holds the 13 sentences of 3.1's own ledger, `RECORDS-3.1-ledger.md`.

## Re-check

- Of 44 findings across the three checks (29 non-KEPT items and 15 ambiguity notes), the first fix commit resolved 37. Of the 7 left open, 5 were judged equivalent in meaning and kept: "The OpenAPI document MUST carry the class" (the emitter's MUST is unchanged), "the declared value MUST be that period", "An operation declaration MAY name", "Where telemetry needs the environment's name", and the Serilog sentences in a bullet without a mark (BE-93's row covers only the handler `ILogger` rule, which keeps the mark). The last commit restored "do not count against this rule". The seventh is the reason clause moved to the records (`plugins/rancho-dotnet/RECORDS.md`, `## 2.2.0`).
- No obligation, condition, exception, named thing or ⚙ mark was lost or moved off the sentence stating its rule. BE-105/106, BE-110 and BE-119 each sit on one bullet again, as in 3.1; BE-118 and BE-124 are named on every sub-bullet they cover.
- Three more points from the re-check were fixed in the last commit: the Audit test no longer says the behavior and the port "each" need a test, which read as two; the fault-isolation boundary's sentence names "The boundary" rather than "It"; and "do not count against this rule".
- No rule combination without a compliant answer was found.

## Check results

### Front matter to §4 Commands and Queries

NEW = c90a813:plugins/rancho-dotnet/skills/law/SKILL.md (line numbers refer to it).
Counts: KEPT 275 · CHANGED 9 · WEAKENED 5 · MISSING 1 (total 290)

O-1 | KEPT | 3
O-2 | KEPT | 3
O-3 | KEPT | 3
O-4 | KEPT | 3
O-5 | KEPT | 3
O-6 | KEPT | 3
O-7 | KEPT | 8
O-8 | KEPT | 8
O-9 | KEPT | 8
O-10 | CHANGED | 8 | "carries the core law's mechanism in every tree bound to it" -> "carries the core law's mechanism." (scope qualifier dropped)
O-11 | KEPT | 8
O-12 | KEPT | 8
O-13 | KEPT | 8
O-14 | KEPT | 8
O-15 | CHANGED | 8 | "consult it by rule id rather than reading it whole" -> "look rules up by id; do not read it whole" (preference became prohibition: widened)
O-16 | MISSING | 10 | "Three annexes" count dropped; NEW says "Each annex in references/" (open set)
O-17 | WEAKENED | 10 | "bind a tree only once the tree meets the annex's trigger" -> "binds ... once the tree meets its trigger" ("only" dropped)
O-18 | KEPT | 12-14
O-19 | KEPT | 12-14
O-20 | KEPT | 12-14
O-21 | KEPT | 12-14
O-22 | KEPT | 12-14
O-23 | KEPT | 12-14
O-24 | KEPT | 12-14
O-25 | KEPT | 10
O-26 | KEPT | 10
O-27 | WEAKENED | 10 | "binds the whole tree, exactly as this file does" -> "as this file does" ("exactly" dropped)
O-28 | KEPT | 16-20
O-29 | KEPT | 16-20
O-30 | KEPT | 22
O-31 | KEPT | 22
O-32 | KEPT | 22
O-33 | KEPT | 26
O-34 | KEPT | 26
O-35 | KEPT | 26
O-36 | KEPT | 26
O-37 | KEPT | 26
O-38 | KEPT | 32
O-39 | KEPT | 33
O-40 | KEPT | 34
O-41 | KEPT | 35
O-42 | KEPT | 36
O-43 | KEPT | 37
O-44 | KEPT | 38
O-45 | KEPT | 39
O-46 | KEPT | 41
O-47 | KEPT | 45
O-48 | KEPT | 45
O-49 | KEPT | 47-57
O-50 | KEPT | 47-57
O-51 | KEPT | 47-57
O-52 | KEPT | 47-57
O-53 | KEPT | 47-57
O-54 | KEPT | 47-57
O-55 | KEPT | 47-57
O-56 | KEPT | 47-57
O-57 | KEPT | 47-57
O-58 | KEPT | 47-57
O-59 | KEPT | 47-57
O-60 | KEPT | 47-57
O-61 | KEPT | 59
O-62 | KEPT | 63
O-63 | KEPT | 63
O-64 | KEPT | 63
O-65 | KEPT | 63
O-66 | KEPT | 63
O-67 | KEPT | 63
O-68 | KEPT | 63
O-69 | KEPT | 63
O-70 | KEPT | 63
O-71 | KEPT | 63
O-72 | KEPT | 63
O-73 | KEPT | 63
O-74 | KEPT | 63
O-75 | KEPT | 63
O-76 | KEPT | 63
O-77 | KEPT | 63
O-78 | KEPT | 63
O-79 | KEPT | 65
O-80 | KEPT | 65
O-81 | KEPT | 66
O-82 | KEPT | 66
O-83 | KEPT | 66
O-84 | KEPT | 67
O-85 | KEPT | 71
O-86 | KEPT | 71
O-87 | KEPT | 73
O-88 | KEPT | 73
O-89 | KEPT | 73
O-90 | KEPT | 74
O-91 | KEPT | 75
O-92 | KEPT | 75
O-93 | KEPT | 75
O-94 | KEPT | 75
O-95 | KEPT | 76
O-96 | KEPT | 76
O-97 | KEPT | 80
O-98 | KEPT | 81
O-99 | KEPT | 82
O-100 | KEPT | 83
O-101 | KEPT | 84
O-102 | KEPT | 85
O-103 | KEPT | 86
O-104 | KEPT | 87
O-105 | KEPT | 88
O-106 | KEPT | 90
O-107 | KEPT | 90
O-108 | KEPT | 90
O-109 | KEPT | 90
O-110 | KEPT | 90
O-111 | KEPT | 92
O-112 | KEPT | 92
O-113 | KEPT | 92
O-114 | KEPT | 91
O-115 | KEPT | 91
O-116 | KEPT | 91
O-117 | KEPT | 93
O-118 | KEPT | 93
O-119 | KEPT | 93
O-120 | KEPT | 93
O-121 | KEPT | 93
O-122 | KEPT | 94
O-123 | KEPT | 94
O-124 | KEPT | 94
O-125 | KEPT | 94
O-126 | KEPT | 94
O-127 | KEPT | 95
O-128 | KEPT | 95
O-129 | KEPT | 95
O-130 | KEPT | 95
O-131 | KEPT | 95
O-132 | KEPT | 95
O-133 | CHANGED | 96 | "Contracts references Platform.Application because a published query message MUST..." -> "A published query message in Contracts MUST..."; stated reference and its reason dropped (table row line 83 still permits it)
O-134 | KEPT | 96
O-135 | KEPT | 96
O-136 | KEPT | 96
O-137 | KEPT | 96
O-138 | CHANGED | 97 | bullet at 97 carries no ⚙; ⚙ BE-105, BE-106 now sits only on the line-100 bullet
O-139 | CHANGED | 98 | no ⚙ BE-105, BE-106 on line 98 sentence (mark moved to line 100 bullet)
O-140 | CHANGED | 99 | no ⚙ BE-105, BE-106 on line 99 sentence (mark moved to line 100 bullet)
O-141 | KEPT | 100
O-142 | CHANGED | 100 | "after startup MUST NOT name or invoke an Infrastructure type directly" -> "MUST NOT name or invoke one directly"; "one" can bind to "any other Infrastructure type", exempting registration entry points after startup (ambiguous, possibly narrowed)
O-143 | WEAKENED | 100 | "through the dispatcher, not through Api" -> "Infrastructure handlers are reached through the dispatcher" ("not through Api" dropped)
O-144 | KEPT | 100
O-145 | KEPT | 100
O-146 | KEPT | 100
O-147 | KEPT | 101
O-148 | KEPT | 101
O-149 | KEPT | 101
O-150 | KEPT | 101
O-151 | KEPT | 101
O-152 | KEPT | 101
O-153 | KEPT | 103-107
O-154 | KEPT | 103-107
O-155 | KEPT | 103-107
O-156 | KEPT | 103-107
O-157 | KEPT | 103-107
O-158 | KEPT | 109
O-159 | KEPT | 109
O-160 | KEPT | 109
O-161 | KEPT | 113
O-162 | KEPT | 114
O-163 | KEPT | 115
O-164 | KEPT | 116
O-165 | KEPT | 117
O-166 | KEPT | 118
O-167 | KEPT | 120
O-168 | KEPT | 120
O-169 | KEPT | 120
O-170 | KEPT | 120
O-171 | KEPT | 120
O-172 | KEPT | 121
O-173 | KEPT | 122
O-174 | KEPT | 122
O-175 | KEPT | 122
O-176 | KEPT | 122
O-177 | KEPT | 122
O-178 | KEPT | 122
O-179 | KEPT | 128
O-180 | KEPT | 128
O-181 | KEPT | 128
O-182 | KEPT | 129
O-183 | KEPT | 129
O-184 | KEPT | 130
O-185 | KEPT | 130
O-186 | KEPT | 130
O-187 | KEPT | 130
O-188 | KEPT | 131
O-189 | KEPT | 131
O-190 | KEPT | 131
O-191 | KEPT | 131
O-192 | KEPT | 132
O-193 | KEPT | 132
O-194 | KEPT | 133
O-195 | KEPT | 134
O-196 | KEPT | 134
O-197 | KEPT | 134
O-198 | KEPT | 134
O-199 | KEPT | 135
O-200 | KEPT | 136
O-201 | KEPT | 136
O-202 | KEPT | 136
O-203 | KEPT | 138-149
O-204 | KEPT | 138-149
O-205 | KEPT | 153
O-206 | KEPT | 153
O-207 | KEPT | 154
O-208 | KEPT | 154
O-209 | KEPT | 154
O-210 | KEPT | 154
O-211 | KEPT | 155
O-212 | WEAKENED | 155 | "one type per concept, so passing the wrong id does not compile" -> "one type per concept" (compile-time non-interchangeability criterion dropped)
O-213 | KEPT | 156
O-214 | KEPT | 156
O-215 | KEPT | 156
O-216 | KEPT | 156
O-217 | KEPT | 157
O-218 | KEPT | 157
O-219 | KEPT | 157
O-220 | KEPT | 157
O-221 | KEPT | 157
O-222 | KEPT | 157
O-223 | KEPT | 161
O-224 | KEPT | 161
O-225 | KEPT | 162
O-226 | KEPT | 162
O-227 | KEPT | 163
O-228 | KEPT | 163
O-229 | KEPT | 162
O-230 | KEPT | 164
O-231 | KEPT | 165
O-232 | KEPT | 166
O-233 | KEPT | 166
O-234 | KEPT | 170
O-235 | KEPT | 171
O-236 | KEPT | 171
O-237 | KEPT | 171
O-238 | KEPT | 177
O-239 | KEPT | 177
O-240 | KEPT | 177
O-241 | KEPT | 178
O-242 | KEPT | 178
O-243 | KEPT | 178
O-244 | KEPT | 178
O-245 | KEPT | 180
O-246 | KEPT | 180
O-247 | KEPT | 181
O-248 | KEPT | 181
O-249 | KEPT | 181
O-250 | KEPT | 181
O-251 | KEPT | 181
O-252 | KEPT | 181
O-253 | KEPT | 182
O-254 | KEPT | 182
O-255 | KEPT | 182
O-256 | KEPT | 182
O-257 | KEPT | 182
O-258 | KEPT | 182
O-259 | CHANGED | 183 | subject "A query handler" -> "It" (nearest antecedent is the command handler / pure-computation one on line 182); "keyless entity types ... maps for reading" -> "keyless read types ... maps"
O-260 | CHANGED | 183 | same "It" antecedent ambiguity as O-259 ("or with parameterized SQL projected into the DTO")
O-261 | KEPT | 184
O-262 | KEPT | 184
O-263 | KEPT | 184
O-264 | KEPT | 185
O-265 | KEPT | 185
O-266 | WEAKENED | 185 | "A migration's SQL (§5) is written, not built from input, and is outside this rule" -> "A migration's SQL (§5) is outside this rule"; the condition that migration SQL is not built from input is gone, so the exemption is unconditional
O-267 | KEPT | 186
O-268 | KEPT | 186
O-269 | KEPT | 186
O-270 | KEPT | 186
O-271 | KEPT | 187
O-272 | KEPT | 187
O-273 | KEPT | 188
O-274 | KEPT | 188
O-275 | KEPT | 188
O-276 | KEPT | 187
O-277 | KEPT | 189
O-278 | KEPT | 189
O-279 | KEPT | 189
O-280 | KEPT | 191-198
O-281 | KEPT | 191-198
O-282 | KEPT | 191-198
O-283 | KEPT | 200
O-284 | KEPT | 200
O-285 | KEPT | 202
O-286 | KEPT | 202
O-287 | KEPT | 202
O-288 | KEPT | 202
O-289 | KEPT | 203
O-290 | KEPT | 203

#### Findings

##### Non-KEPT items

1. O-10 (CHANGED, NEW 8). OLD: "carries the core law's mechanism in every tree bound to it." NEW: "carries the core law's mechanism." Scope qualifier dropped.
2. O-15 (CHANGED, widened, NEW 8). OLD: "consult it by rule id rather than reading it whole." NEW: "look rules up by id; do not read it whole." A stated preference became a prohibition.
3. O-16 (MISSING, NEW 10). OLD: "Three annexes, beside the inventory in `references/`, hold rules that..." NEW: "Each annex in `references/` binds the whole tree..." The count is gone, so the set of annexes is open: any annex file added to `references/` would bind.
4. O-17 (WEAKENED, NEW 10). OLD: "hold rules that bind a tree only once the tree meets the annex's trigger". NEW: "binds the whole tree, as this file does, once the tree meets its trigger." The word "only" is dropped, so NEW no longer says that annex rules do not bind before the trigger.
5. O-27 (WEAKENED, NEW 10). OLD: "the annex binds the whole tree, exactly as this file does." NEW: "as this file does". The word "exactly" is dropped.
6. O-133 (CHANGED, NEW 96). OLD: "`Contracts` references `Platform.Application` because a published query message MUST name both..." NEW: "A published query message in `Contracts` MUST name both..." The stated reference and its reason are gone. Table line 83 still permits the reference.
7. O-138, O-139, O-140 (CHANGED, ⚙ mark, NEW 97-99). In OLD, `⚙ BE-105, BE-106` closed the paragraph that included "`Api` names only the registration entry points...", "exactly one, `Add<Module>Module(...)`" and "one `Add<Feature>` extension per platform feature". In NEW those three sentences are a separate bullet with no mark. The mark appears only on the line-100 bullet.
8. O-142 (CHANGED, ambiguous, NEW 100). OLD: "after startup MUST NOT name or invoke an Infrastructure type directly". NEW: "MUST NOT name any other Infrastructure type ... and after startup MUST NOT name or invoke one directly". The nearest antecedent of "one" is "any other Infrastructure type". On that reading, Api may invoke registration entry points after startup.
9. O-143 (WEAKENED, NEW 100). OLD: "dispatch reaches an Infrastructure handler through the dispatcher, not through `Api`". NEW: "Infrastructure handlers are reached through the dispatcher." The exclusion "not through `Api`" is dropped.
10. O-212 (WEAKENED, NEW 155). OLD: "one type per concept, so passing the wrong id does not compile." NEW: "one type per concept." The compile-time non-interchangeability criterion is dropped.
11. O-259, O-260 (CHANGED, ambiguous, NEW 183). OLD: "A query handler reads a relational database through keyless entity types its module's Infrastructure maps for reading..." NEW: "It reads a relational database through keyless read types its module's Infrastructure maps over a table or view..." The subject became "It". The preceding sentence (line 182) ends with "A command handler stays in `Application`", so "It" can bind to the command handler. Also, the EF term "keyless entity types" became "keyless read types", and "for reading" is dropped.
12. O-266 (WEAKENED, NEW 185). OLD: "A migration's SQL (§5) is written, not built from input, and is outside this rule." NEW: "A migration's SQL (§5) is outside this rule." The exemption is now unconditional. It no longer rests on the migration's SQL not being built from input.

##### New or ambiguous wording on otherwise-KEPT items

- O-18..O-24 (NEW 10-14). In OLD the trigger list followed a colon after "...the annex's trigger:". In NEW the list follows "Each is named where its rules would otherwise stand." with no lead-in, so the reader must infer that the bullets are the triggers.
- O-120, O-121 (NEW 93). OLD: "A package reference qualifies with `IncludeAssets="analyzers"` and `PrivateAssets="all"`; a project reference with ..." NEW gives these after a colon, with no "qualifies". They can be read as examples rather than the required condition.
- O-146 (NEW 100). The rationale "is written by the generator, not in `Api`" is dropped. The exception is intact, so the item is rated KEPT.
- New wording: line 96 adds "in `Contracts`" to "A published query message". This narrows nothing in practice, because published queries live in `Contracts/Queries/` (line 116).

### §4 Pagination to §7 Cache, events, and storage

O-291 | KEPT | 207
O-292 | KEPT | 207
O-293 | CHANGED (widened) | 207 | subject 'A handler returning' became 'Returning': no longer limited to handlers
O-294 | KEPT | 207
O-295 | KEPT | 209
O-296 | KEPT | 209
O-297 | KEPT | 210
O-298 | KEPT | 210
O-299 | KEPT | 210
O-300 | KEPT | 210
O-301 | KEPT | 211
O-302 | KEPT | 211
O-303 | KEPT | 212
O-304 | KEPT | 212
O-305 | KEPT | 212
O-306 | KEPT | 212
O-307 | KEPT | 212
O-308 | KEPT | 212
O-309 | KEPT | 208
O-310 | KEPT | 208
O-311 | KEPT | 213
O-312 | KEPT | 213
O-313 | KEPT | 214
O-314 | KEPT | 214
O-315 | CHANGED (mark) | 215 | mark BE-119 now sits inside the last sub-bullet (key ring); this rule's sub-bullet/lead line carries no mark (OLD: one paragraph ending in the mark)
O-316 | CHANGED (mark) | 215 | mark BE-119 now sits inside the last sub-bullet (key ring); this rule's sub-bullet/lead line carries no mark (OLD: one paragraph ending in the mark)
O-317 | CHANGED (mark) | 216 | mark BE-119 now sits inside the last sub-bullet (key ring); this rule's sub-bullet/lead line carries no mark (OLD: one paragraph ending in the mark)
O-318 | CHANGED (mark) | 216 | mark BE-119 now sits inside the last sub-bullet (key ring); this rule's sub-bullet/lead line carries no mark (OLD: one paragraph ending in the mark)
O-319 | CHANGED (mark) | 216 | mark BE-119 now sits inside the last sub-bullet (key ring); this rule's sub-bullet/lead line carries no mark (OLD: one paragraph ending in the mark)
O-320 | CHANGED (mark) | 216 | mark BE-119 now sits inside the last sub-bullet (key ring); this rule's sub-bullet/lead line carries no mark (OLD: one paragraph ending in the mark)
O-321 | CHANGED (mark) | 216 | mark BE-119 now sits inside the last sub-bullet (key ring); this rule's sub-bullet/lead line carries no mark (OLD: one paragraph ending in the mark)
O-322 | CHANGED (mark) | 217 | mark BE-119 now sits inside the last sub-bullet (key ring); this rule's sub-bullet/lead line carries no mark (OLD: one paragraph ending in the mark)
O-323 | CHANGED (mark) | 217 | mark BE-119 now sits inside the last sub-bullet (key ring); this rule's sub-bullet/lead line carries no mark (OLD: one paragraph ending in the mark)
O-324 | KEPT | 218
O-325 | KEPT | 218
O-326 | KEPT | 218
O-327 | KEPT | 220
O-328 | KEPT | 224
O-329 | KEPT | 224
O-330 | KEPT | 226
O-331 | KEPT | 227
O-332 | KEPT | 227
O-333 | KEPT | 227
O-334 | KEPT | 227
O-335 | KEPT | 228
O-336 | KEPT | 228
O-337 | KEPT | 228
O-338 | KEPT | 229
O-339 | KEPT | 229
O-340 | KEPT | 229
O-341 | KEPT | 230
O-342 | AMBIGUOUS | 230 | 'resolve the attempt identity first' - 'first' can read as before opening the unit of work; OLD: opens the unit of work, then resolves
O-343 | KEPT | 230
O-344 | KEPT | 230
O-345 | AMBIGUOUS | 231 | 'Otherwise:' is now a sibling bullet of 'A command that persists...', so it can read as the else of persisting (i.e. non-persisting commands)
O-346 | KEPT | 231
O-347 | KEPT | 231
O-348 | KEPT | 231
O-349 | KEPT | 231
O-350 | KEPT | 232
O-351 | KEPT | 232
O-352 | KEPT | 232
O-353 | KEPT | 233
O-354 | KEPT | 233
O-355 | KEPT | 233
O-356 | KEPT | 233
O-357 | KEPT | 233
O-358 | AMBIGUOUS | 234 | 'a failure result it [the behavior] returns' became 'a returned failure result' - returned by whom is unstated
O-359 | KEPT | 234
O-360 | KEPT | 234
O-361 | MISSING | 234 | 'so a replay returns it' dropped; only the (core §4) cite remains
O-362 | KEPT | 234
O-363 | KEPT | 234
O-364 | KEPT | 235
O-365 | KEPT | 235
O-366 | KEPT | 239
O-367 | KEPT | 241
O-368 | KEPT | 241
O-369 | KEPT | 241
O-370 | KEPT | 242
O-371 | KEPT | 242
O-372 | KEPT | 242
O-373 | KEPT | 243
O-374 | KEPT | 243
O-375 | KEPT | 244
O-376 | KEPT | 245
O-377 | KEPT | 245
O-378 | AMBIGUOUS | 245 | 'its class is Internal' was a clause of the fingerprint phrase; now a separate sentence after 'The record stores...', so 'Its' can bind to the record
O-379 | KEPT | 244
O-380 | KEPT | 244
O-381 | KEPT | 246
O-382 | KEPT | 246
O-383 | KEPT | 247
O-384 | KEPT | 247
O-385 | KEPT | 248
O-386 | KEPT | 248
O-387 | KEPT | 250
O-388 | KEPT | 254
O-389 | KEPT | 254
O-390 | KEPT | 254
O-391 | KEPT | 255
O-392 | KEPT | 256
O-393 | KEPT | 256
O-394 | KEPT | 257
O-395 | KEPT | 257
O-396 | KEPT | 258
O-397 | CHANGED (mark) | 259 | BE-110 no longer on this bullet; it now sits only on the external-write sub-bullet (line 261)
O-398 | CHANGED (mark) | 259 | BE-110 no longer on this bullet; it now sits only on the external-write sub-bullet (line 261)
O-399 | KEPT | 261
O-400 | KEPT | 261
O-401 | KEPT | 261
O-402 | KEPT | 262
O-403 | KEPT | 262
O-404 | KEPT | 263
O-405 | KEPT | 264
O-406 | KEPT | 265
O-407 | KEPT | 266
O-408 | KEPT | 266
O-409 | KEPT | 266
O-410 | KEPT | 266
O-411 | KEPT | 270
O-412 | KEPT | 272
O-413 | KEPT | 273
O-414 | KEPT | 273
O-415 | KEPT | 274
O-416 | KEPT | 274
O-417 | KEPT | 275
O-418 | KEPT | 275
O-419 | KEPT | 275
O-420 | KEPT | 276
O-421 | KEPT | 276
O-422 | KEPT | 276
O-423 | KEPT | 277
O-424 | KEPT | 277
O-425 | KEPT | 281
O-426 | KEPT | 281
O-427 | KEPT | 285
O-428 | KEPT | 286
O-429 | KEPT | 287
O-430 | KEPT | 288
O-431 | KEPT | 290
O-432 | KEPT | 290
O-433 | KEPT | 291
O-434 | KEPT | 292
O-435 | KEPT | 293
O-436 | KEPT | 294
O-437 | KEPT | 294
O-438 | KEPT | 295
O-439 | KEPT | 295
O-440 | KEPT | 296
O-441 | KEPT | 297
O-442 | KEPT | 298
O-443 | KEPT | 299
O-444 | KEPT | 300
O-445 | KEPT | 301
O-446 | CHANGED (widened) | 303 | 'A free-standing Infrastructure service belonging to none of these' became 'Any other Infrastructure type'
O-447 | KEPT | 303
O-448 | KEPT | 303
O-449 | KEPT | 303
O-450 | AMBIGUOUS | 303 | 'the read types the DbContext exposes' became 'the read types it exposes' ('it' may bind to the query handler); 'in a public constructor or member' became 'publicly'
O-451 | AMBIGUOUS | 303 | separate sentence about the source-generated dispatcher naming each concrete handler folded into a parenthetical '(every concrete query handler, so each is public)' that can read as the whole scope of the generated-code exception
O-452 | KEPT | 303
O-453 | KEPT | 304
O-454 | KEPT | 304
O-455 | AMBIGUOUS | 304 | 'A new top-level folder there' lost 'there' (Infrastructure)
O-456 | KEPT | 304
O-457 | KEPT | 305
O-458 | KEPT | 309
O-459 | KEPT | 309
O-460 | KEPT | 310
O-461 | KEPT | 310
O-462 | KEPT | 310
O-463 | KEPT | 311
O-464 | KEPT | 311
O-465 | KEPT | 311
O-466 | KEPT | 311
O-467 | KEPT | 311
O-468 | KEPT | 311
O-469 | KEPT | 311
O-470 | KEPT | 311
O-471 | KEPT | 312
O-472 | KEPT | 313
O-473 | KEPT | 313
O-474 | KEPT | 314
O-475 | KEPT | 314
O-476 | KEPT | 314
O-477 | KEPT | 315
O-478 | KEPT | 315
O-479 | KEPT | 315
O-480 | KEPT | 315
O-481 | KEPT | 315
O-482 | KEPT | 309
O-483 | KEPT | 309
O-484 | KEPT | 319
O-485 | KEPT | 319
O-486 | KEPT | 319
O-487 | KEPT | 320
O-488 | KEPT | 320
O-489 | KEPT | 321
O-490 | KEPT | 321
O-491 | KEPT | 321
O-492 | KEPT | 322
O-493 | KEPT | 323
O-494 | KEPT | 323
O-495 | KEPT | 323
O-496 | KEPT | 324
O-497 | KEPT | 324
O-498 | KEPT | 325
O-499 | KEPT | 325
O-500 | KEPT | 326
O-501 | KEPT | 326
O-502 | KEPT | 326
O-503 | KEPT | 330
O-504 | KEPT | 330
O-505 | KEPT | 330
O-506 | KEPT | 330
O-507 | KEPT | 331
O-508 | KEPT | 331
O-509 | KEPT | 332
O-510 | KEPT | 332
O-511 | KEPT | 332
O-512 | KEPT | 332
O-513 | KEPT | 332
O-514 | KEPT | 333
O-515 | KEPT | 333
O-516 | KEPT | 333
O-517 | KEPT | 334
O-518 | KEPT | 334
O-519 | KEPT | 334
O-520 | KEPT | 335
O-521 | KEPT | 335
O-522 | KEPT | 335
O-523 | KEPT | 339
O-524 | KEPT | 339
O-525 | KEPT | 340
O-526 | KEPT | 341
O-527 | KEPT | 345
O-528 | KEPT | 349
O-529 | KEPT | 353
O-530 | KEPT | 353
O-531 | KEPT | 355-364
O-532 | KEPT | 355-364
O-533 | KEPT | 355-364
O-534 | KEPT | 355-364
O-535 | KEPT | 355-364
O-536 | KEPT | 355-364
O-537 | KEPT | 355-364
O-538 | KEPT | 355-364
O-539 | KEPT | 367
O-540 | KEPT | 367
O-541 | KEPT | 367
O-542 | KEPT | 367
O-543 | KEPT | 368
O-544 | KEPT | 369
O-545 | KEPT | 369
O-546 | KEPT | 369
O-547 | KEPT | 370
O-548 | KEPT | 370
O-549 | KEPT | 371
O-550 | KEPT | 371
O-551 | KEPT | 372
O-552 | KEPT | 373
O-553 | KEPT | 373
O-554 | KEPT | 373
O-555 | KEPT | 373
O-556 | KEPT | 374
O-557 | KEPT | 374
O-558 | KEPT | 374
O-559 | KEPT | 374
O-560 | KEPT | 374
O-561 | KEPT | 375
O-562 | KEPT | 375
O-563 | KEPT | 375
O-564 | KEPT | 376
O-565 | KEPT | 377
O-566 | KEPT | 377
O-567 | KEPT | 377
O-568 | KEPT | 377
O-569 | KEPT | 379
O-570 | KEPT | 379
O-571 | KEPT | 379
O-572 | KEPT | 379
O-573 | KEPT | 378
O-574 | KEPT | 378
O-575 | KEPT | 378
O-576 | KEPT | 378
O-577 | KEPT | 380
O-578 | KEPT | 380
O-579 | KEPT | 380
O-580 | KEPT | 380
O-581 | KEPT | 380
O-582 | KEPT | 380
O-583 | KEPT | 384
O-584 | KEPT | 386
O-585 | KEPT | 386
O-586 | KEPT | 386
O-587 | KEPT | 386
O-588 | KEPT | 387
O-589 | KEPT | 387
O-590 | KEPT | 387
O-591 | KEPT | 387
O-592 | KEPT | 387
O-593 | KEPT | 387
O-594 | KEPT | 387
O-595 | KEPT | 388
O-596 | KEPT | 388
O-597 | KEPT | 388
O-598 | KEPT | 389
O-599 | KEPT | 389
O-600 | KEPT | 389
O-601 | KEPT | 389
O-602 | KEPT | 395
O-603 | KEPT | 395
O-604 | KEPT | 395
O-605 | KEPT | 395
O-606 | KEPT | 395
O-607 | KEPT | 395
O-608 | KEPT | 395
O-609 | KEPT | 395
O-610 | KEPT | 396
O-611 | KEPT | 396
O-612 | KEPT | 396
O-613 | KEPT | 397
O-614 | KEPT | 397
O-615 | KEPT | 397
O-616 | KEPT | 397
O-617 | KEPT | 399-405
O-618 | KEPT | 408
O-619 | KEPT | 408
O-620 | KEPT | 412
O-621 | KEPT | 412
O-622 | KEPT | 412
O-623 | KEPT | 412
O-624 | KEPT | 412
O-625 | KEPT | 413
O-626 | KEPT | 413
O-627 | KEPT | 414
O-628 | KEPT | 414
O-629 | KEPT | 414
O-630 | KEPT | 415
O-631 | KEPT | 415
O-632 | KEPT | 416
O-633 | KEPT | 416
O-634 | KEPT | 417
O-635 | KEPT | 417
O-636 | KEPT | 417
O-637 | KEPT | 421
O-638 | KEPT | 425
O-639 | KEPT | 427
O-640 | KEPT | 427
O-641 | KEPT | 427
O-642 | KEPT | 428
O-643 | KEPT | 429
O-644 | KEPT | 429
O-645 | KEPT | 430
O-646 | KEPT | 430
O-647 | KEPT | 431
O-648 | KEPT | 432

#### Findings

Counts: KEPT 337, MISSING 1, CHANGED (widened) 2, CHANGED (mark) 11, AMBIGUOUS 7, WEAKENED 0. Total 358.

1. O-361 MISSING (NEW 234). OLD: "a failure result it returns is recorded after the rollback, in a transaction of its own on the module's context, so a replay returns it (core §4)". NEW: "record a returned failure result after the rollback, in its own transaction on the module's context (core §4)". The replay-returns-the-recorded-failure statement is gone.
2. O-293 CHANGED, widened (NEW 207). OLD: "A handler returning an unbounded collection, such as a `List<T>` or an `IEnumerable<T>`, is a defect." NEW: "Returning an unbounded collection, such as a `List<T>` or `IEnumerable<T>`, is a defect." With no subject, it now covers any returning code, such as repositories or helpers.
3. O-446 CHANGED, widened (NEW 303). OLD: "A free-standing Infrastructure service belonging to none of these is a defect." NEW: "Any other Infrastructure type is a defect." This agrees with the MUST in O-441, but the defect sentence's scope moves from "service" to "type".
4. O-397, O-398 CHANGED (mark) (NEW 259). OLD put "declares ... whether it persists aggregate state. Only a command that does opens the unit of work." in the same bullet that ends in BE-110. NEW puts these two sentences in their own bullet with no mark, and BE-110 sits only on the external-write sub-bullet at 261. Enforcement's BE-110 row starts with "A command declares whether it persists aggregate state".
5. O-315..O-323 CHANGED (mark) (NEW 215-217). OLD had one paragraph ending in BE-119. NEW splits it into a lead line and three sub-bullets, and BE-119 is inside the third (key ring). Enforcement's BE-119 row checks the codec (the first sub-bullet) and says the key ring is "reached by review". So the mark now visibly sits on the unenforced part. Suggest putting it on the lead line or on the codec sub-bullet.
6. O-342 AMBIGUOUS (230). OLD: "it opens the unit of work, then ... resolves the attempt identity". NEW: "open the unit of work. If the command declares a key, resolve the attempt identity first". "first" can read as before the unit of work.
7. O-345 AMBIGUOUS (231). OLD ran on as "Otherwise it runs the handler" after the replay check. NEW makes "Otherwise:" a sibling bullet of "A command that persists aggregate state", so it can read as the case of a non-persisting command.
8. O-358 AMBIGUOUS (234). OLD: "a failure result it returns" (the behavior). NEW: "a returned failure result". The returner is unstated: the handler or the behavior.
9. O-378 AMBIGUOUS (245). OLD: "...never a plain hash, and its class is `Internal`" (the fingerprint's). NEW: "...never a plain hash. Its class is `Internal`." As a separate sentence, "Its" can bind to "The record".
10. O-450 AMBIGUOUS (303). OLD: "the read types the `DbContext` exposes". NEW: "the read types it exposes", where "it" can be the query handler. Also, "in a public constructor or member" became "publicly".
11. O-451 AMBIGUOUS (303). OLD had its own sentence: "The source-generated dispatcher names each concrete handler in its generated registration, so every concrete query handler is public." NEW: "any type the dispatcher's generated code in `Api` must name (every concrete query handler, so each is public)". The parenthetical can read as the full list of what that exception covers.
12. O-455 AMBIGUOUS, minor (304). "A new top-level folder there" lost "there".

Minor wording, judged KEPT: O-390 lost "against this rule" ("do not count"), but the bullet's context fixes it. O-613 changed "The class MUST be emitted into the OpenAPI document" to "The OpenAPI document MUST carry the class", but line 397 keeps the emitter MUST. O-593/594 became "the declared value MUST be that period", which is equivalent. Nothing else in NEW §4 Pagination through §7 is a new obligation.

### §8 AI Agents to §16 Verification

O-649 | KEPT | 436
O-650 | KEPT | 436
O-651 | KEPT | 440
O-652 | KEPT | 440
O-653 | KEPT | 440
O-654 | KEPT | 444
O-655 | KEPT | 444
O-656 | KEPT | 444
O-657 | KEPT | 444
O-658 | KEPT | 445
O-659 | KEPT | 445
O-660 | KEPT | 445
O-661 | KEPT | 446
O-662 | KEPT | 446
O-663 | KEPT | 446
O-664 | KEPT | 446
O-665 | KEPT | 447
O-666 | KEPT | 448
O-667 | KEPT | 448
O-668 | KEPT | 449
O-669 | KEPT | 449
O-670 | KEPT | 448
O-671 | KEPT | 448
O-672 | KEPT | 448
O-673 | KEPT | 453
O-674 | KEPT | 457
O-675 | KEPT | 457
O-676 | KEPT | 457
O-677 | KEPT | 458
O-678 | KEPT | 464
O-679 | KEPT | 464
O-680 | KEPT | 464
O-681 | KEPT | 465
O-682 | KEPT | 465
O-683 | KEPT | 466
O-684 | KEPT | 466
O-685 | KEPT | 467
O-686 | KEPT | 467
O-687 | KEPT | 468
O-688 | KEPT | 469
O-689 | KEPT | 470
O-690 | KEPT | 471
O-691 | KEPT | 471
O-692 | KEPT | 471
O-693 | KEPT | 473
O-694 | KEPT | 477
O-695 | KEPT | 477
O-696 | KEPT | 477
O-697 | KEPT | 477
O-698 | KEPT | 477
O-699 | KEPT | 477
O-700 | KEPT | 478
O-701 | KEPT | 478
O-702 | KEPT | 479
O-703 | KEPT | 480
O-704 | KEPT | 481
O-705 | KEPT | 482
O-706 | KEPT | 483
O-707 | KEPT | 483
O-708 | KEPT | 484
O-709 | KEPT | 484
O-710 | KEPT | 485
O-711 | KEPT | 485
O-712 | KEPT | 489
O-713 | KEPT | 490
O-714 | KEPT | 490
O-715 | KEPT | 490
O-716 | KEPT | 490
O-717 | KEPT | 491
O-718 | KEPT | 491
O-719 | KEPT | 492
O-720 | KEPT | 492
O-721 | KEPT | 492
O-722 | KEPT | 492
O-723 | KEPT | 492
O-724 | KEPT | 492
O-725 | KEPT | 492
O-726 | KEPT | 492
O-727 | KEPT | 492
O-728 | KEPT | 492
O-729 | KEPT | 493
O-730 | KEPT | 494
O-731 | KEPT | 494
O-732 | KEPT | 495
O-733 | KEPT | 495
O-734 | KEPT | 495
O-735 | KEPT | 499
O-736 | KEPT | 499
O-737 | KEPT | 499
O-738 | KEPT | 499
O-739 | KEPT | 499
O-740 | KEPT | 499
O-741 | KEPT | 499
O-742 | KEPT | 500
O-743 | KEPT | 500
O-744 | KEPT | 501
O-745 | KEPT | 502
O-746 | KEPT | 503
O-747 | KEPT | 504
O-748 | KEPT | 505
O-749 | KEPT | 506
O-750 | KEPT | 506
O-751 | KEPT | 510
O-752 | KEPT | 510
O-753 | KEPT | 510
O-754 | KEPT | 511
O-755 | KEPT | 511
O-756 | KEPT | 512
O-757 | KEPT | 512
O-758 | KEPT | 512
O-759 | KEPT | 514
O-760 | KEPT | 515
O-761 | KEPT | 515
O-762 | KEPT | 516
O-763 | KEPT | 516
O-764 | KEPT | 517
O-765 | KEPT | 517
O-766 | KEPT | 518
O-767 | KEPT | 518
O-768 | KEPT | 519
O-769 | KEPT | 519
O-770 | KEPT | 519
O-771 | KEPT | 520
O-772 | KEPT | 520
O-773 | KEPT | 520
O-774 | KEPT | 520
O-775 | KEPT | 524
O-776 | KEPT | 528
O-777 | KEPT | 528
O-778 | KEPT | 528
O-779 | KEPT | 529
O-780 | KEPT | 529
O-781 | KEPT | 529
O-782 | CHANGED | 530 | OLD closed list "The framework endpoints - probes, MapHub and the OpenAPI document - are"; NEW drops "The" and the closing "and": "Framework endpoints - probes, MapHub, the OpenAPI document - are", which can read as examples of an open set exempt from BE-80/BE-81
O-783 | KEPT | 530
O-784 | KEPT | 530
O-785 | KEPT | 530
O-786 | KEPT | 530
O-787 | KEPT | 530
O-788 | KEPT | 531
O-789 | KEPT | 531
O-790 | KEPT | 531
O-791 | KEPT | 532
O-792 | KEPT | 532
O-793 | KEPT | 532
O-794 | KEPT | 532
O-795 | KEPT | 533
O-796 | KEPT | 534
O-797 | KEPT | 534
O-798 | KEPT | 534
O-799 | KEPT | 534
O-800 | KEPT | 534
O-801 | KEPT | 534
O-802 | KEPT | 534
O-803 | KEPT | 536
O-804 | KEPT | 536
O-805 | KEPT | 536
O-806 | KEPT | 537
O-807 | KEPT | 537
O-808 | KEPT | 537
O-809 | KEPT | 538
O-810 | KEPT | 538
O-811 | KEPT | 539
O-812 | KEPT | 541
O-813 | KEPT | 541
O-814 | KEPT | 541
O-815 | KEPT | 543
O-816 | KEPT | 543
O-817 | KEPT | 542
O-818 | KEPT | 542
O-819 | KEPT | 544
O-820 | KEPT | 544
O-821 | KEPT | 544
O-822 | KEPT | 545
O-823 | KEPT | 545
O-824 | KEPT | 545
O-825 | KEPT | 547
O-826 | KEPT | 547
O-827 | KEPT | 548
O-828 | KEPT | 548
O-829 | KEPT | 548
O-830 | KEPT | 548
O-831 | KEPT | 548
O-832 | KEPT | 549
O-833 | KEPT | 549
O-834 | KEPT | 550
O-835 | KEPT | 554
O-836 | KEPT | 554
O-837 | KEPT | 556
O-838 | KEPT | 556
O-839 | KEPT | 556
O-840 | KEPT | 556
O-841 | KEPT | 556
O-842 | KEPT | 557
O-843 | KEPT | 558
O-844 | WEAKENED | 559 | OLD "every outcome MUST have a test proving its translation" (a test per outcome); NEW "MUST have a test proving: ... each outcome's translation" (one test covering all outcomes satisfies it)
O-845 | KEPT | 560
O-846 | KEPT | 560
O-847 | KEPT | 561
O-848 | KEPT | 561
O-849 | KEPT | 562
O-850 | KEPT | 562
O-851 | KEPT | 563
O-852 | KEPT | 563
O-853 | KEPT | 563
O-854 | KEPT | 565
O-855 | KEPT | 565
O-856 | KEPT | 565
O-857 | KEPT | 565
O-858 | KEPT | 566
O-859 | KEPT | 566
O-860 | KEPT | 567
O-861 | KEPT | 568
O-862 | KEPT | 568
O-863 | KEPT | 568
O-864 | CHANGED | 570 | text kept; test id BE-118 now sits only on the third Audit sub-bullet (572); this sub-bullet carries no id, while NEW elsewhere gives each sub-bullet its own id
O-865 | CHANGED | 570 | as O-864: no BE-118 on this sub-bullet
O-866 | CHANGED | 571 | text kept; no BE-118 on this sub-bullet (id only on 572)
O-867 | KEPT | 572
O-868 | KEPT | 572
O-869 | CHANGED | 574 | text kept; BE-124 now only on the second Pagination sub-bullet (575); this one carries no id
O-870 | CHANGED | 574 | as O-869: no BE-124 on this sub-bullet ("in another test" kept)
O-871 | KEPT | 575
O-872 | CHANGED | 577 | OLD "MUST have a test proving X (BE-125), and a test proving Y ..., and a test proving Z ..., and a test proving W" (four tests); NEW "MUST have a test proving each of:" admits one test proving all four
O-873 | CHANGED | 578 | as O-872 (separate-test requirement now ambiguous)
O-874 | CHANGED | 579 | as O-872
O-875 | CHANGED | 580 | as O-872
O-876 | KEPT | 581
O-877 | KEPT | 581
O-878 | KEPT | 581
O-879 | KEPT | 581
O-880 | KEPT | 582
O-881 | KEPT | 582
O-882 | KEPT | 582
O-883 | KEPT | 583
O-884 | KEPT | 583
O-885 | KEPT | 583
O-886 | KEPT | 585
O-887 | KEPT | 586
O-888 | KEPT | 586
O-889 | KEPT | 587
O-890 | KEPT | 587
O-891 | KEPT | 588
O-892 | KEPT | 588
O-893 | KEPT | 588
O-894 | KEPT | 589
O-895 | KEPT | 589
O-896 | KEPT | 589
O-897 | KEPT | 590
O-898 | KEPT | 591
O-899 | KEPT | 591
O-900 | KEPT | 591
O-901 | KEPT | 595
O-902 | KEPT | 596
O-903 | KEPT | 596
O-904 | KEPT | 597
O-905 | KEPT | 598
O-906 | KEPT | 598
O-907 | KEPT | 598
O-908 | KEPT | 599
O-909 | KEPT | 600
O-910 | KEPT | 600
O-911 | KEPT | 600
O-912 | KEPT | 601
O-913 | KEPT | 601
O-914 | KEPT | 601
O-915 | KEPT | 601
O-916 | KEPT | 601
O-917 | KEPT | 601
O-918 | KEPT | 601
O-919 | KEPT | 601
O-920 | KEPT | 602
O-921 | KEPT | 603
O-922 | KEPT | 607
O-923 | KEPT | 607
O-924 | KEPT | 607

#### Findings

Counts: KEPT 265, CHANGED 10, WEAKENED 1, MISSING 0 (276 items, O-649..O-924).

##### Non-KEPT items

1. O-782 (NEW 530) CHANGED - closed list may now read as open.
   OLD: "The framework endpoints - the readiness and liveness probes (§10), a hub's `MapHub` (real-time annex) and the OpenAPI document - are mapped by the framework"
   NEW: "Framework endpoints - the readiness and liveness probes (§10), a hub's `MapHub` (real-time annex), the OpenAPI document - are mapped by the framework"
   Losing "The" and the closing "and" lets the three read as examples, widening what may sit outside BE-80/BE-81. Fix: restore "The framework endpoints - ..., ... and the OpenAPI document -".

2. O-844 (NEW 559) WEAKENED - test granularity.
   OLD: "Where a handler translates outcomes that are types, every outcome MUST have a test proving its translation (BE-141)."
   NEW: "MUST have a test proving: ... where a handler translates outcomes that are types: each outcome's translation (BE-141)."
   OLD gives every outcome a test; NEW is met by one test. enforcement.md BE-141 says "every outcome's translation has a test".

3. O-872..O-875 (NEW 576-580) CHANGED - four tests merged into one possible test.
   OLD: "MUST have a test proving a replayed key ... (BE-125), and a test proving two concurrent attempts ... (BE-29), and a test proving the same key ... `422` (BE-116), and a test proving a request that carries no key ... `400` (BE-137)."
   NEW: "every command declaring a key MUST have a test proving each of:" followed by the four items.
   The text still covers all four cases, but "a test proving each of" allows a single test.

4. O-864, O-865, O-866 (NEW 570-571) CHANGED - test id moved.
   OLD: the whole Audit bullet ends in "(BE-118)".
   NEW: "(BE-118)" appears only on the third sub-bullet (572). The success/failure and non-persisting sub-bullets carry no id. Elsewhere in NEW §14 (Authorization, Idempotency, Correlation) every sub-bullet has its own id, so these read as un-ided. enforcement.md BE-118 covers all three.

5. O-869, O-870 (NEW 574) CHANGED - same problem with BE-124.
   OLD: one sentence ends in "(BE-124)" and covers both the paginated-query and bounded-collection tests.
   NEW: "(BE-124)" appears only on the bounded-collection sub-bullet (575). enforcement.md BE-124 covers both.

##### Wording notes (judged KEPT, recorded for review)

- NEW 446 "An operation declaration MAY name a stricter policy". OLD had "the operation declaration". The meaning is unchanged.
- NEW 477-478: ⚙ BE-93 still follows "A handler MUST NOT reference `ILogger`." as in OLD. The Serilog sentences moved to their own bullet with no mark. The checklist tags O-694..O-700 BE-93 because they shared the bullet, but enforcement.md BE-93 covers only the handler rule, so no mark was lost.
- NEW 514-517: ⚙ BE-97 stays on "A secret MUST NOT appear in any committed file." The test-key exemption (516) and the credential-reference rule (517), which enforcement.md BE-97 also covers, now sit in separate unmarked sub-bullets. In OLD they shared the marked bullet.
- NEW 557 / 564 / 569 / 573 / 584: "MUST have a test proving:" has no subject. Each "for every X:" sub-bullet supplies it, so this is readable but looser than OLD's "every X MUST have a test".
- NEW 558 "not only that one succeeds". OLD had "not only one proving success". The substance is the same.
- NEW 601 "Declare it with a marker attribute on the method, declared once in `SharedKernel`". "declared once" could attach to the method. OLD: "an attribute the solution declares once in `SharedKernel`".
- NEW 548 "a non-persisting command". OLD and the rest of NEW use the term "a command that does not persist" (§4).
- NEW 519 "Where telemetry needs the environment's name". OLD had "belongs in telemetry". Equivalent.

##### New or widened obligations, rule conflicts

None found in §8-§16. Every NEW sentence in this range traces to OLD text, and no new combination of rules lacks a compliant answer.

## Checklist

Format: `O-<n> | §<section> | obligation | ⚙ ids or - | source: "<first words of source sentence>"`
Items marked "(illustration)" record that a fenced code block exists; items marked "(from illustration comment)" record normative content that appears only inside a fenced block's comments or field names.
In §14 the ids column records the rule id each Testing entry names, tagged "(test id)"; those entries carry no ⚙ mark in the source. All source quotes were machine-checked as verbatim substrings of the source file.

#### Front matter (description)

O-1 | §front-matter | The law's scope: project references, domain and application layers, EF Core persistence, authorization, data classification, AI agents, quotas, observability, configuration, real-time, and the API layer (a .NET server stack law) | - | source: "The .NET server stack law - project references, domain and"
O-2 | §front-matter | This stack law is subordinate to the core law `/rancho:law` | - | source: "Subordinate to the core law in /rancho:law."
O-3 | §front-matter | Read this law in full | - | source: "Read in full, after the core law, before writing, reviewing,"
O-4 | §front-matter | Read it after the core law | - | source: "Read in full, after the core law, before writing, reviewing,"
O-5 | §front-matter | Read it before writing, before reviewing, and before placing code | - | source: "Read in full, after the core law, before writing, reviewing,"
O-6 | §front-matter | Scope: applies in a tree whose `CLAUDE.md` names `Stack law: /rancho-dotnet:law` | - | source: "Read in full, after the core law, before writing, reviewing,"

#### .NET Stack Law (preamble)

O-7 | §preamble | The core law `/rancho:law` governs (it takes precedence) | - | source: "The core law, `/rancho:law`, governs: read it first."
O-8 | §preamble | Read the core law first | - | source: "The core law, `/rancho:law`, governs: read it first."
O-9 | §preamble | This stack law is the stack law of core §2 and adds the .NET rules | - | source: "This stack law (core §2) adds the .NET rules and carries"
O-10 | §preamble | This stack law carries the core law's mechanism in every tree bound to it | - | source: "This stack law (core §2) adds the .NET rules and carries"
O-11 | §preamble | Definition: "Core §N" means §N of the core law | - | source: "Core §N is §N of the core law."
O-12 | §preamble | Definition: `⚙` means the rule can be made to fail the build (core §1) | - | source: "`⚙` = can be made to fail the build (core §1)."
O-13 | §preamble | Every `⚙` in this file is inventoried in `references/enforcement.md` | - | source: "Every `⚙` here, and every core rule this role carries,"
O-14 | §preamble | Every core rule this role carries is inventoried in `references/enforcement.md` | - | source: "Every `⚙` here, and every core rule this role carries,"
O-15 | §preamble | Consult `references/enforcement.md` by rule id rather than reading it whole | - | source: "Every `⚙` here, and every core rule this role carries,"
O-16 | §preamble | There are three annexes, in `references/` beside the inventory | - | source: "Three annexes, beside the inventory in `references/`, hold rules that"
O-17 | §preamble | An annex's rules bind a tree only once the tree meets that annex's trigger | - | source: "Three annexes, beside the inventory in `references/`, hold rules that"
O-18 | §preamble | `annex-agents.md` trigger: the first code that sends data to a model | - | source: "`annex-agents.md`: the first code that sends data to a model"
O-19 | §preamble | `annex-agents.md` trigger also: the first code that exposes an operation to a model as a tool | - | source: "`annex-agents.md`: the first code that sends data to a model"
O-20 | §preamble | `annex-agents.md` trigger examples: an agent manifest, `Microsoft.Agents.AI`, an MCP server, Semantic Kernel, a direct model client, or any other | - | source: "`annex-agents.md`: the first code that sends data to a model"
O-21 | §preamble | `annex-caching.md` trigger: the first code that reads or writes a cache | - | source: "`annex-caching.md`: the first code that reads or writes a cache"
O-22 | §preamble | `annex-caching.md` trigger examples: a cache decorator, `IMemoryCache`, `IDistributedCache`, `HybridCache`, output caching, Redis, or any other | - | source: "`annex-caching.md`: the first code that reads or writes a cache"
O-23 | §preamble | `annex-realtime.md` trigger: the first code that pushes messages to a client over a persistent connection | - | source: "`annex-realtime.md`: the first code that pushes messages to a client"
O-24 | §preamble | `annex-realtime.md` trigger examples: a SignalR hub, server-sent events, a raw WebSocket, or any other | - | source: "`annex-realtime.md`: the first code that pushes messages to a client"
O-25 | §preamble | Each annex is named in this file where its rules would otherwise stand | - | source: "Each is named where its rules would otherwise stand."
O-26 | §preamble | Once the tree meets a trigger, the annex binds the whole tree (not only the triggering code) | - | source: "Once the tree meets a trigger, the annex binds the whole"
O-27 | §preamble | A triggered annex binds exactly as this file does (same force) | - | source: "Once the tree meets a trigger, the annex binds the whole"
O-28 | §preamble | A tree is bound to this stack law by one line in the `CLAUDE.md` at its root | - | source: "A tree is bound to this stack law by one line"
O-29 | §preamble | The binding line is exactly `Stack law: /rancho-dotnet:law` (fenced block) | - | source: "Stack law: /rancho-dotnet:law"
O-30 | §preamble | If `/rancho:law` is not available in the session, stop | - | source: "If `/rancho:law` is not available in the session, stop and"
O-31 | §preamble | If `/rancho:law` is not available, say so | - | source: "If `/rancho:law` is not available in the session, stop and"
O-32 | §preamble | This file alone is not the law (core law is required with it) | - | source: "This file alone is not the law."

#### Role

O-33 | §Role | The role of a tree bound here is `server` | - | source: "`server`. A tree bound here publishes the API contract (core"
O-34 | §Role | A tree bound here publishes the API contract (core §4) | - | source: "A tree bound here publishes the API contract (core §4),"
O-35 | §Role | A tree bound here owns its modules | - | source: "A tree bound here publishes the API contract (core §4),"
O-36 | §Role | A tree bound here owns its modules' persistence | - | source: "A tree bound here publishes the API contract (core §4),"
O-37 | §Role | A tree bound here owns every egress boundary in core §8 | - | source: "A tree bound here publishes the API contract (core §4),"

#### Core map

O-38 | §Core map | Core §3 Module Boundary Law is carried by §2 Project Reference Law · §4 Transactions and consistency, Integration event contracts · §5 Persistence | - | source: "| §3 Module Boundary Law | §2 Project Reference Law"
O-39 | §Core map | Core §4 API Contract is carried by §13 API Layer · §4 Pagination, Idempotency · §9 Transport | - | source: "| §4 API Contract | §13 API Layer"
O-40 | §Core map | Core §5 Validation is carried by §4 Pipeline behaviors | - | source: "| §5 Validation | §4 Pipeline behaviors |"
O-41 | §Core map | Core §6 Error Contract is carried by §2 (`Result` placement) · §3 Aggregates · §4 Commands and Queries, Pipeline behaviors · §5 Persistence · §13 API Layer · §15 Forbidden | - | source: "| §6 Error Contract | §2 (`Result` placement) · §3 Aggregates"
O-42 | §Core map | Core §7 Security Baseline is carried by §6 Authorization · §8 AI Agents · §10 Observability (Audit trail) · §11 Configuration and Secrets · §13 API Layer | - | source: "| §7 Security Baseline | §6 Authorization · §8 AI Agents"
O-43 | §Core map | Core §8 Data Boundaries is carried by §7 Data Classification · §5 Read-side caching · §12 Real-time | - | source: "| §8 Data Boundaries | §7 Data Classification · §5 Read-side caching"
O-44 | §Core map | Core §9 Observability is carried by §10 Observability | - | source: "| §9 Observability | §10 Observability |"
O-45 | §Core map | Core §10 Verification is carried by §14 Testing · §16 Verification · `references/enforcement.md` | - | source: "| §10 Verification | §14 Testing · §16 Verification"
O-46 | §Core map | Gaps between core and this stack law: none | - | source: "Gaps: none."

#### Layout

O-47 | §Layout | Layout paths are relative to the tree root | - | source: "Paths are relative to the tree root - the directory whose"
O-48 | §Layout | Definition: the tree root is the directory whose `CLAUDE.md` binds this stack law | - | source: "Paths are relative to the tree root - the directory whose"
O-49 | §Layout | (illustration) Fenced layout tree exists, first line `src/` | - | source: "src/"
O-50 | §Layout | Modules live at `src/Modules/<Module>/` | - | source: "Modules/<Module>/"
O-51 | §Layout | `<Module>.Domain/` may use only the BCL and `Microsoft.Extensions.Compliance.Abstractions` | - | source: "<Module>.Domain/          BCL and `Microsoft.Extensions.Compliance.Abstractions` only."
O-52 | §Layout | `<Module>.Domain/` has no other framework reference | - | source: "No other framework reference."
O-53 | §Layout | `<Module>.Application/` holds commands, queries, handlers, behaviors | - | source: "<Module>.Application/     Commands, queries, handlers, behaviors."
O-54 | §Layout | `<Module>.Infrastructure/` holds EF Core, adapters, `DbContext` | - | source: "<Module>.Infrastructure/  EF Core, adapters, DbContext."
O-55 | §Layout | `<Module>.Contracts/` is the module's public surface | - | source: "<Module>.Contracts/       Public surface."
O-56 | §Layout | `<Module>.Contracts/` is the only project other modules may reference | - | source: "The only project other modules may reference."
O-57 | §Layout | `src/Shared/SharedKernel/` holds base types only | - | source: "Shared/SharedKernel/        Base types only."
O-58 | §Layout | `SharedKernel` holds no business rules | - | source: "No business rules."
O-59 | §Layout | `src/Api/` holds the composition root, endpoints, and auth wiring | - | source: "Api/                        Composition root, endpoints, auth wiring."
O-60 | §Layout | `tests/` sits at the tree root beside `src/` | - | source: "tests/"
O-61 | §Layout | §2 states what each project may reference | - | source: "§2 states what each project may reference."

#### 1. Stack

O-62 | §1 | Stack: .NET 10 (LTS) | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-63 | §1 | Stack: EF Core | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-64 | §1 | Stack: SQL Server | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-65 | §1 | Stack: Redis | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-66 | §1 | Stack: SignalR | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-67 | §1 | Stack: Mediator 3.x, source-generated | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-68 | §1 | Stack: FluentValidation | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-69 | §1 | Stack: Serilog | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-70 | §1 | Stack: Microsoft.Extensions.Compliance | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-71 | §1 | Stack: OpenTelemetry | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-72 | §1 | Stack: Azure Key Vault | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-73 | §1 | Stack: Microsoft Agent Framework (governed by the agents annex) | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-74 | §1 | Stack: xUnit v3 | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-75 | §1 | Stack: NSubstitute | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-76 | §1 | Stack: Shouldly | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-77 | §1 | Stack: Testcontainers | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-78 | §1 | Stack: ArchUnitNET | - | source: ".NET 10 (LTS) · EF Core · SQL Server · Redis"
O-79 | §1 | `TreatWarningsAsErrors` is enabled solution-wide and MUST stay enabled | - | source: "`TreatWarningsAsErrors` and `Nullable` are enabled solution-wide and MUST stay"
O-80 | §1 | `Nullable` is enabled solution-wide and MUST stay enabled | - | source: "`TreatWarningsAsErrors` and `Nullable` are enabled solution-wide and MUST stay"
O-81 | §1 | Suppressing a warning requires a comment naming the rule | - | source: "Suppressing a warning requires a comment naming the rule and"
O-82 | §1 | Suppressing a warning requires the comment to give the reason | - | source: "Suppressing a warning requires a comment naming the rule and"
O-83 | §1 | Where the suppressed warning enforces a rule of the law, the reason is its exceptions-register entry's id (core §11) | - | source: "Suppressing a warning requires a comment naming the rule and"
O-84 | §1 | Verify third-party licenses before pinning | - | source: "Verify third-party licenses before pinning."

#### 2. Project Reference Law

O-85 | §2 | Four facts decide every reference | - | source: "Four facts decide every reference. The table below is their"
O-86 | §2 | The reference table is the consequence of the four facts, not a second set of rules | - | source: "The table below is their consequence, not a second set"
O-87 | §2 | Definition: a shared project is `SharedKernel` or `Platform.*` | - | source: "A **shared** project - `SharedKernel`, `Platform.*` - MUST NOT reference"
O-88 | §2 | A shared project MUST NOT reference a module | - | source: "A **shared** project - `SharedKernel`, `Platform.*` - MUST NOT reference"
O-89 | §2 | A shared project MUST NOT reference `Api` | - | source: "A **shared** project - `SharedKernel`, `Platform.*` - MUST NOT reference"
O-90 | §2 | A module reaches another module only through that module's `Contracts` | - | source: "A module reaches another module only through that module's `Contracts`."
O-91 | §2 | Inside a module, dependencies point inward: `Infrastructure` to `Application` to `Domain` and `Contracts` | - | source: "Inside a module, dependencies point inward: `Infrastructure` to `Application` to"
O-92 | §2 | `Domain` and `Contracts` are peers | - | source: "`Domain` and `Contracts` are peers and MUST NOT reference each"
O-93 | §2 | `Domain` MUST NOT reference `Contracts` | - | source: "`Domain` and `Contracts` are peers and MUST NOT reference each"
O-94 | §2 | `Contracts` MUST NOT reference `Domain` | - | source: "`Domain` and `Contracts` are peers and MUST NOT reference each"
O-95 | §2 | `Api` composes | - | source: "`Api` composes; nothing references `Api`."
O-96 | §2 | Nothing references `Api` | - | source: "`Api` composes; nothing references `Api`."
O-97 | §2 | Table row `SharedKernel`: may reference no projects; may declare only the BCL and `Microsoft.Extensions.Compliance.Abstractions` | - | source: "| `SharedKernel` | none | the BCL, and `Microsoft.Extensions.Compliance.Abstractions` |"
O-98 | §2 | Table row `<Module>.Domain`: may reference `SharedKernel`; may declare only the BCL and `Microsoft.Extensions.Compliance.Abstractions` | - | source: "| `<Module>.Domain` | `SharedKernel` | the BCL, and"
O-99 | §2 | Table row `Platform.Application`: may reference `SharedKernel`; may declare dispatch, validation, authorization and logging abstractions - `Mediator.Abstractions`, `FluentValidation`, `Microsoft.AspNetCore.Authorization`; `Microsoft.Extensions.*.Abstractions` | - | source: "| `Platform.Application` | `SharedKernel` | dispatch, validation, authorization and logging"
O-100 | §2 | Table row `<Module>.Contracts`: may reference `SharedKernel`, `Platform.Application`; declares no packages of its own | - | source: "| `<Module>.Contracts` | `SharedKernel`, `Platform.Application` | none of its own"
O-101 | §2 | Table row `<Module>.Application`: may reference own Domain + Contracts, other modules' Contracts, `Platform.Application`; may declare validation and `Microsoft.Extensions.*.Abstractions` | - | source: "| `<Module>.Application` | own Domain + Contracts, other modules' Contracts,"
O-102 | §2 | Table row `Platform.Infrastructure`: may reference `SharedKernel`, `Platform.Application`; may declare EF Core, messaging, telemetry instrumentation, ASP.NET Core Data Protection, ASP.NET Core authorization, the SDK of any external system it adapts, `Microsoft.Extensions.*.Abstractions`, `Microsoft.Extensions.Options` with its configuration extensions | - | source: "| `Platform.Infrastructure` | `SharedKernel`, `Platform.Application` | EF Core, messaging,"
O-103 | §2 | Table row `<Module>.Infrastructure`: may reference own Application, Domain, Contracts, `Platform.Infrastructure`; may declare EF Core, messaging, ASP.NET Core authorization, the SDK of any external system it adapts, `Microsoft.Extensions.*.Abstractions`, `Microsoft.Extensions.Options` with its configuration extensions (no telemetry instrumentation, no Data Protection) | - | source: "| `<Module>.Infrastructure` | own Application, Domain, Contracts, `Platform.Infrastructure` |"
O-104 | §2 | Table row `Api`: may reference any `*.Contracts`, any `<Module>.Application`, any `<Module>.Infrastructure`, `Platform.Infrastructure`, `Platform.Application`, `SharedKernel`; may declare ASP.NET Core, hosting, OpenAPI, the dispatcher's source generator, telemetry exporters, the logging provider and `Microsoft.Extensions.Telemetry` with `Microsoft.Extensions.Compliance.Redaction`, EF Core design-time tooling as a private asset; never a package an Infrastructure project declares | - | source: "| `Api` | any `*.Contracts`, any `<Module>.Application`, any `<Module>.Infrastructure`,"
O-105 | §2 | Table row `<Anything>.Tests`: may reference any project; may declare test packages | - | source: "| `<Anything>.Tests` | any project | test packages |"
O-106 | §2 | Every project MUST declare its kind as an MSBuild property | BE-01 | source: "Every project MUST declare its kind as an MSBuild property"
O-107 | §2 | The kind's value MUST be one of the table's rows | BE-01 | source: "Every project MUST declare its kind as an MSBuild property"
O-108 | §2 | A project whose kind is missing, misspelled, or absent from the table may reference nothing | BE-01 | source: "A project whose kind is missing, misspelled, or absent from"
O-109 | §2 | A project whose kind is missing, misspelled, or absent from the table may be referenced by nothing | BE-01 | source: "A project whose kind is missing, misspelled, or absent from"
O-110 | §2 | Adding a kind of project means adding a row to the table, derived from the four facts | BE-01 | source: "A project whose kind is missing, misspelled, or absent from"
O-111 | §2 | A test project MUST NOT be referenced by a project of any other kind | - | source: "A test project MUST NOT be referenced by a project"
O-112 | §2 | A fixture built to prove a check fails MUST NOT appear in the solution file | - | source: "A fixture built to prove a check fails MUST NOT"
O-113 | §2 | A fixture built to prove a check fails MUST NOT appear under the production source root | - | source: "A fixture built to prove a check fails MUST NOT"
O-114 | §2 | The table is an allowlist over direct `ProjectReference` entries, not over the transitive closure | BE-01 | source: "The table is an allowlist over **direct** `ProjectReference` entries, not"
O-115 | §2 | A project MAY declare a reference it could also reach transitively | BE-01 | source: "A project MAY declare a reference it could also reach"
O-116 | §2 | A project MUST NOT declare a reference its row omits | BE-01 | source: "A project MAY declare a reference it could also reach"
O-117 | §2 | Definition: an analyzer is a package or project reference that contributes analyzers only, and no assembly the project compiles against or runs with | - | source: "An analyzer - a package or project reference that contributes"
O-118 | §2 | An analyzer is admitted in every project | - | source: "An analyzer - a package or project reference that contributes"
O-119 | §2 | An analyzer is outside the reference table | - | source: "An analyzer - a package or project reference that contributes"
O-120 | §2 | A package reference qualifies as an analyzer with `IncludeAssets="analyzers"` and `PrivateAssets="all"` | - | source: "A package reference qualifies with `IncludeAssets="analyzers"` and `PrivateAssets="all"`;"
O-121 | §2 | A project reference qualifies as an analyzer with `OutputItemType="Analyzer"` and `ReferenceOutputAssembly="false"` | - | source: "A package reference qualifies with `IncludeAssets="analyzers"` and `PrivateAssets="all"`;"
O-122 | §2 | The Domain project MUST NOT reference EF Core | BE-02 | source: "The Domain project MUST NOT reference EF Core, Mediator, ASP.NET,"
O-123 | §2 | The Domain project MUST NOT reference Mediator | BE-02 | source: "The Domain project MUST NOT reference EF Core, Mediator, ASP.NET,"
O-124 | §2 | The Domain project MUST NOT reference ASP.NET | BE-02 | source: "The Domain project MUST NOT reference EF Core, Mediator, ASP.NET,"
O-125 | §2 | The Domain project MUST NOT reference `IHttpContextAccessor` | BE-02 | source: "The Domain project MUST NOT reference EF Core, Mediator, ASP.NET,"
O-126 | §2 | The Domain project MUST NOT reference any application type | BE-02 | source: "The Domain project MUST NOT reference EF Core, Mediator, ASP.NET,"
O-127 | §2 | `Platform.*` carries the dispatch pipeline | - | source: "`Platform.*` carries the dispatch pipeline - `Result`, the message abstractions,"
O-128 | §2 | `Platform.*` carries `Result` | - | source: "`Platform.*` carries the dispatch pipeline - `Result`, the message abstractions,"
O-129 | §2 | `Platform.*` carries the message abstractions | - | source: "`Platform.*` carries the dispatch pipeline - `Result`, the message abstractions,"
O-130 | §2 | `Platform.*` carries the solution-wide page shape and the cursor type | - | source: "`Platform.*` carries the dispatch pipeline - `Result`, the message abstractions,"
O-131 | §2 | `Platform.*` carries the behaviors | - | source: "`Platform.*` carries the dispatch pipeline - `Result`, the message abstractions,"
O-132 | §2 | `Result` lives in `Platform.Application`, not in `SharedKernel` | - | source: "`Result` lives in `Platform.Application`, not in `SharedKernel`."
O-133 | §2 | `Contracts` references `Platform.Application` (reason: published query names its `Result` response and dispatch abstraction) | - | source: "`Contracts` references `Platform.Application` because a published query message MUST"
O-134 | §2 | A published query message MUST name its `Result` response | - | source: "`Contracts` references `Platform.Application` because a published query message MUST"
O-135 | §2 | A published query message MUST name the abstraction it is dispatched by | - | source: "`Contracts` references `Platform.Application` because a published query message MUST"
O-136 | §2 | Nothing else from `Platform.Application` may appear in a contract's public surface | - | source: "Nothing else from `Platform.Application` may appear in a contract's public"
O-137 | §2 | Exception: the page and cursor types a published collection query returns may appear in a contract's public surface | - | source: "Nothing else from `Platform.Application` may appear in a contract's public"
O-138 | §2 | `Api` names only the registration entry points of Infrastructure projects | BE-105, BE-106 | source: "`Api` names only the registration entry points of Infrastructure projects."
O-139 | §2 | Each module's Infrastructure exposes exactly one registration entry point, `Add<Module>Module(IServiceCollection, IConfiguration)` | BE-105, BE-106 | source: "Each module's Infrastructure exposes exactly one, `Add<Module>Module(IServiceCollection, IConfiguration)`."
O-140 | §2 | `Platform.Infrastructure` exposes one `Add<Feature>` extension per platform feature, such as the outbox or correlation logging | BE-105, BE-106 | source: "`Platform.Infrastructure` exposes one `Add<Feature>` extension per platform feature, such"
O-141 | §2 | `Api` MUST NOT name any other Infrastructure type - no client, credential, `DbContext`, repository, adapter, SDK type, or any other | BE-105, BE-106 | source: "`Api` MUST NOT name any other Infrastructure type - no"
O-142 | §2 | After startup, `Api` MUST NOT name or invoke an Infrastructure type directly | BE-105, BE-106 | source: "`Api` MUST NOT name any other Infrastructure type - no"
O-143 | §2 | Dispatch reaches an Infrastructure handler through the dispatcher, not through `Api` | BE-105, BE-106 | source: "`Api` MUST NOT name any other Infrastructure type - no"
O-144 | §2 | Each Infrastructure project builds and registers its own clients | BE-105, BE-106 | source: "Each Infrastructure project builds and registers its own clients, including"
O-145 | §2 | Each Infrastructure project builds and registers its own `DbContext` | BE-105, BE-106 | source: "Each Infrastructure project builds and registers its own clients, including"
O-146 | §2 | Exception: code the dispatcher's source generator emits into `Api` is written by the generator, not in `Api`, and is outside this rule | - | source: "Code the dispatcher's source generator emits into `Api` is written"
O-147 | §2 | Definition: the composition root is `Api` together with the registration entry points it calls | - | source: "The **composition root** is `Api` together with the registration entry"
O-148 | §2 | Where the law confines something to the composition root, both `Api` and the registration entry points are admitted, and nothing else | - | source: "Wherever this law confines something to the composition root -"
O-149 | §2 | Confined to the composition root: reading configuration (§11) | - | source: "Wherever this law confines something to the composition root -"
O-150 | §2 | Confined to the composition root: learning the environment (§11) | - | source: "Wherever this law confines something to the composition root -"
O-151 | §2 | Confined to the composition root: resolving services from the container (§15) | - | source: "Wherever this law confines something to the composition root -"
O-152 | §2 | Confined to the composition root: registering decorators (§5) | - | source: "Wherever this law confines something to the composition root -"
O-153 | §2 | A module is a Bounded Context | - | source: "A module is a Bounded Context."
O-154 | §2 | A module's projects exist at most once per module, never per feature | - | source: "Its projects exist **at most once per module**, never per"
O-155 | §2 | A module with no model of its own omits `Domain` | - | source: "Its projects exist **at most once per module**, never per"
O-156 | §2 | A module that publishes nothing - to its siblings or over the wire - omits `Contracts` | - | source: "Its projects exist **at most once per module**, never per"
O-157 | §2 | `Application` is what makes a module dispatchable at all | - | source: "Its projects exist **at most once per module**, never per"
O-158 | §2 | A module has one Domain, shared by all of its features | - | source: "A module has one Domain, shared by all of its"
O-159 | §2 | Features are use cases over the module's model, not copies of it | - | source: "A module has one Domain, shared by all of its"
O-160 | §2 | Vertical slicing applies to the Application project only | - | source: "Vertical slicing applies to the **Application project only**."
O-161 | §2 | Folder table row `Domain`: organized by aggregate; `<Aggregate>/` holds the root, its entities, value objects, outcomes, domain events, specifications, repository port | - | source: "| `Domain` | Aggregate | `<Aggregate>/` - root, its entities,"
O-162 | §2 | Folder table row `Application`: organized by feature (use case); `<Feature>/` holds the command or query, handler, validator, response DTO, operation declaration | - | source: "| `Application` | Feature (use case) | `<Feature>/` - command or"
O-163 | §2 | Folder table row `Infrastructure`: organized by technical concern; `Persistence/`, `Queries/`, `Messaging/`, `External/` | - | source: "| `Infrastructure` | Technical concern | `Persistence/`, `Queries/`, `Messaging/`, `External/` |"
O-164 | §2 | Folder table row `Contracts`: organized by published surface; `IntegrationEvents/`, `Queries/`, and the module's wire ids (§3) | - | source: "| `Contracts` | Published surface | `IntegrationEvents/`, `Queries/`, and the"
O-165 | §2 | Folder table row `Platform.Application`: organized by technical concern; `Abstractions/`, `Behaviors/` | - | source: "| `Platform.Application` | Technical concern | `Abstractions/`, `Behaviors/` |"
O-166 | §2 | Folder table row `Platform.Infrastructure`: organized by technical concern; `Persistence/` (including the audit trail and the cursor codec) and `Messaging/` | - | source: "| `Platform.Infrastructure` | Technical concern | `Persistence/` - including the"
O-167 | §2 | Grouping by pattern is forbidden | BE-03 | source: "Grouping by pattern is forbidden: no `Application/Handlers/`, no `Application/Commands/`,"
O-168 | §2 | No `Application/Handlers/` folder | BE-03 | source: "Grouping by pattern is forbidden: no `Application/Handlers/`, no `Application/Commands/`,"
O-169 | §2 | No `Application/Commands/` folder | BE-03 | source: "Grouping by pattern is forbidden: no `Application/Handlers/`, no `Application/Commands/`,"
O-170 | §2 | No `Domain/Entities/` folder | BE-03 | source: "Grouping by pattern is forbidden: no `Application/Handlers/`, no `Application/Commands/`,"
O-171 | §2 | No `Domain/ValueObjects/` folder | BE-03 | source: "Grouping by pattern is forbidden: no `Application/Handlers/`, no `Application/Commands/`,"
O-172 | §2 | Duplicating an aggregate across feature folders inside one module is a defect | - | source: "Duplicating an aggregate across feature folders inside one module is"
O-173 | §2 | Domain event reactions are Application slices | - | source: "Domain event reactions are Application slices too, named for what"
O-174 | §2 | A reaction slice is named for what it does: `Application/<Reaction>/` | - | source: "Domain event reactions are Application slices too, named for what"
O-175 | §2 | A reaction changes state as a command handler does: loads an aggregate through its repository and calls its methods | - | source: "A reaction changes state as a command handler does -"
O-176 | §2 | A reaction runs inside the command's unit of work | - | source: "A reaction changes state as a command handler does -"
O-177 | §2 | A reaction is not a handler of a message | - | source: "It is not a handler of a message and calls"
O-178 | §2 | A reaction calls no handler | - | source: "It is not a handler of a message and calls"

#### 3. Domain Layer

##### Aggregates

O-179 | §3 Aggregates | Aggregate state MUST be private | BE-04 | source: "Aggregate state MUST be private. No public setters."
O-180 | §3 Aggregates | No public setters on aggregates | BE-04 | source: "No public setters."
O-181 | §3 Aggregates | Collections are exposed as `IReadOnlyCollection<T>` over a private backing field | BE-04 | source: "Collections are exposed as `IReadOnlyCollection<T>` over a private backing field."
O-182 | §3 Aggregates | State MUST change only through methods named in domain vocabulary | - | source: "State MUST change only through methods named in domain vocabulary."
O-183 | §3 Aggregates | `SetStatus`-style methods are a defect; `Approve`-style domain verbs are the rule | - | source: "`SetStatus` is a defect; `Approve` is the rule."
O-184 | §3 Aggregates | An aggregate MUST be valid at every instant of its public lifetime | - | source: "An aggregate MUST be valid at every instant of its"
O-185 | §3 Aggregates | Constructors and methods enforce invariants | - | source: "Constructors and methods enforce invariants."
O-186 | §3 Aggregates | There is no "validate later" step | - | source: "There is no "validate later" step and no `IsValid` property."
O-187 | §3 Aggregates | There is no `IsValid` property | - | source: "There is no "validate later" step and no `IsValid` property."
O-188 | §3 Aggregates | An aggregate method MUST NOT return `Result` | BE-05 | source: "An aggregate method MUST NOT return `Result`, a DTO, or"
O-189 | §3 Aggregates | An aggregate method MUST NOT return a DTO | BE-05 | source: "An aggregate method MUST NOT return `Result`, a DTO, or"
O-190 | §3 Aggregates | An aggregate method MUST NOT return any application type | BE-05 | source: "An aggregate method MUST NOT return `Result`, a DTO, or"
O-191 | §3 Aggregates | An aggregate method returns a domain outcome, a domain value, or nothing | BE-05 | source: "It returns a domain outcome, a domain value, or nothing."
O-192 | §3 Aggregates | Expected business rejections MUST be domain outcome values - an enum or sealed outcome type in domain vocabulary | - | source: "Expected business rejections MUST be domain outcome values - an"
O-193 | §3 Aggregates | Expected business rejections MUST NOT be exceptions | - | source: "They MUST NOT be exceptions."
O-194 | §3 Aggregates | Invariant breaches MUST throw | - | source: "Invariant breaches MUST throw."
O-195 | §3 Aggregates | Aggregates MUST NOT reference repositories | BE-06 | source: "Aggregates MUST NOT reference repositories, `DbContext`, `HttpClient`, or any I/O."
O-196 | §3 Aggregates | Aggregates MUST NOT reference `DbContext` | BE-06 | source: "Aggregates MUST NOT reference repositories, `DbContext`, `HttpClient`, or any I/O."
O-197 | §3 Aggregates | Aggregates MUST NOT reference `HttpClient` | BE-06 | source: "Aggregates MUST NOT reference repositories, `DbContext`, `HttpClient`, or any I/O."
O-198 | §3 Aggregates | Aggregates MUST NOT reference any I/O | BE-06 | source: "Aggregates MUST NOT reference repositories, `DbContext`, `HttpClient`, or any I/O."
O-199 | §3 Aggregates | An aggregate root MUST be marked as one at the type level | BE-78 | source: "An aggregate root MUST be marked as one at the"
O-200 | §3 Aggregates | An aggregate MUST NOT hold a reference to another aggregate | BE-79 | source: "An aggregate MUST NOT hold a reference to another aggregate,"
O-201 | §3 Aggregates | Including: no navigation property between aggregate roots | BE-79 | source: "An aggregate MUST NOT hold a reference to another aggregate,"
O-202 | §3 Aggregates | An aggregate holds another aggregate's id instead of a reference | BE-79 | source: "It holds that aggregate's id."
O-203 | §3 Aggregates | (illustration) Fenced C# aggregate-method example exists, first comment `// Illustrative.` | - | source: "// Illustrative."
O-204 | §3 Aggregates | (from illustration comment) An expected business condition returns an outcome value; an impossible condition throws; the method raises a domain event after mutating; takes `DateTimeOffset now` as an argument | - | source: "if (<business condition>) return <Outcome>.<Rejection>;   // expected: a value"

##### Value Objects

O-205 | §3 Value Objects | Primitives in aggregates and entities SHOULD be replaced by Value Objects wherever the primitive carries meaning or constraints | - | source: "Primitives in aggregates and entities SHOULD be replaced by Value"
O-206 | §3 Value Objects | Example: a `string` email becomes `EmailAddress` | - | source: "A `string` email becomes `EmailAddress`."
O-207 | §3 Value Objects | Value Objects MUST be immutable | - | source: "Value Objects MUST be immutable, MUST validate in their constructor,"
O-208 | §3 Value Objects | Value Objects MUST validate in their constructor | - | source: "Value Objects MUST be immutable, MUST validate in their constructor,"
O-209 | §3 Value Objects | Value Objects MUST have structural equality | - | source: "Value Objects MUST be immutable, MUST validate in their constructor,"
O-210 | §3 Value Objects | A Value Object MUST be impossible to construct in an invalid state | - | source: "A Value Object MUST be impossible to construct in an"
O-211 | §3 Value Objects | Every identifier MUST be a strongly-typed id | - | source: "Every identifier MUST be a strongly-typed id, one type per"
O-212 | §3 Value Objects | One strongly-typed id type per concept, so passing the wrong id does not compile | - | source: "Every identifier MUST be a strongly-typed id, one type per"
O-213 | §3 Value Objects | No public member of `Domain` exposes a bare `Guid` | BE-08 | source: "No public member of `Domain`, or of a command or"
O-214 | §3 Value Objects | No public member of a command or query message exposes a bare `Guid` | BE-08 | source: "No public member of `Domain`, or of a command or"
O-215 | §3 Value Objects | Exception to the bare-`Guid` rule: a strongly-typed id's own constructor, its value member, and its compiler-generated members (such as a positional record's `Deconstruct`) | BE-08 | source: "No public member of `Domain`, or of a command or"
O-216 | §3 Value Objects | `int` stays permitted for counts and quantities | - | source: "`int` stays permitted for counts and quantities."
O-217 | §3 Value Objects | A command or query message MUST NOT reference a `Domain` type | BE-109 | source: "A command or query message, and a Response DTO, MUST"
O-218 | §3 Value Objects | A Response DTO MUST NOT reference a `Domain` type | BE-109 | source: "A command or query message, and a Response DTO, MUST"
O-219 | §3 Value Objects | An id a message or Response DTO carries is the module's published id type from `Contracts` | - | source: "An id they carry is the module's published id type"
O-220 | §3 Value Objects | The published id type in `Contracts` is the one wire id for that concept | - | source: "An id they carry is the module's published id type"
O-221 | §3 Value Objects | The handler converts the wire id, and any other wire value, into domain values | - | source: "An id they carry is the module's published id type"
O-222 | §3 Value Objects | A module with an endpoint therefore has a `Contracts` project | - | source: "A module with an endpoint therefore has a `Contracts` project."

##### Entities and Domain Events

O-223 | §3 Entities | Entities inside an aggregate MUST be reachable only through the root | - | source: "Entities inside an aggregate MUST be reachable and modifiable only"
O-224 | §3 Entities | Entities inside an aggregate MUST be modifiable only through the root | - | source: "Entities inside an aggregate MUST be reachable and modifiable only"
O-225 | §3 Entities | Domain events MUST be raised by the aggregate | - | source: "Domain events MUST be raised by the aggregate and recorded"
O-226 | §3 Entities | Domain events MUST be recorded on the aggregate | - | source: "Domain events MUST be raised by the aggregate and recorded"
O-227 | §3 Entities | The unit of work dispatches domain events to their in-module reactions before it commits | - | source: "The unit of work dispatches them to their in-module reactions"
O-228 | §3 Entities | Reactions to domain events run inside the same transaction | - | source: "The unit of work dispatches them to their in-module reactions"
O-229 | §3 Entities | A handler MUST NOT raise a domain event | - | source: "A handler MUST NOT raise a domain event."
O-230 | §3 Entities | Domain events SHOULD be named in the past tense | - | source: "Domain events SHOULD be named in the past tense."
O-231 | §3 Entities | Domain events MUST carry ids and values, never entity references | BE-10 | source: "Domain events MUST carry ids and values, never entity references."
O-232 | §3 Entities | Domain events are intra-module | BE-07 | source: "Domain events are intra-module and MUST NOT leave the module."
O-233 | §3 Entities | Domain events MUST NOT leave the module | BE-07 | source: "Domain events are intra-module and MUST NOT leave the module."

##### Specifications

O-234 | §3 Specifications | Specifications MAY encapsulate reusable query predicates | - | source: "Specifications MAY encapsulate reusable query predicates."
O-235 | §3 Specifications | A Specification MUST NOT be the authority on whether a mutation is legal | - | source: "A Specification MUST NOT be the authority on whether a"
O-236 | §3 Specifications | The authority on mutation legality is the aggregate method | - | source: "That authority is the aggregate method."
O-237 | §3 Specifications | Never pre-check with a Specification and rely on the aggregate not to re-check (the aggregate always re-checks) | - | source: "Never pre-check with a Specification and rely on the aggregate"

#### 4. Application Layer

##### Commands and Queries

O-238 | §4 Commands and Queries | A Command mutates | - | source: "A Command mutates. A Query reads."
O-239 | §4 Commands and Queries | A Query reads | - | source: "A Query reads."
O-240 | §4 Commands and Queries | A Command MUST NOT return data beyond an identifier, its outcome, and the new version of the aggregate it changed (core §4 Wire contract) | BE-11 | source: "A Command MUST NOT return data beyond an identifier, its"
O-241 | §4 Commands and Queries | Command handlers MUST mutate state only by loading an aggregate through its repository, calling its methods, and persisting through the repository | BE-13 | source: "Command handlers MUST mutate state only by loading an aggregate"
O-242 | §4 Commands and Queries | There is no other write path for a command | BE-13 | source: "There is no other write path: writing through `DbContext` or"
O-243 | §4 Commands and Queries | Writing through `DbContext` from a command handler is forbidden | BE-13 | source: "There is no other write path: writing through `DbContext` or"
O-244 | §4 Commands and Queries | Writing through SQL from a command handler is forbidden | BE-13 | source: "There is no other write path: writing through `DbContext` or"
O-245 | §4 Commands and Queries | A Query MUST NOT mutate | BE-12 | source: "A Query MUST NOT mutate: its handler MUST NOT call"
O-246 | §4 Commands and Queries | A query handler MUST NOT call `SaveChangesAsync` | BE-12 | source: "A Query MUST NOT mutate: its handler MUST NOT call"
O-247 | §4 Commands and Queries | A query handler MUST bypass the domain | BE-108 | source: "A query handler MUST bypass the domain and project straight"
O-248 | §4 Commands and Queries | A query handler projects straight into its Response DTO | BE-108 | source: "A query handler MUST bypass the domain and project straight"
O-249 | §4 Commands and Queries | A query handler reads untracked | BE-108 | source: "A query handler MUST bypass the domain and project straight"
O-250 | §4 Commands and Queries | A query handler MUST NOT reference a `Domain` type | BE-108 | source: "A query handler MUST bypass the domain and project straight"
O-251 | §4 Commands and Queries | A query handler MUST NOT materialize an aggregate | BE-108 | source: "A query handler MUST bypass the domain and project straight"
O-252 | §4 Commands and Queries | A query handler MUST NOT use an aggregate repository | BE-108 | source: "A query handler MUST bypass the domain and project straight"
O-253 | §4 Commands and Queries | The query message lives in `Application` | BE-15 | source: "The query message, its Response DTO, and the `IQueryHandler<,>` abstraction"
O-254 | §4 Commands and Queries | The query's Response DTO lives in `Application` | BE-15 | source: "The query message, its Response DTO, and the `IQueryHandler<,>` abstraction"
O-255 | §4 Commands and Queries | The `IQueryHandler<,>` abstraction lives in `Application` | BE-15 | source: "The query message, its Response DTO, and the `IQueryHandler<,>` abstraction"
O-256 | §4 Commands and Queries | Every concrete query handler lives in `Infrastructure`, by convention | BE-15 | source: "**Every concrete query handler lives in `Infrastructure`, by convention**, a"
O-257 | §4 Commands and Queries | This includes a pure-computation query handler (also in `Infrastructure`) | BE-15 | source: "**Every concrete query handler lives in `Infrastructure`, by convention**, a"
O-258 | §4 Commands and Queries | A command handler stays in `Application` | BE-15 | source: "**Every concrete query handler lives in `Infrastructure`, by convention**, a"
O-259 | §4 Commands and Queries | A query handler reads a relational database through keyless entity types its module's Infrastructure maps for reading, over a table or a view, queried with LINQ | - | source: "A query handler reads a relational database through keyless entity"
O-260 | §4 Commands and Queries | Alternatively a query handler reads with parameterized SQL projected into the DTO, such as EF Core's `Database.SqlQuery<T>` with an interpolated string | - | source: "A query handler reads a relational database through keyless entity"
O-261 | §4 Commands and Queries | A read type carries the module's global query filters, such as a tenant or soft-delete filter | - | source: "A read type carries the module's global query filters, such"
O-262 | §4 Commands and Queries | SQL bypasses global query filters and MUST apply them itself | - | source: "A read type carries the module's global query filters, such"
O-263 | §4 Commands and Queries | A handler that reads with SQL is covered by a test against a real database (§14) | - | source: "A handler that reads with SQL is covered by a"
O-264 | §4 Commands and Queries | SQL MUST NOT be built from input by any means but the parameterized form | BE-113 | source: "SQL MUST NOT be built from input by any means"
O-265 | §4 Commands and Queries | The raw-string APIs - `SqlQueryRaw`, `FromSqlRaw`, `ExecuteSqlRaw`, or any other - MUST NOT be used in a module | BE-113 | source: "The raw-string APIs - `SqlQueryRaw`, `FromSqlRaw`, `ExecuteSqlRaw`, or any"
O-266 | §4 Commands and Queries | Exception: a migration's SQL (§5) is written, not built from input, and is outside the raw-SQL rule | - | source: "A migration's SQL (§5) is written, not built from input,"
O-267 | §4 Commands and Queries | `Repository` is reserved for the aggregate write path | BE-17 | source: "`Repository` is reserved for the aggregate write path."
O-268 | §4 Commands and Queries | A read-side type that returns DTOs MUST NOT be called a repository | BE-17 | source: "A read-side type that returns DTOs MUST NOT be called"
O-269 | §4 Commands and Queries | Name such a read-side type `<Feature>Reader` | BE-17 | source: "A read-side type that returns DTOs MUST NOT be called"
O-270 | §4 Commands and Queries | Usually the query handler is the read data access and no separate type is warranted | BE-17 | source: "Usually the query handler is the read data access and"
O-271 | §4 Commands and Queries | One handler per message | - | source: "One handler per message."
O-272 | §4 Commands and Queries | Handlers MUST NOT call other handlers directly | - | source: "Handlers MUST NOT call other handlers directly."
O-273 | §4 Commands and Queries | A handler MAY dispatch another module's published query through the dispatcher (core §3) | - | source: "A handler MAY dispatch another module's published query through the"
O-274 | §4 Commands and Queries | Such a cross-module dispatch runs the full pipeline | - | source: "A handler MAY dispatch another module's published query through the"
O-275 | §4 Commands and Queries | Such a dispatch runs on its own module's connection, never enlisted in the command's transaction | - | source: "It runs on its own module's connection, never enlisted in"
O-276 | §4 Commands and Queries | Shared logic belongs in the domain or a named application service | - | source: "Shared logic belongs in the domain or a named application"
O-277 | §4 Commands and Queries | Handlers MUST NOT reference `HttpContext` | BE-18 | source: "Handlers MUST NOT reference `HttpContext`, `IHttpContextAccessor`, or any transport type."
O-278 | §4 Commands and Queries | Handlers MUST NOT reference `IHttpContextAccessor` | BE-18 | source: "Handlers MUST NOT reference `HttpContext`, `IHttpContextAccessor`, or any transport type."
O-279 | §4 Commands and Queries | Handlers MUST NOT reference any transport type | BE-18 | source: "Handlers MUST NOT reference `HttpContext`, `IHttpContextAccessor`, or any transport type."
O-280 | §4 Commands and Queries | (illustration) Fenced C# outcome-translation switch example exists, first comment `// Illustrative. Translation happens here and nowhere else: a switch expression over an enum outcome, with no discard arm.` | - | source: "// Illustrative. Translation happens here and nowhere else: a switch"
O-281 | §4 Commands and Queries | (from illustration comment) Translation of a domain outcome to `Result` happens in the handler and nowhere else | - | source: "// Illustrative. Translation happens here and nowhere else: a switch"
O-282 | §4 Commands and Queries | (from illustration) A rejection outcome maps to a `Result` failure (e.g. `Result.Conflict(<Module>Errors.<Rejection>)`), success to `Result.Success()` | - | source: "<Outcome>.<Rejection> => Result.Conflict(<Module>Errors.<Rejection>),"
O-283 | §4 Commands and Queries | Translation is a switch expression | - | source: "Translation is a switch expression, never a switch statement."
O-284 | §4 Commands and Queries | Translation is never a switch statement | - | source: "Translation is a switch expression, never a switch statement."
O-285 | §4 Commands and Queries | Over an enum outcome, CS8524 (raised for the enum's unnamed values) is disabled in the solution's build configuration | - | source: "Over an enum outcome, CS8524, raised for the enum's unnamed"
O-286 | §4 Commands and Queries | The CS8524 disablement carries its reason (§1) | - | source: "Over an enum outcome, CS8524, raised for the enum's unnamed"
O-287 | §4 Commands and Queries | Over an enum outcome, no discard arm is written | - | source: "Over an enum outcome, CS8524, raised for the enum's unnamed"
O-288 | §4 Commands and Queries | So a missed enum member raises CS8509, an error under `TreatWarningsAsErrors` (§1) | - | source: "Over an enum outcome, CS8524, raised for the enum's unnamed"
O-289 | §4 Commands and Queries | Where outcomes are types, the switch ends in a discard arm that throws `UnreachableException` | - | source: "Where outcomes are types, the switch ends in a discard"
O-290 | §4 Commands and Queries | Where outcomes are types, each outcome's translation is proven by a test (§14) | - | source: "Where outcomes are types, the switch ends in a discard"

##### Pagination

O-291 | §4 Pagination | Every query returning a collection whose size is not bounded by construction MUST be paginated | BE-19 | source: "Every query returning a collection whose size is not bounded"
O-292 | §4 Pagination | A collection bounded by construction declares its bound (core §4) | BE-19 | source: "Every query returning a collection whose size is not bounded"
O-293 | §4 Pagination | A handler returning an unbounded collection, such as a `List<T>` or an `IEnumerable<T>`, is a defect | BE-19 | source: "A handler returning an unbounded collection, such as a `List<T>`"
O-294 | §4 Pagination | A declared stream (core §4 Wire contract) is not a collection (outside the pagination rule) | - | source: "A declared stream (core §4 *Wire contract*) is not a"
O-295 | §4 Pagination | One shared page shape for the whole solution | BE-20 | source: "One shared page shape for the whole solution."
O-296 | §4 Pagination | A per-feature paging envelope is a defect | BE-20 | source: "A per-feature paging envelope is a defect."
O-297 | §4 Pagination | Every paginated query MUST declare a maximum page size | BE-21, BE-22 | source: "Every paginated query MUST declare a maximum page size, under"
O-298 | §4 Pagination | The declared maximum is under a solution-wide ceiling | BE-21, BE-22 | source: "Every paginated query MUST declare a maximum page size, under"
O-299 | §4 Pagination | A request above the maximum MUST be rejected by the validation behavior | BE-21, BE-22 | source: "A request above the maximum MUST be rejected by the"
O-300 | §4 Pagination | Silent clamping of page size is forbidden | BE-21, BE-22 | source: "Silent clamping is forbidden."
O-301 | §4 Pagination | Omitting the page size MUST yield the operation's declared default | - | source: "Omitting the page size MUST yield the operation's declared default,"
O-302 | §4 Pagination | Omitting the page size never yields "all" | - | source: "Omitting the page size MUST yield the operation's declared default,"
O-303 | §4 Pagination | Ordering MUST be deterministic | BE-23 | source: "Ordering MUST be deterministic."
O-304 | §4 Pagination | A paginated query MUST declare its ordering as a descriptor | BE-23 | source: "A paginated query MUST declare its ordering as a descriptor"
O-305 | §4 Pagination | The ordering descriptor's last term is a unique key | BE-23 | source: "A paginated query MUST declare its ordering as a descriptor"
O-306 | §4 Pagination | Ordering is not expressed only as an `OrderBy` chain | BE-23 | source: "A paginated query MUST declare its ordering as a descriptor"
O-307 | §4 Pagination | The executed `ORDER BY` MUST be generated from the descriptor, never written beside it | BE-23 | source: "The `ORDER BY` and the keyset predicate the query executes"
O-308 | §4 Pagination | The executed keyset predicate MUST be generated from the descriptor, never written beside it | BE-23 | source: "The `ORDER BY` and the keyset predicate the query executes"
O-309 | §4 Pagination | A bounded set a UI grid shows in full counts as bounded by construction only where it declares its bound | - | source: "A bounded set a UI grid shows in full is"
O-310 | §4 Pagination | ... and only where it rejects a request beyond its bound | - | source: "A bounded set a UI grid shows in full is"
O-311 | §4 Pagination | One cursor type solution-wide | BE-24 | source: "One cursor type solution-wide, whose payload is exactly the sort"
O-312 | §4 Pagination | The cursor payload is exactly the sort key and the id | BE-24 | source: "One cursor type solution-wide, whose payload is exactly the sort"
O-313 | §4 Pagination | Cursors are opaque to the client (core §4 Wire contract) | - | source: "Cursors are opaque to the client (core §4 *Wire contract*)"
O-314 | §4 Pagination | Cursors MUST be validated server-side | - | source: "Cursors are opaque to the client (core §4 *Wire contract*)"
O-315 | §4 Pagination | A cursor is encrypted | BE-119 | source: "A cursor is encrypted and authenticated."
O-316 | §4 Pagination | A cursor is authenticated | BE-119 | source: "A cursor is encrypted and authenticated."
O-317 | §4 Pagination | A cursor is encoded and decoded only through a cursor codec port | BE-119 | source: "It is encoded and decoded only through a cursor codec"
O-318 | §4 Pagination | The cursor codec port is declared in `Platform.Application` | BE-119 | source: "It is encoded and decoded only through a cursor codec"
O-319 | §4 Pagination | The cursor codec is implemented in `Platform.Infrastructure` | BE-119 | source: "It is encoded and decoded only through a cursor codec"
O-320 | §4 Pagination | The cursor codec uses ASP.NET Core Data Protection | BE-119 | source: "It is encoded and decoded only through a cursor codec"
O-321 | §4 Pagination | Data Protection is used under a purpose per query | BE-119 | source: "It is encoded and decoded only through a cursor codec"
O-322 | §4 Pagination | A cursor that fails to decode is a validation failure | BE-119 | source: "A cursor that fails to decode is a validation failure,"
O-323 | §4 Pagination | A cursor decode failure is never an exception that escapes | BE-119 | source: "A cursor that fails to decode is a validation failure,"
O-324 | §4 Pagination | The key ring is persisted in the database through `PersistKeysToDbContext`, or in Blob storage with its keys protected by Key Vault | BE-119 | source: "The key ring is persisted in the database through `PersistKeysToDbContext`,"
O-325 | §4 Pagination | The key ring is never persisted in a cache | BE-119 | source: "The key ring is persisted in the database through `PersistKeysToDbContext`,"
O-326 | §4 Pagination | The key ring is shared by every instance | BE-119 | source: "The key ring is persisted in the database through `PersistKeysToDbContext`,"
O-327 | §4 Pagination | Pagination's agent rule is in `annex-agents.md` | - | source: "Its agent rule: `annex-agents.md`."

##### Pipeline behaviors

O-328 | §4 Pipeline behaviors | Pipeline behaviors run in a fixed order | - | source: "Fixed order. Changing it requires written justification."
O-329 | §4 Pipeline behaviors | Changing the behavior order requires written justification | - | source: "Changing it requires written justification."
O-330 | §4 Pipeline behaviors | Behavior 1 is Logging / correlation | - | source: "**Logging / correlation**"
O-331 | §4 Pipeline behaviors | Behavior 2 is Authorization | BE-25 | source: "**Authorization** - resolves the message's declared policy and evaluates it"
O-332 | §4 Pipeline behaviors | The authorization behavior resolves the message's declared policy | BE-25 | source: "**Authorization** - resolves the message's declared policy and evaluates it"
O-333 | §4 Pipeline behaviors | The authorization behavior evaluates the policy against the explicit principal | BE-25 | source: "**Authorization** - resolves the message's declared policy and evaluates it"
O-334 | §4 Pipeline behaviors | A message with no declared policy MUST fail to dispatch | BE-25 | source: "A message with no declared policy MUST fail to dispatch."
O-335 | §4 Pipeline behaviors | Behavior 3 is Validation, using FluentValidation | - | source: "**Validation** - FluentValidation."
O-336 | §4 Pipeline behaviors | Validation failures short-circuit to a `Result` failure | - | source: "Failures short-circuit to a `Result` failure; the handler is never"
O-337 | §4 Pipeline behaviors | On validation failure the handler is never invoked | - | source: "Failures short-circuit to a `Result` failure; the handler is never"
O-338 | §4 Pipeline behaviors | Behavior 4 is Unit of work and idempotency | - | source: "**Unit of work and idempotency** - commands only. One behavior."
O-339 | §4 Pipeline behaviors | The unit-of-work/idempotency behavior applies to commands only | - | source: "**Unit of work and idempotency** - commands only. One behavior."
O-340 | §4 Pipeline behaviors | Unit of work and idempotency are one behavior (not two) | - | source: "One behavior."
O-341 | §4 Pipeline behaviors | For a command that persists aggregate state, the behavior opens the unit of work | - | source: "For a command that persists aggregate state, it opens the"
O-342 | §4 Pipeline behaviors | Then, for a command that declares a key, it resolves the attempt identity | - | source: "For a command that persists aggregate state, it opens the"
O-343 | §4 Pipeline behaviors | Where a record for the attempt exists, it answers with the record as a replay, without invoking the handler | - | source: "For a command that persists aggregate state, it opens the"
O-344 | §4 Pipeline behaviors | Where a record exists but the fingerprints differ, it answers `422`, without invoking the handler | - | source: "For a command that persists aggregate state, it opens the"
O-345 | §4 Pipeline behaviors | Otherwise it runs the handler | - | source: "Otherwise it runs the handler, writes the idempotency record where"
O-346 | §4 Pipeline behaviors | It writes the idempotency record where a key is used | - | source: "Otherwise it runs the handler, writes the idempotency record where"
O-347 | §4 Pipeline behaviors | It dispatches domain events to their reactions | - | source: "Otherwise it runs the handler, writes the idempotency record where"
O-348 | §4 Pipeline behaviors | It commits on success | - | source: "Otherwise it runs the handler, writes the idempotency record where"
O-349 | §4 Pipeline behaviors | It rolls back on any failure or exception | - | source: "Otherwise it runs the handler, writes the idempotency record where"
O-350 | §4 Pipeline behaviors | After a successful commit it hands the committed domain events to their post-commit effects, such as cache invalidation | - | source: "After a successful commit it hands the committed domain events"
O-351 | §4 Pipeline behaviors | A post-commit effect failure is logged as an error | - | source: "After a successful commit it hands the committed domain events"
O-352 | §4 Pipeline behaviors | After a post-commit effect failure, the committed result is still returned | - | source: "After a successful commit it hands the committed domain events"
O-353 | §4 Pipeline behaviors | On any failure result - the handler's, or one at commit - the behavior looks up the attempt identity first | - | source: "On any failure result - the handler's, or one at"
O-354 | §4 Pipeline behaviors | ... and answers with the existing record where one exists | - | source: "On any failure result - the handler's, or one at"
O-355 | §4 Pipeline behaviors | Otherwise it returns the failure | - | source: "On any failure result - the handler's, or one at"
O-356 | §4 Pipeline behaviors | A commit-time failure is returned as a `Result` conflict | - | source: "On any failure result - the handler's, or one at"
O-357 | §4 Pipeline behaviors | Except: a stale expected version, or a create-only unique-index violation, at commit is returned as a precondition failure (§13; core §6) | - | source: "On any failure result - the handler's, or one at"
O-358 | §4 Pipeline behaviors | Where a key is used, a failure result the behavior returns is recorded | - | source: "Where a key is used, a failure result it returns"
O-359 | §4 Pipeline behaviors | The failure record is written after the rollback | - | source: "Where a key is used, a failure result it returns"
O-360 | §4 Pipeline behaviors | The failure record is written in a transaction of its own on the module's context | - | source: "Where a key is used, a failure result it returns"
O-361 | §4 Pipeline behaviors | So a replay returns the recorded failure (core §4) | - | source: "Where a key is used, a failure result it returns"
O-362 | §4 Pipeline behaviors | A unique-index violation while recording the failure is answered by the attempt identity, as above | - | source: "Where a key is used, a failure result it returns"
O-363 | §4 Pipeline behaviors | An exception is not recorded as an idempotency result | - | source: "An exception is not recorded."
O-364 | §4 Pipeline behaviors | For a command that does not persist, the behavior opens nothing | - | source: "For a command that does not persist, it opens nothing,"
O-365 | §4 Pipeline behaviors | For a non-persisting command, after the handler returns, the behavior fails the dispatch if the module's context holds a staged change | - | source: "For a command that does not persist, it opens nothing,"

##### Idempotency

O-366 | §4 Idempotency | A command that executes twice on a retry is a defect in the command, not in the caller | - | source: "A command that executes twice on a retry is a"
O-367 | §4 Idempotency | Every command MUST declare its idempotency mechanism in its operation declaration (§6) | BE-26 | source: "Every command MUST declare its idempotency mechanism in its operation"
O-368 | §4 Idempotency | The idempotency mechanism is one of: naturally idempotent, client-supplied id, or idempotency key | BE-26 | source: "Every command MUST declare its idempotency mechanism in its operation"
O-369 | §4 Idempotency | An undeclared command MUST fail to dispatch, exactly as an undeclared policy does | BE-26 | source: "An undeclared command MUST fail to dispatch, exactly as an"
O-370 | §4 Idempotency | Where a key is used, the identity of an attempt is `(principal, operation, key)` | - | source: "Where a key is used, the identity of an attempt"
O-371 | §4 Idempotency | The same key with a different payload MUST fail with `422` | - | source: "The same key with a different payload MUST fail with"
O-372 | §4 Idempotency | The same key with a different payload never overwrites (core §4) | - | source: "The same key with a different payload MUST fail with"
O-373 | §4 Idempotency | The idempotency record MUST be written through the same unit of work as the state change | BE-27 | source: "The idempotency record MUST be written through the same unit"
O-374 | §4 Idempotency | Redis MUST NOT be the idempotency store | BE-27 | source: "Redis MUST NOT be the store."
O-375 | §4 Idempotency | The idempotency store MUST carry a unique index on the attempt identity | BE-28 | source: "The store MUST carry a unique index on the attempt"
O-376 | §4 Idempotency | The idempotency record stores a fingerprint of the request payload | - | source: "The idempotency record stores a fingerprint of the request payload:"
O-377 | §4 Idempotency | The fingerprint is an HMAC under a server key, never a plain hash | - | source: "The idempotency record stores a fingerprint of the request payload:"
O-378 | §4 Idempotency | The fingerprint's data class is `Internal` | - | source: "The idempotency record stores a fingerprint of the request payload:"
O-379 | §4 Idempotency | The unit-of-work behavior answers a violation of the attempt-identity index at commit by the attempt identity, as it answers any failure result (§4 Pipeline behaviors, item 4) | - | source: "The unit-of-work behavior answers a violation of that index at"
O-380 | §4 Idempotency | A violation of the attempt-identity index is never answered as a conflict | - | source: "The unit-of-work behavior answers a violation of that index at"
O-381 | §4 Idempotency | Two concurrent attempts with the same key MUST resolve as one execution and one replay | - | source: "Two concurrent attempts with the same key MUST resolve as"
O-382 | §4 Idempotency | Two concurrent same-key attempts never resolve as a `500` | - | source: "Two concurrent attempts with the same key MUST resolve as"
O-383 | §4 Idempotency | Idempotency records MUST be retained longer than the longest caller retry window | - | source: "Records MUST be retained longer than the longest caller retry"
O-384 | §4 Idempotency | Idempotency records MUST be swept afterwards | - | source: "Records MUST be retained longer than the longest caller retry"
O-385 | §4 Idempotency | Idempotency is a pipeline behavior | BE-30 | source: "Idempotency is a pipeline behavior."
O-386 | §4 Idempotency | A per-endpoint or per-handler idempotency check is a defect | BE-30 | source: "A per-endpoint or per-handler check is a defect."
O-387 | §4 Idempotency | Idempotency's agent rule is in `annex-agents.md` | - | source: "Its agent rule: `annex-agents.md`."

##### Transactions and consistency

O-388 | §4 Transactions | One transaction SHOULD modify exactly one aggregate | - | source: "One transaction SHOULD modify exactly one aggregate."
O-389 | §4 Transactions | A second aggregate changed by a domain-event reaction in the same transaction needs the PR to name the invariant that requires it | - | source: "A second, changed by a domain-event reaction in the same"
O-390 | §4 Transactions | Outbox rows, idempotency records and audit records are pipeline bookkeeping, not aggregates, and do not count against the one-aggregate rule | - | source: "Outbox rows, idempotency records and audit records are bookkeeping written"
O-391 | §4 Transactions | A transaction MUST NOT span a module boundary | BE-32 | source: "A transaction MUST NOT span a module boundary."
O-392 | §4 Transactions | Integration events MUST be written to the outbox in the same transaction as the state change that caused them | BE-33 | source: "Integration events MUST be written to the outbox in the"
O-393 | §4 Transactions | Publishing integration events directly from a handler is forbidden | BE-33 | source: "Publishing directly from a handler is forbidden."
O-394 | §4 Transactions | Every integration event handler MUST be idempotent | - | source: "Every integration event handler MUST be idempotent."
O-395 | §4 Transactions | Delivery is at-least-once; assume redelivery | - | source: "Delivery is at-least-once; assume redelivery."
O-396 | §4 Transactions | Handlers MUST NOT call external systems inside a transaction | - | source: "Handlers MUST NOT call external systems inside a transaction."
O-397 | §4 Transactions | A command declares, in its operation declaration (§6), whether it persists aggregate state | BE-110 | source: "A command declares, in its operation declaration (§6), whether it"
O-398 | §4 Transactions | Only a command that persists aggregate state opens the unit of work | BE-110 | source: "Only a command that does opens the unit of work."
O-399 | §4 Transactions | A command whose effect is an external write runs without a transaction | BE-110 | source: "A command whose effect is an external write runs without"
O-400 | §4 Transactions | An external-write command MUST NOT use a key-based idempotency mechanism | BE-110 | source: "A command whose effect is an external write runs without"
O-401 | §4 Transactions | An external-write command MUST NOT use the outbox | BE-110 | source: "A command whose effect is an external write runs without"
O-402 | §4 Transactions | An external-write command MUST NOT stage a database change | - | source: "It MUST NOT stage a database change; the unit-of-work behavior"
O-403 | §4 Transactions | The unit-of-work behavior refuses a staged change from a non-persisting command (§4 Pipeline behaviors, item 4; its test §14) | - | source: "It MUST NOT stage a database change; the unit-of-work behavior"
O-404 | §4 Transactions | An external-write command is idempotent naturally or by a client-supplied id, such as a blob name derived from the aggregate id | - | source: "It is idempotent naturally or by a client-supplied id, such"
O-405 | §4 Transactions | An external-write command MAY load an aggregate to decide whether the write may proceed | - | source: "It MAY load an aggregate to decide whether the write"
O-406 | §4 Transactions | Recording the result of the external write is a separate command that persists | - | source: "Recording the result of the write is a separate command"
O-407 | §4 Transactions | An external effect that a repeat would duplicate - such as sending an email or taking a payment - is not an external-write command | - | source: "An external effect that a repeat would duplicate - such"
O-408 | §4 Transactions | For such an effect, a command that persists records the intent and writes an outbox row | - | source: "A command that persists records the intent and writes an"
O-409 | §4 Transactions | The outbox delivery performs the duplicating effect | - | source: "A command that persists records the intent and writes an"
O-410 | §4 Transactions | The outbox delivery passes an idempotency key derived from the row wherever the external system accepts one | - | source: "A command that persists records the intent and writes an"

##### Integration event contracts

O-411 | §4 Integration events | An integration event is a published contract | - | source: "An integration event is a published contract."
O-412 | §4 Integration events | Every integration event type MUST carry its version in its name: `<Event>V<N>` | BE-84 | source: "Every integration event type MUST carry its version in its"
O-413 | §4 Integration events | The published contract surface MUST be snapshotted and committed | BE-85 | source: "The published contract surface MUST be snapshotted and committed."
O-414 | §4 Integration events | A change to the shape of an existing version MUST fail the build, as an OpenAPI document change does | BE-85 | source: "A change to the shape of an existing version MUST"
O-415 | §4 Integration events | Adding an optional field to an existing version is permitted | - | source: "Adding an optional field to an existing version is permitted."
O-416 | §4 Integration events | Renaming, removing, retyping, or changing the meaning of a field is not permitted in an existing version; that is a new version | - | source: "Renaming, removing, retyping, or changing the meaning of a field"
O-417 | §4 Integration events | Introducing `<Event>V2` MUST NOT retire `<Event>V1` until no consumer subscribes to it | - | source: "Introducing `<Event>V2` MUST NOT retire `<Event>V1` until no consumer subscribes"
O-418 | §4 Integration events | ... and until no outbox row holds a V1 event | - | source: "Introducing `<Event>V2` MUST NOT retire `<Event>V1` until no consumer subscribes"
O-419 | §4 Integration events | Both versions are published during the overlap | - | source: "Both are published during the overlap."
O-420 | §4 Integration events | A consumer MUST tolerate fields it does not know | - | source: "A consumer MUST tolerate fields it does not know and"
O-421 | §4 Integration events | A consumer MUST NOT fail on a field it does not use | - | source: "A consumer MUST tolerate fields it does not know and"
O-422 | §4 Integration events | Producers ship before consumers | - | source: "Producers ship before consumers."
O-423 | §4 Integration events | Event payloads MUST be ids, primitives, and value objects that serialize stably | BE-86 | source: "Event payloads MUST be ids, primitives, and value objects that"
O-424 | §4 Integration events | A domain type MUST NOT appear in an integration event | BE-86 | source: "A domain type MUST NOT appear in an integration event."

##### Placement of interfaces and implementations

O-425 | §4 Placement | Placement is decided by dependencies, never by the name | - | source: "Placement is decided by **dependencies, never by the name**."
O-426 | §4 Placement | `Service`, `Manager`, `Provider`, and `Client` in a name are not placement signals | - | source: "`Service`, `Manager`, `Provider`, and `Client` are not signals."
O-427 | §4 Placement | Placement row: interface `Domain`, implementation `Domain` - domain service, a pure business rule spanning aggregates, belonging to no single root | - | source: "| `Domain` | `Domain` | Domain service - a pure"
O-428 | §4 Placement | Placement row: interface `Domain`, implementation `Infrastructure` - repository port, expressed in domain vocabulary | - | source: "| `Domain` | `Infrastructure` | Repository port, expressed in domain vocabulary"
O-429 | §4 Placement | Placement row: interface `Application`, implementation `Application` - use-case logic with no external dependency: strategies, policies, orchestrators, decision tables, mappers | - | source: "| `Application` | `Application` | Use-case logic with no external"
O-430 | §4 Placement | Placement row: interface `Application`, implementation `Infrastructure` - adapter: anything crossing a process boundary or naming a technology, including concrete query handlers | - | source: "| `Application` | `Infrastructure` | Adapter - anything crossing a process"
O-431 | §4 Placement | The interface goes in the layer that can express the signature in its own vocabulary | - | source: "**Where the interface goes:** the layer that can express the"
O-432 | §4 Placement | A port whose signature would force `Domain` to reference an application type MUST be declared in `Application` | - | source: "A port whose signature would force `Domain` to reference an"
O-433 | §4 Placement | Implementation placement: both tests must agree | - | source: "**Where the implementation goes** - both tests must agree:"
O-434 | §4 Placement | Test 1: if it requires a package or namespace `Application` may not reference, it goes in `Infrastructure` | - | source: "Does it require a package or namespace `Application` may not"
O-435 | §4 Placement | Test 2: if it can be unit tested with no test doubles for anything but domain types, it goes in `Application` | - | source: "Can it be unit tested with no test doubles for"
O-436 | §4 Placement | An implementation that is pure logic MUST live in `Application` | - | source: "An implementation that is pure logic MUST live in `Application`,"
O-437 | §4 Placement | Exception: a concrete query handler, which §4 Commands and Queries places in `Infrastructure` by convention | - | source: "An implementation that is pure logic MUST live in `Application`,"
O-438 | §4 Placement | A real-time push adapter (the implementation of an `Application` port sending to a hub's clients through `IHubContext<T>`) is implemented in `Api`, not in `Infrastructure` (real-time annex) | - | source: "A real-time push adapter - the implementation of an `Application`"
O-439 | §4 Placement | `Api` hosts the hubs | - | source: "A real-time push adapter - the implementation of an `Application`"
O-440 | §4 Placement | Application logic in `Infrastructure` is invisible to the build and MUST be caught in review | - | source: "Application logic in `Infrastructure` is invisible to the build and"
O-441 | §4 Placement | Every Infrastructure type MUST be an adapter | BE-34, BE-107 | source: "Every Infrastructure type MUST be an adapter: it implements a"
O-442 | §4 Placement | Admissible Infrastructure type: implements a port declared in `Domain` or `Application` | BE-34, BE-107 | source: "Every Infrastructure type MUST be an adapter: it implements a"
O-443 | §4 Placement | Admissible Infrastructure type: implements a framework extension point such as `IEntityTypeConfiguration<T>`, `IHostedService` or `DbContext` | BE-34, BE-107 | source: "Every Infrastructure type MUST be an adapter: it implements a"
O-444 | §4 Placement | Admissible Infrastructure type: a registration entry point (§2) | BE-34, BE-107 | source: "Every Infrastructure type MUST be an adapter: it implements a"
O-445 | §4 Placement | Admissible Infrastructure type: a type the project maps or binds - an entity or read type in its model (such as the outbox row or the idempotency record), or an options class (§11) | BE-34, BE-107 | source: "Every Infrastructure type MUST be an adapter: it implements a"
O-446 | §4 Placement | A free-standing Infrastructure service belonging to none of these is a defect | BE-34, BE-107 | source: "A free-standing Infrastructure service belonging to none of these is"
O-447 | §4 Placement | Infrastructure types are `internal` | BE-34, BE-107 | source: "Infrastructure types are `internal`, except the registration entry points, any"
O-448 | §4 Placement | Exception to `internal`: the registration entry points | BE-34, BE-107 | source: "Infrastructure types are `internal`, except the registration entry points, any"
O-449 | §4 Placement | Exception to `internal`: any type the dispatcher's generated code in `Api` must name | BE-34, BE-107 | source: "Infrastructure types are `internal`, except the registration entry points, any"
O-450 | §4 Placement | Exception to `internal`: the types those expose in a public constructor or member - such as the module's `DbContext` (taken by a query handler's constructor) and the read types the `DbContext` exposes | BE-34, BE-107 | source: "Infrastructure types are `internal`, except the registration entry points, any"
O-451 | §4 Placement | The source-generated dispatcher names each concrete handler in its generated registration, so every concrete query handler is public | BE-34, BE-107 | source: "The source-generated dispatcher names each concrete handler in its generated"
O-452 | §4 Placement | `InternalsVisibleTo` names only test assemblies and the proxy assembly of the test project's mocking library, such as `DynamicProxyGenAssembly2` | BE-34, BE-107 | source: "`InternalsVisibleTo` names only test assemblies and the proxy assembly of"
O-453 | §4 Placement | Infrastructure types MUST live under `Persistence/`, `Queries/`, `Messaging/`, or `External/` | BE-35 | source: "Infrastructure types MUST live under `Persistence/`, `Queries/`, `Messaging/`, or `External/`,"
O-454 | §4 Placement | Exception: the registration entry point sits at the Infrastructure project root | BE-35 | source: "Infrastructure types MUST live under `Persistence/`, `Queries/`, `Messaging/`, or `External/`,"
O-455 | §4 Placement | A new top-level Infrastructure folder is added to the check's allowed set | BE-35 | source: "A new top-level folder there is added to the check's"
O-456 | §4 Placement | A new top-level Infrastructure folder carries its justification in the PR | BE-35 | source: "A new top-level folder there is added to the check's"
O-457 | §4 Placement | `Infrastructure/Queries/<Feature>/` MUST mirror the feature folder name used in `Application` | BE-36 | source: "`Infrastructure/Queries/<Feature>/` MUST mirror the feature folder name used in `Application`."

#### 5. Persistence

O-458 | §5 | Persistence uses EF Core | - | source: "EF Core, code-first (migrations: §5 *Migrations*)."
O-459 | §5 | Persistence is code-first (migrations per §5 Migrations) | - | source: "EF Core, code-first (migrations: §5 *Migrations*)."
O-460 | §5 | One `DbContext` per module | BE-37 | source: "One `DbContext` per module, one schema per module."
O-461 | §5 | One schema per module | BE-37 | source: "One `DbContext` per module, one schema per module."
O-462 | §5 | A `DbContext` MUST NOT map another module's tables | BE-37 | source: "A `DbContext` MUST NOT map another module's tables."
O-463 | §5 | Mapping MUST live in `IEntityTypeConfiguration<T>` classes | BE-38 | source: "Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure, and the"
O-464 | §5 | Mapping classes live in Infrastructure | BE-38 | source: "Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure, and the"
O-465 | §5 | The domain MUST NOT know it is persisted | BE-38 | source: "Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure, and the"
O-466 | §5 | A Domain type MUST NOT carry a mapping attribute, an EF attribute included | BE-38 | source: "Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure, and the"
O-467 | §5 | A Domain type the EF model maps MUST NOT have a public parameterless constructor | BE-38 | source: "Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure, and the"
O-468 | §5 | A Domain type the EF model maps MUST NOT have a public setter | BE-38 | source: "Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure, and the"
O-469 | §5 | Use EF's private-constructor and backing-field support | BE-38 | source: "Mapping MUST live in `IEntityTypeConfiguration<T>` classes in Infrastructure, and the"
O-470 | §5 | If a mapping cannot be expressed without changing the domain type, the mapping is wrong, not the domain type | - | source: "If a mapping cannot be expressed without changing the domain"
O-471 | §5 | Lazy loading MUST be disabled | - | source: "Lazy loading MUST be disabled."
O-472 | §5 | Integration tests run against real SQL Server via Testcontainers | BE-39 | source: "Integration tests run against real SQL Server via Testcontainers."
O-473 | §5 | No other provider may stand in for SQL Server - the EF InMemory provider, SQLite in memory, or any other | BE-39 | source: "No other provider may stand in for it - the"
O-474 | §5 | Every aggregate MUST carry an application-managed version | - | source: "Every aggregate MUST carry an application-managed version, which the aggregate"
O-475 | §5 | The aggregate increments the version on every change to itself or to anything inside it | - | source: "Every aggregate MUST carry an application-managed version, which the aggregate"
O-476 | §5 | The version is mapped as the aggregate's concurrency token | - | source: "Every aggregate MUST carry an application-managed version, which the aggregate"
O-477 | §5 | A concurrency conflict MUST surface as an outcome, never as an exception above the persistence adapter | - | source: "A concurrency conflict or a unique-key violation MUST surface as"
O-478 | §5 | A unique-key violation MUST surface as an outcome, never as an exception above the persistence adapter | - | source: "A concurrency conflict or a unique-key violation MUST surface as"
O-479 | §5 | The repository or unit of work that receives `DbUpdateConcurrencyException`, or the provider's unique-key error, catches that exception, where it is raised | - | source: "A concurrency conflict or a unique-key violation MUST surface as"
O-480 | §5 | It catches that exception and nothing else | - | source: "A concurrency conflict or a unique-key violation MUST surface as"
O-481 | §5 | It translates the caught exception (core §6; §4 Pipeline behaviors) | - | source: "A concurrency conflict or a unique-key violation MUST surface as"
O-482 | §5 | SQL Server is the default store | - | source: "SQL Server is the default store."
O-483 | §5 | A second store requires a documented reason in the module's README | - | source: "A second store requires a documented reason in the module's"

##### Migrations

O-484 | §5 Migrations | Every schema change ships as an EF Core migration | - | source: "Every schema change ships as an EF Core migration, generated,"
O-485 | §5 Migrations | The migration is generated, reviewed, and committed | - | source: "Every schema change ships as an EF Core migration, generated,"
O-486 | §5 Migrations | The migration is committed with the change that requires it | - | source: "Every schema change ships as an EF Core migration, generated,"
O-487 | §5 Migrations | A committed migration MUST NOT be edited | BE-87 | source: "A committed migration MUST NOT be edited."
O-488 | §5 Migrations | Correct a committed migration with a new migration | - | source: "Correct it with a new migration."
O-489 | §5 Migrations | Migrations are forward-only | - | source: "Migrations are forward-only."
O-490 | §5 Migrations | A down migration MUST NOT be relied on above local development | - | source: "A down migration MUST NOT be relied on above local"
O-491 | §5 Migrations | Recovery is a new forward migration, written deliberately | - | source: "A down migration MUST NOT be relied on above local"
O-492 | §5 Migrations | A migration MUST be safe to run while the previous version of the application is still serving | - | source: "A migration MUST be safe to run while the previous"
O-493 | §5 Migrations | A destructive change MUST be split across releases | - | source: "A destructive change - dropping a column or table, narrowing"
O-494 | §5 Migrations | Definition of destructive change: dropping a column or table, narrowing a type, adding a `NOT NULL` column with no default | - | source: "A destructive change - dropping a column or table, narrowing"
O-495 | §5 Migrations | Destructive-change sequence: add the new, write both, backfill, stop reading the old, then drop | - | source: "A destructive change - dropping a column or table, narrowing"
O-496 | §5 Migrations | A data migration MUST be idempotent | - | source: "A data migration MUST be idempotent and re-runnable."
O-497 | §5 Migrations | A data migration MUST be re-runnable | - | source: "A data migration MUST be idempotent and re-runnable."
O-498 | §5 Migrations | A migration MUST NOT contain business logic | BE-88 | source: "A migration MUST NOT contain business logic and MUST NOT"
O-499 | §5 Migrations | A migration MUST NOT call application code | BE-88 | source: "A migration MUST NOT contain business logic and MUST NOT"
O-500 | §5 Migrations | Seed data MUST be limited to reference data the domain cannot function without | - | source: "Seed data MUST be limited to reference data the domain"
O-501 | §5 Migrations | Demo records MUST NOT be seeded by a migration | - | source: "Demo records and test fixtures MUST NOT be seeded by"
O-502 | §5 Migrations | Test fixtures MUST NOT be seeded by a migration | - | source: "Demo records and test fixtures MUST NOT be seeded by"

##### Repositories and the aggregate graph

O-503 | §5 Repositories | A repository serves exactly one aggregate root | - | source: "A repository serves exactly one aggregate root."
O-504 | §5 Repositories | A repository's interface is declared in `Domain` beside the root it serves | - | source: "Its interface is declared in `Domain` beside the root it"
O-505 | §5 Repositories | A repository's implementation lives in `Infrastructure` | - | source: "Its interface is declared in `Domain` beside the root it"
O-506 | §5 Repositories | A repository implementation depends on the module's `DbContext` | - | source: "Its interface is declared in `Domain` beside the root it"
O-507 | §5 Repositories | A repository MUST load the complete aggregate in one operation - the root, its entities, and its value objects | - | source: "A repository MUST load the complete aggregate in one operation"
O-508 | §5 Repositories | Partial loads are forbidden | - | source: "Partial loads are forbidden."
O-509 | §5 Repositories | The repository owns the graph shape | - | source: "The repository owns the graph shape."
O-510 | §5 Repositories | A command handler MUST NOT specify includes | - | source: "A command handler MUST NOT specify includes, projections, or tracking"
O-511 | §5 Repositories | A command handler MUST NOT specify projections | - | source: "A command handler MUST NOT specify includes, projections, or tracking"
O-512 | §5 Repositories | A command handler MUST NOT specify tracking behavior | - | source: "A command handler MUST NOT specify includes, projections, or tracking"
O-513 | §5 Repositories | A command handler MUST NOT know how the aggregate is assembled | - | source: "A command handler MUST NOT specify includes, projections, or tracking"
O-514 | §5 Repositories | Persistence is whole-aggregate | - | source: "Persistence is whole-aggregate."
O-515 | §5 Repositories | Add, update, and remove operate on the root | - | source: "Add, update, and remove operate on the root; the aggregate's"
O-516 | §5 Repositories | The aggregate's internals are persisted and deleted with the root | - | source: "Add, update, and remove operate on the root; the aggregate's"
O-517 | §5 Repositories | A repository MUST NOT expose `IQueryable` | BE-40 | source: "A repository MUST NOT expose `IQueryable`, `DbSet`, or any other"
O-518 | §5 Repositories | A repository MUST NOT expose `DbSet` | BE-40 | source: "A repository MUST NOT expose `IQueryable`, `DbSet`, or any other"
O-519 | §5 Repositories | A repository MUST NOT expose any other EF type | BE-40 | source: "A repository MUST NOT expose `IQueryable`, `DbSet`, or any other"
O-520 | §5 Repositories | A generic `IRepository<TRoot, TId>` MAY provide the universal operations: get by id, add, remove | - | source: "A generic `IRepository<TRoot, TId>` MAY provide the universal operations: get"
O-521 | §5 Repositories | Anything beyond get by id, add, remove MUST be declared on a per-aggregate interface | - | source: "Anything beyond that MUST be declared on a per-aggregate interface"
O-522 | §5 Repositories | Per-aggregate repository methods have intention-revealing names | - | source: "Anything beyond that MUST be declared on a per-aggregate interface"

##### EF Core mapping for DDD

O-523 | §5 EF mapping | Value Objects MUST be mapped as owned types, as complex types, or (for a single-value value object) through a value converter | - | source: "Value Objects MUST be mapped as owned types, as complex"
O-524 | §5 EF mapping | Value Objects are never mapped as entities with their own identity | - | source: "Value Objects MUST be mapped as owned types, as complex"
O-525 | §5 EF mapping | Strongly-typed ids MUST be mapped with value converters | - | source: "Strongly-typed ids MUST be mapped with value converters."
O-526 | §5 EF mapping | Private collections MUST be mapped to their backing field | - | source: "Private collections MUST be mapped to their backing field."

##### Redis

O-527 | §5 Redis | Redis rules are in `annex-caching.md` | - | source: "In `annex-caching.md`."

##### Read-side caching

O-528 | §5 Read-side caching | Read-side caching rules are in `annex-caching.md` | - | source: "In `annex-caching.md`."

#### 6. Authorization

O-529 | §6 | Every dispatchable operation is declared once | - | source: "Every dispatchable operation is declared once, in the slice that"
O-530 | §6 | An operation is declared in the slice that owns it | - | source: "Every dispatchable operation is declared once, in the slice that"
O-531 | §6 | (illustration) Fenced C# operation declaration example exists, first comment `// Illustrative.` | - | source: "// Illustrative."
O-532 | §6 | (from illustration) An operation declaration carries `Id` of the form `"<module>.<operation>"` | - | source: "Id:          "<module>.<operation>","
O-533 | §6 | (from illustration) An operation declaration carries `Policy` (e.g. `Policies.<Module>.Write`) | - | source: "Policy:      Policies.<Module>.Write,"
O-534 | §6 | (from illustration) An operation declaration carries `Kind` (`OperationKind.Command` / query) | - | source: "Kind:        OperationKind.Command,"
O-535 | §6 | (from illustration comment) An operation declaration carries `Idempotency`: `ClientSuppliedId`, or `Natural`, or `Key` (§4) | - | source: "Idempotency: IdempotencyMechanism.ClientSuppliedId,   // or Natural, or Key (§4)"
O-536 | §6 | (from illustration comment) An operation declaration carries `Transaction`: `TransactionRequirement.Required`, or `None` for an external write (§4) | - | source: "Transaction: TransactionRequirement.Required,         // None: an external write (§4)"
O-537 | §6 | (from illustration comment) An operation declaration MAY carry `RateLimit` (e.g. `RateLimits.<Stricter>`), optional (§9) | - | source: "RateLimit:   RateLimits.<Stricter>,                   // optional (§9)"
O-538 | §6 | (from illustration comment) An operation declaration carries `Human`: `HumanRequirement.Required` makes the operation unreachable without a live user | - | source: "Human:       HumanRequirement.Required);              // unreachable without a live user"
O-539 | §6 | Every Command MUST declare a named policy | BE-50 | source: "Every Command and Query MUST declare a named policy."
O-540 | §6 | Every Query MUST declare a named policy | BE-50 | source: "Every Command and Query MUST declare a named policy."
O-541 | §6 | Bare `[Authorize]` is a defect | BE-50 | source: "Bare `[Authorize]` and unattributed endpoints are defects."
O-542 | §6 | Unattributed endpoints are a defect | BE-50 | source: "Bare `[Authorize]` and unattributed endpoints are defects."
O-543 | §6 | `AuthorizationOptions.FallbackPolicy` MUST deny | BE-51 | source: "`AuthorizationOptions.FallbackPolicy` MUST deny."
O-544 | §6 | Policies are evaluated by the authorization behavior | - | source: "Policies are evaluated by the authorization behavior via `IAuthorizationService`, against"
O-545 | §6 | Policies are evaluated via `IAuthorizationService` | - | source: "Policies are evaluated by the authorization behavior via `IAuthorizationService`, against"
O-546 | §6 | Policies are evaluated against the explicit principal from the dispatch context | - | source: "Policies are evaluated by the authorization behavior via `IAuthorizationService`, against"
O-547 | §6 | The object check (core §7) is made after the load and before the handler acts | - | source: "The object check (core §7) is made after the load"
O-548 | §6 | The object check goes through a resource-authorization port declared in `Platform.Application` | - | source: "The object check (core §7) is made after the load"
O-549 | §6 | The resource-authorization port takes the resource and the policy | - | source: "The object check (core §7) is made after the load"
O-550 | §6 | The resource-authorization port reads the acting principal from the dispatch context | - | source: "The object check (core §7) is made after the load"
O-551 | §6 | The port's implementation calls `IAuthorizationService.AuthorizeAsync(principal, resource, policy)` | - | source: "Its implementation calls `IAuthorizationService.AuthorizeAsync(principal, resource, policy)`."
O-552 | §6 | Each module's resource authorization handlers are `AuthorizationHandler<TRequirement, TResource>` over its aggregates | - | source: "Each module's resource authorization handlers - `AuthorizationHandler<TRequirement, TResource>` over its"
O-553 | §6 | Resource authorization handlers live in the module's `Infrastructure` as framework extension points (BE-34) | - | source: "Each module's resource authorization handlers - `AuthorizationHandler<TRequirement, TResource>` over its"
O-554 | §6 | Resource authorization handlers read the aggregate through its public members | - | source: "Each module's resource authorization handlers - `AuthorizationHandler<TRequirement, TResource>` over its"
O-555 | §6 | Resource authorization handlers are registered by the module's registration entry point | - | source: "Each module's resource authorization handlers - `AuthorizationHandler<TRequirement, TResource>` over its"
O-556 | §6 | A query applies the principal's record scope in its read - a global query filter on its read types, or a predicate in its SQL | - | source: "A query applies the principal's record scope in its read"
O-557 | §6 | A query never applies record scope by filtering the result | - | source: "A query applies the principal's record scope in its read"
O-558 | §6 | Definition: record scope is the records the principal may see, such as its tenant's or its own | - | source: "The scope - the records the principal may see, such"
O-559 | §6 | The record scope is resolved at the composition root, or by the authority adapter, into the dispatch context | - | source: "The scope - the records the principal may see, such"
O-560 | §6 | The read takes the record scope from the dispatch context | - | source: "The scope - the records the principal may see, such"
O-561 | §6 | A transport-level authorization filter - `[Authorize(<policy>)]`, middleware, or a vendor package's equivalent - MAY enforce in addition | - | source: "A transport-level authorization filter - `[Authorize(<policy>)]`, middleware, or a vendor"
O-562 | §6 | A transport-level authorization filter MUST NOT be the only enforcement point | - | source: "A transport-level authorization filter - `[Authorize(<policy>)]`, middleware, or a vendor"
O-563 | §6 | Every operation's policy decision MUST be reachable from the authorization behavior (core §7) | - | source: "Every operation's policy decision MUST be reachable from the authorization"
O-564 | §6 | `IHttpContextAccessor` MUST NOT be referenced outside the composition root (core §7) | BE-52 | source: "`IHttpContextAccessor` MUST NOT be referenced outside the composition root (core"
O-565 | §6 | Identity-provider claims are mapped to policies at the composition root | BE-53 | source: "Identity-provider claims are mapped to policies at the composition root,"
O-566 | §6 | Externally-sourced permissions are mapped to policies in the adapter that wraps the authority | BE-53 | source: "Identity-provider claims are mapped to policies at the composition root,"
O-567 | §6 | Application code MUST NOT read raw claims | BE-53 | source: "Application and domain code MUST NOT read raw claims."
O-568 | §6 | Domain code MUST NOT read raw claims | BE-53 | source: "Application and domain code MUST NOT read raw claims."
O-569 | §6 | A claim read from a token before its signature is validated MAY select how the token is validated - such as which issuer's keys, or which remote validation endpoint | - | source: "A claim read from a token before its signature is"
O-570 | §6 | A pre-validation claim MUST NOT be trusted for anything else: not identity, not tenancy, not a permission lookup | - | source: "A claim read from a token before its signature is"
O-571 | §6 | Once validated, the claim that made the selection MUST agree with the validated token | - | source: "Once the token is validated, the claim that made the"
O-572 | §6 | If the selecting claim disagrees with the validated token, the token is refused | - | source: "Once the token is validated, the claim that made the"
O-573 | §6 | Where permissions come from - a token claim, a directory, a remote authorization service - is a composition-root concern | - | source: "Where permissions come from - a token claim, a directory,"
O-574 | §6 | Application and domain code name a policy | - | source: "Application and domain code name a policy, never a permission"
O-575 | §6 | Application and domain code never name a permission source | - | source: "Application and domain code name a policy, never a permission"
O-576 | §6 | Application and domain code never name a role literal | - | source: "Application and domain code name a policy, never a permission"
O-577 | §6 | An external authority is reached through one Infrastructure adapter, behind a port | BE-102 | source: "An external authority is reached through one Infrastructure adapter, behind"
O-578 | §6 | The authority adapter wraps the authority's client | BE-102 | source: "An external authority is reached through one Infrastructure adapter, behind"
O-579 | §6 | The authority adapter's project declares the authority's SDK | BE-102 | source: "An external authority is reached through one Infrastructure adapter, behind"
O-580 | §6 | The adapter translates the authority's permission codes to the application's policy vocabulary | BE-102 | source: "The adapter translates the authority's permission codes to the application's"
O-581 | §6 | The adapter translates the authority's failures into outcomes (core §6) | BE-102 | source: "The adapter translates the authority's permission codes to the application's"
O-582 | §6 | The authority's types and codes MUST NOT be referenced outside that adapter's project | BE-102 | source: "The authority's types and codes MUST NOT be referenced outside"

##### External authorities

O-583 | §6 External authorities | Which identity provider and which authority the organization runs is recorded in `DECISIONS.md`, not in this law | - | source: "Which provider and which authority the organization runs is recorded"
O-584 | §6 External authorities | When a request depends on a source outside the process (one that validates the token, or one that answers for permissions), that source's unavailability MUST deny | - | source: "When a request depends on a source outside the process"
O-585 | §6 External authorities | An unreachable, timing-out, or erroring authority MUST NOT grant | - | source: "An unreachable, timing-out, or erroring authority MUST NOT grant, MUST"
O-586 | §6 External authorities | An unreachable, timing-out, or erroring authority MUST NOT accept the token unvalidated | - | source: "An unreachable, timing-out, or erroring authority MUST NOT grant, MUST"
O-587 | §6 External authorities | An unreachable, timing-out, or erroring authority MUST NOT fall back to a default permission set | - | source: "An unreachable, timing-out, or erroring authority MUST NOT grant, MUST"
O-588 | §6 External authorities | An external authority's decision (validation result or permission decision) MAY be cached | BE-100 | source: "An external authority's decision - a validation result or a"
O-589 | §6 External authorities | The cached decision is keyed by the acting principal | BE-100 | source: "An external authority's decision - a validation result or a"
O-590 | §6 External authorities | The cache lifetime MUST be bounded | BE-100 | source: "Its lifetime MUST be bounded and declared in an options"
O-591 | §6 External authorities | The cache lifetime MUST be declared in an options class | BE-100 | source: "Its lifetime MUST be bounded and declared in an options"
O-592 | §6 External authorities | The cache lifetime SHOULD be configurable | - | source: "The lifetime SHOULD be configurable."
O-593 | §6 External authorities | Where the authority's client caches for a fixed period it does not expose, the declared value states that period | - | source: "Where the authority's client caches for a fixed period it"
O-594 | §6 External authorities | ... and the declared value MUST NOT differ from that fixed period | - | source: "Where the authority's client caches for a fixed period it"
O-595 | §6 External authorities | An unexpired cache entry MAY be served while the authority is unreachable | - | source: "An unexpired entry MAY be served while the authority is"
O-596 | §6 External authorities | An expired entry MUST NOT be served | - | source: "An expired entry MUST NOT be served, and its lifetime"
O-597 | §6 External authorities | An entry's lifetime MUST NOT be extended because the authority cannot be reached | - | source: "An expired entry MUST NOT be served, and its lifetime"
O-598 | §6 External authorities | Externally-resolved permissions are the user's permissions, not the caller's | - | source: "Externally-resolved permissions are the user's permissions, not the caller's."
O-599 | §6 External authorities | The permission lookup MUST be keyed on the acting principal from the dispatch context | - | source: "The lookup MUST be keyed on the acting principal from"
O-600 | §6 External authorities | The permission lookup is never keyed on the HTTP request | - | source: "The lookup MUST be keyed on the acting principal from"
O-601 | §6 External authorities | The intersection of external permissions with an agent's manifest is in `annex-agents.md` | - | source: "Their intersection with an agent's manifest is in `annex-agents.md`."

#### 7. Data Classification

##### Declaring a class

O-602 | §7 Declaring | A class is declared on the type that carries the value - the Value Object or the strongly-typed id in `Domain` | BE-54 | source: "A class is declared on the type that carries the"
O-603 | §7 Declaring | A class is also declared on the module's published type for the same concept in `Contracts` | BE-54 | source: "A class is declared on the type that carries the"
O-604 | §7 Declaring | An architecture test MUST prove the Domain and Contracts declarations agree (core §8) | BE-54 | source: "An architecture test MUST prove the two agree (core §8)."
O-605 | §7 Declaring | Where a property is projected from a classified source (such as a column a query handler reads into a `string`), the declaration names that source by its `Contracts` twin (§3) | BE-54 | source: "Where a property is projected from a classified source, such"
O-606 | §7 Declaring | The same architecture test proves the projected property and its source agree | BE-54 | source: "Where a property is projected from a classified source, such"
O-607 | §7 Declaring | A class is a `DataClassification` from Microsoft.Extensions.Compliance | BE-54 | source: "A class is a `DataClassification` from Microsoft.Extensions.Compliance."
O-608 | §7 Declaring | The solution's taxonomy is defined once, in `SharedKernel` | BE-54 | source: "The solution's taxonomy is defined once in `SharedKernel`: one attribute"
O-609 | §7 Declaring | One attribute derived from `DataClassificationAttribute` per class of core §8: `Public`, `Internal`, `Confidential`, `Restricted` | BE-54 | source: "The solution's taxonomy is defined once in `SharedKernel`: one attribute"
O-610 | §7 Declaring | An architecture test MUST assert that every property reachable from an egress boundary resolves to a class | BE-55 | source: "An architecture test MUST assert that every property reachable from"
O-611 | §7 Declaring | A property whose type carries no declaration is `Restricted` | BE-55 | source: "A property whose type carries no declaration is `Restricted`, and"
O-612 | §7 Declaring | An undeclared property fails at the boundary | BE-55 | source: "A property whose type carries no declaration is `Restricted`, and"
O-613 | §7 Declaring | The class MUST be emitted into the OpenAPI document as `x-data-class` (core §4 Wire contract) | BE-83 | source: "The class MUST be emitted into the OpenAPI document as"
O-614 | §7 Declaring | `x-data-class` is emitted on every property of every schema the document contains - Request and Response DTOs, problem details, the page | BE-83 | source: "The class MUST be emitted into the OpenAPI document as"
O-615 | §7 Declaring | `x-data-class` is emitted on the schema of every parameter | BE-83 | source: "The class MUST be emitted into the OpenAPI document as"
O-616 | §7 Declaring | The emitter MUST fail rather than emit a property whose class it cannot resolve | BE-83 | source: "The emitter MUST fail rather than emit a property whose"
O-617 | §7 Declaring | (illustration) Fenced C# `[Restricted] NationalId` example exists, first comment `// Illustrative.`; `ToString()` returns `"***"` with comment "never the raw value" | - | source: "// Illustrative."
O-618 | §7 Declaring | A `Confidential` or `Restricted` type MUST redact in `ToString()` | BE-56 | source: "A `Confidential` or `Restricted` type MUST redact in `ToString()` and"
O-619 | §7 Declaring | A `Confidential` or `Restricted` type MUST NOT define an implicit conversion to `string` | BE-56 | source: "A `Confidential` or `Restricted` type MUST redact in `ToString()` and"

##### Logging (classification)

O-620 | §7 Logging | Logging is structured | BE-57 | source: "Logging is structured, through source-generated `LoggerMessage` methods."
O-621 | §7 Logging | Logging goes through source-generated `LoggerMessage` methods | BE-57 | source: "Logging is structured, through source-generated `LoggerMessage` methods."
O-622 | §7 Logging | A message template MUST be a constant | BE-57 | source: "A message template MUST be a constant and MUST NOT"
O-623 | §7 Logging | A message template MUST NOT interpolate a value | BE-57 | source: "A message template MUST be a constant and MUST NOT"
O-624 | §7 Logging | A log scope is begun with a state object (`ILogger.BeginScope<TState>`), never a template | BE-57 | source: "A message template MUST be a constant and MUST NOT"
O-625 | §7 Logging | Redaction is Microsoft's: `EnableRedaction()` on the logging builder | - | source: "Redaction is Microsoft's: `EnableRedaction()` on the logging builder, and the"
O-626 | §7 Logging | The erasing redactor is used for `Confidential` and `Restricted` | - | source: "Redaction is Microsoft's: `EnableRedaction()` on the logging builder, and the"
O-627 | §7 Logging | Every parameter of a logging method carries its class attribute | - | source: "Every parameter of a logging method carries its class attribute,"
O-628 | §7 Logging | Exception: the first `ILogger`, `LogLevel` and `Exception` parameters need no class attribute | - | source: "Every parameter of a logging method carries its class attribute,"
O-629 | §7 Logging | An exception passed to a logging method is governed by core §8's exception rule and the §10 processor, not by redaction | - | source: "An exception passed to a logging method is governed by"
O-630 | §7 Logging | Every project that declares a logging method references `Microsoft.Extensions.Telemetry.Abstractions` | - | source: "Every project that declares a logging method references `Microsoft.Extensions.Telemetry.Abstractions`,"
O-631 | §7 Logging | That package's logging generator replaces the in-box one and reads the class attributes | - | source: "Every project that declares a logging method references `Microsoft.Extensions.Telemetry.Abstractions`,"
O-632 | §7 Logging | `PersonId` is loggable (core §8) | - | source: "`PersonId` is loggable; `NationalId` is not (core §8)."
O-633 | §7 Logging | `NationalId` is not loggable (core §8) | - | source: "`PersonId` is loggable; `NationalId` is not (core §8)."
O-634 | §7 Logging | Exception messages MUST NOT echo a classified value | - | source: "Exception messages, `ProblemDetails`, and validation errors MUST NOT echo a"
O-635 | §7 Logging | `ProblemDetails` MUST NOT echo a classified value | - | source: "Exception messages, `ProblemDetails`, and validation errors MUST NOT echo a"
O-636 | §7 Logging | Validation errors MUST NOT echo a classified value | - | source: "Exception messages, `ProblemDetails`, and validation errors MUST NOT echo a"

##### The model-visible surface

O-637 | §7 Model-visible surface | Model-visible surface rules are in `annex-agents.md` | - | source: "In `annex-agents.md`."

##### Cache, events, and storage

O-638 | §7 Cache, events, storage | Classification cache rules are in `annex-caching.md` | - | source: "Its cache rules: `annex-caching.md`."
O-639 | §7 Cache, events, storage | Integration events MUST NOT carry `Restricted` payloads | BE-61 | source: "Integration events and outbox rows MUST NOT carry `Restricted` payloads."
O-640 | §7 Cache, events, storage | Outbox rows MUST NOT carry `Restricted` payloads | BE-61 | source: "Integration events and outbox rows MUST NOT carry `Restricted` payloads."
O-641 | §7 Cache, events, storage | Reference `Restricted` data by id instead | BE-61 | source: "Reference by id."
O-642 | §7 Cache, events, storage | `Restricted` columns MUST be encrypted at rest | - | source: "`Restricted` columns MUST be encrypted at rest."
O-643 | §7 Cache, events, storage | The encryption MUST live in Infrastructure - in the mapping, or below it in the database driver, such as SQL Server Always Encrypted | - | source: "The encryption MUST live in Infrastructure - in the mapping,"
O-644 | §7 Cache, events, storage | The encryption MUST be transparent to every path that reads the column | - | source: "The encryption MUST live in Infrastructure - in the mapping,"
O-645 | §7 Cache, events, storage | A read type that maps an encrypted column carries the same converter | - | source: "A read type that maps an encrypted column carries the"
O-646 | §7 Cache, events, storage | SQL (§4) MUST NOT read a column encrypted in the mapping | - | source: "A read type that maps an encrypted column carries the"
O-647 | §7 Cache, events, storage | A column MUST NOT be encrypted in a form that does not support every operation a query performs on it (e.g. the range comparison keyset paging makes on its sort key, §4) | - | source: "A column MUST NOT be encrypted in a form that"
O-648 | §7 Cache, events, storage | The domain type MUST NOT know it is encrypted | - | source: "The domain type MUST NOT know it is encrypted."

#### 8. AI Agents

O-649 | §8 | AI agent rules are in `annex-agents.md` | - | source: "In `annex-agents.md`. An agent manifest is the one list"
O-650 | §8 | An agent manifest is the one list of operations an agent may invoke | - | source: "An agent manifest is the one list of operations an"

#### 9. Quotas and Rate Limits

O-651 | §9 | Limits attach to the caller and to the run, not to the transport | - | source: "Limits attach to the caller and to the run, not"
O-652 | §9 | Both caller limits and run limits are required | - | source: "Both are required, and neither substitutes for the other."
O-653 | §9 | Neither caller nor run limit substitutes for the other | - | source: "Both are required, and neither substitutes for the other."

##### Transport

O-654 | §9 Transport | Every endpoint MUST be covered by a rate limit | BE-70 | source: "Every endpoint MUST be covered by a rate limit: a"
O-655 | §9 Transport | Coverage is by a `GlobalLimiter`, or a named policy the endpoint registrar applies to every endpoint (§13) | BE-70 | source: "Every endpoint MUST be covered by a rate limit: a"
O-656 | §9 Transport | `[DisableRateLimiting]` is forbidden | BE-70 | source: "`[DisableRateLimiting]`, and any other way of removing an endpoint from"
O-657 | §9 Transport | Any other way of removing an endpoint from its limit is forbidden | BE-70 | source: "`[DisableRateLimiting]`, and any other way of removing an endpoint from"
O-658 | §9 Transport | Limits are keyed on the acting principal | - | source: "Limits are keyed on the acting principal, falling back to"
O-659 | §9 Transport | For anonymous endpoints, limits fall back to a client identity | - | source: "Limits are keyed on the acting principal, falling back to"
O-660 | §9 Transport | Keying on IP alone is not a limit | - | source: "Keying on IP alone is not a limit."
O-661 | §9 Transport | The endpoint registrar applies the write policy to every command endpoint | - | source: "The endpoint registrar applies the write policy to every command"
O-662 | §9 Transport | The operation declaration MAY name a stricter policy | - | source: "The endpoint registrar applies the write policy to every command"
O-663 | §9 Transport | The operation declaration MUST NOT remove the write policy | - | source: "The endpoint registrar applies the write policy to every command"
O-664 | §9 Transport | The stricter policy chains the write limiter with its own through `RateLimiter.CreateChained` inside the named policy, rather than replacing it | - | source: "The stricter policy chains the write limiter with its own"
O-665 | §9 Transport | An expensive read's operation declaration MUST name a tighter policy than the default | - | source: "An expensive read's operation declaration MUST name a tighter policy"
O-666 | §9 Transport | A rejection (core §4 Limits) carries `Retry-After` wherever the limiter can estimate when permits return - a token bucket, a fixed or a sliding window | - | source: "A rejection (core §4 *Limits*) carries `Retry-After` wherever the limiter"
O-667 | §9 Transport | A concurrency limiter's rejection does not carry `Retry-After` | - | source: "A rejection (core §4 *Limits*) carries `Retry-After` wherever the limiter"
O-668 | §9 Transport | A limit is enforced per instance unless a shared limiter enforces it | - | source: "A limit is enforced per instance unless a shared limiter"
O-669 | §9 Transport | A limit stated for the whole service is divided by the instance count or enforced by a shared limiter | - | source: "A limit is enforced per instance unless a shared limiter"
O-670 | §9 Transport | ASP.NET Core writes neither the status nor the header by default | - | source: "ASP.NET Core writes neither: `OnRejected` sets `429` and copies `MetadataName.RetryAfter`"
O-671 | §9 Transport | `OnRejected` sets `429` | - | source: "ASP.NET Core writes neither: `OnRejected` sets `429` and copies `MetadataName.RetryAfter`"
O-672 | §9 Transport | `OnRejected` copies `MetadataName.RetryAfter` into the `Retry-After` header | - | source: "ASP.NET Core writes neither: `OnRejected` sets `429` and copies `MetadataName.RetryAfter`"

##### Agent runs

O-673 | §9 Agent runs | Agent run limits are in `annex-agents.md` | - | source: "In `annex-agents.md`."

##### Configuration (limits)

O-674 | §9 Configuration | Limits and budgets MUST be configuration, not constants in code | BE-73 | source: "Limits and budgets MUST be configuration, not constants in code,"
O-675 | §9 Configuration | Limits and budgets MUST fail closed | BE-73 | source: "Limits and budgets MUST be configuration, not constants in code,"
O-676 | §9 Configuration | A missing or unparseable limit denies rather than admits | BE-73 | source: "Limits and budgets MUST be configuration, not constants in code,"
O-677 | §9 Configuration | Every limit MUST emit a metric when it is hit | - | source: "Every limit MUST emit a metric when it is hit."

#### 10. Observability

##### Correlation

O-678 | §10 Correlation | The correlation id MUST live in the dispatch context beside the principal | BE-90 | source: "The correlation id MUST live in the dispatch context beside"
O-679 | §10 Correlation | The correlation id is a required constructor parameter of the dispatch context | BE-90 | source: "The correlation id MUST live in the dispatch context beside"
O-680 | §10 Correlation | The correlation id's value is the current `Activity`'s trace id | - | source: "Its value is the current `Activity`'s trace id."
O-681 | §10 Correlation | Every problem details response carries the trace id as `traceId` (core §4 Wire contract) | - | source: "Every problem details response carries the trace id as `traceId`,"
O-682 | §10 Correlation | `traceId` is written from `Activity.Current.TraceId`, not from ASP.NET Core's default (`Activity.Id` or `TraceIdentifier`) | - | source: "Every problem details response carries the trace id as `traceId`,"
O-683 | §10 Correlation | Scope: where a request from outside the trust boundary can reach the application with the caller's `traceparent` (directly, or through an ingress that forwards the header) | - | source: "Where a request from outside the trust boundary can reach"
O-684 | §10 Correlation | In that case the application adopts no caller's trace | - | source: "Where a request from outside the trust boundary can reach"
O-685 | §10 Correlation | The composition root installs a `DistributedContextPropagator` that starts every such request's trace | - | source: "The composition root installs a `DistributedContextPropagator` that starts every such"
O-686 | §10 Correlation | That propagator records the caller's `traceparent` as a link (`Activity.AddLink`) | - | source: "The composition root installs a `DistributedContextPropagator` that starts every such"
O-687 | §10 Correlation | The composition root sets OpenTelemetry's default propagator to a plain `TraceContextPropagator` (`Sdk.SetDefaultTextMapPropagator`) | - | source: "The composition root installs a `DistributedContextPropagator` that starts every such"
O-688 | §10 Correlation | An outbox row MUST persist the trace context - `traceparent` and `tracestate` - of the transaction that wrote it | BE-91 | source: "An outbox row MUST persist the trace context - `traceparent`"
O-689 | §10 Correlation | The dispatcher MUST restore that trace context when delivering | - | source: "The dispatcher MUST restore that trace context when delivering."
O-690 | §10 Correlation | A background job MUST start a trace per execution | - | source: "A background job MUST start a trace per execution and"
O-691 | §10 Correlation | A background job MUST log a start event once per execution | - | source: "A background job MUST start a trace per execution and"
O-692 | §10 Correlation | The enricher supplies the background job's trace id | - | source: "A background job MUST start a trace per execution and"
O-693 | §10 Correlation | Correlation's agent rule is in `annex-agents.md` | - | source: "Its agent rule: `annex-agents.md`."

##### Logging (observability)

O-694 | §10 Logging | Code logs through `ILogger` and source-generated `LoggerMessage` methods | BE-93 | source: "Code logs through `ILogger` and source-generated `LoggerMessage` methods; Serilog is"
O-695 | §10 Logging | Serilog is the provider that writes the output | BE-93 | source: "Code logs through `ILogger` and source-generated `LoggerMessage` methods; Serilog is"
O-696 | §10 Logging | Serilog is registered on the logging builder: `builder.Logging.AddSerilog(logger, dispose: true)` | BE-93 | source: "Code logs through `ILogger` and source-generated `LoggerMessage` methods; Serilog is"
O-697 | §10 Logging | The Serilog logger is built from a `LoggerConfiguration` | BE-93 | source: "Code logs through `ILogger` and source-generated `LoggerMessage` methods; Serilog is"
O-698 | §10 Logging | The Serilog logger is never assigned to the static `Log.Logger` | BE-93 | source: "Code logs through `ILogger` and source-generated `LoggerMessage` methods; Serilog is"
O-699 | §10 Logging | Replacing `ILoggerFactory` - as `UseSerilog()` does - is forbidden | BE-93 | source: "Code logs through `ILogger` and source-generated `LoggerMessage` methods; Serilog is"
O-700 | §10 Logging | The logging behavior (§4) is the only place a message's dispatch is logged | BE-93 | source: "The logging behavior (§4) is the only place a message's"
O-701 | §10 Logging | A handler MUST NOT reference `ILogger` | BE-93 | source: "A handler MUST NOT reference `ILogger`."
O-702 | §10 Logging | Log levels have fixed meanings | - | source: "Levels have fixed meanings."
O-703 | §10 Logging | `Error` means a defect that needs someone | - | source: "`Error` is a defect that needs someone."
O-704 | §10 Logging | `Warning` means degraded but handled | - | source: "`Warning` is degraded but handled."
O-705 | §10 Logging | `Information` means a business-meaningful event | - | source: "`Information` is a business-meaningful event."
O-706 | §10 Logging | `Debug` means developer detail | - | source: "`Debug` is developer detail and MUST NOT be enabled in"
O-707 | §10 Logging | `Debug` MUST NOT be enabled in production | - | source: "`Debug` is developer detail and MUST NOT be enabled in"
O-708 | §10 Logging | An expected business rejection MUST NOT be logged at `Error` | - | source: "An expected business rejection MUST NOT be logged at `Error`."
O-709 | §10 Logging | A `Result` failure is an outcome, not a fault | - | source: "A `Result` failure is an outcome, not a fault."
O-710 | §10 Logging | Module, message type, principal id, and correlation id are supplied by the enricher | - | source: "Module, message type, principal id, and correlation id are supplied"
O-711 | §10 Logging | A call site MUST NOT restate module, message type, principal id, or correlation id | - | source: "A call site MUST NOT restate them."

##### Metrics and traces

O-712 | §10 Metrics | Metrics and traces use OpenTelemetry (core §9) | - | source: "OpenTelemetry (core §9)."
O-713 | §10 Metrics | An OpenTelemetry processor removes `exception.message` from every exception whose type the solution does not declare (core §8) | - | source: "An OpenTelemetry processor, and its equivalent in the logging pipeline,"
O-714 | §10 Metrics | The equivalent processor exists in the logging pipeline and does the same | - | source: "An OpenTelemetry processor, and its equivalent in the logging pipeline,"
O-715 | §10 Metrics | The processor keeps `exception.type` and the stack trace | - | source: "An OpenTelemetry processor, and its equivalent in the logging pipeline,"
O-716 | §10 Metrics | Automatic instrumentation passes through the same processors | - | source: "Automatic instrumentation passes through the same processors."
O-717 | §10 Metrics | Every pipeline behavior MUST emit a span | - | source: "Every pipeline behavior MUST emit a span."
O-718 | §10 Metrics | Authorization, validation, and handler time MUST be separable in a trace | - | source: "Authorization, validation, and handler time MUST be separable in a"
O-719 | §10 Metrics | Metrics MUST cover dispatch count per message type | - | source: "Metrics MUST cover dispatch count and duration per message type,"
O-720 | §10 Metrics | Metrics MUST cover dispatch duration per message type | - | source: "Metrics MUST cover dispatch count and duration per message type,"
O-721 | §10 Metrics | Metrics MUST cover authorization denials | - | source: "Metrics MUST cover dispatch count and duration per message type,"
O-722 | §10 Metrics | Metrics MUST cover validation failures | - | source: "Metrics MUST cover dispatch count and duration per message type,"
O-723 | §10 Metrics | Metrics MUST cover outbox depth | - | source: "Metrics MUST cover dispatch count and duration per message type,"
O-724 | §10 Metrics | Metrics MUST cover outbox delivery lag | - | source: "Metrics MUST cover dispatch count and duration per message type,"
O-725 | §10 Metrics | Metrics MUST cover cache hit rate | - | source: "Metrics MUST cover dispatch count and duration per message type,"
O-726 | §10 Metrics | Metrics MUST cover limit and budget rejections (§9) | - | source: "Metrics MUST cover dispatch count and duration per message type,"
O-727 | §10 Metrics | Metrics MUST cover agent run duration | - | source: "Metrics MUST cover dispatch count and duration per message type,"
O-728 | §10 Metrics | Metrics MUST cover agent run spend | - | source: "Metrics MUST cover dispatch count and duration per message type,"
O-729 | §10 Metrics | Metric dimensions MUST come from a declared closed set of keys | BE-94 | source: "Metric dimensions MUST come from a declared closed set of"
O-730 | §10 Metrics | Metrics are exported with exemplars | - | source: "Metrics are exported with exemplars, sampled by the trace (`ExemplarFilterType.TraceBased`)."
O-731 | §10 Metrics | Exemplars are sampled by the trace (`ExemplarFilterType.TraceBased`) | - | source: "Metrics are exported with exemplars, sampled by the trace (`ExemplarFilterType.TraceBased`)."
O-732 | §10 Metrics | Liveness MUST NOT depend on a downstream | - | source: "Liveness MUST NOT depend on a downstream."
O-733 | §10 Metrics | Readiness MUST check the database | - | source: "Readiness MUST check the database and the outbox dispatcher."
O-734 | §10 Metrics | Readiness MUST check the outbox dispatcher | - | source: "Readiness MUST check the database and the outbox dispatcher."

##### Audit trail

O-735 | §10 Audit | The audit trail (core §9) is a store no application role may update | - | source: "The audit trail (core §9) is a store no application"
O-736 | §10 Audit | The audit trail is a store no application role may delete from | - | source: "The audit trail (core §9) is a store no application"
O-737 | §10 Audit | The audit trail is written through one port, `IAuditTrail` | - | source: "The audit trail (core §9) is a store no application"
O-738 | §10 Audit | `IAuditTrail` is declared in `Platform.Application` | - | source: "The audit trail (core §9) is a store no application"
O-739 | §10 Audit | `IAuditTrail` is implemented in `Platform.Infrastructure` | - | source: "The audit trail (core §9) is a store no application"
O-740 | §10 Audit | The audit trail is one table in each module's schema, for the success records that module's unit of work writes (§5) | - | source: "It is one table in each module's schema, for the"
O-741 | §10 Audit | Plus one audit table the port's own context writes | - | source: "It is one table in each module's schema, for the"
O-742 | §10 Audit | A persisting command's success is recorded through its unit of work, as the outbox row is | - | source: "A persisting command's success is recorded through its unit of"
O-743 | §10 Audit | Every other audit record is written through the port, outside any unit of work | - | source: "Every other record is written through the port outside any"
O-744 | §10 Audit | Audit record (via port): a denial, whether by the policy or by the object check | - | source: "Every other record is written through the port outside any"
O-745 | §10 Audit | Audit record (via port): a command's failure, written after its rollback | - | source: "Every other record is written through the port outside any"
O-746 | §10 Audit | Audit record (via port): a command that does not persist (§4), through a context or connection of the port's own, never the module's | - | source: "Every other record is written through the port outside any"
O-747 | §10 Audit | Audit record (via port): a read of `Restricted` data | - | source: "Every other record is written through the port outside any"
O-748 | §10 Audit | Audit record (via port): an export, report or file handed to a user (core §8) | - | source: "Every other record is written through the port outside any"
O-749 | §10 Audit | A failure to write an audit record fails the operation it records | - | source: "A failure to write an audit record fails the operation"
O-750 | §10 Audit | A denial stays a denial (an audit-write failure does not turn it into something else) | - | source: "A denial stays a denial."

#### 11. Configuration and Secrets

O-751 | §11 | Configuration binds to strongly-typed options classes | BE-95 | source: "Configuration binds to strongly-typed options classes."
O-752 | §11 | Outside the composition root, code MUST NOT read configuration from its source - `IConfiguration`, a process environment variable, or any other | BE-95 | source: "Outside the composition root, code MUST NOT read configuration from"
O-753 | §11 | Code outside the composition root receives a bound options class | - | source: "It receives a bound options class."
O-754 | §11 | Every options class MUST have a validator | BE-96 | source: "Every options class MUST have a validator and MUST be"
O-755 | §11 | Every options class MUST be registered with `ValidateOnStart` | BE-96 | source: "Every options class MUST have a validator and MUST be"
O-756 | §11 | An options default MUST NOT silently weaken a control | - | source: "An options default MUST NOT silently weaken a control."
O-757 | §11 | A missing limit, timeout, or policy denies (§9) | - | source: "A missing limit, timeout, or policy denies (§9); it never"
O-758 | §11 | A missing limit, timeout, or policy never falls back to permissive | - | source: "A missing limit, timeout, or policy denies (§9); it never"
O-759 | §11 | Secrets come from Key Vault through managed identity in every environment above local development | BE-97 | source: "Secrets come from Key Vault through managed identity in every"
O-760 | §11 | A secret MUST NOT appear in any committed file | BE-97 | source: "A secret MUST NOT appear in any committed file."
O-761 | §11 | Definition: a secret is any value that grants access on its own, such as a connection string, a package-feed token, or any other | - | source: "A secret is any value that grants access on its"
O-762 | §11 | Exception: a key generated for a test and trusted by nothing outside it is not a secret where it is listed, by file and key, in `tests/test-keys.txt` | - | source: "A key generated for a test and trusted by nothing"
O-763 | §11 | `tests/test-keys.txt` is a reviewed test-key list | - | source: "A key generated for a test and trusted by nothing"
O-764 | §11 | A committed file that needs a credential MUST reference it - an environment-variable macro such as `nuget.config`'s `%NUGET_PAT%`, or a credential provider | - | source: "A committed file that needs a credential MUST reference it"
O-765 | §11 | A committed file MUST NOT carry a placeholder to be overwritten in place | - | source: "A committed file that needs a credential MUST reference it"
O-766 | §11 | Local development uses user secrets | - | source: "Local development uses user secrets."
O-767 | §11 | A developer MUST NOT need a production credential to run the application | - | source: "A developer MUST NOT need a production credential to run"
O-768 | §11 | Configuration that differs by environment MUST differ by value, never by code path | BE-98 | source: "Configuration that differs by environment MUST differ by value, never"
O-769 | §11 | Outside the composition root, code MUST NOT learn which environment it is running in, by any means | BE-98 | source: "Outside the composition root, code MUST NOT learn which environment"
O-770 | §11 | Where the environment's name belongs in telemetry, the composition root puts it there - a resource attribute, a log enricher | - | source: "Where the environment's name belongs in telemetry, the composition root"
O-771 | §11 | A feature flag is configuration | - | source: "A feature flag is configuration: typed, validated, and removable."
O-772 | §11 | A feature flag is typed | - | source: "A feature flag is configuration: typed, validated, and removable."
O-773 | §11 | A feature flag is validated | - | source: "A feature flag is configuration: typed, validated, and removable."
O-774 | §11 | A feature flag is removable | - | source: "A feature flag is configuration: typed, validated, and removable."

#### 12. Real-time

O-775 | §12 | Real-time rules are in `annex-realtime.md` | - | source: "In `annex-realtime.md`."

#### 13. API Layer

O-776 | §13 | Every endpoint MUST be a type implementing the shared endpoint abstraction | BE-80 | source: "Every endpoint MUST be a type implementing the shared endpoint"
O-777 | §13 | The endpoint type takes its Request DTO, the message it dispatches, and its Response DTO as type arguments | BE-80 | source: "Every endpoint MUST be a type implementing the shared endpoint"
O-778 | §13 | A route handler passed as a lambda or method group to a minimal-API route builder - `MapGet`, `MapPost`, `MapMethods`, or any other - is forbidden | BE-80 | source: "A route handler passed as a lambda or method group"
O-779 | §13 | Endpoints are registered by discovery over the endpoint types | BE-81 | source: "Endpoints are registered by discovery over those types at the"
O-780 | §13 | Endpoint discovery happens at the composition root | BE-81 | source: "Endpoints are registered by discovery over those types at the"
O-781 | §13 | Hand-registering an endpoint is a defect | BE-81 | source: "Hand-registering one is a defect."
O-782 | §13 | The framework endpoints are the readiness and liveness probes (§10), a hub's `MapHub` (real-time annex), and the OpenAPI document | - | source: "The framework endpoints - the readiness and liveness probes (§10),"
O-783 | §13 | Framework endpoints are mapped by the framework, not by an endpoint type | - | source: "The framework endpoints - the readiness and liveness probes (§10),"
O-784 | §13 | Framework endpoints are a declared list at the composition root | - | source: "They are a declared list at the composition root, outside"
O-785 | §13 | Framework endpoints are outside the endpoint-type and discovery rules (BE-80, BE-81) | - | source: "They are a declared list at the composition root, outside"
O-786 | §13 | Framework endpoints stay under the rate limit (§9, the `GlobalLimiter`) | - | source: "They are a declared list at the composition root, outside"
O-787 | §13 | Each framework endpoint carries a named policy (§6), such as an anonymous one for the probes, or falls to the fallback, which denies it | - | source: "They are a declared list at the composition root, outside"
O-788 | §13 | The registrar applies each endpoint's authorization metadata from the operation declaration of the message it dispatches | - | source: "The registrar applies each endpoint's authorization and rate-limit metadata from"
O-789 | §13 | The registrar applies each endpoint's rate-limit metadata from the operation declaration of the message it dispatches | - | source: "The registrar applies each endpoint's authorization and rate-limit metadata from"
O-790 | §13 | An endpoint declares neither authorization nor rate-limit metadata itself | - | source: "The registrar applies each endpoint's authorization and rate-limit metadata from"
O-791 | §13 | An endpoint MUST map its Request DTO to a message | BE-82 | source: "An endpoint MUST map its Request DTO to a message,"
O-792 | §13 | An endpoint MUST dispatch the message | BE-82 | source: "An endpoint MUST map its Request DTO to a message,"
O-793 | §13 | An endpoint MUST translate `Result` to HTTP (core §4) | BE-82 | source: "An endpoint MUST map its Request DTO to a message,"
O-794 | §13 | An endpoint MUST NOT reference any type other than: its DTOs, the message it dispatches, and every type those are composed of; the shared endpoint abstraction; the dispatcher and the `Result` it returns; the result translator; and framework primitives (such as `Task`, `CancellationToken`, HTTP result types) | BE-82 | source: "It MUST NOT reference any type other than its DTOs,"
O-795 | §13 | Endpoints MUST NOT reference domain types | BE-77 | source: "Endpoints MUST NOT reference domain types."
O-796 | §13 | Success shape: `200` with a Response DTO | - | source: "Success shapes: `200` with a Response DTO, `201` with a"
O-797 | §13 | Success shape: `201` with a location | - | source: "Success shapes: `200` with a Response DTO, `201` with a"
O-798 | §13 | Success shape: `204` for no content | - | source: "Success shapes: `200` with a Response DTO, `201` with a"
O-799 | §13 | Success shape: a stream the endpoint declares | - | source: "Success shapes: `200` with a Response DTO, `201` with a"
O-800 | §13 | A write that leaves a versioned resource in place returns a body carrying its new `etag`, never `204` (core §4 Wire contract) | - | source: "A write that leaves a versioned resource in place returns"
O-801 | §13 | A delete returns `204` | - | source: "A write that leaves a versioned resource in place returns"
O-802 | §13 | Failures are `ProblemDetails` | - | source: "Failures are `ProblemDetails`."
O-803 | §13 | An endpoint maps `If-Match` to the message's expected version | - | source: "An endpoint maps `If-Match` to the message's expected version, and"
O-804 | §13 | An endpoint maps `If-None-Match: *` to the message's create-only flag | - | source: "An endpoint maps `If-Match` to the message's expected version, and"
O-805 | §13 | These headers are mapped as any other request field is | - | source: "An endpoint maps `If-Match` to the message's expected version, and"
O-806 | §13 | The repository's load reports a stale expected version as an outcome | - | source: "The repository's load reports a stale expected version as an"
O-807 | §13 | The handler translates a stale-version outcome to a `Result` precondition failure before it acts | - | source: "The repository's load reports a stale expected version as an"
O-808 | §13 | The repository also uses the expected version as the concurrency token's original value | - | source: "The repository also uses the expected version as the concurrency"
O-809 | §13 | For a create-only message, a violation of the aggregate id's unique index at commit is a precondition failure, not a conflict | - | source: "For a create-only message, a violation of the aggregate id's"
O-810 | §13 | The unit-of-work behavior returns the create-only violation as a precondition failure (§4 Pipeline behaviors, item 4) | - | source: "For a create-only message, a violation of the aggregate id's"
O-811 | §13 | The result translator maps every precondition failure to `412` | - | source: "The result translator maps every precondition failure to `412`."
O-812 | §13 | Every problem type a module returns is a URI the module declares | - | source: "Every problem type a module returns is a URI the"
O-813 | §13 | A problem type declaration carries its status, title, and whether it is user-facing | - | source: "Every problem type a module returns is a URI the"
O-814 | §13 | The OpenAPI document's `x-problem-types` is generated from those declarations (core §4 Wire contract) | - | source: "Every problem type a module returns is a URI the"
O-815 | §13 | The result translator maps each validation failure's message path to the request path it came from (core §4 Wire contract) | - | source: "The result translator maps each validation failure's message path to"
O-816 | §13 | A message property keeps its request path unless the endpoint's mapping declares a different one | - | source: "The result translator maps each validation failure's message path to"
O-817 | §13 | `Platform` declares the problem types the pipeline and the framework return - validation, authorization, idempotency, preconditions, rate limits, unsupported media, and any other | - | source: "`Platform` declares the problem types the pipeline and the framework"
O-818 | §13 | `Platform` overrides ASP.NET Core's per-status defaults with its problem types through `CustomizeProblemDetails` | - | source: "`Platform` declares the problem types the pipeline and the framework"
O-819 | §13 | Scope: where the client is on another origin, the CORS policy applies as follows | - | source: "Where the client is on another origin, the CORS policy"
O-820 | §13 | The CORS policy admits the request headers the wire contract uses - `traceparent`, `tracestate`, `If-Match`, `If-None-Match`, `Idempotency-Key`, or any other it adds | - | source: "Where the client is on another origin, the CORS policy"
O-821 | §13 | The CORS policy exposes the response headers a client reads, such as `ETag`, `Location` and `Retry-After` | - | source: "Where the client is on another origin, the CORS policy"
O-822 | §13 | The endpoint registrar sets `Cache-Control: no-store` on every endpoint whose Response DTO resolves to `Confidential` or `Restricted` (BE-55) | - | source: "The endpoint registrar sets `Cache-Control: no-store` on every endpoint whose"
O-823 | §13 | The endpoint registrar sets `Cache-Control: private` on every other endpoint whose representation depends on the principal (core §4 Wire contract) | - | source: "The endpoint registrar sets `Cache-Control: no-store` on every endpoint whose"
O-824 | §13 | Which endpoints depend on the principal is reached by review | - | source: "Which endpoints depend on the principal is reached by review."
O-825 | §13 | An upload endpoint's Request DTO carries the file as a stream with its metadata | - | source: "An upload endpoint's Request DTO carries the file as a"
O-826 | §13 | An upload's size limit is enforced at the request boundary - the endpoint's request size limit - before the body is buffered | - | source: "An upload endpoint's Request DTO carries the file as a"
O-827 | §13 | An upload endpoint dispatches a command that does not persist, to store the file in quarantine | - | source: "It dispatches a command that does not persist to store"
O-828 | §13 | The stored file's name is derived by the server from the uploader's principal and the command's client-supplied id | - | source: "It dispatches a command that does not persist to store"
O-829 | §13 | The client-supplied id used for the name is one the validation behavior has parsed | - | source: "It dispatches a command that does not persist to store"
O-830 | §13 | The name is derived with a keyed hash, such as an HMAC under a server key | - | source: "It dispatches a command that does not persist to store"
O-831 | §13 | The stored name is never derived from the client's file name (§4) | - | source: "It dispatches a command that does not persist to store"
O-832 | §13 | A persisting command records the uploaded file once the scan passes (core §7 File uploads) | - | source: "A persisting command, run under the uploader's principal recorded with"
O-833 | §13 | That persisting command runs under the uploader's principal recorded with the quarantined file | - | source: "A persisting command, run under the uploader's principal recorded with"
O-834 | §13 | A change to a Request or Response DTO that does not regenerate and commit the OpenAPI document is incomplete | - | source: "A change to a Request or Response DTO that does"

#### 14. Testing

O-835 | §14 | A missing required test is a defect exactly as a violated rule is (core §1) | - | source: "A missing required test is a defect exactly as a"
O-836 | §14 | The required tests of a rule in an annex are in that annex's own Testing section | - | source: "The required tests of a rule in an annex are"
O-837 | §14 | Domain tests are pure unit tests | - | source: "**Domain**: pure unit tests, no substitutes, no I/O."
O-838 | §14 | Domain tests use no substitutes | - | source: "**Domain**: pure unit tests, no substitutes, no I/O."
O-839 | §14 | Domain tests do no I/O | - | source: "**Domain**: pure unit tests, no substitutes, no I/O."
O-840 | §14 | Every invariant MUST have a test | BE-138 (test id) | source: "Every invariant and every domain outcome MUST have a test"
O-841 | §14 | Every domain outcome MUST have a test | BE-138 (test id) | source: "Every invariant and every domain outcome MUST have a test"
O-842 | §14 | Application tests are handler tests with substituted infrastructure | - | source: "**Application**: handler tests with substituted infrastructure."
O-843 | §14 | Every declared policy that some principal does not satisfy MUST have a test proving such a principal is denied, not only one proving success | BE-122 (test id) | source: "Every declared policy that some principal does not satisfy MUST"
O-844 | §14 | Where a handler translates outcomes that are types, every outcome MUST have a test proving its translation | BE-141 (test id) | source: "Where a handler translates outcomes that are types, every outcome"
O-845 | §14 | Integration tests use real SQL Server via Testcontainers | - | source: "**Integration**: real SQL Server via Testcontainers, real pipeline behaviors."
O-846 | §14 | Integration tests use real pipeline behaviors | - | source: "**Integration**: real SQL Server via Testcontainers, real pipeline behaviors."
O-847 | §14 | Every query handler that reads with SQL MUST have a test against real SQL Server proving its SQL projects into its Response DTO | BE-112 (test id) | source: "**Queries**: every query handler that reads with SQL MUST have"
O-848 | §14 | ... and proving its SQL applies the module's query filters | BE-112 (test id) | source: "**Queries**: every query handler that reads with SQL MUST have"
O-849 | §14 | A command that runs without a transaction MUST have a test proving a staged database change fails the dispatch | BE-111 (test id) | source: "**Transactions**: a command that runs without a transaction MUST have"
O-850 | §14 | ... and proving the staged change is never committed | BE-111 (test id) | source: "**Transactions**: a command that runs without a transaction MUST have"
O-851 | §14 | ArchUnitNET architecture tests are first-class | - | source: "**Architecture**: ArchUnitNET tests are first-class and MUST NOT be skipped"
O-852 | §14 | Architecture tests MUST NOT be skipped to unblock a change | - | source: "**Architecture**: ArchUnitNET tests are first-class and MUST NOT be skipped"
O-853 | §14 | Architecture tests MUST NOT be marked inconclusive to unblock a change | - | source: "**Architecture**: ArchUnitNET tests are first-class and MUST NOT be skipped"
O-854 | §14 | Every external authority MUST have a test proving an unreachable authority denies | BE-99 (test id) | source: "**Authorization**: every external authority MUST have a test proving that"
O-855 | §14 | ... proving a timing-out authority denies | BE-99 (test id) | source: "**Authorization**: every external authority MUST have a test proving that"
O-856 | §14 | ... proving an erroring authority denies | BE-99 (test id) | source: "**Authorization**: every external authority MUST have a test proving that"
O-857 | §14 | ... proving an expired cache entry is not served while the authority is unreachable | BE-99 (test id) | source: "**Authorization**: every external authority MUST have a test proving that"
O-858 | §14 | Every token validation that reads a claim before the signature is checked MUST have a test proving a token whose selecting claims disagree with its validated issuer or endpoint is refused | BE-104 (test id) | source: "Every token validation that reads a claim before the signature"
O-859 | §14 | ... and proving no claim read before validation reaches the principal | BE-104 (test id) | source: "Every token validation that reads a claim before the signature"
O-860 | §14 | Every policy MUST have a test proving an in-process dispatch with no HTTP context receives the same decision as the HTTP path | BE-101 (test id) | source: "Every policy MUST have a test proving an in-process dispatch"
O-861 | §14 | Every operation that acts on a record the caller identifies MUST have a test proving a principal holding the policy but not the record is denied | BE-117 (test id) | source: "Every operation that acts on a record the caller identifies,"
O-862 | §14 | Exception: not required where the operation's policy grants every record | BE-117 (test id) | source: "Every operation that acts on a record the caller identifies,"
O-863 | §14 | Every query MUST have a test proving it returns no record the principal may not see | BE-117 (test id) | source: "Every operation that acts on a record the caller identifies,"
O-864 | §14 | Every persisting command MUST have a test proving its success audit record commits with its change | BE-118 (test id) | source: "**Audit**: every persisting command MUST have a test proving its"
O-865 | §14 | ... and proving its failure audit record is written after rollback | BE-118 (test id) | source: "**Audit**: every persisting command MUST have a test proving its"
O-866 | §14 | Every command that does not persist MUST have a test proving its audit record is written | BE-118 (test id) | source: "**Audit**: every persisting command MUST have a test proving its"
O-867 | §14 | The authorization behavior MUST have a test proving a denial is recorded | BE-118 (test id) | source: "**Audit**: every persisting command MUST have a test proving its"
O-868 | §14 | The resource-authorization port MUST have a test proving a denial is recorded | BE-118 (test id) | source: "**Audit**: every persisting command MUST have a test proving its"
O-869 | §14 | Every paginated query MUST have a test proving a request above its maximum is rejected | BE-124 (test id) | source: "**Pagination**: every paginated query MUST have a test proving a"
O-870 | §14 | Every paginated query MUST have a test proving no row is skipped or repeated across a page boundary | BE-124 (test id) | source: "**Pagination**: every paginated query MUST have a test proving a"
O-871 | §14 | Every collection bounded by construction whose size a request determines MUST have a test proving a request beyond its bound is rejected, never truncated | BE-124 (test id) | source: "**Pagination**: every paginated query MUST have a test proving a"
O-872 | §14 | Every command declaring a key MUST have a test proving a replayed key returns the recorded result without executing twice | BE-125 (test id) | source: "**Idempotency**: every command declaring a key MUST have a test"
O-873 | §14 | Every command declaring a key MUST have a test proving two concurrent attempts under one key resolve as one execution and one replay rather than an error | BE-29 (test id) | source: "**Idempotency**: every command declaring a key MUST have a test"
O-874 | §14 | Every command declaring a key MUST have a test proving the same key with a different payload is refused with `422` | BE-116 (test id) | source: "**Idempotency**: every command declaring a key MUST have a test"
O-875 | §14 | Every command declaring a key MUST have a test proving a request that carries no key is refused with `400` | BE-137 (test id) | source: "**Idempotency**: every command declaring a key MUST have a test"
O-876 | §14 | Every versioned write MUST have a test proving a write without `If-Match` is refused with `428` | BE-115 (test id) | source: "**Concurrency**: every versioned write MUST have a test proving a"
O-877 | §14 | ... proving a write with a stale version is refused with `412` | BE-115 (test id) | source: "**Concurrency**: every versioned write MUST have a test proving a"
O-878 | §14 | ... proving `If-Match: *` is refused with `428` | BE-115 (test id) | source: "**Concurrency**: every versioned write MUST have a test proving a"
O-879 | §14 | ... proving a create with `If-None-Match: *` for an existing id is refused with `412` | BE-115 (test id) | source: "**Concurrency**: every versioned write MUST have a test proving a"
O-880 | §14 | Every upload endpoint MUST have a test proving a file whose content does not match its declared type is refused | BE-120 (test id) | source: "**Uploads**: every upload endpoint MUST have a test proving a"
O-881 | §14 | ... proving a file above the maximum size is refused | BE-120 (test id) | source: "**Uploads**: every upload endpoint MUST have a test proving a"
O-882 | §14 | ... proving a stored file is not served before its scan passes | BE-120 (test id) | source: "**Uploads**: every upload endpoint MUST have a test proving a"
O-883 | §14 | Every rate-limit policy MUST have a test proving a request beyond its limit is refused with `429` as problem details | BE-127 (test id) | source: "**Rate limits**: every rate-limit policy, and the `GlobalLimiter`, MUST have"
O-884 | §14 | The `GlobalLimiter` MUST have the same `429` test | BE-127 (test id) | source: "**Rate limits**: every rate-limit policy, and the `GlobalLimiter`, MUST have"
O-885 | §14 | The rate-limit test proves `Retry-After` is carried where its limiter can estimate when permits return | BE-127 (test id) | source: "**Rate limits**: every rate-limit policy, and the `GlobalLimiter`, MUST have"
O-886 | §14 | The outbox MUST have a test proving the trace context survives the round trip: written with the transaction, restored on delivery, present on the consumer's log line | BE-92 (test id) | source: "**Correlation**: the outbox MUST have a test proving the trace"
O-887 | §14 | A request from outside the trust boundary that carries a `traceparent` MUST have a test proving it gets a new trace id | BE-139 (test id) | source: "A request from outside the trust boundary that carries a"
O-888 | §14 | ... and proving it gets a link to the caller's trace | BE-139 (test id) | source: "A request from outside the trust boundary that carries a"
O-889 | §14 | The dispatch pipeline MUST have a test proving every behavior registered in it, and the handler, emits a span | BE-128 (test id) | source: "**Observability**: the dispatch pipeline MUST have a test proving that"
O-890 | §14 | ... spans nested in pipeline order, so authorization, validation and handler time are separable | BE-128 (test id) | source: "**Observability**: the dispatch pipeline MUST have a test proving that"
O-891 | §14 | Every migration MUST be applied to a Testcontainers database in CI | BE-89 (test id) | source: "**Migrations**: every migration MUST be applied to a Testcontainers database"
O-892 | §14 | Every migration MUST be proven to run clean twice | BE-89 (test id) | source: "**Migrations**: every migration MUST be applied to a Testcontainers database"
O-893 | §14 | A destructive migration MUST additionally have a test proving the previous application version still serves against the new schema | BE-89 (test id) | source: "A destructive migration MUST additionally have a test proving the"
O-894 | §14 | Every `Confidential` and `Restricted` type MUST have a test proving it redacts in `ToString()` | BE-126 (test id) | source: "**Classification**: every `Confidential` and `Restricted` type MUST have a test"
O-895 | §14 | ... proving it redacts in a serialized log event | BE-126 (test id) | source: "**Classification**: every `Confidential` and `Restricted` type MUST have a test"
O-896 | §14 | ... proving a classified logging-method parameter is redacted in the provider's output | BE-126 (test id) | source: "**Classification**: every `Confidential` and `Restricted` type MUST have a test"
O-897 | §14 | Test names state the rule: `<Operation>_Returns<Rejection>_When<Condition>` | - | source: "Test names state the rule: `<Operation>_Returns<Rejection>_When<Condition>`."
O-898 | §14 | Tests MUST NOT assert on implementation details | - | source: "Tests MUST NOT assert on implementation details - no verifying"
O-899 | §14 | Tests MUST NOT verify private calls | - | source: "Tests MUST NOT assert on implementation details - no verifying"
O-900 | §14 | Tests MUST NOT assert on generated SQL | - | source: "Tests MUST NOT assert on implementation details - no verifying"

#### 15. Forbidden

O-901 | §15 | `dynamic` is forbidden | BE-129 | source: "`dynamic` ⚙ BE-129 · service location outside the composition root,"
O-902 | §15 | Service location outside the composition root is forbidden | BE-130 | source: "service location outside the composition root, except that a hosted"
O-903 | §15 | Exception: a hosted service MAY create a scope through `IServiceScopeFactory` and resolve its per-iteration dependencies from it | BE-130 | source: "service location outside the composition root, except that a hosted"
O-904 | §15 | Static mutable state is forbidden | BE-131 | source: "static mutable state ⚙ BE-131"
O-905 | §15 | Reading the system clock in domain or application code is forbidden - `DateTime.Now`, `DateTimeOffset.UtcNow`, `DateTime.Today`, or any other ambient read | BE-132 | source: "reading the system clock in domain or application code -"
O-906 | §15 | Inject `TimeProvider` into the handler | BE-132 | source: "reading the system clock in domain or application code -"
O-907 | §15 | Pass the time to the aggregate as an argument | BE-132 | source: "reading the system clock in domain or application code -"
O-908 | §15 | `async void` is forbidden outside event handlers | BE-133 | source: "`async void` outside event handlers ⚙ BE-133"
O-909 | §15 | `.Result` is forbidden | BE-134 | source: "`.Result`, `.Wait()`, `.GetAwaiter().GetResult()` ⚙ BE-134"
O-910 | §15 | `.Wait()` is forbidden | BE-134 | source: "`.Result`, `.Wait()`, `.GetAwaiter().GetResult()` ⚙ BE-134"
O-911 | §15 | `.GetAwaiter().GetResult()` is forbidden | BE-134 | source: "`.Result`, `.Wait()`, `.GetAwaiter().GetResult()` ⚙ BE-134"
O-912 | §15 | Catching `Exception` without rethrowing is forbidden | BE-135 | source: "catching `Exception` without rethrowing, except at a declared fault-isolation boundary"
O-913 | §15 | Exception: catching `Exception` without rethrow is allowed at a declared fault-isolation boundary | BE-135 | source: "catching `Exception` without rethrowing, except at a declared fault-isolation boundary"
O-914 | §15 | Fault-isolation boundary examples: one iteration of a hosted-service loop, one outbox delivery, a post-commit effect (§4), or any other unit whose failure must not stop the host | BE-135 | source: "catching `Exception` without rethrowing, except at a declared fault-isolation boundary"
O-915 | §15 | A fault-isolation boundary is declared by a marker on the method | BE-135 | source: "which is declared by a marker on the method, an"
O-916 | §15 | The marker is an attribute the solution declares once, in `SharedKernel` | BE-135 | source: "which is declared by a marker on the method, an"
O-917 | §15 | A fault-isolation boundary MUST log at `Error` with the exception | BE-135 | source: "MUST log at `Error` with the exception, within core §8's"
O-918 | §15 | That log stays within core §8's telemetry rule | BE-135 | source: "MUST log at `Error` with the exception, within core §8's"
O-919 | §15 | A fault-isolation boundary MUST let `OperationCanceledException` propagate on shutdown | BE-135 | source: "MUST let `OperationCanceledException` propagate on shutdown ⚙ BE-135"
O-920 | §15 | `#region` is forbidden | BE-136 | source: "`#region` ⚙ BE-136"
O-921 | §15 | Commented-out code is forbidden | - | source: "commented-out code"

#### 16. Verification

O-922 | §16 | Verification is `dotnet build` with warnings as errors | - | source: "`dotnet build` (warnings are errors) and `dotnet test` (includes architecture"
O-923 | §16 | Verification is `dotnet test`, which includes the architecture tests | - | source: "`dotnet build` (warnings are errors) and `dotnet test` (includes architecture"
O-924 | §16 | Both `dotnet build` and `dotnet test` MUST pass before a change is complete | - | source: "Both MUST pass before a change is complete."


Total items: 924
