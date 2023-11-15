using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Maximatron.Controls;
using Maximatron.ViewModels;
// ReSharper disable All

namespace Maximatron.Services;

public struct UserObjectData
{
    public string Class;
    public string Title;
    public string TextContent;
    public bool IsChecked;
    public bool IsChild;
}

public class SavingService
{
    public static FilePickerFileType MaximatronFiles { get; } = new("Maximatron document")
    {
        Patterns = new[] { "*.maximatron"},
    };
    public static async Task<string> Load(Visual visual, bool quickLoad=false, string path="")
    {
        if (quickLoad && (path == string.Empty || path == ""))
        {
            Console.WriteLine("[ERROR]: trying to quickLoad without a path");
            return string.Empty;
        }
        
        // Au cas ou si jamais ça a save un truc avec le file:/// devant (normalement ça arrive jamais)
        string filePath = path.Replace("file:///", "");
        
        // On get la page en entiere (techniquement y'en a toujours une mais on sait jamais)
        var topLevel = (PageView)TopLevel.GetTopLevel(visual)!;
        if (topLevel == null)
            throw new Exception($"No topLevel : {visual}");

        // We are loading a new file, so we need to restart the pageView init
        topLevel.init = false;

        
        // On get le UserViewStackPanel (là ou tous les truc que le user creer sont localiser)
        Panel? userViewPanel = topLevel.FindControl<Panel>("UserViewStackPanel");
        if (userViewPanel == null)
            throw new Exception($"No UserViewStackPanel : {topLevel}");

        if (quickLoad && path != string.Empty)
        {
            try
            {
                // Juste get ce qu'il y as de le fichier
                string fileContent = File.ReadAllText(path);
                // On converti la string en info que l'on vas pouvoir utiliser
                List<UserObjectData> save = LoadInfoFromFile(fileContent);
                CreateUserObjectFromSave(userViewPanel, save);
                
                topLevel.FinishLoad();
                
                return path;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] : " + ex.Message);
                return path;
            }
        }
        
        
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false,
            FileTypeFilter = new [] { MaximatronFiles, FilePickerFileTypes.TextPlain, } 
            
        });

        if (files.Count >= 1)
        {
            // Get the file then store it in a string
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            var fileContent = await streamReader.ReadToEndAsync();
            
            // We convert the string into info that we can use later
            List<UserObjectData> save = LoadInfoFromFile(fileContent);
            CreateUserObjectFromSave(userViewPanel, save);

            filePath = files[0].Path.ToString();
            filePath = filePath.Replace("file:///", "");
            
            // Debug.
            Console.WriteLine($"[INFO] file load at : { filePath}");
            topLevel.FinishLoad();

            return filePath;
        }
        
        return path;
    }
    
    public static async Task<string> LoadFolder(Visual visual)
    {
        // On get la page en entiere (techniquement y'en a toujours une mais on sait jamais)
        var topLevel = TopLevel.GetTopLevel(visual);
        if (topLevel == null)
            throw new Exception($"No topLevel : {visual}");
        
        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Open Folder",
            AllowMultiple = false
        });

        if (folder.Count >= 1)
        {
            Console.WriteLine(folder[0].Path);
            return folder[0].Path.ToString();
        }
        
        return string.Empty;
    }
    

    private static List<UserObjectData> LoadInfoFromFile(string input, bool debug = false)
    {

        // Check if there is info in input
        if (input == string.Empty)
        {
            Console.WriteLine($"[WARNING] : nothing in save file !");
            return new List<UserObjectData>();
        }

        List<UserObjectData> dataResults = new List<UserObjectData>();
        
        // We split each line (each one represant a control)
        string[] infoPart = input.Split("<-!->");
        foreach (var row in infoPart)
        {
            // Creating new userObject data, we will init them later
            UserObjectData data = new UserObjectData();
            
            // we split each colunm (each one represant a property of a control)
            string[] columns = row.Split("|");
            
            foreach (string column in columns)
            {
                
                // We get rid of the blank
                string result = column.Trim();
                
                // Check of the current culunm
                if (result.Contains("[Class]"))
                {
                    var info = result.Replace("[Class]", "");
                    info = info.Trim();
                    // Check if what we're reading is right
                    // Ps : When loading, there is a thing with nothing in it
                    //      this is a flag for the end of the file
                    if (info == string.Empty)
                    {
                        break;
                    }
                    
                    data.Class = info.Trim();
                }
                
                if (result.Contains("[Title]"))
                {
                    var info = result.Replace("[Title]", "");
                    data.Title = info.Trim();
                }
                
                if (result.Contains("[Text]"))
                {
                    var info = result.Replace("[Text]", "");
                    data.TextContent = info.Trim();
                }
                
                if (result.Contains("[IsCheck]"))
                {
                    var info = result.Replace("[IsCheck]", "");
                    // on converti la string en bool.
                    data.IsChecked = info.Contains("True");
                }
                                
                if (result.Contains("[IsChild]"))
                {
                    var info = result.Replace("[IsChild]", "");
                    // Converte bool in string (true -> checked | false -> unchecked)
                    data.IsChild = info.Contains("True");
                }
                
            }

            // Debug
            if (debug)
            {
                var finalResult = "[Class] " + data.Class + "| [Title] " + data.Title + "| [Text] " + data.TextContent + "| [IsCheck] " + data.IsChecked;
                Console.WriteLine($"[INFO] : {finalResult}");
            }
            
            // Adding the result to the data
            dataResults.Add(data);
        }

        return dataResults;
    }

    private static void CreateUserObjectFromSave(Panel? panel, List<UserObjectData>? save)
    {
        if (panel == null)
            return;
        
        // we clear the userView even if there is nothing in save file 
        // (if there is nothing, this is a page with nothing in it)
        panel.Children.Clear();
        
        // we start by cheking if we have some infos
        if (save == null || save.Count == 0)
        {
            return;
        }
        

        // This var is to keep track of the last valid list
        // This will be for when adding child in list 
        // The default value is the userView panel in case there are some issues
        Panel lastList = panel;

        // On loop dans les control des la save
        foreach (UserObjectData info in save)
        {
            UserObject? userObject = null;
            UserObjectModel? model;

            switch (info.Class)
            {
                case "BasicField":
                    userObject = PageView.CreateTextField();
                    break;
                case "BasicList":
                    if (!info.IsChild)
                    {
                        userObject = PageView.CreateList();
                        lastList = (Panel)userObject.Content!;
                    }
                    else
                    {
                        // if this list is already a child of a list, then we are disalowing the ability
                        // of making a list inside this one (otherwise, we can have list in list in list etc...)
                        userObject = PageView.CreateList(false);
                    }
                    break;
                case "BasicCheckBox":
                    userObject = PageView.CreateCheckBox();
                    break;
                
                default: // The default case is trigger when we reach the end of the save data
                    return;
            }
            
            // We set the view model
            model = (UserObjectModel?)userObject.DataContext;


            if (userObject == null)
                throw new Exception($"USER OBJECT NULL WHEN LOADING"); 
            
            if (model == null)
                throw new Exception($"No UserObjectModel in : {userObject}"); 
            
            // we init the user object
            model.Title = info.Title;
            model.TextContent = info.TextContent;
            model.IsCheck = info.IsChecked;

            if (info.IsChild)
            {
                // this is a child, so we add it to the lasted valid parent
                lastList.Children.Add(userObject);
            }
            else
            {
                // we add the panel the the main view
                panel.Children.Add(userObject);
            }
        }            
    }

    public static async Task<string> Save(Visual visual, bool quickSave, string savePath)
    {
        // On get la page en entiere (techniquement y'en a toujours une mais on sait jamais)
        var topLevel = TopLevel.GetTopLevel(visual);
        if (topLevel == null)
            throw new Exception($"No topLevel : {visual}");

        
        // On get le UserViewStackPanel (là ou tous les truc que le user creer sont localiser)
        Panel? userViewPanel = topLevel.FindControl<Panel>("UserViewStackPanel");
        if (userViewPanel == null)
            throw new Exception($"No UserViewStackPanel : {topLevel}");
        
        string filePath = savePath;
        
        // Si on fais pas de quick save, alors on affiche un dialoque pour savoir ou save.
        if (!quickSave)
        {
            // On creer la fenetre de dialogue

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Text File",
                SuggestedFileName = "New Document",
                FileTypeChoices = new [] { MaximatronFiles, FilePickerFileTypes.TextPlain,  }
                
            });
            
            if (file != null)
            {
                // On store là ou le user save son fichier
                filePath = file.Path.ToString();
                
                
                await using var stream = await file.OpenWriteAsync();
                using var streamWriter = new StreamWriter(stream);
                
                // Ecriture du texte
                await streamWriter.WriteLineAsync(GetPageUserContent(userViewPanel));
            }
        }
        else
        {
            // Si jamais le path st vide on fais juste une save classique (avec le dialogue)
            if (filePath == string.Empty)
            {
                string result = await Save(visual, false, string.Empty);
                return result;
            }
            
            string saveString = GetPageUserContent(userViewPanel);
            try
            {
                // Ecriture du texte
                File.WriteAllText(filePath, saveString, Encoding.UTF8);

                // Debug
                Console.WriteLine("File saved: " + filePath);
            }
            catch (IOException ex) 
            {
                // Si jamais ça foire, genre le path est pas valid on arrette et on fais une save classique
                Console.WriteLine("[ERROR] : " + ex.Message);
                return string.Empty;
            }
        }

        // On enleve un truc au debut
        filePath = filePath.Replace("file:///", "");
        
        // Debug.
        Console.WriteLine($"[INFO] file save at : {filePath}");
        return filePath;
    }

    

    private static string GetPageUserContent(Panel? userPanel)
    {
        // On check que l'on as bien un panel
        if (userPanel == null)
        {
            Console.WriteLine("[ERROR] : no panel given in GetPageUserContent");
            return string.Empty;
        }
        
        // On creer la saveString, c'est ce que l'on vas return plus tard.
        string saveString = string.Empty;
        
        // On boucle dans tous les control de la page
        foreach (Control control in userPanel.Children)
        {
            // On verifi que le control actuel est bien un UserObject
            // Sinon on vas essayer de save des trucs qui existe pas
            if (control is UserObject userObject && control.DataContext is UserObjectModel model)
            {
                // Les infos du control actuel
                // Que l'on vas rajouter apres dans la saveString
                string result = string.Empty;
                
                // On recup les infos du control
                var data = GetData(model, userObject);
                
                // On cree la ligne avec toutes les infos du control
                result += "[Class] " + data.Class + "| [Title] " + data.Title + "| [Text] " + data.TextContent + "| [IsCheck] " + data.IsChecked + "| [IsChild] " + data.IsChild;
                
                // On delete les "<-!->". Si il reste, il fouteron la merde au load
                result = result.Replace("<-!->", "");
                
                // On rajoute un <-!-> qui indique la fin d'une ligne 
                // PS : le \n c'est uniquement pour que ça fasse joli dans le .txt
                result += "<-!->\n";
                
                //  si le control est une liste on vas ajouter les infos des enfant
                if (data.Class == "BasicList")
                {
                    StackPanel? panel = (StackPanel?)userObject.Content;
                    if (panel != null)
                    {
                        // Il suffit juste de re call la meme fonct mais avec le stack panel du parent
                        // a la place du truc de base
                        result += GetPageUserContent(panel);
                    }
                }
                
                // On as fini de recup les infos, on peut les ajouter a la saveString final
                saveString += result;
            }
            else
            {
                // y'a un truc qui n'est pas un UserObject, on l'ignore.
                Console.WriteLine($"[WARNING] : {control} is not a UserObject. {control} will be ignored at saving");
            }
        }

        // Debug.
        //Console.WriteLine($"[INFO] : Saving successful : {saveString} ");
        
        // On return les info qu'on vas save
        return saveString;
    }

    public static UserObjectData GetData(UserObjectModel model, UserObject userObject)
    {
        // Recuperation des infos
        string objectClass = userObject.Classes[0];
        string? title = model.Title;
        string? text = model.TextContent;
        bool isCheck = model.IsCheck;
        bool isChild = userObject.Parent!.Name == "ListPanel";
 
        
        // On check la class et on degage les "|" qui pourrais interferer dans la save
        if (objectClass != string.Empty)
            objectClass = objectClass.Replace("|", "");
        if (title != string.Empty)
            title = title.Replace("|", "");
        if (text != string.Empty)
            text = text.Replace("|", "");

        // On return tout ce que l'on as trouvé
        return new UserObjectData()
        {
            Class = objectClass,
            Title = title,
            TextContent = text,
            IsChecked = isCheck,
            IsChild = isChild,
        };
    }
    
    
    
    public static void SaveStringToFile(string filePath, string content)
    {
        try
        {
            File.WriteAllText(filePath, content);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] : " + ex.Message);
        }
    }

    public static string ReadStringFromFile(string filePath)
    {
        try
        {
            // Juste return ce qu'il y as de le fichier
            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] : " + ex.Message);
            return String.Empty;
        }
    
    }



     
   
}