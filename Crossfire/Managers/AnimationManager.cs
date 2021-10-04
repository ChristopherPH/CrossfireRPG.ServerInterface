using CrossfireCore;
using CrossfireCore.ServerInterface;
using CrossfireCore.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire.Managers
{
    public class AnimationManager
    {
        public AnimationManager(SocketConnection Connection, MessageBuilder Builder, MessageParser Parser)
        {
            _Connection = Connection;
            _Builder = Builder;
            _Parser = Parser;

            _Connection.OnStatusChanged += _Connection_OnStatusChanged;
            _Parser.Animation += _Parser_Animation;
            _Parser.Tick += _Parser_Tick;
        }

        private SocketConnection _Connection;
        private MessageBuilder _Builder;
        private MessageParser _Parser;

        private Dictionary<UInt16, Animation> _Animations = new Dictionary<UInt16, Animation>();

        public Animation GetAnimation(UInt16 Animation)
        {
            if (_Animations.TryGetValue(Animation, out var val))
                return val;

            return null;
        }

        private void _Connection_OnStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            _Animations.Clear();
        }

        private void _Parser_Tick(object sender, MessageParserBase.TickEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void _Parser_Animation(object sender, MessageParserBase.AnimationEventArgs e)
        {
            _Animations[e.AnimationNumber] = new Animation()
            {
                AnimationFaces = e.AnimationFaces,
                AnimationFlags = e.AnimationFlags,
                AnimationNumber = e.AnimationNumber,
            };
        }
    }

    public class Animation
    {
        public UInt16 AnimationNumber { get; set; }
        public UInt16 AnimationFlags { get; set; }  //unused
        public UInt16[] AnimationFaces { get; set; }
    }
}
