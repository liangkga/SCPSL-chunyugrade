using Exiled.API.Interfaces;

namespace ChildrensDayPlugin
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
    }
}