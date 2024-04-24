using System;
using System.IO;
using AchiesUtilities.Collections;
using AchiesUtilities.Web.Proxy;
using System.Linq;
using AchiesUtilities.Web.Proxy.Parsing;
using NebulaAuth.Core;
using Newtonsoft.Json;

namespace NebulaAuth.Model;

public static class ProxyStorage
{
    public const string FORMAT = ADDRESS_FORMAT + ":{USER}:{PASS}";
    public const string ADDRESS_FORMAT = "{IP}:{PORT}";

    public static readonly ProxyScheme DefaultScheme = new ProxyScheme(
        ProxyDefaultFormats.UniversalHostFirstColonDelimiter, false, ProxyProtocol.HTTP,
        ProxyPatternProtocol.HTTP | ProxyPatternProtocol.HTTPs,
        ProxyPatternHostFormat.Domain | ProxyPatternHostFormat.IPv4, PatternRequirement.Optional,
        PatternRequirement.Optional);


    public static ObservableDictionary<int, ProxyData> Proxies { get; } = new();


    static ProxyStorage()
    {

        if (File.Exists("proxies.json") == false)
            return;
        try
        {
            var json = File.ReadAllText("proxies.json");
            var proxies = JsonConvert.DeserializeObject<Proxies>(json) ?? throw new NullReferenceException();
            Proxies = proxies.ProxiesData;
            Proxies = new ObservableDictionary<int, ProxyData>(
                Proxies.OrderBy(p => p.Key)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

            if (proxies.DefaultProxy != null)
            {
                MaClient.DefaultProxy = Proxies[proxies.DefaultProxy.Value];
            }
        }
        catch (Exception ex)
        {
            SnackbarController.SendSnackbar("Ошибка при загрузке прокси");
            SnackbarController.SendSnackbar(ex.Message);
        }



    }

    public static void SetProxy(int? id, ProxyData proxyData)
    {
        if (id == null)
        {
            if (Proxies.Any() == false)
            {
                id = 0;
            }
            else
            {
                id = Proxies.Keys.Max() + 1;
            }
        }

        Proxies[id] = proxyData;

        Save();
    }
    public static void RemoveProxy(int id)
    {
        Proxies.Remove(id);
        Save();
    }
    public static bool CompareProxy(ProxyData proxyData1, ProxyData proxyData2)
    {
        return proxyData1.Address == proxyData2.Address && proxyData1.Port == proxyData2.Port;
    }


    public static void Save()
    {
        var p = Create();
        var json = JsonConvert.SerializeObject(p, Formatting.Indented);
        File.WriteAllText("proxies.json", json);
    }

    public static string GetProxyString(ProxyData proxyData)
    {
        return proxyData.AuthEnabled ? proxyData.ToString(FORMAT) : proxyData.ToString(ADDRESS_FORMAT);
    }

    private static Proxies Create()
    {
        int? def = null;
        if (MaClient.DefaultProxy != null)
        {
            var search = Proxies.FirstOrDefault(p => p.Value.Equals(MaClient.DefaultProxy));
            if (search.Value != null!)
            {
                def = search.Key;
            }
        }

        return new Proxies
        {
            ProxiesData = Proxies,
            DefaultProxy = def
        };
    }


}

public class Proxies
{
    public ObservableDictionary<int, ProxyData> ProxiesData { get; set; }
    public int? DefaultProxy { get; set; }
}