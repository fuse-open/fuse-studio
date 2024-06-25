using System;
using System.IO;
using NUnit.Framework;

namespace Outracks.IO.Tests
{
    class FileNameTest
    {
        [Test]
        public void FileNameCannotBeNull()
        {
            var e = Assert.Throws<ArgumentException>(() => new FileName(null));
            Assert.AreEqual("file name can not be null", e.Message);
        }

        [Test]
        public void FileNameCannotBeEmpty()
        {
            var e = Assert.Throws<ArgumentException>(() => new FileName(""));
            Assert.AreEqual("file name can not be empty", e.Message);
        }

        [Test]
        public void FileNameCannotContainInvalidCharacters()
        {
            var fileName = "foo" + Path.GetInvalidFileNameChars()[0];
            var e = Assert.Throws<ArgumentException>(() =>
            {
                new FileName(fileName);
            });
            Assert.AreEqual("file name '" + fileName +  "' contains invalid characters", e.Message);
        }

        [Test]
        public void Extension()
        {
            Assert.AreEqual("", new FileName("foo").Extension);
            Assert.AreEqual(".", new FileName("foo.").Extension);
            Assert.AreEqual(".exe", new FileName("foo.exe").Extension);
            Assert.AreEqual(".bat", new FileName("foo.exe.bat").Extension);
            Assert.AreEqual(".exe", new FileName(".exe").Extension);
        }

        [Test]
        public void WithoutExtension()
        {
            Assert.AreEqual(new FileName("foo"), new FileName("foo").WithoutExtension);
            Assert.AreEqual(new FileName("foo"), new FileName("foo.").WithoutExtension);
            Assert.AreEqual(new FileName("foo"), new FileName("foo.exe").WithoutExtension);
            Assert.AreEqual(new FileName("foo.exe"), new FileName("foo.exe.bat").WithoutExtension);

            //NOTE: This is just describing the current behaviour while adding tests, and
            //is maybe not the best way of handling it.
            //The point is that we have a class invariant disallowing empty file names, but
            //there's no way of doing that in this case. Lorents suggested changing the property to use
            //Optional, but that would require a lot of handling of Optional elsewhere in the code
            Assert.Throws<ArgumentException>(() => { var v = new FileName(".exe").WithoutExtension; });
        }

        [Test]
        public void HasExtension()
        {
            Assert.True(new FileName("foo.bar").HasExtension(".bar"));
            Assert.True(new FileName("foo.bar").HasExtension("bar"));
            Assert.False(new FileName("foo.bar").HasExtension("baz"));

            Assert.True(new FileName("foo.").HasExtension(""));
            Assert.True(new FileName("foo.").HasExtension("."));
            Assert.False(new FileName("foo.").HasExtension("baz"));

            Assert.False(new FileName("foo").HasExtension(""));
            Assert.False(new FileName("foo").HasExtension("."));
            Assert.False(new FileName("foo").HasExtension("baz"));

            Assert.True(new FileName(".foo").HasExtension("foo"));
            Assert.True(new FileName(".foo").HasExtension(".foo"));
            Assert.False(new FileName(".foo").HasExtension(".baz"));
        }

        [Test]
        public void StripExtension()
        {
            Assert.AreEqual(new FileName("foo"), new FileName("foo").StripExtension(""));
            Assert.AreEqual(new FileName("foo"), new FileName("foo").StripExtension("."));
            Assert.AreEqual(new FileName("foo"), new FileName("foo").StripExtension("foo"));
            Assert.AreEqual(new FileName("foo"), new FileName("foo").StripExtension(".bar"));

            Assert.AreEqual(new FileName("foo"), new FileName("foo.").StripExtension(""));
            Assert.AreEqual(new FileName("foo"), new FileName("foo.").StripExtension("."));
            Assert.AreEqual(new FileName("foo."), new FileName("foo.").StripExtension("foo"));

            Assert.AreEqual(new FileName("foo.bar"), new FileName("foo.bar").StripExtension(""));
            Assert.AreEqual(new FileName("foo.bar"), new FileName("foo.bar").StripExtension("foo"));
            Assert.AreEqual(new FileName("foo"), new FileName("foo.bar").StripExtension("bar"));
            Assert.AreEqual(new FileName("foo"), new FileName("foo.bar").StripExtension(".bar"));
        }

        [Test]
        public void AddExtension()
        {
            Assert.AreEqual(new FileName("foo.bar"), new FileName("foo").AddExtension("bar"));
            Assert.AreEqual(new FileName("foo.bar"), new FileName("foo").AddExtension(".bar"));
            Assert.AreEqual(new FileName("foo."), new FileName("foo").AddExtension("."));
            Assert.AreEqual(new FileName("foo."), new FileName("foo").AddExtension("."));
        }

        [Test]
        public void Add()
        {
            Assert.AreEqual(new FileName("foo"), new FileName("foo").Add(""));
            Assert.AreEqual(new FileName("foobar"), new FileName("foo").Add("bar"));
            Assert.AreEqual(new FileName("foo.bar"), new FileName("foo").Add(".bar"));
        }

        [Test]
        public void Replace()
        {
            Assert.AreEqual(new FileName("faa"), new FileName("foo").Replace("o", "a"));
            Assert.AreEqual(new FileName("foo"), new FileName("foo").Replace("X", "a"));
            Assert.AreEqual(new FileName("fo_ar"), new FileName("foo.bar").Replace("o.b", "_"));
        }

        [Test]
        public void Contains()
        {
            Assert.True(new FileName("foo.bar").Contains("oo.ba"));
            Assert.False(new FileName("foo").Contains("ooo"));
        }
    }
}
