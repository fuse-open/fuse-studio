using SketchConverter.UxBuilder;

namespace SketchConverter.API
{
    public static class Factory
    {
        public static IConverter CreateConverterWithSymbolsUxBuilder(ILogger logger)
        {
            return new Converter(new SketchParser.SketchParser(logger), new SymbolsUxBuilder(logger), logger);
        }
    }
}
