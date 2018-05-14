using Uno;
using Uno.Compiler;
using Uno.Collections;
using Uno.UX;

namespace Fuse
{
    public abstract class AppBase : Uno.Application
    {
        [UXPrimary]
        /** The @Node.Children of the virtual root @Node of the @App.
            Note that the virtual root node might be different from the @RootViewport depending
            on platform. */
        public abstract IList<Node> Children
        {
            get;
        }
        
        [UXContent]
		/** The @Node.Resources of the virtual root node of the @App.
			Note that the virtual root node might be different from the @RootViewport depending
			on platform */
		public IList<Resource> Resources { get { return new List<Resource>(); } }
    }
    
    
    [IgnoreMainClass]
    public abstract class App : AppBase
    {
        public override IList<Node> Children { get { return new List<Node>(); } }
        
        public static new App Current
        {
            get { return Uno.Application.Current as App; }
        }

        protected virtual void OnDraw() {}

        protected virtual void OnUpdate() {}
    }
}
