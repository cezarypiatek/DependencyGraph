open System
open System.Windows.Forms
open DependencyGraph

let wb = new WebBrowser()
let list = new CheckedListBox();


let displaySvg (bytes:byte[])=    
    let svg = System.Text.Encoding.UTF8.GetString(bytes)    
    wb.DocumentText <- svg

let drawGraph dependencies =
    dependencies    
    |> GraphVizUtil.generateGraphSvg
    |> displaySvg 

let getUncheckedAssemblyNames (args:ItemCheckEventArgs)=
    let isUnChecked index=
        if index = args.Index then
            args.NewValue = CheckState.Unchecked
        else
            list.GetItemCheckState(index) = CheckState.Unchecked
    [|for i=0 to list.Items.Count-1 do if isUnChecked i then yield list.Items.[i]:?>string |]

let mutable checkHandler:ItemCheckEventHandler = null 

let registedCheckHandler dependencies=    
    checkHandler <- new ItemCheckEventHandler(fun _ args ->
        let ignoredAssemblies = getUncheckedAssemblyNames args
        dependencies 
        |>  AssemblyExplorer.filterAssemblies  ignoredAssemblies  
        |> drawGraph 
    )
    list.ItemCheck.AddHandler(checkHandler)

let unregisterCheckHandler()=
    list.ItemCheck.RemoveHandler(checkHandler)

let loadToList assemblies=
    list.Items.Clear() |> ignore
    for ass in assemblies do
       list.Items.Add(ass) |> ignore
       list.SetItemChecked(list.Items.Count-1,true)

let presentOnList dependencies=
    unregisterCheckHandler()
    AssemblyExplorer.getAssemblyList dependencies  |> loadToList  
    registedCheckHandler dependencies

let presentAssemblyDependencies path=
    let dependencies = AssemblyExplorer.getAssemblyDependencies path
    drawGraph dependencies
    presentOnList dependencies

let readFileName()=
    let dialog = new OpenFileDialog()
    dialog.Filter <- "*.dll|*.dll|*.exe|*.exe"
    if dialog.ShowDialog() = DialogResult.OK then
       Some(dialog.FileName)
    else None

let loadAssembly()=
    match readFileName() with
    |Some path -> presentAssemblyDependencies path
    |None -> ()
       
let prepareMenu()=
    let menu = new MenuStrip()
    let item = new ToolStripMenuItem("Load assembly")
    menu.Items.Add(item) |> ignore
    item.Click.Add(fun _-> loadAssembly())
    menu

let prepareMainForm()=
    let form = new Form(Width=500, Height=400, Visible=true, Text="Dependency Graph")
    wb.Dock<- DockStyle.Fill 
    wb.Navigate("about:blank")
    form.Controls.Add(wb)    
    list.Dock <- DockStyle.Right    
    list.Width <-200
    form.Controls.Add(list)
    form.Controls.Add(prepareMenu())
    form


let form = prepareMainForm()
[<STAThread()>]
do Application.Run(form)