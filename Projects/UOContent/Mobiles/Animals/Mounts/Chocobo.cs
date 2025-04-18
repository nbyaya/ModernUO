using System;
using ModernUO.Serialization;
using Server.Items;

namespace Server.Mobiles
{
    [SerializationGenerator(0, false)]
    public partial class Chocobo : BaseMount
    {
        public override string DefaultName => "a yellow chocobo";
        private const int m_minStr = 100;
        private const int m_maxStr = 300;
        private const double m_aboveMinMax = m_maxStr - m_minStr;
        private const double m_tamingSkillBase = 67.1;
        private const double m_tamingSkillMaxDelta = 30;
        [Constructible]
        public Chocobo() : base(0xDA, 0x3EA4, AIType.AI_Animal)
        {
            Hue = 0x496;

            BaseSoundID = 0x4B0;

            SetStr(m_minStr, m_maxStr);
            SetDex(200, 255);
            SetInt(100, 150);

            SetHits(150, 400);

            SetDamage(10, 25);

            SetDamageType(ResistanceType.Physical, 50);
            SetDamageType(ResistanceType.Fire, 50);

            SetResistance(ResistanceType.Physical, 25, 30);
            SetResistance(ResistanceType.Fire, 50, 75);
            SetResistance(ResistanceType.Cold, 20, 30);
            SetResistance(ResistanceType.Poison, 50, 75);
            SetResistance(ResistanceType.Energy, 20, 30);

            SetSkill(SkillName.MagicResist, 75.1, 80.0);
            SetSkill(SkillName.Tactics, 79.3, 94.0);
            SetSkill(SkillName.Wrestling, 79.3, 94.0);

            Fame = 1500;
            Karma = 1500;

            Tamable = true;
            ControlSlots = RawStr <= 200 ? 1 : 2;
            MinTameSkill = Math.Round(GetTamingSkillBasedOnStrength(), 1);

            PackItem(new BrightlyColoredEggs());
        }

        private double GetTamingSkillBasedOnStrength()
        {
            double aboveMin = RawStr - m_minStr;
            return m_tamingSkillBase + (m_tamingSkillMaxDelta * (aboveMin / m_aboveMinMax));
        }

        public override int StepsMax => 6400;
        public override string CorpseName => "a chocobo corpse";

        public override int Meat => 1;
        public override MeatType MeatType => MeatType.Bird;
		public override int Feathers => 50;
        public override FoodType FavoriteFood => FoodType.FruitsAndVeggies;
		
		private static MonsterAbility[] _abilities = { MonsterAbilities.FireBreath };
        public override MonsterAbility[] GetMonsterAbilities() => _abilities;
    }
}
