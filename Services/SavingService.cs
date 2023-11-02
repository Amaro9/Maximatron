using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Maximatron.Controls;
using Maximatron.ViewModels;

namespace Maximatron.Services;

public struct UserObjectData
{
    public string Class;
    public string Title;
    public string TextContent;
    public bool IsChecked;
}

public class SavingService
{
    public static async void OpenFileButton_Clicked(Visual visual)
    {
        var topLevel = TopLevel.GetTopLevel(visual);

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            await using var stream = await files[0].OpenReadAsync();
            using var streamReader = new StreamReader(stream);
            var fileContent = await streamReader.ReadToEndAsync();
            LoadInfoFromFile(fileContent);
        }
    }

    private static void LoadInfoFromFile(string input)
    {

        // Check qu'on a bien des info
        if (input == string.Empty)
        {
            Console.WriteLine($"[ERROR] : nothing in save file !");
            return;
        }
        
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
                    // Check si ce que l'on lit est valid 
                    // Ps : lors d'un load y'aura tjrs une colonne avec rien dedans c'est 
                    //      aussi pour ça qu'on fais ça :)
                    if (info == string.Empty)
                        continue;
                    
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
            }
            
            // Debug
            var finalResult = "[Class] " + data.Class + "| [Title] " + data.Title + "| [Text] " + data.TextContent + "| [IsCheck] " + data.IsChecked;
            Console.WriteLine($"[INFO] : {finalResult}");
            
        }
        
    }


    public static async void SaveFileButton_Clicked(Visual visual)
    {
        // On get la page en entiere (techniquement y'en a toujours une mais on sait jamais)
        var topLevel = TopLevel.GetTopLevel(visual);
        if (topLevel == null)
            throw new Exception($"No topLevel : {visual}");

        
        // On get le UserViewStackPanel (là ou tous les truc que le user creer sont localiser)
        Panel? userViewPanel = topLevel.FindControl<Panel>("UserViewStackPanel");
        if (userViewPanel == null)
            throw new Exception($"No UserViewStackPanel : {topLevel}");
        
       
        
        // Start async operation to open the dialog.
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Text File",
            SuggestedFileName = "New Document",
            FileTypeChoices = new [] { FilePickerFileTypes.TextPlain }
            
        });
        
        if (file is not null)
        {
            await using var stream = await file.OpenWriteAsync();
            using var streamWriter = new StreamWriter(stream);
            
            // Ecriture du texte
            await streamWriter.WriteLineAsync(GetPageUserContent(userViewPanel));
        }
    }

    

    private static string GetPageUserContent(Panel userPanel)
    {
        string saveString = string.Empty;
        foreach (Control control in userPanel.Children)
        {
            if (control is UserObject userObject && control.DataContext is UserObjectModel model)
            {
                string result = string.Empty;
                
                string? objectClass = userObject.Classes[0];
                string? title = model.Title;
                string? text = model.TextContent;
                bool isCheck = model.IsCheck;

                if (objectClass != string.Empty)
                    objectClass = objectClass.Replace("|", "");
                if (title != string.Empty)
                    title = title.Replace("|", "");
                if (text != string.Empty)
                    text = text.Replace("|", "");
                
                
                result += "[Class] " + objectClass + "| [Title] " + title + "| [Text] " + text + "| [IsCheck] " + isCheck;
                
                /*
                 *  Le "<-!->" signifie est le symbole de separation des userObject dans les save du coup si
                 *  on load un truc avec l'utlisateur qui met lui même des "<-!->" ça vas mal load
                 *  on a fais pareil avec les "|" au dessus (juste la faut le faire plus tot parce que
                 *  on en rajoute dans le result, on veut suppr ceux du user pas les notre)
                */
                result = result.Replace("<-!->", "");
                
                // On indique qu'on a fini de mettre des info
                result += "<-!->";
                saveString += result;
            }
            else
            {
                Console.WriteLine($"[WARNING] : {control} is not a UserObject. {control} will be ignored at saving");
            }
        }

        Console.WriteLine(saveString);
        return saveString;
    }
   
}