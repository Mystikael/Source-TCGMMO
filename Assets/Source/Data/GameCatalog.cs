using System.Collections.Generic;

namespace SourceTCG.Data
{
    public sealed class KiAffinityDef
    {
        public string Id;
        public string Name;
        public string IconFile;
    }

    public sealed class ResourceDef
    {
        public string Id;
        public int Tier;
        public string Category;
        public string Name;
    }

    public static class GameCatalog
    {
        public const int AffinityCount = 12;
        public const int ResourceCount = 27;
        public const int TierMin = 0;
        public const int TierMax = 8;

        public static readonly IReadOnlyList<KiAffinityDef> Affinities = new[]
        {
            new KiAffinityDef { Id = "fyr", Name = "Fyr", IconFile = "Fyr.png" },
            new KiAffinityDef { Id = "watyr", Name = "Watyr", IconFile = "Watyr.png" },
            new KiAffinityDef { Id = "matyr", Name = "Matyr", IconFile = "Matyr.png" },
            new KiAffinityDef { Id = "ayr", Name = "Ayr", IconFile = "Ayr.png" },
            new KiAffinityDef { Id = "aeth", Name = "Aeth", IconFile = "Aeth.png" },
            new KiAffinityDef { Id = "lux", Name = "Lux", IconFile = "Lux.png" },
            new KiAffinityDef { Id = "vyb", Name = "Vyb", IconFile = "Vyb.png" },
            new KiAffinityDef { Id = "grav", Name = "Grav", IconFile = "Grav.png" },
            new KiAffinityDef { Id = "psionic", Name = "Psionic", IconFile = "Psionic.png" },
            new KiAffinityDef { Id = "omega", Name = "Omega", IconFile = "Omega.png" },
            new KiAffinityDef { Id = "veritas", Name = "Veritas", IconFile = "Veritas.png" },
            new KiAffinityDef { Id = "astral", Name = "Astral", IconFile = "Astral.png" },
        };

        static readonly string[] Terra = { "Slate", "Copir", "Tinn", "Iyern", "Myriite", "Omnium", "Rayburst", "Deminyte", "Divinium" };
        static readonly string[] Fauna = { "Critter Pelt", "Primal Leather", "Wilderkin Hide", "Brute Scale", "Embermane Fur", "Chimaera skin", "Stardust Mantle", "Akshinu Essence", "Furah's Coat" };
        static readonly string[] Flora = { "Fibren", "Wyrmwood", "Moa'tah", "Muushis", "Yisilthorne", "Aetherbloom", "Umbrashade", "Netherpetal", "God Root" };

        public static readonly IReadOnlyList<ResourceDef> Resources = BuildResources();

        static List<ResourceDef> BuildResources()
        {
            var list = new List<ResourceDef>(27);
            for (var tier = 0; tier <= 8; tier++)
            {
                list.Add(new ResourceDef { Id = $"terra_t{tier}", Tier = tier, Category = "terra", Name = Terra[tier] });
                list.Add(new ResourceDef { Id = $"fauna_t{tier}", Tier = tier, Category = "fauna", Name = Fauna[tier] });
                list.Add(new ResourceDef { Id = $"flora_t{tier}", Tier = tier, Category = "flora", Name = Flora[tier] });
            }
            return list;
        }

        public static string GetAffinityName(string affinityId)
        {
            if (string.IsNullOrEmpty(affinityId)) return "Ki";
            foreach (var aff in Affinities)
            {
                if (aff.Id == affinityId) return aff.Name;
            }
            return affinityId;
        }
    }
}