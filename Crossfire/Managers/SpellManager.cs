using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public class SpellManager : DataManager<Spell>
    {
        public SpellManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
            : base(Connection, Builder, Parser)
        {
            Parser.AddSpell += Parser_AddSpell;
            Parser.UpdateSpell += Parser_UpdateSpell;
            Parser.DeleteSpell += Parser_DeleteSpell;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => true;

        private void Parser_AddSpell(object sender, MessageParserBase.AddSpellEventArgs e)
        {
            AddData(new Spell()
            {
                SpellTag = e.SpellTag,
                Level = e.Level,
                CastingTime = e.CastingTime,
                Mana = e.Mana,
                Grace = e.Grace,
                Damage = e.Damage,
                Skill = e.Skill,
                Path = e.Path,
                Face = e.Face,
                Name = e.Name,
                Description = e.Description,
                Usage = e.Usage,
                Requirements = e.Requirements,
            });
        }

        private void Parser_UpdateSpell(object sender, MessageParserBase.UpdateSpellEventArgs e)
        {
            UpdateData(x => x.SpellTag == e.SpellTag, (data) =>
            {
                switch (e.UpdateType)
                {
                    case CrossfireCore.NewClient.UpdateSpellTypes.Mana:
                        data.Mana = (short)e.UpdateValue;
                        return true;

                    case CrossfireCore.NewClient.UpdateSpellTypes.Grace:
                        data.Grace = (short)e.UpdateValue;
                        return true;

                    case CrossfireCore.NewClient.UpdateSpellTypes.Damage:
                        data.Damage = (short)e.UpdateValue;
                        return true;

                    default:
                        return false;
                }
            }, e.UpdateType);
        }

        private void Parser_DeleteSpell(object sender, MessageParserBase.DeleteSpellEventArgs e)
        {
            RemoveData(x => x.SpellTag == e.SpellTag);
        }
    }

    public class Spell
    {
        public UInt32 SpellTag { get; set; }
        public Int16 Level { get; set; }
        public Int16 CastingTime { get; set; }
        public Int16 Mana { get; set; }
        public Int16 Grace { get; set; }
        public Int16 Damage { get; set; }
        public byte Skill { get; set; }
        public UInt32 Path { get; set; }
        public Int32 Face { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte Usage { get; set; }
        public string Requirements { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
