namespace Outracks.UnoDevelop.CodeNinja.AmbientParser
{
    public interface ICodeReader<out T>
    {
        T PeekTokenReverse();
        T ReadTokenReverse();
        T ReadToken();
        T PeekToken();
        int Offset { get; set; }
        int Length { get; }
        string PeekTextReverse(int charCount);
        string PeekText(int charCount);
        string ReadText(int charCount);
        string ReadTextReverse(int charCount);
    }
}
