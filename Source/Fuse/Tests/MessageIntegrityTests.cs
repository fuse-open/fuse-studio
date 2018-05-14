using System;
using System.Reactive.Subjects;
using NUnit.Framework;
using Outracks.Fuse.Protocol;
using Outracks.IO;

namespace Outracks.EditorService.Tests
{
	[TestFixture]
	class MessageIntegrityTests
	{
		string BasePath;
//		string GetCodeSuggestionsRequestDb;
//		string GetCodeSuggestionResponseDb;
//		string GotoDefinitionRequestDb;
//		string GotoDefinitionResponseDb;
		string HelloRequestDb;
		string HelloResponseDb;

		[OneTimeSetUp]
		public void Init()
		{
			BasePath = TestContext.CurrentContext.TestDirectory + "/Messages/";
//			GetCodeSuggestionsRequestDb = BasePath + "GetCodeSuggestionsRequest.msgDB";
//			GetCodeSuggestionResponseDb = BasePath + "GetCodeSuggestionResponse.msgDB";
//			GotoDefinitionRequestDb = BasePath + "GotoDefinitionRequest.msgDB";
//			GotoDefinitionResponseDb = BasePath + "GotoDefinitionResponse.msgDB";
			HelloRequestDb = BasePath + "HelloRequest.msgDB";
			HelloResponseDb = BasePath + "HelloResponse.msgDB";
		}

		/*[Test]
		public void GetCodeSuggestionsRequest()
		{
			AssertIfDiffers(MessageDatabase.From(new GetCodeSuggestionsRequest()), GetCodeSuggestionsRequestDb);
		}

		[Test]
		public void GetCodeSuggestionsResponse()
		{
			AssertIfDiffers(MessageDatabase.From(new GetCodeSuggestionsResponse()), GetCodeSuggestionResponseDb);
		}

		[Test]
		public void GotoDefinitionRequest()
		{
			AssertIfDiffers(MessageDatabase.From(new GotoDefinitionRequest()), GotoDefinitionRequestDb);
		}

		[Test]
		public void GotoDefinitionResponse()
		{
			AssertIfDiffers(MessageDatabase.From(new GotoDefinitionResponse()), GotoDefinitionResponseDb);
		}*/

		[Test]
		public void HelloRequest()
		{
			AssertIfDiffers(MessageDatabase.From(new HelloRequest()), HelloRequestDb);
		}

		[Test]
		public void HelloResponse()
		{
			AssertIfDiffers(MessageDatabase.From(new HelloResponse()), HelloResponseDb);
		}

		static void AssertIfDiffers(MessageDatabase database, string filePathStr)
		{			
#if DUMP_MODE			
			var relativePath = RelativeFilePath.Parse(filePathStr);
			var dumpPath = AbsoluteDirectoryPath.Parse("../../") / relativePath;
			database.Dump(dumpPath);
#endif
			var filePath = AbsoluteFilePath.Parse(filePathStr);
			var originalDatabase = MessageDatabase.From(filePath);
			MessageDatabase.From(filePath);
			
			var errors = new Subject<string>();
			errors.Subscribe(Console.WriteLine);
			Assert.True(MessageDatabase.IsEqualWhileIgnoreComments(originalDatabase, database, errors), "Looks like you have changed the plugin API");
		}
	}
}