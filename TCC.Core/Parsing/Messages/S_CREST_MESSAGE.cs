﻿using TCC.TeraCommon.Game.Messages;
using TCC.TeraCommon.Game.Services;

namespace TCC.Parsing.Messages
{
    public class S_CREST_MESSAGE : ParsedMessage
    {
        public uint Type { get; private set; }
        public uint SkillId { get; private set; }

        public S_CREST_MESSAGE(TeraMessageReader reader) : base(reader)
        {
            reader.Skip(4);
            Type = reader.ReadUInt32();
            SkillId = reader.ReadUInt32();

            //Debug.WriteLine(nameof(S_CREST_MESSAGE));
            //Debug.WriteLine("\t Type: {0}", Type);
            //Debug.WriteLine("\t SkillId: {0}", SkillId);
        }
    }
}
