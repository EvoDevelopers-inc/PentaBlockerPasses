﻿using System.Drawing;
using System.Globalization;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;

namespace PentaBlockerPasses;

    [MinimumApiVersion(90)]
    public class PentaBlockerPasses : BasePlugin
    {
        public override string ModuleAuthor => "thesamefabius && EvoDevelopers";
        public override string ModuleName => "PentaStrike Blocker Passes";
        public override string ModuleVersion => "v1.0.4";

        private Config _config;

        public override void Load(bool hotReload)
        {
            _config = LoadConfig();
            RegisterListener<Listeners.OnServerPrecacheResources>(OnPrecacheResources);
            RegisterEventHandler<EventRoundStart>(EventRoundStart);

            AddTimer(1500, () => { },TimerFlags.REPEAT);
        }

        private HookResult EventRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            var playersCount = Utilities.GetPlayers()
                .Where(u => u.PlayerPawn.Value != null && u.TeamNum != (int)CsTeam.None &&
                            u.TeamNum != (int)CsTeam.Spectator && u.PlayerPawn.Value.IsValid).ToList();

            if (playersCount.Count >= _config.Players) return HookResult.Continue;

            if (!_config.Maps.TryGetValue(Server.MapName, out var entitiesMap)) return HookResult.Continue;

            foreach (var entity in entitiesMap)
            {
                var color = entity.Color;

                SpawnProp(entity.ModelPath, new[] { color[0], color[1], color[2] },
                    GetVectorFromString(entity.Origin), GetQAngleFromString(entity.Angles), entity.Scale);
            }

            Server.PrintToChatAll(
                " " + ReplaceColorTags(_config.Message.Replace("{MINPLAYERS}", _config.Players.ToString())));

            return HookResult.Continue;
        }

        private Vector GetVectorFromString(string vector) => GetFromString(vector, (x, y, z) => new Vector(x, y, z));

        private QAngle GetQAngleFromString(string angles) => GetFromString(angles, (x, y, z) => new QAngle(x, y, z));

        private static T GetFromString<T>(string values, Func<float, float, float, T> createInstance)
        {
            var split = values.Split(' ');

            if (split.Length >= 3 &&
                float.TryParse(split[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
                float.TryParse(split[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
            {
                return createInstance(x, y, z);
            }

            return default!;
        }

        private void SpawnProp(string modelPath, int[] color, Vector origin, QAngle angles, float? entityScale)
        {
            var prop = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic_override");

            if (prop == null) return;

            prop.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;
            prop.Render = Color.FromArgb(color[0], color[1], color[2]);
            prop.Teleport(origin, angles, new Vector(0, 0, 0));
            prop.DispatchSpawn();
            Server.NextFrame(() => prop.SetModel(modelPath));

            var bodyComponent = prop.CBodyComponent;
            if (bodyComponent is not { SceneNode: not null }) return;

            if (entityScale != null && entityScale != 0.0f)
                bodyComponent.SceneNode.GetSkeletonInstance().Scale = entityScale.Value;
        }

        private Config LoadConfig()
        {
            if (_config != null)
                return _config;
            
            var configPath = Path.Combine(ModuleDirectory, "blocker_passes.json");
            if (!File.Exists(configPath)) return CreateConfig(configPath);

            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath))!;

            return config;
        }
        
        private void OnPrecacheResources(ResourceManifest manifest)
        {
            _config = LoadConfig();

            if (_config.Maps.TryGetValue(Server.MapName, out var entitiesMap))
            {
                foreach (var entity in entitiesMap)
                {
                    manifest.AddResource(entity.ModelPath);
                }
            }
        }

        private Config CreateConfig(string configPath)
        {
            var config = new Config
            {
                Players = 6,
                MessageByAdvertisement = "[{DEFAULT}PENTA{PURPLE}STRIKE{DEFAULT}][{RED}PentaBlockerPasses{DEFAULT}] Открытый код — наша фишка! Большинство плагинов ждут вас на {LIME}GitHub{DEFAULT}!",
                Message =
                    "[{DEFAULT}PENTA{PURPLE}STRIKE{DEFAULT}] Некоторые проходы заблокированы. Для разблокировки требуется {RED}{MINPLAYERS}{DEFAULT} игроков!",
                Maps = new Dictionary<string, List<Entities>>
                {
                    {
                        "de_mirage", new List<Entities>
                        {
                            new()
                            {
                                ModelPath =
                                    "models/props/de_dust/hr_dust/dust_windows/dust_rollupdoor_96x128_surface_lod.vmdl",
                                Color = new[] { 30, 144, 255 },
                                Origin = "-1600.46 -741.124 -172.965",
                                Angles = "0 180 0",
                                Scale = 0.0f
                            },
                            new()
                            {
                                ModelPath = "models/props/de_mirage/small_door_b.vmdl",
                                Color = new[] { 255, 255, 255 },
                                Origin = "588.428 704.941 -136.517",
                                Angles = "0 270.256 0",
                                Scale = 0.0f
                            },
                            new()
                            {
                                ModelPath = "models/props/de_mirage/large_door_c.vmdl",
                                Color = new[] { 255, 255, 255 },
                                Origin = "-1007.87 -359.812 -323.64",
                                Angles = "0 270.106 0",
                                Scale = 0.0f
                            },
                            new()
                            {
                                ModelPath =
                                    "models/props/de_nuke/hr_nuke/chainlink_fence_001/chainlink_fence_001_256_capped.vmdl",
                                Color = new[] { 255, 255, 255 },
                                Origin = "-961.146 -14.2419 -169.489",
                                Angles = "0 269.966 0",
                                Scale = 0.0f
                            },
                            new()
                            {
                                ModelPath =
                                    "models/props/de_nuke/hr_nuke/chainlink_fence_001/chainlink_fence_001_256_capped.vmdl",
                                Color = new[] { 255, 255, 255 },
                                Origin = "-961.146 -14.2419 -43.0083",
                                Angles = "0 269.966 0",
                                Scale = 0.0f
                            }
                        }
                    }
                }
            };

            File.WriteAllText(configPath,
                JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));

            return config;
        }

        private string ReplaceColorTags(string input)
        {
            string[] colorPatterns =
            {
                "{DEFAULT}", "{WHITE}", "{DARKRED}", "{GREEN}", "{LIGHTYELLOW}", "{LIGHTBLUE}", "{OLIVE}", "{LIME}",
                "{RED}", "{LIGHTPURPLE}", "{PURPLE}", "{GREY}", "{YELLOW}", "{GOLD}", "{SILVER}", "{BLUE}",
                "{DARKBLUE}",
                "{BLUEGREY}", "{MAGENTA}", "{LIGHTRED}", "{ORANGE}"
            };

            string[] colorReplacements =
            {
                $"{ChatColors.Default}", $"{ChatColors.White}", $"{ChatColors.Darkred}", $"{ChatColors.Green}",
                $"{ChatColors.LightYellow}", $"{ChatColors.LightBlue}", $"{ChatColors.Olive}", $"{ChatColors.Lime}",
                $"{ChatColors.Red}", $"{ChatColors.LightPurple}", $"{ChatColors.Purple}", $"{ChatColors.Grey}",
                $"{ChatColors.Yellow}", $"{ChatColors.Gold}", $"{ChatColors.Silver}", $"{ChatColors.Blue}",
                $"{ChatColors.DarkBlue}", $"{ChatColors.BlueGrey}", $"{ChatColors.Magenta}", $"{ChatColors.LightRed}",
                $"{ChatColors.Orange}"
            };

            for (var i = 0; i < colorPatterns.Length; i++)
                input = input.Replace(colorPatterns[i], colorReplacements[i]);

            return input;
        }
        
        
        private void AdvertisementToChat()
        {
            Server.PrintToChatAll(ReplaceColorTags(_config.MessageByAdvertisement));
        }
    }

    public class Config
    {
        public int Players { get; init; }
        public required string Message { get; init; }
        public required string MessageByAdvertisement{ get; init; }
        public Dictionary<string, List<Entities>> Maps { get; init; } = new();
    }

    public class Entities
    {
        public required string ModelPath { get; init; }
        public int[] Color { get; init; } = { 255, 255, 255 };
        public required string Origin { get; init; }
        public required string Angles { get; init; }
        public float? Scale { get; init; }
    }
