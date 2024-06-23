using Outracks.UnoDevelop.UXNinja;

namespace Uno.UXNinja.TestsCommon
{
    public class DummyReader : ICodeReader
    {
        public TokenType PeekTokenReverse()
        {
            return (TokenType) 0;
        }

        public TokenType ReadTokenReverse()
        {
            return (TokenType) 0;
        }

        public TokenType ReadToken()
        {
            return (TokenType) 0;
        }

        public TokenType PeekToken()
        {
            return (TokenType) 0;
        }

        public int Offset { get; set; }
        public int Length { get; private set; }
        public string PeekTextReverse(int charCount)
        {
            return null;
        }

        public string PeekText(int charCount)
        {
            return null;
        }

        public string ReadText(int charCount)
        {
            return null;
        }

        public string ReadTextReverse(int charCount)
        {
            return null;
        }
    }
}