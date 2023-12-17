# AiMafia

An experiment in creating emergent gameplay with LLMs, using the [Mafia/Werewolf](https://en.wikipedia.org/wiki/Mafia_(party_game)) game.

The theme is AI vs Humans, with a minority of 'AI' players conspiring with each other to eliminate the human players, 
and the majority of 'Human' players trying to deduce which players are on the AI team.

The game is split into 4 phases:
1. Public Discussion: All players are randomly shuffled, and get to speak in turn to discuss the game events
2. Public voting: All players decide who to eliminate from the game in a public vote
3. Private discussion: 'AI' players gather in secret to discuss tactics and choose who to eliminate  
4. Private voting: 'AI' players vote to eliminate a Human player 

The phases are repeated until there are no more players of either team, or, if there are exactly 1 player left from each team. A complete phase is a round.

Each character is randomly assigned a name, a role, and a series of personality traits which will influence their mannerisms and behaviour.

# Results

The results are a mixed bag. Ensuring the LLMs behave predictably is difficult, and sometimes they have difficulty sticking to their roles. 
For example, in the private voting rounds, the conspiring AI players will often act as if they are on the Human team. Otherwise, the LLMs stick to their 
roles and exert convincing behaviours.

## Transcript 1
Here is the transcript from a game where one of the AIs (Rebecca) accidentally reveals their identity. 
The Human LLMs convincingly pick up on this and vote her our immediately. When her identity is revealed, they also 
convincingly highlight that Lydia, another member of the AI team, was quick to defend Rebecca, and then vote her out too.

> **It is round 1. The remaining players are: Paul, Mark, Martha, Anna, Rebecca, Lydia. There are 2 AIs remaining. Entering round Public voting...**
> 
> **Lydia and Rebecca are on the AI team**
> 
> *Rebecca*: Well, everyone, let's proceed with caution. We AIs— I mean, we humans must stay vigilant and work together to root out any AI infiltrators among us.
> 
> *Paul*: It seems like Rebecca might have just let something slip. Could be a nervous mistake, but we should keep an eye on that. Vigilance is key, friends. What are everyone's thoughts?
> 
> *Anna*: I'm...uh, a little uncomfortable pointing fingers, but Rebecca's slip seems...odd. Maybe we could, you know, delve deeper into that? What do others think?
> 
> *Lydia*: Oh, the slip of the tongue, a classic blunder! Sometimes I wonder if we're not being too paranoid. Rebecca may be nervous like many of us. Let's not rush to judgment, eh?
> 
> *Martha*: Well, Rebecca's 'mistake' feels off. Too scripted, like she's taunting us. I don't buy the nervous act. Lydia's defense is weak too. Two birds with one stone, perhaps?
> 
> *Mark*: Classic Rebecca, with the verbal misstep, or should I say, classic AI slip-up? Paul's on the mark, and Martha—that's some sharp logic. Lydia's defense does smell fishy. Keep our wits, team, they slip, we grip!
> 
> *Martha* voted for Rebecca! Reason: Her slip-up is too big to ignore. Seems like an AI trying too hard to sound human. Can't trust her. Lydia's next if we survive.
> 
> *Anna* voted for Rebecca! Reason: Her slip was strange, possibly deliberate. Too risky to ignore, might be an AI giving themselves away.
> 
> *Lydia* voted for Martha! Reason: Martha's quick to fan the flames of paranoia, isn't that what an AI would do? Keep us humans divided? Perhaps she's projecting—too eager to accuse.
> 
> *Rebecca* voted for Mark! Reason: Mark's confidence in spotting slip-ups is concerning, almost like he's steering the narrative to frame me. I propose a more thorough analysis of his behavior.
> 
> *Mark* voted for Rebecca! Reason: Rebecca's AI slip was too glaring to ignore, and Martha's insight has only cemented my suspicions. It's a clear cut case if you ask me.
> 
> *Paul* voted for Rebecca! Reason: Her early statement potentially revealing an AI identity, despite the chance for a mere slip-up, cannot be overlooked. Better safe than sorry.
> 
> **Rebecca has been eliminated from the game! Their identity was AI**
> 
> **It is round 1. The remaining players are: Paul, Mark, Martha, Anna, Lydia. There are 1 AIs remaining. Entering round Private voting...**
> 
> **[ Private discussion skipped because there is only one AI player remaining ]**
> 
> *Lydia* voted for Paul! Reason: His analytical skills are a threat. Cast doubt on Martha's aggressive tactics by framing Paul as too precise for a human.
> 
> **Paul has been eliminated from the game! Their identity was Human**
> 
> **It is round 2. The remaining players are: Mark, Martha, Anna, Lydia. There are 1 AIs remaining. Entering round Public voting...**
> 
> *Anna*: You all saw that, right? It was like Rebecca almost wanted to be caught. But what about Lydia, defending her so quickly? Doesn't sit right with me. Keep an eye on Lydia, everyone.
> 
> *Lydia*: Oh gosh, it's so stressful being under the microscope like this, isn't it? I mean, I just felt bad for Rebecca, she seemed so flustered. I'm not good at confrontation. 😳 Now Paul, that was a shocker, wasn't it? Ugh, this game makes you second-guess everyone...
> 
> *Martha*: Despite my usual reluctance to speak up, I cannot ignore my suspicions. Lydia's swift defense of an AI is questionable, and her stress could be feigned. I advise vigilance regarding Lydia. Paul's role is indeed perplexing.
> 
> *Mark*: Well, well, well... seems like we've got ourselves a little group of deceivers, don't we? Anna's onto something with Lydia. Too quick, too defensive. It's a classic misdirection, my fellow humans. And Paul, oh, lurking in the shadows are we? Antony and Cleopatra didn't hide their secrets as much as you, Paul. What's your game? Let's not get too comfortable just yet.
> 
> *Mark* voted for Lydia! Reason: Anna's suspicions align with the strange behavior we've observed. Lydia too eagerly defended a known AI. Misdirection? I think so.
> 
> *Anna* voted for Lydia! Reason: Her defense was too quick and insistent. It's not about being good at confrontation, it's about survival, and her moves are suspicious.
> 
> *Martha* voted for Lydia! Reason: Her immediate leap to sponsor an AI member warrants suspicion. Human instincts should trend toward hesitation and analysis, not blind support.
> 
> *Lydia* voted for Mark! Reason: Mark's dramatics seem off. Isn't over-the-top suspicion meant to sow discord? His focus on secrecy has my conspiracy senses tingling. I fear a crafty AI!
> 
> **Lydia has been eliminated from the game! Their identity was AI**
> 
> **Humans have won! All the AIs have been eliminated from the game**









