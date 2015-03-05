namespace IsaacSeedOverlay

module IsaacSeedOverlay =
    open Program
    open System
    open System.Windows.Forms
    open System.Reflection
    open System.Runtime.CompilerServices
    open System.Runtime.InteropServices

    [<assembly: AssemblyTitle("Isaac Seed Overlay")>]
    [<assembly: AssemblyDescription("Overlay showing the current seed")>]
    [<assembly: AssemblyConfiguration("")>]
    [<assembly: AssemblyCompany("Sebastian Paaske Tørholm")>]
    [<assembly: AssemblyProduct("Isaac Seed Overlay")>]
    [<assembly: AssemblyCopyright("Copyright ©  2015")>]
    [<assembly: AssemblyTrademark("")>]

    [<assembly: AssemblyVersion("1.0.0.0")>]

    [<STAThread>]
    do Application.Run(new MainForm())