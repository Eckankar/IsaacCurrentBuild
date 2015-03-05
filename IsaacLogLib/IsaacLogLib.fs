﻿namespace IsaacLogLib

open System
open System.Text.RegularExpressions
open System.IO

module IsaacLogLib =
    let inline (<<) f g x = f(g x)

    let (=~) s t = String.Compare(s, t, StringComparison.InvariantCultureIgnoreCase) = 0

    let activatedItems = ["Anarchist Cookbook"; "The Bean"; "Best Friend"; "The Bible"; "Blank Card"; "Blood Rights"; "Bob's Rotten Head"; "The Book Of Belial"; "Book Of Revelations"; "Book Of Secrets"; "Book Of Shadows"; "The Book Of Sin"; "The Boomerang"; "Box Of Spiders"; "Breath Of Life"; "Butter Bean"; "The Candle"; "Converter"; "Crack The Sky"; "Crystal Ball"; "D4"; "The D6"; "D10"; "D20"; "D100"; "Dad's Key"; "Dead Sea Scrolls"; "Deck Of Cards"; "Doctor's Remote"; "Flush!"; "Forget Me Now"; "The Gamekid"; "Guppy's Head"; "Guppy's Paw"; "Head Of Krampus"; "How To Jump"; "The Hourglass"; "Isaac's Tears"; "IV Bag"; "The Jar"; "Kamikaze!"; "Lemon Mishap"; "Magic Fingers"; "Mom's Bottle Of Pills"; "Mom's Bra"; "Mom's Pad"; "Monster Manual"; "Monstro's Tooth"; "Mr. Boom"; "My Little Unicorn"; "The Nail"; "The Necronomicon"; "Notched Axe"; "Pandora's Box"; "The Pinking Shears"; "The Pony"; "The Poop"; "Portable Slot"; "Prayer Card"; "Razor Blade"; "Red Candle"; "Remote Detonator"; "Satanic Bible"; "Scissors"; "Shoop Da Whoop!"; "Spider Butt"; "Tammy's Head"; "Telepathy For Dummies"; "Teleport"; "Undefined"; "Unicorn Stump"; "We Need To Go Deeper!"; "White Pony"; "Yum Heart"]
    let guppyItems = ["Dead Cat"; "Guppy's Collar"; "Guppy's Head"; "Guppy's Paw"; "Guppy's Tail"; "Guppy's Hairball"]
    let flyItems = ["???'s Only Friend"; "BBF"; "Best Bud"; "Big Fan"; "Distant Admiration"; "Forever Alone"; "Halo of Flies"; "Hive Mind"; "Skatole"; "Smart Fly"; "The Mulligan"; "Infestation"]

    let (|Match|_|) pattern input =
        let m = Regex.Match(input, pattern) in
        if m.Success then Some (List.tail [ for g in m.Groups -> g.Value ]) else None

    let rec findLastSection rx ls g =
        match ls with
           | [] -> g
           | (l :: ls) -> match l with
                            | Match rx data -> findLastSection rx ls (Some (data, l::ls))
                            | _             -> findLastSection rx ls g

    let rec findLastMatch rx ls g =
        match ls with
           | [] -> g
           | (l :: ls) -> match l with
                            | Match rx data -> findLastMatch rx ls (Some data)
                            | _             -> findLastMatch rx ls g

    let findLastGame  ls = findLastSection "RNG Start Seed: (.{4} .{4})" ls None
    let findLastFloor ls = findLastSection "Level::Init m_Stage (\d+), m_AltStage (\d+)" ls None
    let findCurse     ls = Option.map List.head <| findLastMatch "(Curse of(?: the)? [^\s!]+)" ls None

    let normalizeCurse s =
        match s with
          | "Curse of Blind" -> "Curse of the Blind"
          | "Curse of Maze"  -> "Curse of the Maze"
          | _                -> s

    let rec obtainedItems ls its =
        match ls with
            | [] -> its
            | (l :: ls) -> match l with
                            | Match "Adding collectible \d+ \(([^)]+)\)" [item] -> obtainedItems ls (item :: its)
                            | _                                                 -> obtainedItems ls its

    let pidToPlayer pid =
        match pid with
            | 0 -> "Isaac"
            | 1 -> "Maggie"
            | 2 -> "Cain"
            | 3 -> "Judas"
            | 4 -> "???"
            | 5 -> "Eve"
            | 6 -> "Samson"
            | 7 -> "Azazel"
            | 8 -> "Lazarus"
            | 9 -> "Eden"
            | 10 -> "The Lost"
            | _  -> sprintf "<PID #%d>" pid

    let currentPlayer ls =
        ls |> List.map (fun l ->
                    match l with 
                        | Match "Initialized player with Variant 0 and Subtype (\d)" [pid] -> [pid]
                        | _                                                                -> []
              )
           |> List.concat |> List.rev |> (fun l -> match l with | [] -> "<ERROR>" | (pid::_) -> pidToPlayer <| Convert.ToInt32 pid)

    let addFloorNumber name num curse =
        match curse with
            | Some "Curse of the Labyrinth" -> sprintf "%s XL" name
            | _                             -> sprintf "%s %d" name num

    let getFloorName fnum alt curse =
        match fnum with
            | 1 | 2 -> addFloorNumber (if alt then "Cellar" else "Basement") fnum curse
            | 3 | 4 -> addFloorNumber (if alt then "Catacombs" else "Caves") (fnum-2) curse
            | 5 | 6 -> addFloorNumber (if alt then "Necropolis" else "Depths") (fnum-4) curse
            | 7 | 8 -> addFloorNumber (if alt then "Utero" else "Womb") (fnum-6) curse
            | 9     -> if alt then "Cathedral" else "Sheol"
            | 11    -> if alt then "Chest" else "Dark Room"
            | _     -> sprintf "Unknown floor (fnum=%d)" fnum

    let isInList ls item = Option.isSome (List.tryFind (fun e -> e =~ item) ls)

    let isActive = isInList activatedItems

    let purgeActive = List.filter (not << isActive)
    let lastActive = List.tryFind isActive

    let uniq ls = List.foldBack (fun e a -> if isInList a e then a else e :: a) ls []


    type IsaacLog() = 
        let mutable passiveItems = []
        let mutable activeItem = None
        let mutable seed = ""
        let mutable currentFloor = ""
        let mutable currentCurse = None
        let mutable guppyProgress = []
        let mutable lotfProgress = []
        let mutable character = ""
        let mutable changed = false

        member this.PassiveItems with get() : string list = passiveItems
        member this.ActiveItem with get() : string option = activeItem
        member this.Seed with get() : string = seed
        member this.CurrentFloor with get() : string = currentFloor
        member this.CurrentCurse with get() : string option = currentCurse
        member this.GuppyProgress with get() : string list = guppyProgress
        member this.LotFProgress with get() : string list = lotfProgress
        member this.Character with get() : string = character
        member this.HasChanged with get() : bool = changed

        member private this.set (target : byref<_>, value) =
            if target <> value then changed <- true
            target <- value

        member this.refreshLog () =
            let file = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\My Games\\Binding of Isaac Rebirth\\log.txt"
            let f = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            let sr = new StreamReader(f)
            let lines = (sr.ReadToEnd ()).Split([|'\n'|]) |> Array.toList
            sr.Close()
            f.Close()

            changed <- false

            match findLastGame lines with
                | Some ([seed'], lastGame) ->
                    let obtItems = obtainedItems lastGame []
                    
                    this.set(&activeItem, lastActive obtItems)
                    this.set(&passiveItems, purgeActive obtItems)

                    this.set(&seed, seed')

                    let (fnum, alt, lastFloor) = match findLastFloor lastGame with
                                                    | Some ([fnum; alt], lastFloor) ->  (fnum, alt, lastFloor)
                                                    | _                             -> ("-1", "-1", [])

                    let curse = Option.map normalizeCurse <| findCurse lastFloor
                    let floorName = getFloorName (Convert.ToInt32 fnum) (alt = "1") curse

                    this.set(&currentFloor, floorName)
                    this.set(&currentCurse, curse)
                    this.set(&character, currentPlayer lastGame)

                    this.set(&guppyProgress, uniq (List.filter (isInList guppyItems) obtItems))
                    this.set(&lotfProgress, uniq (List.filter (isInList flyItems) obtItems))
                | _ ->
                    ()
                
