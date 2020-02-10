using System;
using System.Collections.Generic;
using System.Linq;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;

namespace Curiosity.Systems.Library.Models
{
    public class CuriosityCharacter
    {
        public long CharacterId { get; set; }
        public long UserId { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }
        public long Cash { get; set; }
        public bool MarkedAsRegistered { get; set; }
        public long LocationId { get; set; }
        public int Gender { get; set; } = 0; // Default Gender is Male
        public Position LastPosition { get; set; }
        public CharacterHeritage Style { get; set; } = new CharacterHeritage();
        public CharacterLifeStyle LifeStyle { get; set; } = new CharacterLifeStyle();
        public CharacterStats Stats { get; set; } = new CharacterStats();
        public CharacterAppearance Appearance { get; set; } = new CharacterAppearance();
    }
}