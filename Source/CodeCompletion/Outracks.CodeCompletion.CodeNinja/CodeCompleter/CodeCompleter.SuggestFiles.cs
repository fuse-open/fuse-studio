using System.Linq;
using Outracks.CodeCompletion;
using Outracks.IO;
using Uno.Compiler;
using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        bool SuggestImportApplyFiles()
        {   
            if (_reader.PeekToken() == TokenType.StringLiteral)
            {
                int offset = _reader.Offset;

                if (_reader.ReadTokenReverse() == TokenType.LeftParen)
                {
                    _reader.Offset += 2;
                    string prefix = _reader.PeekText(offset - _reader.Offset);
                    _reader.Offset -= 2;

                    string[] filters = new[] { "*.*" };

                    while (true)
                    {
                        int lastOFfset = _reader.Offset;
                        var t = _reader.ReadTokenReverse();
                        if (t == TokenType.Identifier)
                        {
                            var id = _reader.PeekText(lastOFfset - _reader.Offset);
                            if (id == "Texture2D") filters = new[] { "*.png", "*.jpg", "*.tga" };
                            if (id == "TextureCube") filters = new[] { "*.png", "*.jpg", "*.tga" };
                            if (id == "Model") filters = new[] { "*.dae", "*.fbx" };
                            continue;
                        }
                        else if (t == TokenType.Period) continue;
                        else if (t == TokenType.Whitespace) continue;
                        else if (t == TokenType.Import)
                        {
                            var srcpath = System.IO.Path.GetDirectoryName(_source.FullPath).ToUnixPath();
                            var path = srcpath;

                            prefix = prefix.ToUnixPath();
                            if (prefix.Contains('/'))
                            {
                                prefix = prefix.Substring(0, prefix.LastIndexOf('/'));
                                path += "/" + prefix;
                            }

                            if (!(System.IO.File.Exists(path) || System.IO.Directory.Exists(path)))
                                return true;

                            foreach (var filter in filters)
                            {
                                foreach (var f in System.IO.Directory.GetFiles(path, filter).Select(AbsoluteFilePath.Parse))
                                {
	                                var ff = f.RelativeTo(AbsoluteDirectoryPath.Parse(path)).NativeRelativePath.ToUnixPath(); 
                                    Suggest(SuggestItemType.File, new FileEntity(ff), ff);
                                }
                            }

                            foreach (var f in System.IO.Directory.GetDirectories(path).Select(AbsoluteFilePath.Parse))
                            {
								var ff = f.RelativeTo(AbsoluteDirectoryPath.Parse(path)).NativeRelativePath.ToUnixPath(); 
                                Suggest(SuggestItemType.Directory, new FileEntity(ff), ff);
                            }

                            return true;
                        }
                        else break;
                    }
                }

                _reader.Offset = offset;
            }

            return false;
        }
    }
}
