namespace Outracks.Common.Tests
{
    interface IFoo : IMatchTypes<Bar, Baz, Biz>
    {
        string InternalPrettyPrint();
    }

    class Bar : IFoo
    {
        public string bar = "bar";
        public string InternalPrettyPrint() { return bar; }
    }

    class Baz : IFoo
    {
        public string baz = "baz";
        public string InternalPrettyPrint() { return baz; }
    }

    class Biz : IFoo
    {
        public string biz = "biz";
        public string InternalPrettyPrint() { return biz; }
    }

    static class ExternalPrettyPrintExtension
    {
        public static string ExternalPrettyPrint(this IFoo foo)
        {
            return foo.MatchWith(
                bar => bar.bar,
                baz => baz.baz,
                biz => biz.biz);
        }
    }

    class Test
    {
        //Unused method, but maybe someone wants it for manual testing or something?
        public static void DoTest()
        {

            IFoo foo = new Bar();

	        #pragma warning disable 0219
            var a = foo.InternalPrettyPrint();
            var b = foo.ExternalPrettyPrint();
	        #pragma warning restore 0219
        }
    }

}
