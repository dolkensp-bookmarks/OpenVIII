﻿using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class SWAP : JsmInstruction
    {
        public SWAP()
        {
        }

        public SWAP(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(SWAP)}()";
        }
    }
}