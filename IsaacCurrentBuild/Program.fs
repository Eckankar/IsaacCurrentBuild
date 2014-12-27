open System
open System.Text.RegularExpressions
open System.IO

let inline (<<) f g x = f(g x)

let (=~) s t = String.Compare(s, t, StringComparison.InvariantCultureIgnoreCase) = 0

let activatedItems = ["Anarchist Cookbook"; "The Bean"; "Best Friend"; "The Bible"; "Blank Card"; "Blood Rights"; "Bob's Rotten Head"; "The Book Of Belial"; "Book Of Revelations"; "Book Of Secrets"; "Book Of Shadows"; "The Book Of Sin"; "The Boomerang"; "Box Of Spiders"; "Breath Of Life"; "Butter Bean"; "The Candle"; "Converter"; "Crack The Sky"; "Crystal Ball"; "D4"; "The D6"; "D10"; "D20"; "D100"; "Dad's Key"; "Dead Sea Scrolls"; "Deck Of Cards"; "Doctor's Remote"; "Flush!"; "Forget Me Now"; "The Gamekid"; "Guppy's Head"; "Guppy's Paw"; "Head Of Krampus"; "How To Jump"; "The Hourglass"; "Isaac's Tears"; "IV Bag"; "The Jar"; "Kamikaze!"; "Lemon Mishap"; "Magic Fingers"; "Mom's Bottle Of Pills"; "Mom's Bra"; "Mom's Pad"; "Monster Manual"; "Monstro's Tooth"; "Mr. Boom"; "My Little Unicorn"; "The Nail"; "The Necronomicon"; "Notched Axe"; "Pandora's Box"; "The Pinking Shears"; "The Pony"; "The Poop"; "Portable Slot"; "Prayer Card"; "Razor Blade"; "Red Candle"; "Remote Detonator"; "Satanic Bible"; "Scissors"; "Shoop Da Whoop!"; "Spider Butt"; "Tammy's Head"; "Telepathy For Dummies"; "Teleport"; "Undefined"; "Unicorn Stump"; "We Need To Go Deeper!"; "White Pony"; "Yum Heart"]
let guppyItems = ["Dead Cat"; "Guppy's Collar"; "Guppy's Head"; "Guppy's Paw"; "Guppy's Tail"; "Guppy's Hairball"]
let flyItems = ["???'s Only Friend"; "BBF"; "Best Bud"; "Big Fan"; "Distant Admiration"; "Forever Alone"; "Halo of Flies"; "Hive Mind"; "Skatole"; "Smart Fly"; "The Mulligan"; "Infestation"]

let (|Match|_|) pattern input =
    let m = Regex.Match(input, pattern) in
    if m.Success then Some (List.tail [ for g in m.Groups -> g.Value ]) else None

let rec findLastGame ls g =
    match ls with
       | [] -> g
       | (l :: ls) -> match l with
                        | Match "RNG Start Seed: (.{4} .{4})" [seed] -> findLastGame ls (Some (seed, l::ls))
                        | _                        -> findLastGame ls g

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

let isInList ls item = Option.isSome (List.tryFind (fun e -> e =~ item) ls)

let isActive = isInList activatedItems

let purgeActive = List.filter (not << isActive)
let lastActive = List.tryFind isActive

let uniq ls = List.foldBack (fun e a -> if isInList a e then a else e :: a) ls []

let printTransformationStatus items transItems transname fulltrans =
            match (uniq (List.filter (isInList transItems) items)) with
                | []                -> ()
                | its -> printfn "%s status: %s (%s)" transname
                                                      (if List.length its >= 3
                                                       then fulltrans
                                                       else sprintf "%d/3" (List.length its))
                                                      (String.concat ", " its)

[<EntryPoint>]
let main argv =
    let file = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\My Games\\Binding of Isaac Rebirth\\log.txt"
    let f = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
    let sr = new StreamReader(f)
    let lines = (sr.ReadToEnd ()).Split([|'\n'|]) |> Array.toList
    sr.Close()
    f.Close()

    match findLastGame lines None with
        | Some (seed, lastGame) ->
            printfn "Playing as %s" (currentPlayer lastGame)

            let obtItems = obtainedItems lastGame []
            let its = match (lastActive obtItems, purgeActive obtItems) with
                        | (None, its)    -> its
                        | (Some it, its) -> it :: its

            printfn "Current seed: %s" seed
            printfn "Current items: %s" (String.concat ", " its)
            
            printTransformationStatus obtItems guppyItems "Guppy" "CATMODE"
            printTransformationStatus obtItems flyItems "Lord of the Flies" "Jeff Goldblum"

            0
        | None ->
            printfn "Can't find any Isaac game info."
            0
