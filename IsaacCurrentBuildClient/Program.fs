namespace IsaacCurrentBuildClient

module IsaacCurrentBuildClient =
    open System
    open System.Reflection
    open System.Net
    open System.Web
    open System.Runtime.CompilerServices
    open System.Runtime.InteropServices
    open IsaacLogLib.IsaacLogLib
    open System.Configuration
    open System.Threading
    open System.Text

    [<Literal>]
    let VERSION = "1.0.2.0"

    [<assembly: AssemblyTitle("Isaac Current Build Client")>]
    [<assembly: AssemblyDescription("Sends data on your current build to isaacbuild.coq.dk")>]
    [<assembly: AssemblyConfiguration("")>]
    [<assembly: AssemblyCompany("Sebastian Paaske Tørholm")>]
    [<assembly: AssemblyProduct("Isaac Current Build Client")>]
    [<assembly: AssemblyCopyright("Copyright ©  2015")>]
    [<assembly: AssemblyTrademark("")>]

    [<assembly: AssemblyVersion(VERSION)>]
    do ()

    let initialize_token (config : Configuration) (settings : KeyValueConfigurationCollection) =
        let user = settings.Item "Username"
        printf "Please input username: "
        user.Value <- (Console.ReadLine ()).Trim ()
        let token = settings.Item "AuthenticationKey"
        printf "Please input authentication token (if you don't have one; ask @pishmoffle on Twitter): "
        token.Value <- (Console.ReadLine ()).Trim ()
        config.Save ()

    let verify_token conf =
        let user = conf "Username"
        let token = conf "AuthenticationKey"
        let endpoint = conf "APIEndpoint"
        try 
            let resp = (new WebClient()).DownloadString (sprintf "%s/verify/%s/%s" endpoint user token)
            true
        with
            | :? WebException -> false

    let rec loop seed (log : IsaacLog) conf interval =
        let user = conf "Username"
        let token = conf "AuthenticationKey"
        let endpoint = conf "APIEndpoint"

        log.refreshLog ()
        if log.Seed <> seed then
            printf "Seed: %s\n" log.Seed

        if log.HasChanged then
            Console.WriteLine "Game state has changed; sending update to server."
            let req = HttpWebRequest.CreateHttp (sprintf "%s/update/%s/%s" endpoint user token)
            req.Method <- "POST"
            req.ContentType <- "application/x-www-form-urlencoded"

            let data = HttpUtility.ParseQueryString String.Empty
            data.Add ("character", log.Character)
            data.Add ("seed", log.Seed)
            data.Add ("floor", log.CurrentFloor)
            data.Add ("curse", match log.CurrentCurse with Some c -> c | None -> "")
            data.Add ("items", String.concat ", " log.PassiveItems)
            data.Add ("activeitem", match log.ActiveItem with Some i -> i | None -> "")
            data.Add ("guppy", String.concat ", " log.GuppyProgress)
            data.Add ("lotf", String.concat ", " log.LotFProgress)
            data.Add ("client_version", VERSION)

            let ascii = new ASCIIEncoding()
            let dataBytes = ascii.GetBytes (data.ToString ())
            req.ContentLength <- (int64) dataBytes.Length
            
            try 
                let rs = req.GetRequestStream ()
                rs.Write (dataBytes, 0, dataBytes.Length)
                rs.Flush ()
                rs.Close ()
                ignore (req.GetResponse ())
            with 
                | :? WebException as ex ->
                    Console.WriteLine "Error communicating with server. Data not updated."
                    Console.WriteLine (ex.ToString ())
                
        
        Thread.Sleep (1000 * interval)  
        loop log.Seed log conf interval


    [<EntryPoint>]
    let main args =
        let log = IsaacLog()
        let config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None)
        let settings = config.AppSettings.Settings
        let conf s = (settings.Item s).Value

        Console.WriteLine "== Isaac Current Build updater =="
        if conf "AuthenticationKey" = "" then initialize_token config settings

        Console.Write "Verifying authentication token... "

        if not (verify_token conf) then
            Console.WriteLine "FAILED"
            Console.WriteLine "Please go to http://isaacbuild.coq.dk to get an authentication token, then place it in your App.config."
            ignore (Console.ReadLine ())
            exit 0
        else
            Console.WriteLine "OK"

        let interval = max 1 (Int32.Parse (conf "RefreshInterval"))

        loop "" log conf interval

        0

        
