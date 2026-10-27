// ===========================================================================
// sample_story.ink
// ===========================================================================
// A short branching narrative demonstrating every feature of the
// InkDialogueSystem: variables, conditional choices, knots/stitches,
// tags for speaker/portrait/save points, gather points, external
// function calls, and an ending.
//
// Compile this file with Inky (https://www.inklestudios.com/ink/) or the
// ink-unity-integration compiler to produce sample_story.json, which is
// what InkFileLoader actually reads at runtime.
// ===========================================================================

// --- Variables (top-level) -------------------------------------------------
VAR player_name = "Wren"
VAR affinity_mira = 0
VAR affinity_kade = 0
VAR gold = 25
VAR met_stranger = false

// --- External functions (defined in DialogueManager.BindExternalFunctions) -
EXTERNAL GetPlayerName()
EXTERNAL RollDice(sides)
EXTERNAL HasItem(itemId)

// --- Main story starts here -----------------------------------------------

#speaker:Narrator
The door of the Hollow Lantern groans open. Rain drips from your cloak onto the warped floorboards.
-> intro

=== intro ===
    #speaker:Narrator
    The common room is half-empty tonight. A bard plucks at a broken lute in the corner.
    #speaker:Innkeeper
    "You're back, {GetPlayerName()}. The usual?"
    * "Just water. I'm on a job." -> water_order
    * "Mead. And keep them coming." -> mead_order
    * "Say nothing. Find a seat." -> silent_seat

=== water_order ===
    #speaker:Innkeeper
    "Wise. The roads have been strange lately."
    ~ gold -= 1
    #portrait:innkeeper_smile
    * "Strange how?" -> stranger_intro
    * "I don't have time for stories." -> silent_seat

=== mead_order ===
    #speaker:Innkeeper
    "Mead it is."
    ~ gold -= 4
    ~ affinity_mira += 1
    #portrait:innkeeper_grin
    * "Strange how?" -> stranger_intro
    * "I don't have time for stories." -> silent_seat

=== silent_seat ===
    #speaker:Narrator
    You slide into a corner booth. The candle on the table gutters in a draft from the chimney.
    -> stranger_intro

=== stranger_intro ===
    #speaker:Narrator
    A figure slides into the seat across from you. Hood up, hands flat on the table.
    ~ met_stranger = true
    #portrait:stranger_dark
    #speaker:Stranger
    "You're the one they call {player_name}. I have work."
    -> stranger_offer

=== stranger_offer ===
    #speaker:Stranger
    "A vault. Three days west. The lock is Ink -- sealed with a story only you can read."
    #speaker:{player_name}
    "How much?"
    #speaker:Stranger
    "Two hundred. Half now."
    ~ gold += 100
    * "I'm in." -> accept_job
    * "What's the catch?" -> ask_catch
    * "No. I don't take sealed jobs." -> refuse_job

=== ask_catch ===
    #speaker:Stranger
    "The catch is the story itself. People who read it... change."
    * "I'm in." -> accept_job
    * "No. I don't take sealed jobs." -> refuse_job

=== accept_job ===
    #save:checkpoint_accept
    #speaker:Narrator
    You leave the Hollow Lantern at dawn. The road west winds through fog.
    ~ affinity_mira += 2
    -> journey

=== refuse_job ===
    #save:checkpoint_refuse
    #speaker:Narrator
    The stranger nods once and stands. The draft from the door is colder than it should be.
    ~ affinity_kade += 2
    -> ending_alone

=== journey ===
    #speaker:Narrator
    Three days. The fog does not lift. You roll a six-sided die to mark the nights.
    The die comes up {RollDice(6)}.
    #speaker:{player_name}
    "Bad luck to roll without a question."
    #speaker:Narrator
    On the fourth morning you see the vault.
    -> vault_door

=== vault_door ===
    #speaker:Narrator
    The vault is carved into a hillside. The door is a single slab of black slate.
    Inscribed on it, in Ink, is a single line: "Tell me what you fear, and I will open."
    * "I fear being forgotten." -> vault_opened
    * "I fear being remembered." -> vault_opened
    * "I fear nothing." -> vault_closed

=== vault_opened ===
    #save:checkpoint_vault
    #speaker:Narrator
    The slate grinds inward. The story accepts your answer.
    #speaker:Narrator
    Inside: a single book. The first page is blank. As you watch, words crawl onto it.
    -> good_ending

=== vault_closed ===
    #speaker:Narrator
    The slate does not move. The story does not believe you.
    #speaker:Narrator
    You sit on the wet grass and consider your options.
    * "I fear being forgotten." -> vault_opened
    * "I fear being remembered." -> vault_opened

=== good_ending ===
    #speaker:Narrator
    The book writes itself for an hour, then closes. You will never know what it said.
    But you will dream it, every night, for the rest of your life.
    #speaker:{player_name}
    "Worth two hundred."
    -> END

=== ending_alone ===
    #speaker:Narrator
    You finish your drink alone. The bard has stopped playing.
    The stranger is gone. So is the rain, but the floor is still wet.
    #speaker:{player_name}
    "Some doors, you don't open."
    -> END

// --- End of story ----------------------------------------------------------
