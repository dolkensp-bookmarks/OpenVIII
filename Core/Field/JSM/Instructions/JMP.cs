﻿using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class JMP : JsmInstruction, IJumpToOpcode
    {
        public Int32 Offset { get; set; }

        public JMP(Int32 offset)
        {
            Offset = offset;
        }

        public JMP(Int32 offset, IStack<IJsmExpression> stack)
            : this(offset)
        {
        }

        private Int32 _index = -1;

        public Int32 Index
        {
            get
            {
                if (_index == -1)
                    throw new ArgumentException($"{nameof(JMP)} instruction isn't indexed yet.", nameof(Index));
                return _index;
            }
            set
            {
                //if (_index != -1)
                //    throw new ArgumentException($"{nameof(JMP)} instruction has already been indexed: {_index}.", nameof(Index));
                _index = value;
            }
        }

        public override String ToString()
        {
            return _index < 0
                ? $"{nameof(JMP)}({nameof(Offset)}: {Offset})"
                : $"{nameof(JMP)}({nameof(Index)}: {Index})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            // This instruction is part of conditional jumps and isn't exists as is.
        }

        public override IAwaitable Execute(IServices services)
        {
            // This instruction is part of conditional jumps and isn't exists as is.
            return DummyAwaitable.Instance;
        }
    }
}