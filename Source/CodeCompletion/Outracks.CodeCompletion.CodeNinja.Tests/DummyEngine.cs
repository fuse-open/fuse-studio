using System.Collections.Generic;
using Outracks.CodeCompletion;
using Outracks.IO;
using Uno.Compiler;
using Uno.Compiler.Core;

namespace Outracks.CodeNinja.Tests
{
    public class DummyEngine : IEngine
    {        
        readonly Compiler _compiler;
        public Compiler Compiler { get { return _compiler; } }
	    public SourcePackage MainPackage { get; private set; }
	    public void Invalidate() {}
	    public IEnumerable<AbsoluteFilePath> AllSourceFiles { get; private set; }

        public DummyEngine(Compiler compiler)
        {
            _compiler = compiler;
        }
    }
}
