﻿using OpenVIII.Fields.Scripts.Instructions;
using System;
using System.Collections.Generic;


namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public static partial class Control
        {
            private sealed class ProcessedJumps
            {
                private readonly HashSet<IJumpToInstruction> _processed = new HashSet<IJumpToInstruction>();

                public ProcessedJumps()
                {
                }

                public Boolean TryProcess(IJumpToInstruction jmp)
                {
                    return _processed.Add(jmp);
                }

                public void Process(IJumpToInstruction jmp)
                {
                    if (!_processed.Add(jmp))
                        throw new InvalidProgramException($"The jump instruction ({jmp}) has already been processed.");
                }
            }
        }
    }
}