// =============================================================================
//  detective_story.ink
//  Second demo narrative for the ZaiGames Unity Dialogue System.
//
//  Genre:     Sci-fi noir detective
//  Protagonist: Detective Mara Voss (cybernetic-eye homicide cop)
//  Setting:   Neon precinct + rain-soaked rooftop crime scene, 2147
//
//  Demonstrates ink features beyond sample_story.ink:
//    1. Variables       — VAR declarations + ~ assignment (clues, suspicion)
//    2. Conditional      — {condition} text inline + {cond | false_branch}
//    3. Sticky choices  — {true} keeps a choice visible after picking it
//    4. Stitches        — knot.stitch sub-diverts for tighter flow
//    5. Logic gates     — choices appear only when enough clues gathered
//    6. Glue            — <> joins lines into one rendered line
//    7. Multi-line gather — pulls choices back to one place
//
//  Tag conventions (parsed by DialogueTagParser.cs):
//    #speaker:    who is talking
//    #portrait:   portrait key (your view maps to a Sprite)
//    #mood:       mood key (your view maps to color/animation)
//    #audio:      audio cue key (your view maps to an AudioClip)
//    #save        bare tag (parsed as "true") — demo hook for autosave
//
//  Compile: auto-compiled by ink-unity-integration v2.0 ScriptedImporter.
//  Wire up: create a new DialogueStoryAsset SO and point its InkFile at
//  this file's compiled InkFile asset, OR drag the .ink file's InkFile
//  into an existing DialogueStoryAsset and swap it on the DialogueRunner.
// =============================================================================

// --- Global variables tracked across the whole story ---
VAR clues_found = 0
VAR suspicion = 0
VAR told_daichi = false

// --- Entry point ---
# speaker: Narrator
# mood: noir
Rain hammers the precinct windows. The neon sign across the street buzzes M A R A in stuttering pink.
Your cybernetic eye flickers — old warrant, old city, older regrets.

# speaker: Daichi
# portrait: daichi_tired
# mood: tired
"Voss. We caught one. Rooftop on Ninth. Kid, maybe sixteen. No ID, no implants, no prints."
He slides a holo across the desk. The face is bruised but calm — almost peaceful.

# speaker: Daichi
# portrait: daichi_tired
# mood: urgent
# save
"Captain wants this closed by morning. Quietly. You know what that means."

*   [Grab your coat.]
    -> scene_rooftop
*   [Ask who found the body.]
    -> ask_finder
*   [Tell Daichi you're off the clock.]
    -> refuse_case

// --- Refuse path: short, branches back ---
=== ask_finder
# speaker: Daichi
# portrait: daichi_tired
# mood: dismissive
"Patrol drone. Sector seven. flagged it at 02:14, but the timestamp on the body says she'd been up there since midnight."
~ suspicion += 1

# speaker: Mara
# portrait: mara_squint
# mood: suspicious
Two hours unaccounted for. Two hours of rain erasing everything useful.
"Quietly, huh. Someone's already been cleaning."

# speaker: Daichi
# portrait: daichi_tired
# mood: grim
"I didn't say that. Get to the rooftop, Voss."

-> scene_rooftop

// --- Refuse path: Daichi pushes back ---
=== refuse_case
# speaker: Mara
# portrait: mara_weary
# mood: weary
"It's past two, Daichi. I clocked out at six. Find someone else."

# speaker: Daichi
# portrait: daichi_lean
# mood: cold
He doesn't blink. "You're the only one whose eye still works in the rain, Voss. And the only one the Captain can't fire."
~ suspicion += 2

# speaker: Mara
# mood: resigned
The eye. Always the eye. A gift from a case you don't talk about, paid for in scars you can't hide.
"Fine. But I'm billing double."

-> scene_rooftop

// =============================================================================
//  THE ROOFTOP
//  Multi-beat scene: examine three clues (sticky choices), then confront.
// =============================================================================
=== scene_rooftop
# speaker: Narrator
# mood: noir
# audio: amb_rain
The rooftop. Wind. Rain cuts sideways through the sodium lamps.
The girl lies at the center of a perfect circle of dry pavement — impossible, given the storm.

# speaker: Mara
# portrait: mara_squint
# mood: calculating
Your eye whirs. Three things wrong with this picture. Start with the one that bothers you most.

// --- Sticky examination hub: choices stay visible after picking ---
*   {true} [Examine the dry circle.]
    -> examine_circle
*   {true} [Look at her hands.]
    -> examine_hands
*   {true} [Scan the rooftop edge.]
    -> examine_edge
*   {clues_found >= 2 && suspicion >= 1} ["Someone scrubbed this scene. This wasn't a suicide."]
    -> confront_suppression
*   {clues_found >= 3} ["I have enough. Let me think."]
    -> deduction

// --- Stitch: the dry circle ---
=== examine_circle
# speaker: Mara
# portrait: mara_eye
# mood: scanning
# audio: sfx_scanner
You crouch. The eye overlays thermal residue — faint, but there.
A device sat here. Bled heat into the rain for maybe an hour. Then it walked away.

~ clues_found += 1

# speaker: Mara
# mood: grim
A field generator. Someone built a bubble of dry air around her while they worked.
Whoever did this wanted time. Wanted to take their time.

-> scene_rooftop

// --- Stitch: her hands ---
=== examine_hands
# speaker: Mara
# portrait: mara_eye
# mood: scanning
Her fingers are stiff. Curled around something that isn't there anymore.
Under the nail of her index finger — a sliver of metallic paint. Gunmetal grey.

~ clues_found += 1

# speaker: Mara
# mood: thoughtful
She scratched her killer. Or grabbed at something they were holding.
The lab will need a sample, but the eye already cross-references the paint batch —
military surplus. Restricted. Three suppliers in the city.

~ suspicion += 1

-> scene_rooftop

// --- Stitch: the rooftop edge ---
=== examine_edge
# speaker: Mara
# portrait: mara_eye
# mood: scanning
The parapet. Your eye tracks scuff marks — boot soles, size eleven, fresh.
They don't lead to the fire escape. They lead to the edge. Then they stop.

~ clues_found += 1

# speaker: Mara
# mood: cold
Whoever did this didn't leave the way they came.
They jumped. And a sixteen-year-old kid didn't jump twelve stories on her own.

~ suspicion += 1

-> scene_rooftop

// --- Confront: the scene was scrubbed ---
=== confront_suppression
# speaker: Mara
# portrait: mara_cold
# mood: cold
The dry circle. The missing object in her hand. The boot prints that vanish.
This is a stage. Someone dressed it.

# speaker: Narrator
# mood: noir
Your comm buzzes. Daichi. You let it ring.
Some things a cop isn't supposed to say out loud until she's sure who's listening.

~ suspicion += 1
~ told_daichi = true

-> scene_rooftop

// --- Deduction: synthesize, branch ending on suspicion ---
=== deduction
# speaker: Mara
# portrait: mara_resolved
# mood: resolved
You straighten. The rain finds the gap in your collar, runs cold down your spine.
Three clues. One picture.

# speaker: Mara
# mood: cold
The girl didn't fall. She was lowered — gently, carefully, like a thing someone cared about.
The field generator kept the rain off while the killer arranged her hands. Said goodbye.
Then they walked off the edge and let the rain take the rest.

*   ["It's personal. Someone who loved her."]
    -> ending_personal
*   ["It's professional. A cleanup, and she was the leak."]
    -> ending_professional
*   {told_daichi} ["Call Daichi back. Tell him what I found."]
    -> ending_daichi

// =============================================================================
//  ENDINGS
//  Three branching conclusions based on prior choices.
// =============================================================================

=== ending_personal
# speaker: Narrator
# mood: bittersweet
# audio: amb_rain_soft
You find the supplier two days later. A father. He doesn't run.
He tells you about the daughter they took from him, about the field generator he built to keep her dry one last time.

# speaker: Father
# portrait: father_broken
# mood: broken
"She was cold. I couldn't let her be cold anymore."

# speaker: Mara
# portrait: mara_weary
# mood: weary
You read him his rights. Your voice doesn't shake. Your eye records everything.
Some cases you close. Some cases close you.

-> epilogue

=== ending_professional
# speaker: Narrator
# mood: noir
# audio: amb_rain
You file the report. Use the neutral words. The Captain nods, signs it, files it in a drawer that locks.
By morning the body is gone from the morgue. By noon the case number doesn't exist anymore.

# speaker: Mara
# portrait: mara_cold
# mood: cold
You knew. You knew when you saw the dry circle.
Some jobs you take for the kid on the rooftop. Some jobs you take so the kid on the next rooftop might get a chance.

# speaker: Narrator
# mood: noir
Your eye keeps recording. You keep walking. The rain keeps falling.

-> epilogue

=== ending_daichi
# speaker: Daichi
# portrait: daichi_pale
# mood: pale
# audio: sfx_comm
The comm crackles. "Voss. You there? Tell me you found something I can use."

# speaker: Mara
# portrait: mara_cold
# mood: cold
"Someone scrubbed that rooftop, Daichi. Military hardware. Whoever she was, she scared the wrong people."
A long pause. Rain on his end too.

# speaker: Daichi
# portrait: daichi_pale
# mood: grim
"Voss. Step off this one. Please. I'm asking as a friend."

# speaker: Mara
# mood: resolute
You hang up. The eye flickers — M A R A in stuttering pink across the street.
Some cases, the city tells you to stop. Those are the ones you have to finish.

# speaker: Narrator
# mood: noir
You pocket the paint sample. You walk to the edge. You look down.
Somewhere below, the rain keeps erasing everything useful.

-> epilogue

=== epilogue
# speaker: Narrator
# mood: contemplative
# audio: amb_rain
The case file closes. The rain keeps falling. The neon keeps buzzing your name.
Tomorrow there will be another rooftop. There is always another rooftop.
-> END
