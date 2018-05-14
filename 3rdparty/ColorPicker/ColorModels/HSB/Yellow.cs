namespace ColorPicker.ColorModels.CMY 
{
    class Yellow:ColorComponent 
    {
        public static CMYModel sModel = new CMYModel();

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
            return (int)sModel.YComponent(color);
        }

        public override string Name
        {
            get { return "CMY_Yellow"; }
        }
    }
}
