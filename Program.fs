open System
open System.IO
open System.Text.Json
open System.Threading
open AiMafia.Types
open OpenAI_API.Chat
open OpenAI_API.Models

let srcDir = __SOURCE_DIRECTORY__
// let model = "gpt-3.5-turbo-1106"
let model = Model.GPT4_Turbo
let requestSleepInterval = TimeSpan.FromSeconds(5)

let summarizeSysMsg =
    File.ReadAllText(Path.Combine(srcDir, "SystemMsgSummarize.txt"))

module Retry =
    let rec withCount (f: unit -> 'T) (predicate: 'T -> bool) (i: int) =
        if i <= 0 then
            failwith $"Max retries reached for: {typeof<'T>.Name}"
        else
            let t = f ()
            if predicate t then t else withCount f predicate (i - 1)

module List =
    let shuffle list =
        let random = Random()
        list |> List.sortBy (fun _ -> random.Next())

    let stringList (list: string list) =
        match list with
        | [] -> ""
        | list -> list |> List.reduce (fun acc s -> $"{acc}, {s}")

    let stringPara (list: string list) =
        match list with
        | [] -> ""
        | list -> list |> List.reduce (fun acc s -> $"{acc}\n{s}")

let openAiKey = Environment.GetEnvironmentVariable("OPENAI_KEY")
let chatApi = OpenAI_API.OpenAIAPI(openAiKey)


module GameEvent =
    let private stringFromPublicVote (vote: Vote) : string =
        $"{vote.By.Name} voted publicly to eliminate {vote.For.Name}. Reason: {vote.Reason}."

    let private stringFromPrivateVote (vote: Vote) : string =
        $"{vote.By.Name} voted privately to eliminate {vote.For.Name}. Reason: {vote.Reason}."

    let private stringFromPublicEvent (event: Event) =
        match event with
        | Conv { Player = player; Line = line } -> $"(Public) {player.Name}: '{line}'."
        | VoteCast vote -> stringFromPublicVote vote
        | HungVote votes -> votes |> List.map stringFromPublicVote |> List.stringPara
        | Elimination player ->
            $"{player.Name} was eliminated from the game after a public vote. Their true identity was {player.Role}!"

    let rec private stringFromPrivateEvent (event: Event) =
        match event with
        | Conv { Player = player; Line = line } -> $"(Private) {player.Name}: '{line}'."
        | VoteCast vote -> stringFromPrivateVote vote
        | HungVote votes -> votes |> List.map stringFromPrivateVote |> List.stringPara
        | Elimination player -> $"{player.Name} was eliminated from the game after a private vote."

    let private stringFromGameEvent (event: GameEvent) : string =
        match event with
        | GameEvent.Public event -> stringFromPublicEvent event
        | GameEvent.Private event -> stringFromPrivateEvent event

    let summarizePublicRound (events: GameEvent list) : string =
        let publicEvents = events |> List.filter (_.IsPublicEvent)

        publicEvents |> List.map stringFromGameEvent |> List.stringPara


    let summarizePrivateRound (events: GameEvent list) : string =
        events |> List.map stringFromGameEvent |> List.stringPara

    let summariseWithGpt (inputEventSummary: string) : string =
        let chat = chatApi.Chat.CreateConversation()
        chat.Model <- model
        chat.AppendSystemMessage(summarizeSysMsg)

        chat.AppendUserInput($"Summarize the following round events: {inputEventSummary}.\n")

        let chatResponse =
            chat.GetResponseFromChatbotAsync() |> Async.AwaitTask |> Async.RunSynchronously

        // to avoid rate limits
        Thread.Sleep(requestSleepInterval)
        chatResponse


module GameState =
    type private PlayerConfigInfo = { Name: string; Role: PlayerRole }

    let private gameRulesAI = File.ReadAllText(Path.Combine(srcDir, "SystemMsgAI.txt"))

    let private gameRulesHuman =
        File.ReadAllText(Path.Combine(srcDir, "SystemMsgHuman.txt"))

    let private playerNames =
        File.ReadAllLines(Path.Combine(srcDir, "PlayerNames.txt"))
        |> Array.toList
        |> List.map _.Trim()

    let private characterTraits =
        File.ReadAllLines(Path.Combine(srcDir, "CharacterTraits.txt"))
        |> Array.toList
        |> List.map _.Trim()

    let private generateCharacterTraits () =
        $"""Your character traits are: {(characterTraits |> List.shuffle |> List.take 5) |> List.stringList}"""

    let private generateOtherPlayerDescription
        (playerConfig: PlayerConfigInfo)
        (otherPlayerConfig: PlayerConfigInfo list)
        =
        let otherPlayerNames = otherPlayerConfig |> List.map (_.Name) |> List.stringList

        match playerConfig.Role with
        | PlayerRole.AI ->
            let otherAiPlayers =
                otherPlayerConfig
                |> List.choose (fun p ->
                    match p.Role with
                    | PlayerRole.AI -> Some p.Name
                    | _ -> None)
                |> List.stringList

            $"You are playing with: {otherPlayerNames}. The other AI players are: {otherAiPlayers}"
        | PlayerRole.Human -> $"You are playing with: {otherPlayerNames}."

    let private gptSystemMsg (playerConfig: PlayerConfigInfo) (otherPlayers: PlayerConfigInfo list) =
        let roleStr =
            match playerConfig.Role with
            | PlayerRole.AI -> "AI"
            | PlayerRole.Human -> "Human"

        let gameRules =
            match playerConfig.Role with
            | PlayerRole.AI -> gameRulesAI
            | PlayerRole.Human -> gameRulesHuman

        $"{gameRules}.\nYour name is {playerConfig.Name}.\n{generateCharacterTraits ()}.\nYou are on the '{roleStr}' team.\n{generateOtherPlayerDescription playerConfig otherPlayers}"

    let private generatePlayer (playerConfig: PlayerConfigInfo) (otherPlayers: PlayerConfigInfo list) : Player =
        { Name = playerConfig.Name
          Role = playerConfig.Role
          ControlledBy = ControlType.AI({ SystemMsg = gptSystemMsg playerConfig otherPlayers }) }


    let init (config: GameConfig) : GameState =
        if config.NumberOfPlayers > 20 then
            // only have 20 names in PlayerNames.txt
            failwith "Max 20 players supported"

        let numAis: int =
            let div = config.NumberOfPlayers / 4
            if div = 1 then div + 1 else div

        let allocatedNames = playerNames |> List.shuffle |> List.take config.NumberOfPlayers

        let aiPlayers =
            allocatedNames[0 .. numAis - 1]
            |> List.map (fun name -> { Role = PlayerRole.AI; Name = name })

        let humanPlayers =
            allocatedNames[numAis..]
            |> List.map (fun name -> { Role = PlayerRole.Human; Name = name })

        let allPlayerConfigs = aiPlayers |> List.append humanPlayers

        let players =
            allPlayerConfigs
            |> List.map (fun selfConfig ->
                let otherPlayers =
                    allPlayerConfigs
                    |> List.filter (fun otherConfig -> otherConfig.Name <> selfConfig.Name)

                { Name = selfConfig.Name
                  Role = selfConfig.Role
                  ControlledBy = ControlType.AI({ SystemMsg = gptSystemMsg selfConfig otherPlayers }) })

        { Players = players
          Round = { Number = 1; Type = PublicVoting }
          GameEvents = []
          GameEventSummary = [] }

module Vote =
    type ChatRequestGameSummary =
        { RoundSummary: string
          GameSummary: string }

    type OutCome =
        | Loser of Player
        | Tied of Player list

    [<CLIMutable>]
    type VoteResponse = { Player: string; Reason: string }

    let allPlayersButSelf (self: Player) (otherPlayers: Player List) =
        otherPlayers |> List.filter (fun p -> p.Name <> self.Name)

    let requestVote sysMsg usrMsg =
        let request = ChatRequest()
        request.Model <- model
        request.ResponseFormat <- ChatRequest.ResponseFormats.JsonObject

        request.Messages <-
            [| ChatMessage(ChatMessageRole.System, sysMsg)
               ChatMessage(ChatMessageRole.User, usrMsg) |]

        let response =
            chatApi.Chat.CreateChatCompletionAsync(request)
            |> Async.AwaitTask
            |> Async.RunSynchronously

        let gptResponse = response.ToString()
        JsonSerializer.Deserialize<VoteResponse>(gptResponse)

    let private getGameSummary
        (player: Player)
        (gameEvents: GameEvent list)
        (gameSummary: GameSummary list)
        (roundSumType: GameEvent list -> string)
        : string =
        let roundSummary =
            let summary = gameEvents |> roundSumType

            if summary.Length > 0 then
                $"Here is the summary of the round so far:\n{summary}.\n"
            else
                ""

        let gameSummary =
            let summary =
                match player.Role with
                | PlayerRole.AI -> gameSummary |> List.map (_.Value) |> List.stringPara
                | PlayerRole.Human ->
                    gameSummary
                    |> List.choose (function
                        | GameSummary.Public s -> Some s
                        | _ -> None)
                    |> List.stringPara


            if summary.Length > 0 then
                $"Here is the summary of the game so far:\n{summary}\n"
            else
                ""

        $"{gameSummary}{roundSummary}"

    let elicitVote
        (playersToVoteFor: Player list)
        (gameEvents: GameEvent list)
        (gameSummary: GameSummary list)
        (votingMsg: string)
        (player: Player)
        : Vote =
        match player with
        | { ControlledBy = ControlType.AI gptChatInfo
            Role = role } ->

            let usrMsg =
                let allPlayersButVoter =
                    allPlayersButSelf player playersToVoteFor
                    |> List.map (_.Name)
                    |> List.stringList

                let whoToVoteFor =
                    $"{votingMsg}. Here are the list of players you must choose between: {allPlayersButVoter}.\n"

                let votingInstructions =
                    "Return JSON with a 'Player' field with a string containing just the player you wish to vote for, and another field 'Reason' with a string containing your reasons for voting for this player\n"

                let votingMsg summary =
                    $"{summary}{whoToVoteFor}{votingInstructions}"

                match role with
                | PlayerRole.AI ->
                    let roundAndGameSummary =
                        getGameSummary player gameEvents gameSummary GameEvent.summarizePrivateRound

                    votingMsg roundAndGameSummary
                | PlayerRole.Human ->
                    let roundAndGameSummary =
                        getGameSummary player gameEvents gameSummary GameEvent.summarizePublicRound

                    votingMsg roundAndGameSummary

            let isValidVote (voteResponse: VoteResponse) =
                (playersToVoteFor |> List.tryFind (fun p -> p.Name = voteResponse.Player))
                |> Option.map (fun _ -> true)
                |> Option.defaultValue false

            let voteResponse =
                Retry.withCount (fun _ -> requestVote gptChatInfo.SystemMsg usrMsg) isValidVote 3

            // To avoid rate limits
            Thread.Sleep(requestSleepInterval)

            printfn $"{player.Name} voted for {voteResponse.Player}! Reason: {voteResponse.Reason}"

            { By = player
              For = (playersToVoteFor |> List.find (fun p -> p.Name = voteResponse.Player))
              Reason = voteResponse.Reason }

        | { ControlledBy = ControlType.Human } ->
            let playersToEliminate = allPlayersButSelf player playersToVoteFor

            let playerNamesToEliminate =
                allPlayersButSelf player playersToVoteFor |> List.map (_.Name)

            printfn $"{votingMsg}: {playerNamesToEliminate}"
            printfn $"Enter the name of the player you wish to eliminate"
            let mutable playerVoteAttempt = ""

            let isValidChoice () =
                playerNamesToEliminate
                |> List.map (_.ToLower())
                |> List.contains playerVoteAttempt

            while (not (isValidChoice ())) do
                let voteAttempt = Console.ReadLine().ToLower()
                playerVoteAttempt <- voteAttempt

                if not (isValidChoice ()) then
                    printfn "Please select a valid name"


            printfn "Please give a reason for this vote:"

            { For = playersToEliminate |> List.find (fun p -> p.Name = playerVoteAttempt)
              By = player
              Reason = Console.ReadLine() }


    let decideVote (votes: Player list) : OutCome =
        let voteCount = votes |> List.countBy id
        let maxCount = voteCount |> List.maxBy snd |> snd

        let playersWithMaxVotes =
            voteCount |> List.filter (fun (_, i) -> i = maxCount) |> List.map fst

        match playersWithMaxVotes with
        | [] -> failwith "shouldn't happen but fail just in case"
        | [ player ] ->
            printfn $"{player.Name} has been eliminated from the game! Their identity was {player.Role}"
            Loser player
        | tied ->
            printfn $"The vote is tied between {tied |> List.map (_.Name) |> List.stringList}! Voting again!"
            Tied tied



module Chat =
    let private getGptInput player gptInfo usrMsg =
        let chat = chatApi.Chat.CreateConversation()
        chat.Model <- model
        chat.AppendSystemMessage(gptInfo.SystemMsg)

        chat.AppendUserInput(usrMsg)

        printfn $"{player.Name} is thinking..."

        let chatResponse =
            chat.GetResponseFromChatbotAsync() |> Async.AwaitTask |> Async.RunSynchronously


        printfn $"{player.Name}: {chatResponse}"

        // to avoid rate limits
        Thread.Sleep(requestSleepInterval)

        { Player = player; Line = chatResponse }

    let private getGameSummary (gameSummary: GameSummary list) (gameEvents: GameEvent list) =
        let privateGameSummary =
            let summary = gameSummary |> List.map (_.Value) |> List.stringPara

            if summary.Length > 0 then
                $"Here are the events of the game so far:\n{summary}.\n"
            else
                ""

        let privateRoundSummary =
            let summary = gameEvents |> GameEvent.summarizePrivateRound

            if summary.Length > 0 then
                $"Here are the events of the round so far:\n{summary}\n"
            else
                ""

        let firstInputMsg =
            if privateGameSummary.Length = 0 && privateRoundSummary.Length = 0 then
                "You are the first person to speak.\n"
            else
                ""

        $"This is a public discussion round.\n{firstInputMsg}{privateGameSummary}{privateRoundSummary}It is your turn speak to the group."

    let elicitPublicMessage (player: Player) (gameSummary: GameSummary list) (gameEvents: GameEvent list) =
        match player with
        | { ControlledBy = ControlType.Human } ->
            printfn $"{player.Name}, it's your turn to speak. What would you like to say to the group?"

            GameEvent.Public(
                Conv(
                    { Player = player
                      Line = Console.ReadLine() }
                )
            )

        | { Role = role
            ControlledBy = ControlType.AI gptInfo } ->
            let usrMsg =
                match role with
                | PlayerRole.AI -> getGameSummary gameSummary gameEvents
                | PlayerRole.Human ->
                    let publicGameSummary = gameSummary |> List.filter (_.IsPublicSummary)

                    getGameSummary publicGameSummary gameEvents

            GameEvent.Public(Conv(getGptInput player gptInfo usrMsg))

    let elicitPrivateMessage (player: Player) (gameSummary: GameSummary list) (gameEvents: GameEvent list) =
        match player with
        | { Role = PlayerRole.AI
            ControlledBy = ControlType.Human } ->
            printfn
                $"{player.Name}, it's your turn to speak, and argue to the group who should be eliminated from the game"

            GameEvent.Public(
                Conv(
                    { Player = player
                      Line = Console.ReadLine() }
                )
            )

        | { Role = PlayerRole.AI
            ControlledBy = ControlType.AI gptInfo } ->
            let usrMsg =
                let gameAndRoundSummary = getGameSummary gameSummary gameEvents

                $"This is a private discussion round between the AI team. {gameAndRoundSummary}. It is your turn speak to the group, and argue who should be eliminated from the game. You are on the AI team. You can speak openly about your AI identity for this discussion, and discuss tactics with your fellow AI players."

            GameEvent.Public(Conv(getGptInput player gptInfo usrMsg))
        | _ -> failwith "Found Human player trying to speak in private voting round!"


module Round =
    let publicDiscussion (gameState: GameState) : GameState =
        let players = gameState.Players |> List.shuffle

        let updatedGameEvents =
            (gameState.GameEvents, players)
            ||> List.fold (fun events player ->
                List.append events [ Chat.elicitPublicMessage player gameState.GameEventSummary events ])

        { gameState with
            GameEvents = updatedGameEvents }

    let privateDiscussion (gameState: GameState) : GameState =
        let players =
            gameState.Players
            |> List.filter (fun p -> p.Role = PlayerRole.AI)
            |> List.shuffle

        let updatedGameEvents =
            (gameState.GameEvents, players)
            ||> List.fold (fun events player ->
                List.append events [ Chat.elicitPrivateMessage player gameState.GameEventSummary events ])

        { gameState with
            GameEvents = updatedGameEvents }


    let rec publicVote (gameState: GameState) : GameState =
        let votingPLayers = gameState.Players |> List.shuffle

        let votingMsg =
            "This is a public voting round. You must now vote publicly to eliminate one of the remaining players"

        let elicitVote =
            Vote.elicitVote votingPLayers gameState.GameEvents gameState.GameEventSummary votingMsg

        let votes = votingPLayers |> List.map elicitVote

        match Vote.decideVote (votes |> List.map (_.For)) with
        | Vote.Loser losingPlayer ->
            let eliminatedEvent = Elimination losingPlayer |> GameEvent.Public

            { gameState with
                GameEvents =
                    let gameEventsWithVotes =
                        List.append gameState.GameEvents (votes |> List.map (VoteCast >> GameEvent.Public))

                    List.append gameEventsWithVotes [ eliminatedEvent ]
                Players = gameState.Players |> List.filter (fun p -> losingPlayer.Name <> p.Name) }

        | Vote.Tied _ ->
            let tiedEvent = HungVote(votes) |> GameEvent.Public

            let updatedGameState =
                { gameState with
                    GameEvents = List.append gameState.GameEvents [ tiedEvent ] }

            publicVote updatedGameState

    let rec privateVote (gameState: GameState) : GameState =
        let votingPlayers =
            gameState.Players
            |> List.filter (fun p -> p.Role = PlayerRole.AI)
            |> List.shuffle

        let playersToVoteFor =
            gameState.Players
            |> List.filter (fun p -> p.Role = PlayerRole.Human)
            |> List.shuffle

        let votingMsg =
            "This is a private voting round between the AI team.
            You are on the AI team, you must now vote to eliminate one of the Human players.
            You must speak openly about your AI role, and plan tactics against the human team.
            You must come to a consensus with your fellow AI team members"

        let elicitVote =
            Vote.elicitVote playersToVoteFor gameState.GameEvents gameState.GameEventSummary votingMsg

        let votes = votingPlayers |> List.map elicitVote

        match Vote.decideVote (votes |> List.map (_.For)) with
        | Vote.Loser losingPlayer ->
            let eliminatedEvent = Elimination losingPlayer |> GameEvent.Private

            { gameState with
                GameEvents =
                    let gameEventsWithVotes =
                        List.append gameState.GameEvents (votes |> List.map (VoteCast >> GameEvent.Private))

                    List.append gameEventsWithVotes [ eliminatedEvent ]
                Players = gameState.Players |> List.filter (fun p -> losingPlayer.Name <> p.Name) }

        | Vote.Tied _ ->
            let tiedEvent = HungVote(votes) |> GameEvent.Private

            let updatedGameState =
                { gameState with
                    GameEvents = List.append gameState.GameEvents [ tiedEvent ] }

            privateVote updatedGameState


module Display =
    let gameLoopInfoMsg (gameState: GameState) =

        match gameState with
        | { Round = round; Players = players } ->
            let playerNames = players |> List.map (_.Name) |> List.stringList
            let aiLeft = players |> List.filter (fun p -> p.Role = PlayerRole.AI) |> List.length

            printfn
                $"It is round {round.Number}. The remaining players are: {playerNames}. There are {aiLeft} AIs remaining. Entering round {round.Type}..."


module Game =
    let private checkEndGame (players: Player list) =
        let aiPlayers, humanPlayers =
            players |> List.partition (fun player -> player.Role = PlayerRole.AI)

        match aiPlayers, humanPlayers with
        | [], h when h |> List.length > 0 ->
            printfn "Humans have won! All the AIs have been eliminated from the game"
            exit 0
        | a, [] when a |> List.length > 0 ->
            printfn "AI have won! All the humans have been eliminated from the game"
            exit 0
        | a, h when a |> List.length = 1 && h |> List.length = 1 ->
            printfn "The game is a tie! There is exactly one human and one ai left"
            exit 0

        | _ -> ()

    let rec loop (gameState: GameState) : GameState =
        checkEndGame gameState.Players
        Display.gameLoopInfoMsg gameState

        match gameState with
        | { Round = { Type = PrivateVoting } } ->
            let stateAfterDiscussion =
                if
                    (gameState.Players
                     |> List.filter (fun p -> p.Role = PlayerRole.AI)
                     |> List.length = 1)
                then
                    // no discussion with a single AI player
                    gameState
                else
                    Round.privateDiscussion gameState

            let stateAfterVote = Round.privateVote stateAfterDiscussion

            let privateGameEvents =
                stateAfterVote.GameEvents
                |> GameEvent.summarizePrivateRound
                |> GameEvent.summariseWithGpt
                |> GameSummary.Private

            let newGameState =
                { stateAfterVote with
                    Round =
                        { Number = stateAfterVote.Round.Number + 1
                          Type = RoundType.PublicVoting }
                    GameEvents = []
                    GameEventSummary = List.append stateAfterVote.GameEventSummary [ privateGameEvents ] }

            loop newGameState
        | { Round = { Type = PublicVoting } } ->
            let stateAfterDiscussion = Round.publicDiscussion gameState
            let stateAfterVote = Round.publicVote stateAfterDiscussion

            let publicGameEvents =
                stateAfterVote.GameEvents
                |> GameEvent.summarizePublicRound
                |> GameEvent.summariseWithGpt
                |> GameSummary.Public

            let newGameState =
                { stateAfterVote with
                    Round =
                        { stateAfterVote.Round with
                            Type = RoundType.PrivateVoting }
                    GameEvents = []
                    GameEventSummary = List.append stateAfterVote.GameEventSummary [ publicGameEvents ] }

            loop newGameState


[<EntryPoint>]
let main _ =
    let initGameState =
        GameState.init
            { HasRealHuman = false
              NumberOfPlayers = 6 }

    Game.loop initGameState |> ignore
    0
