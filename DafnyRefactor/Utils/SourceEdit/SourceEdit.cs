﻿using System;

namespace DafnyRefactor.Utils.SourceEdit
{
    public class SourceEdit
    {
        public string content;
        public int endPos;
        public int startPos;

        public SourceEdit(int startPos, int endPos, string content)
        {
            if (startPos > endPos)
            {
                throw new ArgumentOutOfRangeException(nameof(endPos),
                    "Start position should be greater or equal than end position");
            }

            this.startPos = startPos;
            this.endPos = endPos;
            this.content = content;
        }

        public SourceEdit(int pos, string content)
        {
            startPos = pos;
            endPos = pos;
            this.content = content;
        }
    }
}