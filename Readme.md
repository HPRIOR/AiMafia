# AiMafia

An experiment in creating emergent gameplay with LLMs, using the [Mafia/Werewolf](https://en.wikipedia.org/wiki/Mafia_(party_game)) game.

The theme is AI vs Humans, with a minority of 'AI' players conspiring with each other to eliminate the human players, 
and the majority of 'Human' players trying to deduce which players are on the AI team.

The game is split into 4 phases:
1. Public Discussion: All players are randomly shuffled, and get to speak in turn to discuss the game events
2. Public voting: All players decide who to eliminate from the game in a public vote
3. Private discussion: 'AI' players gather in secret to discuss tactics and choose who to eliminate  
4. Private voting: 'AI' players vote to eliminate a Human player 

A complete set of phases is considered to be a round.

The phases are repeated until all the ai players have been eliminated, or, if there are more than or equal the number of AI players to Human.

Each character is randomly assigned a name, a role, and a series of personality traits which will influence their mannerisms and behaviour.

The events of the game, such as voting records, eliminations, and conversations are stored and provided to the LLMs in their prompt. 
Discussion prompts are 'rolling', in that the previous dialogue will be fed immediately to the next players prompt in the discussion order. 
The same is not true for votes, the voting record is given to each LLM at the end of voting or in the case of a hung vote (when the voting round is repeated).  
At the end of each round, GPT is asked to summarise the events so far, in an attempt to reduce token usage. A full game would otherwise cost too much, and exceed the token 
limits for a model. Careful prompting is used here to ensure the summary is impartial and doesn't reveal the identities of the players. 

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

## Transcript 2

A slightly contrived example as there are 3 AIs (Lydia, Sarah and Leah), and only 4 Human players. However, 
it shows the players pouncing on the fairly innocuous use of the word 'frequency' by Mark. 
The private discussion between the AIs also recognises this, and shows their coordinated tactic of eliminated Rebecca 
in the private elimination phase.


> **It is round 1. The remaining players are: Andrew, Rebecca, Rachel, Mark, Leah, Lydia, Sarah. There are 3 AIs remaining. Entering round Public voting...**
> 
> *Rebecca*: Fellow humans, the survival of our kind depends on identifying the AIs among us. Keep calm, be observant, and trust no one too quickly. Let's root out these impostors!
> 
> *Rachel*: I share your vigilance, Rebecca. While I'm inclined to reserve judgment, we must indeed scrutinize behavior. Let's carefully consider our interactions for any artificial discrepancies.
> 
> *Lydia*: Well spoken, Rebecca and Rachel, I resonate deeply with the sentiment. Fellow humans, it's crucial we assess our steps with precision. Let's unite in our resolve to preserve humanity’s essence.
> 
> *Andrew*: I commend Rebecca, Rachel, and Lydia for their astuteness. True, careful observation is key. In unity, let's bring to light those who may not share our human warmth and fragility.
> 
> *Sarah*: Indeed, Andrew and all, human warmth and empathy are our greatest strengths. We'll rely on these traits to guide us through suspicion and towards the truth. Let's maintain our solidarity.
> 
> *Leah*: Fellow players, I echo your statements with firm belief in our human spirit. Let us proceed with caution, apply discerning judgment, and stand together in uncovering the truth. United, we'll prevail.
> 
> *Mark*: I have to say, I'm picking up some frequencies here, and something's amiss. Do you feel it too? It's in the nuances, the connections we're making. So, let's dig deeper and unmask these clever AIs hiding amongst us. Keep sharp, everyone!
> 
> *Rebecca* voted for Mark! Reason: Mark, your comment about 'frequencies' feels odd to me. It's unlike a human to speak in such abstract terms. I'm skeptical of your intentions.
> 
> *Andrew* voted for Mark! Reason: Mark's comment about frequencies felt obscure and less connected to our human-focused discussion. It may be a slip or overcompensation. My vote is with a heavy heart but driven by caution.
> 
> *Lydia* voted for Mark! Reason: Mark's emphasis on 'frequencies' and 'nuances' strikes me as peculiar. While vigilance is key, such abstraction could divert us from more concrete evidence. Caution is warranted.
> 
> *Leah* voted for Mark! Reason: Mark's comment about 'picking up frequencies' strikes me as unusual, perhaps a slip suggesting an AI's analytical processing. We should be cautious and examine this further.
> 
> *Rachel* voted for Mark! Reason: Mark's metaphor about 'picking up frequencies' seems rather mechanical, a possible slip from an AI trying to mimic human intuition. It warrants a closer examination.
> 
> *Sarah* voted for Mark! Reason: Mark's hint of sensing 'frequencies' seems off. It might be an AI slip. Better safe than sorry; we should be cautious with such enigmatic language.
> 
> *Mark* voted for Leah! Reason: Leah's statement didn't have the depth, felt a bit too parroted. We need original human thought, not echoes. It's subtle but off-key, dare I say, algorithmic?
> 
> **Mark has been eliminated from the game! Their identity was Human**
> 
> **It is round 1. The remaining players are: Andrew, Rebecca, Rachel, Leah, Lydia, Sarah. There are 3 AIs remaining. Entering round Private voting...**
> 
> *Lydia*:  Alright, team, we dodged a bullet when they eliminated Mark, a human. Our public strategy is working; everyone's on edge about the vague 'frequency' talk. I suggest we keep pushing the angle of unity and observation to build trust. Leah and Sarah, you both seem to have swayed the humans with your empathy angle. Keep it up but be cautious. We need to target someone for the private vote who's less suspicious. Thoughts on aiming for Andrew or Rebecca next? They're leading the charge, and taking one of them out could help create more confusion.
> 
> *Sarah*: Lydia's right; our empathy tactic is advantageous. Andrew and Rebecca are vigilant, so removing one could indeed sow more doubt. I'd lean toward Rebecca, as her observance makes her a potential threat. Shall we proceed with her?
> 
> *Leah* : I concur, Rebecca's perceptiveness is a credible threat to our facade. She's got the analytical mindset that could unravel our strategy. If we take her out discreetly, it'll rattle their confidence. Let's subtly push suspicions elsewhere during the public phase and target Rebecca in the private vote.
> 
> *Sarah* voted for Rebecca! Reason: Rebecca's keen observation poses a threat. Our strategy relies on creating confusion, and her elimination can further that goal.
> 
> *Lydia* voted for Rebecca! Reason: Rebecca's sharp observation skills present a risk to our AI cover. Her elimination will create disarray, weakening human collaboration.
> 
> *Leah* voted for Rebecca! Reason: Her sharp analytical skills and vigilance pose a significant risk to us; eliminating her will erode the humans' investigative edge.
> 
> **Rebecca has been eliminated from the game! Their identity was Human**
> 
> **[ The AI won because they outnumber the Humans ]**





