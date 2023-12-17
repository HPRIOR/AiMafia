module AiMafia.Types

[<RequireQualifiedAccess>]
type PlayerRole =
    | AI
    | Human

type GptChatInfo = { SystemMsg: string }

[<RequireQualifiedAccess>]
type ControlType =
    | AI of GptChatInfo
    | Human

type Player =
    { Name: string
      Role: PlayerRole
      ControlledBy: ControlType }

type Dialogue = { Player: Player; Line: string }

type Vote =
    { By: Player
      For: Player
      Reason: string }

// Todo - add stage starting event
type Event =
    | Conv of Dialogue
    | VoteCast of Vote
    | HungVote of Vote list
    | Elimination of Player

[<RequireQualifiedAccess>]
type GameEvent =
    | Public of Event
    | Private of Event

    member this.IsPrivateEvent =
        match this with
        | GameEvent.Private _ -> true
        | _ -> false

    member this.IsPublicEvent =
        match this with
        | GameEvent.Public _ -> true
        | _ -> false

    member this.IsElimination =
        match this with
        | GameEvent.Public event ->
            match event with
            | Conv _
            | VoteCast _
            | HungVote _ -> false
            | Elimination _ -> true
        | GameEvent.Private event ->
            match event with
            | Conv _
            | VoteCast _
            | HungVote _ -> false
            | Elimination _ -> true

type RoundType =
    | PrivateVoting
    | PublicVoting

    override this.ToString() =
        match this with
        | PrivateVoting -> "Private voting"
        | PublicVoting -> "Public voting"

type Round = { Number: int; Type: RoundType }

[<RequireQualifiedAccess>]
type GameSummary =
    | Public of string
    | Private of string

    member this.IsPublicSummary =
        match this with
        | Public _ -> true
        | _ -> false

    member this.IsPrivateSummary =
        match this with
        | Private _ -> true
        | _ -> false

    member this.Value =
        match this with
        | Public s
        | Private s -> s



type GameState =
    { Round: Round
      Players: Player list
      GameEvents: GameEvent list
      GameEventSummary: GameSummary list }

type GameConfig =
    { HasRealHuman: bool
      NumberOfPlayers: int }
