﻿//Importing reqired modules
using VRC.Core;
using MelonLoader;
using System.IO;
using System;
using System.Text;
using static AvatarLogger.Main;
//Contains code responsible for the actual logging of avatars themsleves
namespace Logging
{
    internal static class Logging
    {
        //Make string to contain friend avatars
        public static string FriendIDs = null;
        //Fetches the frend IDs on the ARES user
        public static System.Collections.IEnumerator FetchFriends()
        {
            while (RoomManager.field_Internal_Static_ApiWorld_0 == null) { yield return null; }
            string[] pals = APIUser.CurrentUser.friendIDs.ToArray();
            foreach (string pal in pals) { FriendIDs += $"{pal},"; }
        }
        //Executes logging of the avatar
        public static void ExecuteLog(dynamic playerHashtable)
        {
            //If avatar loggin is enabled
            if (Config.LogAvatars)
            {
                //If logging of public avatars is disabled
                if (!Config.LogPublicAvatars)
                {
                    //Check to see if the avatar is public and refuse to log if so
                    if (playerHashtable["avatarDict"]["releaseStatus"].ToString() == "public") { return; }
                }
                //If logging of private avatars is disabled
                if (!Config.LogPrivateAvatars)
                {
                    //Check to see if the avatar is private and refuse to log if so
                    if (playerHashtable["avatarDict"]["releaseStatus"].ToString() == "private") { return; }
                }
                //If logging own avatars is disabled
                if (!Config.LogOwnAvatars)
                {
                    //Check if the avatar about to be uploaded belongs to the user and was uploaded from their account
                    if (APIUser.CurrentUser.id == playerHashtable["avatarDict"]["authorId"].ToString())
                    {
                        //If the avatar was uploaded by the user inform them the avatr was not logged and why it was not logged
                        if (Config.ConsoleError) { MelonLogger.Msg($"Your avatar {playerHashtable["avatarDict"]["name"].ToString()} was not logged, you have log own avatars disabled!"); }
                        return;
                    }
                }
                //If logging of friends avatars is disabled
                if (!Config.LogFriendsAvatars)
                {
                    //Check if the avatar about to be logged is uploaded by a friend
                    if (FriendIDs.Contains(playerHashtable["avatarDict"]["authorId"].ToString()))
                    {
                        //If the user is a friend inform the user the log has not occurred and why so
                        if (Config.ConsoleError) { MelonLogger.Msg($"{playerHashtable["avatarDict"]["authorName"].ToString()}'s avatar {playerHashtable["avatarDict"]["name"].ToString()} was not logged, they are a friend!"); }
                        return;
                    }
                }
                //Locate the log file
                string AvatarFile = "GUI\\Log.txt";
                //If the log file does not exist create it and append the credits of the mod
                if (!File.Exists(AvatarFile))
                { File.AppendAllText(AvatarFile, "Mod By LargestBoi & Yui\n"); }
                //Read the entire contents of the log file
                string AvatarFileContents = File.ReadAllText(AvatarFile);
                //If the hash table passed into the method contains a new avatar ID that is not already present within the log file
                if (!AvatarFileContents.Contains(playerHashtable["avatarDict"]["id"].ToString()))
                {
                    //Log the following variables to the log file
                    File.AppendAllLines(AvatarFile, new string[]
                    {
                    //Obtains the cuttent system date/time in unix and logs it as the time the avatar was detected
                    $"Time Detected:{((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString()}",
                    //Continues to extract more data from the hash table and write it to the log file such as:
                    //Avatar ID, Name, Description, Author ID, Author Name and the PC Asset URL
                    $"Avatar ID:{playerHashtable["avatarDict"]["id"]}",
                    $"Avatar Name:{playerHashtable["avatarDict"]["name"]}",
                    $"Avatar Description:{playerHashtable["avatarDict"]["description"]}",
                    $"Author ID:{playerHashtable["avatarDict"]["authorId"]}",
                    $"Author Name:{playerHashtable["avatarDict"]["authorName"]}",
                    });
                    //New optimised Quest/PC asset URL logging 
                    string pcasset = "None";
                    string qasset = "None";
                    foreach (dynamic unitypackage in playerHashtable["avatarDict"]["unityPackages"])
                    {
                        try
                        {
                            switch (unitypackage["platform"].ToString())
                            {
                                //Checks for avi version and logs accordingly for Quest and PC
                                case "standalonewindows":
                                    if (pcasset == "None")
                                    {
                                        pcasset = unitypackage["assetUrl"].ToString();
                                        PC = PC + 1;
                                    }
                                    break;
                                case "android":
                                    if (qasset == "None")
                                    {
                                        qasset = unitypackage["assetUrl"].ToString();
                                        Q = Q + 1;
                                    }
                                    break;
                            }
                        }
                        catch { }
                    }
                    File.AppendAllLines(AvatarFile, new string[]
                    {
                    $"PC Asset URL:{pcasset}",
                    $"Quest Asset URL:{qasset}",
                    $"Image URL:{playerHashtable["avatarDict"]["imageUrl"]}",
                    $"Thumbnail URL:{playerHashtable["avatarDict"]["thumbnailImageUrl"]}",
                    $"Unity Version:{playerHashtable["avatarDict"]["unityPackages"][0]["unityVersion"]}",
                    $"Release Status:{playerHashtable["avatarDict"]["releaseStatus"]}",
                    });
                    //Adjust counter values to whatever the avatrs relese status is
                    string rs = playerHashtable["avatarDict"]["releaseStatus"].ToString();
                    if (rs == "public") { Pub = Pub + 1; };
                    if (rs == "private") { Pri = Pri = 1; };
                    //The last variables extracted are the tags of the avatar, these are added by the avatar uploader or by VRChat administrators/developers,
                    //they are initally stored as an array, if no tags are set the if statemnt will just continue with its else
                    if (playerHashtable["avatarDict"]["tags"].Count > 0)
                    {
                        //Prepares to create a string from the array of tags
                        StringBuilder builder = new StringBuilder();
                        //Adds the text "Tags: " to the string being created as an identifer
                        builder.Append("Tags: ");
                        //For every value in the tags array add it to the string being created
                        foreach (string tag in playerHashtable["avatarDict"]["tags"]) { builder.Append($"{tag},"); }
                        //Write the final created string into the log file containing all extracted and sorted tags
                        File.AppendAllText(AvatarFile, builder.ToString().Remove(builder.ToString().LastIndexOf(",")));
                    }
                    //If there are no tags present the default text "Tags: None" is written into the log file
                    else { File.AppendAllText(AvatarFile, "Tags: None"); }
                    //Inform the user of the successful log
                    if (Config.LogToConsole) { MelonLogger.Msg($"Logged: {playerHashtable["avatarDict"]["name"]}|{playerHashtable["avatarDict"]["releaseStatus"]}!"); }
                    File.AppendAllText(AvatarFile, "\n\n");
                }
            }
        }
    }
}