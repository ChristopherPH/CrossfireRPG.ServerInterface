using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire
{
    public static class NewClient
    {
        public enum NewDrawInfo //NDI
        {
            Black = 0,
            White = 1,
            Navy = 2,
            Red = 3,
            Orange = 4,
            Blue = 5,       // Dodger Blue
            DarkOrange = 6, // DarkOrange2
            Green = 7,      // SeaGreen
            LightGreen = 8, // DarkSeaGreen
            Grey = 9,
            Brown = 10,     //Sienna
            Gold = 11,
            Tan = 12,       //Khaki

            Unique = 0x100, //Print immediately, don't buffer.
            All = 0x200,    //Inform all players of this message
            AllDMs = 0x400, //Inform all logged in DMs. Used in case of errors. Overrides NDI_ALL
            NoTranslate = 0x800,
            Delayed = 0x1000,   //If set, then message is sent only after the player's tick completes.
                                //This allows sending eg quest information after some dialogue even
                                //though quest is processed before.
        }

        public enum MsgTypes
        {
            MOTD = 7,
            Admin = 8,
            Client = 20,
        }

        public enum MsgTypeAdmin
        {
            Rules = 1,
            News = 2,
            Player = 3,     /**< Player coming/going/death */
            DM = 4,         /**< DM related admin actions */
            HiScore = 5,    /**< Hiscore list */
            LoadSave = 6,   /**< load/save operations */
            Login = 7,      /**< login messages/errors */
            Version = 8,    /**< version info */
            Error = 9       /**< Error on command, setup, etc */
        }
    }
}
