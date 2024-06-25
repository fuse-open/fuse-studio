using System.Collections.Generic;
using Outracks.IO;
using Uno.Compiler;
using Uno.Compiler.Core;

namespace Outracks.CodeCompletion
{
    public interface IEngine
    {
        Compiler Compiler { get; }

		SourcePackage MainPackage { get; }

	    void Invalidate();

		IEnumerable<AbsoluteFilePath> AllSourceFiles { get; }
    }
}
