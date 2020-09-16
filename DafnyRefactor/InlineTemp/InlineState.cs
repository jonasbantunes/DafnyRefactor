using System.Collections.Generic;
using DafnyRefactor.InlineTemp.InlineTable;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.CommandLineOptions;
using DafnyRefactor.Utils.SourceEdit;

namespace DafnyRefactor.InlineTemp
{
    public class InlineState : RefactorState
    {
        // public ISymbolTable symbolTable;
        public IInlineTable symbolTable;
        public ApplyInlineTempOptions inlineOptions;
        public IInlineSymbol inlineSymbol;
        public List<SourceEdit> sourceEdits;

        public InlineState(ApplyInlineTempOptions options) : base(options)
        {
            inlineOptions = options;
            sourceEdits = new List<SourceEdit>();
        }
    }
}