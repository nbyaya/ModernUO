using ModernUO.Serialization;
using Server.Items;

namespace Server.Mobiles
{
    [SerializationGenerator(0, false)]
    public partial class Chocobo : BaseMount
    {
        public override string DefaultName => "a yellow chocobo";

        [Constructible]
        public Chocobo() : base(0xDA, 0x3EA4, AIType.AI_Animal)
        {
            Hue = 0x496;

            BaseSoundID = 0x4B0;

            SetStr(100, 300);
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
            ControlSlots = 1;
            MinTameSkill = 97.1;
			
			PackItem(new BrightlyColoredEggs());
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
