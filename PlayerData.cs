namespace ChildrensDayPlugin
{
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
}