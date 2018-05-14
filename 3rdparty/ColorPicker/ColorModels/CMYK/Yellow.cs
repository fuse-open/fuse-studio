using ColorPicker.ExtensionMethods;

namespace ColorPicker.ColorModels.CMYK
{
    class Yellow:ColorComponent 
    {
        public static CMYKModel sModel = new CMYKModel();

        public override int MinValue
        {
            get { return 0; }
        }

        public override int MaxValue
        {
            get { return 100; }
        }

        

        public override int Value(System.Windows.Media.Color color)
        {
            return sModel.YComponent(color).AsPercent();
        }

        public override string Name
        {
            get { return "CMYK_Yellow"; }
        }
    }
}
