﻿using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class NOP : JsmInstruction
    {
        public NOP()
        {
        }

        public NOP(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(NOP)}()";
        }
    }
}