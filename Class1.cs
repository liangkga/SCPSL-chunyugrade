using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Exiled.API.Enums;
using System.Threading.Tasks;

namespace ChildrensDayPlugin
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "chunyugrade";
        public override string Author => "椿雨";
        public override Version Version => new Version(1, 0, 0);
        
        public static Plugin Instance;
        public PlayerDataManager DataManager;
        
        public override void OnEnabled()
        {
            Instance = this;
            DataManager = new PlayerDataManager();
            
            Exiled.Events.Handlers.Player.Verified += OnPlayerVerified;
            Exiled.Events.Handlers.Player.Left += OnPlayerLeft;
            Exiled.Events.Handlers.Player.Died += OnPlayerDied;
            Exiled.Events.Handlers.Player.Escaping += OnPlayerEscaping;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            
            base.OnEnabled();
        }
        
        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Player.Verified -= OnPlayerVerified;
            Exiled.Events.Handlers.Player.Left -= OnPlayerLeft;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDied;
            Exiled.Events.Handlers.Player.Escaping -= OnPlayerEscaping;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            
            DataManager?.SaveAllData();
            Instance = null;
            
            base.OnDisabled();
        }
        
        private void OnPlayerVerified(VerifiedEventArgs ev)
        {
            DataManager.LoadPlayerData(ev.Player);
        }
        
        private void OnPlayerLeft(LeftEventArgs ev)
        {
            DataManager.SavePlayerData(ev.Player);
        }
        
        private void OnPlayerDied(DiedEventArgs ev)
        {
            if (ev.Attacker != null && ev.Attacker != ev.Player)
            {
                DataManager.AddExperience(ev.Attacker, 10);
            }
        }
        
        private void OnPlayerEscaping(EscapingEventArgs ev)
        {
            DataManager.AddExperience(ev.Player, 25);
        }
        
        private void OnRoundStarted()
        {
            Task.Run(async () => await UpdatePlayerHintsAsync());
        }
        
        private async Task UpdatePlayerHintsAsync()
        {
            while (true)
            {
                try
                {
                    foreach (Player player in Player.List)
                    {
                        if (player != null && (player.IsAlive || player.Role.Type.ToString() == "Spectator" || player.Role.Type.ToString() == "Overwatch" || player.Role.Type.ToString() == "Filmmaker"))
                        {
                            string roleName = GetRoleChineseName(player.Role.Type.ToString());
                            
                            // 检查玩家是否开启了DNT
                            if (IsPlayerDNTEnabled(player))
                            {
                                // 如果开启DNT，显示红色提示信息
                                string dntText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=16><color=#00FFFF>玩家: {player.Nickname}</color> | <color=#FFFF00>分数: 0</color> | <color=#00FF00>Tickrate: 60</color> | <color=#FF0000>[DNT] 已开启数据不追踪模式</color> | <color=#ADD8E6>角色: {roleName}</color></size>";
                                try
                                {
                                    player.ShowHint(dntText, (ushort)(500 + 200));
                                }
                                catch (Exception hintEx)
                                {
                                    Log.Error($"显示DNT提示时出错 {player.Nickname}: {hintEx.Message}");
                                }
                                continue; // 跳过数据处理和等级显示
                            }
                            
                            var data = DataManager.GetPlayerData(player);
                            string hintText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n<size=16><color=#00FFFF>玩家: {player.Nickname}</color> | <color=#FFFF00>分数: 0</color> | <color=#00FF00>Tickrate: 60</color> | 等级: {data.Level} | <color=#FFFF00>经验: {data.Experience}/{data.RequiredExperience}</color> | <color=#FFA500>{data.Title}</color> | <color=#ADD8E6>角色: {roleName}</color></size>";
                            
                            try
                            {
                                // 使用Hint显示在底部，持续时间稍长于更新间隔
                                player.ShowHint(hintText, (ushort)(500 + 200));
                            }
                            catch (Exception hintEx)
                            {
                                Log.Error($"显示玩家提示时出错 {player.Nickname}: {hintEx.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"更新玩家提示时出错: {ex.Message}");
                }
                await Task.Delay(500);
            }
        }
        
        private string GetRoleChineseName(string roleName)
        {
            switch (roleName)
            {
                case "ClassD": return "D级人员";
                case "Scientist": return "科学家";
                case "FacilityGuard": return "设施警卫";
                case "NtfCaptain": return "九尾狐-指挥官";
                case "NtfSergeant": return "九尾狐-中士";
                case "NtfSpecialist": return "九尾狐-收容专家";
                case "NtfPrivate": return "九尾狐-列兵";
                case "ChaosConscript": return "混沌分裂者-新兵";
                case "ChaosMarauder": return "混沌分裂者-掠夺者";
                case "ChaosRepressor": return "混沌分裂者-镇压者";
                case "ChaosRifleman": return "混沌分裂者-步枪手";
                case "Scp049": return "SCP-049";
                case "Scp0492": return "SCP-049-2";
                case "Scp096": return "SCP-096";
                case "Scp106": return "SCP-106";
                case "Scp173": return "SCP-173";
                case "Scp939": return "SCP-939";
                case "Scp3114": return "SCP-3114";
                case "Tutorial": return "教程";
                case "Spectator": return "观察者";
                case "Overwatch": return "监督者";
                case "Filmmaker": return "摄影师";
                case "RipAndTear": return "撕裂模式";
                case "Scp079": return "SCP-079";
                case "None": return "无";
                default: return roleName;
            }
        }
        
        public bool IsPlayerDNTEnabled(Player player)
        {
            if (player?.Nickname == null) return false;
            
            // 检查昵称中的DNT标记
            string nickname = player.Nickname.ToUpper();
            if (nickname.Contains("[DNT]") || nickname.Contains("DO NOT TRACK"))
                return true;
            
            // 检查玩家的DoNotTrack属性（如果API支持）
            try
            {
                // 尝试通过玩家的DoNotTrack属性检查
                if (player.DoNotTrack)
                    return true;
            }
            catch
            {
                // 如果API不支持DoNotTrack属性，忽略错误
            }
            
            return false;
        }
    }
    
    public class Config : Exiled.API.Interfaces.IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
    }
    
    public class PlayerData
    {
        public string UserId { get; set; }
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public string Title { get; set; } = "新手";
        
        public int RequiredExperience => Level * 100;
        
        public void AddExperience(int amount)
        {
            Experience += amount;
            CheckLevelUp();
        }
        
        private void CheckLevelUp()
        {
            while (Experience >= RequiredExperience)
            {
                Experience -= RequiredExperience;
                Level++;
                UpdateTitle();
            }
        }
        
        private void UpdateTitle()
        {
            if (Level >= 50)
                Title = "传奇大师";
            else if (Level >= 40)
                Title = "精英战士";
            else if (Level >= 30)
                Title = "资深玩家";
            else if (Level >= 20)
                Title = "经验丰富";
            else if (Level >= 15)
                Title = "老练者";
            else if (Level >= 10)
                Title = "熟练者";
            else if (Level >= 5)
                Title = "进阶者";
            else
                Title = "新手";
        }
    }
    
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