using ColorPicker.ExtensionMethods;

namespace ColorPicker.ColorModels.CMYK
{
    class Cyan : ColorComponent
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
           return sModel.CComponent(color).AsPercent();
        }

        public override string Name
        {
            get { return "CMYK_Cyan"; }
        }
    }
}
