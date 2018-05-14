using System;
using NUnit.Framework;

namespace Outracks.Common.Tests
{
	class TextPositionTests
	{
		[Test]
		public void ToOffsetTest01()
		{
			var foo = "hahasd this is so funn......\ne";
			var textPos = new TextPosition(new LineNumber(1), new CharacterNumber(1));

			var result = textPos.ToOffset(foo);
			Assert.IsTrue(result == 0);
		}

		[Test]
		public void ToOffsetTest02()
		{
			var foo = "hahasd this is so funn......\ne";
			var textPos = new TextPosition(new LineNumber(2), new CharacterNumber(1));

			var result = textPos.ToOffset(foo);
			Assert.IsTrue(result == 29);
		}

		[Test]
		public void ToOffsetTest03()
		{
			var foo = "hahasd this is so funn......\ne";
			var textPos = new TextPosition(new LineNumber(2), new CharacterNumber(2));

			var result = textPos.ToOffset(foo);
			Assert.IsTrue(result == 30);
		}

		[Test]
		public void ToOffsetTest04()
		{
			var foo = "hahasd this is so funn......\ne";
			var textPos = new TextPosition(new LineNumber(3), new CharacterNumber(2));

			Assert.Throws<IndexOutOfRangeException>(() => textPos.ToOffset(foo));			
		}

		[Test]
		public void ToOffsetTest05()
		{
			var foo = "hahasd this is so funn......\ne";
			var textPos = new TextPosition(new LineNumber(2), new CharacterNumber(3));

			Assert.Throws<IndexOutOfRangeException>(() => textPos.ToOffset(foo));
		}

		[Test]
		public void ToOffsetTest06()
		{
			var foo = "hahasd this is so funn......\n" +
				"foo bar\n" + 
				"ehhe";
			var textPos = new TextPosition(new LineNumber(3), new CharacterNumber(3));

			Assert.True(textPos.ToOffset(foo) == 39);
		}

		[Test]
		public void ToOffsetTest07()
		{
			var foo = "hahasd this is so funn......\n" +
				"foo bar\n" +
				"ehhe";
			var textPos = new TextPosition(new LineNumber(3), new CharacterNumber(5));

			Assert.True(textPos.ToOffset(foo) == 41);
		}

		[Test]
		public void ToOffsetTest08()
		{
			var foo = "hahasd this is so funn......\n" +
				"foo bar\r\n" +
				"ehhe\r";
			var textPos = new TextPosition(new LineNumber(3), new CharacterNumber(5));

			Assert.True(textPos.ToOffset(foo) == 41);
		}

		[Test]
		public void ToOffsetTest09()
		{
			var foo = "hahasd this is so funn......\n" +
				"foo bar\r\n" +
				"ehhe\r";
			var textPos = new TextPosition(new LineNumber(2), new CharacterNumber(8));

			Assert.True(textPos.ToOffset(foo) == 36);
		}

		[Test]
		public void ToOffsetTest10()
		{
			var foo = "hahasd this is so funn......\n" +
				"foo bar\r\n" +
				"ehhe\r";
			var textPos = new TextPosition(new LineNumber(3), new CharacterNumber(6));

			Assert.Throws<IndexOutOfRangeException>(() => textPos.ToOffset(foo));
		}

		[Test]
		public void ToOffsetTest11()
		{
			var fooNewLine = "hahasd this is so funn......\n" +
				"foo bar\n" +
				"ehhe";

			var fooCariage = "hahasd this is so funn......\r\n" +
				"foo bar\r\n" +
				"ehhe\r\n";	
			
			var textPos = new TextPosition(new LineNumber(2), new CharacterNumber(4));

			var onlyNewLine = textPos.ToOffset(fooNewLine);
			var notNewline = textPos.ToOffset(fooCariage);
			Assert.True(onlyNewLine == notNewline);
		}
	}
}