﻿namespace Microsoft.Dafny
{
    public class SourceEdit
    {
        public int startPos;
        public int endPos;
        public string content;

        public SourceEdit(int startPos, int endPos, string content)
        {
            this.startPos = startPos;
            this.endPos = endPos;
            this.content = content;
        }
    }
}
