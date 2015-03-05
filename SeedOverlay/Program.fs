namespace IsaacSeedOverlay

module Program =    
    open System
    open System.Windows.Forms
    open System.Drawing
    open System.Configuration
    open IsaacLogLib.IsaacLogLib

    type MainForm() as form =
        inherit Form()

        let log = new IsaacLog()
        let label = new Label()
        let panel = new Panel()
        let timer = new Timer()
        let config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).AppSettings.Settings
        let conf s = (config.Item s).Value

        let onlyShowOnBasementOne = Boolean.Parse(conf "OnlyShowOnBasementOne")

        let showSeed s = label.Text <- if s = "" then "" else sprintf "%s: %s" (conf "SeedText") s

        let updateDisplay this e =
            log.refreshLog()
            if onlyShowOnBasementOne && not (List.exists (fun f -> f = log.CurrentFloor) ["Basement 1"; "Cellar 1"])
            then showSeed ""
            else showSeed log.Seed

        do form.initialize()

        member this.initialize() =
            let w = Int32.Parse (conf "FormWidth")
            let h = Int32.Parse (conf "FormHeight")
            let fontSize = Int32.Parse (conf "FontSize")
            let updateInterval = Int32.Parse (conf "UpdateInterval")

            form.AutoScaleDimensions <- new System.Drawing.SizeF ((float32) w, (float32) h)
            form.ClientSize <- new System.Drawing.Size (w, h)
            form.Text <- "Isaac Seed"
            form.Padding <- new Padding(2)

            form.BackColor <- Color.FromName (conf "BackgroundColor")
            form.Controls.Add label

            label.Font <- new Font(conf "Font", (float32)fontSize)
            label.ForeColor <- Color.FromName (conf "TextColor")
            label.Dock <- DockStyle.Fill

            showSeed "SEED HERE"

            timer.Tick.AddHandler (new EventHandler(updateDisplay))
            timer.Interval <- updateInterval
            timer.Start()