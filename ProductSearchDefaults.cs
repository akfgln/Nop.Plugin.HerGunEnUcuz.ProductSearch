using Nop.Core.Configuration;

namespace Nop.Plugin.HerGunEnUcuz.ProductSearch
{
    public class ProductSearchDefaults
    {
        public static string SystemName => "HerGunEnUcuz.ProductSearch";
        public static string ConfigurationRouteName => "Plugin.HerGunEnUcuz.ProductSearch.Configure";

    }
    public class ProductSearchSettings : ISettings
    {
        public string OldActiveSearchProviderSystemName { get; set; }
    }
}
