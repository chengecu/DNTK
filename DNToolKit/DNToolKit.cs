﻿using Common;
using DNToolKit.Frontend;
using DNToolKit.Listeners;
using DNToolKit.Net;
using DNToolKit.Protocol;
using Newtonsoft.Json;
using Serilog;
using SharpPcap;

namespace DNToolKit;

public class DNToolKit
{
    private const string ConfigName = "./config.json";

    static DNToolKit(){KeyBruteForcer.LoadOldSeeds();}
    public readonly Config Config;
    public readonly Sniffer Sniffer;
    private List<IPacketListener> PacketListeners = new ();

    
    
    public void AddPcapListener(IPcapListener listener)
    {
        Sniffer.PcapListeners.Add(listener);
    }    


    public DNToolKit()
    {
        Config = LoadConfig();
        Sniffer = new Sniffer(this, Config.ClientPrivateRsa);
    }

    public void Start()
    {
        Sniffer.Start(Config.ChooseSniffDevice);
        
    }

    public void Close()
    {
        KeyBruteForcer.StoreOldSeeds();
        Sniffer.Close();
    }


    public void AddGamePacket(Packet p)
    {
        foreach (var packetListener in PacketListeners)
        {
            packetListener.OnPacket(p);
        }
    }

    public void AddPacketListener(IPacketListener listener)
    {
        PacketListeners.Add(listener);
    }
    
    


    public static Config LoadConfig()
    {
        if (!File.Exists(ConfigName))
        {
            File.WriteAllText(ConfigName,JsonConvert.SerializeObject(Config.Default));
            return Config.Default;
        }
        else
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigName));
            if (config is null)
            {
                config = Config.Default;
                Log.Error("Invalid Config File! Using Default...");
            }

            return config;
        }
    }
}