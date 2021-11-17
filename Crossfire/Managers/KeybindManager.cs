using Common;
using Common.Utility;
using CrossfireCore.Parser;
using CrossfireCore.ServerInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Crossfire.Keybinding
{
    public class KeybindManager : Managers.Manager
    {
        public KeybindManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
            : base(Connection, Builder, Parser)
        {
            foreach (KeybindLocations location in Enum.GetValues(typeof(KeybindLocations)))
            {
                BindingStore[location] = new Bindings();
            }

            Parser.Player += Parser_Player;

            LoadBindings(KeybindLocations.Global, true);
        }

        private string _PlayerName = string.Empty;
        static Logger _Logger = new Logger(nameof(KeybindManager));

        public event EventHandler<KeybindsChangedEventArgs> KeybindsChanged;

        public enum KeybindLocations
        {
            Character,
            Global
        }

        public Dictionary<KeybindLocations, Bindings> BindingStore { get; } = new Dictionary<KeybindLocations, Bindings>();

        public class Bindings
        {
            public List<ModifierBinding> ModifierBindings { get; set; } = new List<ModifierBinding>();
            public List<KeyBind> KeyBindings { get; set; } = new List<KeyBind>();
        }

        public ModifierBinding GetModifierBinding(Keys KeyData, ModifierBinding.KeypressTypes KeypressType)
        {
            foreach (var entry in BindingStore.OrderBy(x => x.Key))
            {
                var binding = ModifierBinding.GetModifierBinding(entry.Value.ModifierBindings, KeyData, KeypressType);
                if (binding != null)
                    return binding;
            }

            return null;
        }

        public KeyBind GetKeyBinding(Keys KeyData)
        {
            foreach (var entry in BindingStore.OrderBy(x => x.Key))
            {
                var binding = KeyBind.GetKeybind(entry.Value.KeyBindings, KeyData);
                if (binding != null)
                    return binding;
            }

            return null;
        }

        private void Parser_Player(object sender, MessageParser.PlayerEventArgs e)
        {
            if (e.tag == 0)
            {
                _PlayerName = string.Empty;
                ClearBindings(KeybindLocations.Character);
            }
            else
            {
                _PlayerName = e.PlayerName;
                LoadBindings(KeybindLocations.Character, false);
            }
        }

        string GetLocationFilename(KeybindLocations location)
        {
            switch (location)
            {
                case KeybindLocations.Global:
                    return string.Format("Global.keys");

                case KeybindLocations.Character:
                    if (_PlayerName == string.Empty)
                        return string.Empty;

                    return string.Format("{0}.{1}.keys", Connection?.Host ?? "unknown", _PlayerName);

                default:
                    return string.Empty;
            }
        }

        bool WriteDefaultBindings(string filename)
        {
            var defaultBindings = new Bindings()
            {
                KeyBindings = DefaultKeyBindings,
                ModifierBindings = DefaultModifierBindings,
            };

            try
            {
                XmlHelper.ObjectToXml(defaultBindings, filename);
                return true;
            }
            catch
            {
                return false;
            }
        }

        bool LoadBindings(KeybindLocations location, bool defaultOnError)
        {
            var filename = GetLocationFilename(location);
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            if (defaultOnError && !System.IO.File.Exists(filename))
            {
                if (!WriteDefaultBindings(filename))
                    return false;
            }

            try
            {
                BindingStore[location] = XmlHelper.XmlToObject<Bindings>(filename);
            }
            catch
            {
                if (defaultOnError)
                {
                    if (!WriteDefaultBindings(filename))
                        return false;

                    try
                    {
                        BindingStore[location] = XmlHelper.XmlToObject<Bindings>(filename);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            KeybindsChanged?.Invoke(this, new KeybindsChangedEventArgs());
            return true;
        }

        public bool SaveBindings(KeybindLocations location)
        {
            var filename = GetLocationFilename(location);
            if (string.IsNullOrWhiteSpace(filename))
                return false;

            try
            {
                XmlHelper.ObjectToXml(BindingStore[location], filename);
                return true;
            }
            catch
            {
                return false;
            }
        }

        void ClearBindings(KeybindLocations location)
        {
            BindingStore[location].KeyBindings.Clear();
            BindingStore[location].ModifierBindings.Clear();
            KeybindsChanged?.Invoke(this, new KeybindsChangedEventArgs());
        }

        public static List<ModifierBinding> DefaultModifierBindings { get; } = new List<ModifierBinding>()
        {
            { new ModifierBinding(ModifierBinding.ModifierKeys.Run, ModifierBinding.KeypressTypes.Up, "run_stop") },
            { new ModifierBinding(ModifierBinding.ModifierKeys.Fire, ModifierBinding.KeypressTypes.Up, "fire_stop") },
        };

        public static List<KeyBind> DefaultKeyBindings { get; } = new List<KeyBind>()
        {
            { new KeyBind(KeyBind.GameKeys.A, KeyBind.Modifiers.Any, "apply") },

            { new KeyBind(KeyBind.GameKeys.Up, KeyBind.Modifiers.None ,"north") },
            { new KeyBind(KeyBind.GameKeys.Down, KeyBind.Modifiers.None, "south") },
            { new KeyBind(KeyBind.GameKeys.Left, KeyBind.Modifiers.None, "west") },
            { new KeyBind(KeyBind.GameKeys.Right, KeyBind.Modifiers.None, "east") },

            { new KeyBind(KeyBind.GameKeys.Up, KeyBind.Modifiers.Fire ,"north f") },
            { new KeyBind(KeyBind.GameKeys.Down, KeyBind.Modifiers.Fire, "south f") },
            { new KeyBind(KeyBind.GameKeys.Left, KeyBind.Modifiers.Fire, "west f") },
            { new KeyBind(KeyBind.GameKeys.Right, KeyBind.Modifiers.Fire, "east f") },

            { new KeyBind(KeyBind.GameKeys.Up, KeyBind.Modifiers.Run ,"north r") },
            { new KeyBind(KeyBind.GameKeys.Down, KeyBind.Modifiers.Run, "south r") },
            { new KeyBind(KeyBind.GameKeys.Left, KeyBind.Modifiers.Run, "west r") },
            { new KeyBind(KeyBind.GameKeys.Right, KeyBind.Modifiers.Run, "east r") },

            { new KeyBind(KeyBind.GameKeys.NumPad5, KeyBind.Modifiers.Any, "stay") },

            { new KeyBind(KeyBind.GameKeys.NumPad7, KeyBind.Modifiers.None, "northwest") },
            { new KeyBind(KeyBind.GameKeys.NumPad8, KeyBind.Modifiers.None, "north") },
            { new KeyBind(KeyBind.GameKeys.NumPad9, KeyBind.Modifiers.None, "northeast") },
            { new KeyBind(KeyBind.GameKeys.NumPad4, KeyBind.Modifiers.None, "west") },
            { new KeyBind(KeyBind.GameKeys.NumPad6, KeyBind.Modifiers.None, "east") },
            { new KeyBind(KeyBind.GameKeys.NumPad1, KeyBind.Modifiers.None, "southwest") },
            { new KeyBind(KeyBind.GameKeys.NumPad2, KeyBind.Modifiers.None, "south") },
            { new KeyBind(KeyBind.GameKeys.NumPad3, KeyBind.Modifiers.None, "southeast") },

            { new KeyBind(KeyBind.GameKeys.NumPad7, KeyBind.Modifiers.Fire, "northwest f") },
            { new KeyBind(KeyBind.GameKeys.NumPad8, KeyBind.Modifiers.Fire, "north f") },
            { new KeyBind(KeyBind.GameKeys.NumPad9, KeyBind.Modifiers.Fire, "northeast f") },
            { new KeyBind(KeyBind.GameKeys.NumPad4, KeyBind.Modifiers.Fire, "west f") },
            { new KeyBind(KeyBind.GameKeys.NumPad6, KeyBind.Modifiers.Fire, "east f") },
            { new KeyBind(KeyBind.GameKeys.NumPad1, KeyBind.Modifiers.Fire, "southwest f") },
            { new KeyBind(KeyBind.GameKeys.NumPad2, KeyBind.Modifiers.Fire, "south f") },
            { new KeyBind(KeyBind.GameKeys.NumPad3, KeyBind.Modifiers.Fire, "southeast f") },

            { new KeyBind(KeyBind.GameKeys.NumPad7, KeyBind.Modifiers.Run, "northwest r") },
            { new KeyBind(KeyBind.GameKeys.NumPad8, KeyBind.Modifiers.Run, "north r") },
            { new KeyBind(KeyBind.GameKeys.NumPad9, KeyBind.Modifiers.Run, "northeast r") },
            { new KeyBind(KeyBind.GameKeys.NumPad4, KeyBind.Modifiers.Run, "west r") },
            { new KeyBind(KeyBind.GameKeys.NumPad6, KeyBind.Modifiers.Run, "east r") },
            { new KeyBind(KeyBind.GameKeys.NumPad1, KeyBind.Modifiers.Run, "southwest r") },
            { new KeyBind(KeyBind.GameKeys.NumPad2, KeyBind.Modifiers.Run, "south r") },
            { new KeyBind(KeyBind.GameKeys.NumPad3, KeyBind.Modifiers.Run, "southeast r") },

            { new KeyBind(KeyBind.GameKeys.Add,  KeyBind.Modifiers.None, "rotateshoottype") },
            { new KeyBind(KeyBind.GameKeys.Subtract,  KeyBind.Modifiers.None, "rotateshoottype -") },
            { new KeyBind(KeyBind.GameKeys.Add,  KeyBind.Modifiers.Fire, "rotateshoottype") },
            { new KeyBind(KeyBind.GameKeys.Subtract,  KeyBind.Modifiers.Fire, "rotateshoottype -") },
            { new KeyBind(KeyBind.GameKeys.Add,  KeyBind.Modifiers.Run, "rotateshoottype") },
            { new KeyBind(KeyBind.GameKeys.Subtract,  KeyBind.Modifiers.Run, "rotateshoottype -") },

            { new KeyBind(KeyBind.GameKeys.OemPeriod, KeyBind.Modifiers.None, "stay fire") },

            { new KeyBind(KeyBind.GameKeys.Oemcomma, KeyBind.Modifiers.None, "take") },

            { new KeyBind(KeyBind.GameKeys.OemQuotes, KeyBind.Modifiers.None, "", true) },
            { new KeyBind(KeyBind.GameKeys.OemQuotes, KeyBind.Modifiers.Fire, "say", true) },

            { new KeyBind(KeyBind.GameKeys.Enter, KeyBind.Modifiers.None, "chat", true) },

            { new KeyBind(KeyBind.GameKeys.OemQuestion, KeyBind.Modifiers.Fire, "help") },

            { new KeyBind(KeyBind.GameKeys.D, KeyBind.Modifiers.None, "disarm") },
            { new KeyBind(KeyBind.GameKeys.E, KeyBind.Modifiers.Run, "examine") },
            { new KeyBind(KeyBind.GameKeys.S, KeyBind.Modifiers.None, "search") },
            { new KeyBind(KeyBind.GameKeys.S, KeyBind.Modifiers.Fire, "brace") },
            { new KeyBind(KeyBind.GameKeys.T, KeyBind.Modifiers.None, "ready_skill throwing") },

            { new KeyBind(KeyBind.GameKeys.S, KeyBind.Modifiers.Alt, "togglewindow GameWindowSpells") },
            { new KeyBind(KeyBind.GameKeys.P, KeyBind.Modifiers.Alt, "togglewindow GameWindowPickup") },
            { new KeyBind(KeyBind.GameKeys.K, KeyBind.Modifiers.Alt, "togglewindow GameWindowKeybinds") },
            { new KeyBind(KeyBind.GameKeys.N, KeyBind.Modifiers.Alt, "togglewindow GameWindowNotes") },
            { new KeyBind(KeyBind.GameKeys.C, KeyBind.Modifiers.Alt, "hidewindow GameWindowContainer") },
            { new KeyBind(KeyBind.GameKeys.I, KeyBind.Modifiers.Alt, "showwindow GameWindowInventory") },

            { new KeyBind(KeyBind.GameKeys.Add, KeyBind.Modifiers.Alt, "mapzoomin") },
            { new KeyBind(KeyBind.GameKeys.Subtract, KeyBind.Modifiers.Alt, "mapzoomout") },

            { new KeyBind(KeyBind.GameKeys.F1, KeyBind.Modifiers.None, "hotkey 1") },
            { new KeyBind(KeyBind.GameKeys.F2, KeyBind.Modifiers.None, "hotkey 2") },
            { new KeyBind(KeyBind.GameKeys.F3, KeyBind.Modifiers.None, "hotkey 3") },
            { new KeyBind(KeyBind.GameKeys.F4, KeyBind.Modifiers.None, "hotkey 4") },
            { new KeyBind(KeyBind.GameKeys.F5, KeyBind.Modifiers.None, "hotkey 5") },
            { new KeyBind(KeyBind.GameKeys.F6, KeyBind.Modifiers.None, "hotkey 6") },

            { new KeyBind(KeyBind.GameKeys.F1, KeyBind.Modifiers.Alt, "loadwindowlayout Default") },
            { new KeyBind(KeyBind.GameKeys.F2, KeyBind.Modifiers.Alt, "loadwindowlayout Character") },
            { new KeyBind(KeyBind.GameKeys.F3, KeyBind.Modifiers.Alt, "loadwindowlayout Global") },
            { new KeyBind(KeyBind.GameKeys.F4, KeyBind.Modifiers.Alt, "loadwindowlayout Custom1") },
            { new KeyBind(KeyBind.GameKeys.F5, KeyBind.Modifiers.Alt, "loadwindowlayout Custom2") },
            { new KeyBind(KeyBind.GameKeys.F6, KeyBind.Modifiers.Alt, "loadwindowlayout Custom3") },
            { new KeyBind(KeyBind.GameKeys.F7, KeyBind.Modifiers.Alt, "loadwindowlayout Custom4") },
            { new KeyBind(KeyBind.GameKeys.F8, KeyBind.Modifiers.Alt, "loadwindowlayout Custom5") },
        };
    }

    public class KeybindsChangedEventArgs : EventArgs
    {
        public KeyBind KeyBind { get; set; }
    }
}
