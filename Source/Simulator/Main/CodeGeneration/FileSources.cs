using System.Collections.Generic;
using System.Linq;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.CodeGeneration
{
	using Bytecode;
	using UXIL;
	using Runtime;

	public static class BundleFiles
	{
		public static Expression AddOrUpdateFile(string descriptor, byte[] data)
		{
			return new CallStaticMethod(
				AddOrUpdateFileMethod,
				new StringLiteral(descriptor), 
				new BlobLiteral(data));
		}

		static readonly StaticMemberName AddOrUpdateFileMethod = new StaticMemberName(
			TypeName.Parse("Outracks.Simulator.Runtime.Bundle"),
			new TypeMemberName("AddOrUpdateFile"));
	}

	public static class ImportExpression
	{
		public static Expression GetExpression(this BundleFileSource node, Context ctx)
		{
			return ExpressionConverter.BytecodeFromSimpleLambda(() => 
				FileCache.GetFileSource(node.GetDescriptor(ctx.ProjectDirectory)));
		}

		public static IEnumerable<ProjectDependency> FindFiles(ClassNode document, string projectDirectory)
		{
			return document
				.AllNodesInDocument()
				.SelectMany(
					node =>
						Enumerable.Union(
							node.ReferencePropertiesWithValues.Select(p => p.Source),
							node.ListPropertiesWithValues.SelectMany(p => p.Sources)))
				.OfType<BundleFileSource>()
				.Select(bs => new ProjectDependency(
					path: bs.Path,
					descriptor: bs.GetDescriptor(projectDirectory))); 
		}

		static string GetDescriptor(this BundleFileSource bs, string projectDirectory)
		{
			// TODO: replace with new UXIL propery
			return bs.Path.RelativeTo(projectDirectory).Replace('\\', '/');
		}
		
		public static Statement UpdateFile(string descriptor, byte[] data)
		{
			return ExpressionConverter.BytecodeFromSimpleLambda(() => 
				FileCache.Update(descriptor, data));
		}
	}
}
