using System;
using System.Collections.Generic;
using System.IO;
using Exiled.API.Features;

namespace ChildrensDayPlugin
{
    public class PlayerDataManager
    {
        private Dictionary<string, PlayerData> playerDataCache = new Dictionary<string, PlayerData>();
        private string dataPath = Path.Combine(Paths.Configs, "PlayerLevelData.json");
        
        public PlayerDataManager()
        {
            LoadAllData();
        }
        
        public PlayerData GetPlayerData(Player player)
        {
            // 检查玩家是否开启了DNT
            if (Plugin.Instance.IsPlayerDNTEnabled(player))
            {
                // 如果开启DNT，返回临时数据，不存储到缓存
                return new PlayerData { UserId = player.UserId };
            }
            
            if (!playerDataCache.ContainsKey(player.UserId))
            {
                playerDataCache[player.UserId] = new PlayerData { UserId = player.UserId };
            }
            return playerDataCache[player.UserId];
        }
        
        public void LoadPlayerData(Player player)
        {
            if (!playerDataCache.ContainsKey(player.UserId))
            {
                playerDataCache[player.UserId] = new PlayerData { UserId = player.UserId };
            }
        }
        
        public void SavePlayerData(Player player)
        {
            // 检查player和UserId是否为null
            if (player?.UserId == null)
            {
                return;
            }
            
            // 如果玩家开启DNT，不保存数据
            if (Plugin.Instance.IsPlayerDNTEnabled(player))
            {
                return;
            }
            
            if (playerDataCache.ContainsKey(player.UserId))
            {
                SaveAllData();
            }
        }
        
        public void AddExperience(Player player, int amount)
        {
            // 如果玩家开启DNT，不添加经验
            if (Plugin.Instance.IsPlayerDNTEnabled(player))
            {
                return;
            }
            
            var data = GetPlayerData(player);
            int oldLevel = data.Level;
            data.AddExperience(amount);
            
            if (data.Level > oldLevel)
            {
                Log.Info($"玩家 {player.Nickname} 升级到等级 {data.Level}，称号: {data.Title}");
            }
        }
        
        private void LoadAllData()
        {
            try
            {
                if (File.Exists(dataPath))
                {
                    string[] lines = File.ReadAllLines(dataPath);
                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            string[] parts = line.Split('|');
                            if (parts.Length == 4)
                            {
                                var data = new PlayerData
                                {
                                    UserId = parts[0],
                                    Level = int.Parse(parts[1]),
                                    Experience = int.Parse(parts[2]),
                                    Title = parts[3]
                                };
                                playerDataCache[data.UserId] = data;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"加载玩家数据失败: {ex.Message}");
            }
        }
        
        public void SaveAllData()
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (var kvp in playerDataCache)
                {
                    var data = kvp.Value;
                    lines.Add($"{data.UserId}|{data.Level}|{data.Experience}|{data.Title}");
                }
                File.WriteAllLines(dataPath, lines);
            }
            catch (Exception ex)
            {
                Log.Error($"保存玩家数据失败: {ex.Message}");
            }
        }
    }
}