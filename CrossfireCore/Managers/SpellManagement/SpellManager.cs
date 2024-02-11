using CrossfireCore.ServerConfig;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;

namespace CrossfireCore.Managers.SpellManagement
{
    public class SpellManager : DataListManager<UInt32, Spell, List<Spell>>
    {
        public SpellManager(SocketConnection Connection, MessageBuilder Builder, MessageHandler Handler)
            : base(Connection, Builder, Handler)
        {
            Handler.AddSpell += Handler_AddSpell;
            Handler.UpdateSpell += Handler_UpdateSpell;
            Handler.DeleteSpell += Handler_DeleteSpell;
        }

        protected override bool ClearDataOnConnectionDisconnect => true;
        protected override bool ClearDataOnNewPlayer => true;
        public override DataModificationTypes SupportedModificationTypes => base.SupportedModificationTypes | 
            DataModificationTypes.Added | DataModificationTypes.Updated | DataModificationTypes.Removed;

        private void Handler_AddSpell(object sender, MessageHandler.AddSpellEventArgs e)
        {
            AddDataObject(e.SpellTag, new Spell()
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

        private void Handler_UpdateSpell(object sender, MessageHandler.UpdateSpellEventArgs e)
        {
            UpdateDataObject(e.SpellTag, (data) =>
            {
                switch (e.UpdateType)
                {
                    case NewClient.UpdateSpellTypes.Mana:
                        data.Mana = (short)e.UpdateValue;
                        break;

                    case NewClient.UpdateSpellTypes.Grace:
                        data.Grace = (short)e.UpdateValue;
                        break;

                    case NewClient.UpdateSpellTypes.Damage:
                        data.Damage = (short)e.UpdateValue;
                        break;
                }
            });
        }

        private void Handler_DeleteSpell(object sender, MessageHandler.DeleteSpellEventArgs e)
        {
            RemoveDataObject(e.SpellTag);
        }
    }
}
