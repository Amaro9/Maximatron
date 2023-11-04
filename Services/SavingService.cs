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
  
    public static async void Load(Visual visual)
    {
        
        // On get la page en entiere (techniquement y'en a toujours une mais on sait jamais)
        var topLevel = TopLevel.GetTopLevel(visual);
        if (topLevel == null)
            throw new Exception($"No topLevel : {visual}");

        
        // On get le UserViewStackPanel (là ou tous les truc que le user creer sont localiser)
        Panel? userViewPanel = topLevel.FindControl<Panel>("UserViewStackPanel");
        if (userViewPanel == null)
            throw new Exception($"No UserViewStackPanel : {topLevel}");

        
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            // On recupe le fichier et on store son content dans une string
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            var fileContent = await streamReader.ReadToEndAsync();
            
            // On converti la string en info que l'on vas pouvoir utiliser
            List<UserObjectData> save = LoadInfoFromFile(fileContent);
            CreateUserObjectFromSave(userViewPanel, save);
        }
    }

    public static void StartLoad(Visual visual, string path)
    {
        // On get la page en entiere (techniquement y'en a toujours une mais on sait jamais)
        var topLevel = TopLevel.GetTopLevel(visual);
        if (topLevel == null)
            throw new Exception($"No topLevel : {visual}");

        
        // On get le UserViewStackPanel (là ou tous les truc que le user creer sont localiser)
        Panel? userViewPanel = topLevel.FindControl<Panel>("UserViewStackPanel");
        if (userViewPanel == null)
            throw new Exception($"No UserViewStackPanel : {topLevel}");

        
        try
        {
            // Juste get ce qu'il y as de le fichier
            string fileContent = File.ReadAllText(path);
            // On converti la string en info que l'on vas pouvoir utiliser
            List<UserObjectData> save = LoadInfoFromFile(fileContent);
            CreateUserObjectFromSave(userViewPanel, save);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] : " + ex.Message);
            return;
        }
    }

    private static List<UserObjectData> LoadInfoFromFile(string input)
    {

        // Check qu'on a bien des info
        if (input == string.Empty)
        {
            Console.WriteLine($"[ERROR] : nothing in save file !");
            return new List<UserObjectData>();
        }

        List<UserObjectData> dataResults = new List<UserObjectData>();
        
        // On seppart chaque ligne 
        string[] infoPart = input.Split("<-!->");
        foreach (var row in infoPart)
        {
            // Creation des data que l'on vas reatribuer plus tard
            UserObjectData data = new UserObjectData();
            
            // On séppart chaque part partie de ça colonne
            string[] columns = row.Split("|");
            
            foreach (string column in columns)
            {
                
                // On enleve les espaces
                string result = column.Trim();
                
                // On check pour savoir a quelle partie de l'info on est
                
                if (result.Contains("[Class]"))
                {
                    var info = result.Replace("[Class]", "");
                    info = info.Trim();
                    // Check si ce que l'on lit est valid 
                    // Ps : lors d'un load y'aura tjrs une colonne avec rien dedans c'est 
                    //      aussi pour ça qu'on fais ça :)
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
                
                if (result.Contains("[IsCheck]")) // A remplace avec IsChecked
                {
                    var info = result.Replace("[IsCheck]", "");
                    // on converti la string en bool.
                    data.IsChecked = info.Contains("True");
                }
                                
                if (result.Contains("[IsChild]"))
                {
                    var info = result.Replace("[IsChild]", "");
                    // on converti la string en bool.
                    data.IsChild = info.Contains("True");
                }
            }
            
            // Debug
            var finalResult = "[Class] " + data.Class + "| [Title] " + data.Title + "| [Text] " + data.TextContent + "| [IsCheck] " + data.IsChecked;
            Console.WriteLine($"[INFO] : {finalResult}");
            
            dataResults.Add(data);
        }

        return dataResults;
    }

    private static void CreateUserObjectFromSave(Panel? panel, List<UserObjectData>? save)
    {
        // On commence par check que les infos que l'on reçoit sont bien valide
        if (panel == null || save == null || save.Count == 0)
        {
            return;
        }
        
        // au cas z'ou on clear les enfant du panel, on vas les recree avec la save
        panel.Children.Clear();

        // C'est une var qui permet de savoir qu'elle est le derniere liste 
        // Pour savoir ou faudra add les control si ce sont des child.
        Panel lastList = panel;

        // On loop dans les control des la save
        foreach (UserObjectData info in save)
        {
            UserObject? userObject;
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
                        // c'est deja une sous liste, il faut pas lui donner
                        // la possibiliter de faire des sous liste dans des sous listes
                        userObject = PageView.CreateList(false);
                    }
                    break;
                case "BasicCheckBox":
                    userObject = PageView.CreateCheckBox();
                    break;
                
                default: // quand on load une save y'a toujours un element vide a la fin du coup il sera la dans le switch
                    return;
            }
            
            // On set le model
            model = (UserObjectModel?)userObject.DataContext;


            if (userObject == null)
                throw new Exception($"USER OBJECT NULL WHEN LOADING"); 
            
            if (model == null)
                throw new Exception($"No UserObjectModel in : {userObject}"); 
            
            // On initialise le userObject
            model.Title = info.Title;
            model.TextContent = info.TextContent;
            model.IsCheck = info.IsChecked;

            if (info.IsChild)
            {
                lastList.Children.Add(userObject);
            }
            else
            {
                // On ajout le userObject a la vue
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
        // Console.WriteLine(savePath);
        //
        // Console.WriteLine(filePath);
        
        // Si on fais pas de quick save, alors on affiche un dialoque pour savoir ou save.
        if (!quickSave)
        {
            // On creer la fenetre de dialogue
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Text File",
                SuggestedFileName = "New Document",
                FileTypeChoices = new [] { FilePickerFileTypes.TextPlain }
                
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
                return await Save(visual, false, string.Empty);
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
            }
        }

        
        filePath = filePath.Replace("file:///", "");
        // Debug.
        Console.WriteLine($"[INFO] : {filePath}");
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