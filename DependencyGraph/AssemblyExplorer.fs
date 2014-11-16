namespace DependencyGraph
module AssemblyExplorer=
    open System.Reflection
    open System.IO
    open System.Collections.Generic
    
    let getAssemblyDependencies (assemblyPath)=
        let exploredAssemblies = new HashSet<string>()
        let dir = Path.GetDirectoryName(assemblyPath)
        let rootAssembly = Assembly.LoadFrom(assemblyPath) 

        let getAssemblyPath (assemblyName:AssemblyName)=
            sprintf "%s\\%s.dll" dir assemblyName.Name
    
        let loadAssembly (assemblyName:AssemblyName)=
            let assemblyFilePath = getAssemblyPath assemblyName        
            try
                if File.Exists assemblyFilePath then
                    Some(Assembly.ReflectionOnlyLoadFrom(assemblyFilePath))
                else 
                    Some(Assembly.ReflectionOnlyLoad(assemblyName.FullName))
            with _ -> None

        let formatAssemblyName (am:AssemblyName)=
            sprintf "%s %s" am.Name (am.Version.ToString())

        let rec getDependencies (assembly: Assembly)= seq{            
            let currName = formatAssemblyName (assembly.GetName())
            if exploredAssemblies.Contains currName = false then            
               exploredAssemblies.Add(currName) |> ignore 
               let allReferences = assembly.GetReferencedAssemblies()
               let uniqueReferences =  allReferences|> Seq.distinctBy (fun x -> formatAssemblyName x)          
               for reference in uniqueReferences do 
                   let refFullName = formatAssemblyName reference
                   yield (currName, refFullName)            
                   if exploredAssemblies.Contains refFullName = false then                
                      match loadAssembly reference with
                          |Some refAssembly -> yield! getDependencies refAssembly     
                          |None -> exploredAssemblies.Add(refFullName) |> ignore                            
        }       
        getDependencies rootAssembly |> List.ofSeq  

    let getAssemblyList dependencies=
        dependencies 
        |> List.map (fun (a,b)-> [a;b]) 
        |> List.concat 
        |> List.sort 
        |> Seq.distinct

    let filterAssemblies (blacklist:string[]) dependencies=
        let isOnBlacklist e=
            Array.exists ((=)e) blacklist    
        dependencies |> Seq.filter (fun (x,y)-> (isOnBlacklist x || isOnBlacklist y) = false)