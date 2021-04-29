using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;

namespace linerider
{
    public class DiscordActivityController : GameService
    {
        public Discord.Discord discord;
        public long lastUpdateTime = 0;

        public DiscordActivityController()
        {
            TryCreateNewDiscord();
        }

        public bool TryCreateNewDiscord()
        {
            
            try
            {
                discord = new Discord.Discord(802941234988580914, (UInt64)Discord.CreateFlags.NoRequireDiscord);
            }
            catch
            {
                discord = null;
                //Discord isn't running, we shouldn't crash the game
            }
            return discord != null;
        }

        public void UpdateStatus()
        {
            if (discord != null)
            {
                try
                {
                    discord?.RunCallbacks();
                }
                catch (Discord.ResultException resultException)
                {
                    Console.WriteLine("Discord threw an exception: " + resultException.Result); 
                    discord = null; //Discord probably closed, null it so a new one can be created
                }
            }
        }

        private bool CheckRateLimit()
        {
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastUpdateTime > 10)
            {
                lastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                return true;
            }
            return false;
        }

        public void UpdateDiscordActivityInfo()
        {
            if (!CheckRateLimit()) { return; }

            if (discord == null)
            {
                //Still throws exceptions but doesn't crash the game :p
                if (TryCreateNewDiscord() == false) { return; }
            }

            String toolName = (linerider.Tools.CurrentTools.SelectedTool.ToString().Substring(16)); toolName = toolName.Substring(0, toolName.Length - 4).ToLower();

            String versionText = "LRA:CE version " + linerider.Program.Version;

            String largeKey = Settings.largeImageKey;
            String largeText = versionText + " ==================== Source code: https://github.com/RatherBeLunar/LRA-Community-Edition";
            String smallKey = toolName;
            String smallText = "Currently using the " + toolName + " tool";

            String[] settingStrings = discordSettingToString(new String[] { Settings.discordActivity1, Settings.discordActivity2, Settings.discordActivity3, Settings.discordActivity4 });

            String detailsText = settingStrings[0];
            if (settingStrings[1].Length > 0) { detailsText = detailsText + " | " + settingStrings[1]; }
            String stateText = settingStrings[2];
            if (settingStrings[3].Length > 0) { stateText = stateText + " | " + settingStrings[3]; }

            var activityManager = discord.GetActivityManager();
            var lobbyManager = discord.GetLobbyManager();

            var activity = new Discord.Activity
            {
                Type = 0,
                Details = detailsText,
                State = stateText,
                Timestamps =
                {
                    Start = Program.startTime,
                    End = 0,
                },
                Assets =
            {
                LargeImage = largeKey,
                LargeText = largeText,
                SmallImage = smallKey,
                SmallText = smallText,
            },
                Instance = false
            };

            activityManager.UpdateActivity(activity, result =>
            {
                if (result != Result.Ok)
                {
                    Console.WriteLine("Update Activity {0}", result);
                }
            });
        }

        public static String[] discordSettingToString(String[] settings)
        {
            String[] ret = new String[4];

            String toolName = (linerider.Tools.CurrentTools.SelectedTool.ToString().Substring(16)); toolName = toolName.Substring(0, toolName.Length - 4).ToLower();
            String lineText = "Amount of Lines: " + game.Track.LineCount;
            String unsavedChangesText = "Unsaved changes: " + game.Track.TrackChanges;
            String toolText = "Currently using the " + toolName + " tool";
            String trackText = "Track name: \"" + game.Track.Name + "\"";
            String versionText = "LRA:CE version " + linerider.Program.Version;

            for (int i=0; i<4; i++)
            {
                switch (settings[i])
                {
                    case "none":
                        ret[i] = "";
                        break;
                    case "lineText":
                        ret[i] = lineText;
                        break;
                    case "unsavedChangesText":
                        ret[i] = unsavedChangesText;
                        break;
                    case "toolText":
                        ret[i] = toolText;
                        break;
                    case "trackText":
                        ret[i] = trackText;
                        break;
                    case "versionText":
                        ret[i] = versionText;
                        break;
                    default:
                        ret[i] = "";
                        break;
                }
            }

            return ret;
        }
    }
}
