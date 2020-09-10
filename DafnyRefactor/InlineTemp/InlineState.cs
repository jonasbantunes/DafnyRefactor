using System.Collections.Generic;
using DafnyRefactor.InlineTemp.InlineTable;
using DafnyRefactor.Utils;
using DafnyRefactor.Utils.CommandLineOptions;
using DafnyRefactor.Utils.SourceEdit;
using DafnyRefactor.Utils.SymbolTable;

namespace DafnyRefactor.InlineTemp
{
    public class InlineState : RefactorState
    {
        public SymbolTable<InlineSymbol> symbolTable;
        public ApplyInlineTempOptions inlineOptions;
        public InlineSymbol inlineSymbol;

        // TODO: Check if this field is really neccessary
        public List<SourceEdit> immutabilitySourceEdits;
        public List<SourceEdit> sourceEdits;

        public InlineState(ApplyInlineTempOptions options) : base(options)
        {
            inlineOptions = options;
            sourceEdits = new List<SourceEdit>();
        }
    }
}