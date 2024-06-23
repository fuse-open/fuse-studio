using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;

namespace Outracks.Tests.Diagnostics
{
	// Single threaded, since we set a static global
	[SingleThreaded]
	[TestFixture]
	public class ConsoleTraceExtensionTests
	{
		StringWriter _trace;
		IObservableTraceLogger _originalLogger;

		[Test]
		public void ConsoleTrace_outputs_correct_log_when_subscribing_to_observable_return()
		{
			Observable.Return(1337).ConsoleTrace("elite").Subscribe().Dispose();

			AssertLogEquals(
				@"	T{t} Enter  elite Subscribe(..) -> #{obs_id}
					T{t} Enter  |   elite (#{obs_id}) OnNext(1337)
					T{t} Enter  |   elite (#{obs_id}) OnCompleted()
					T{t} Exit   elite Subscribe(..) -> #{obs_id}
					T{t} Enter  elite (#{obs_id}) Dispose()");
		}

		[Test]
		public void ConsoleTrace_is_prefixed_with_thread_id_when_calling_from_other_thread()
		{
			var subject = new Subject<int>();
			var waitHandle = new ManualResetEvent(false);
			using (subject
				.ConsoleTrace("Subject")
				.Select(x => x * 2)
				.Delay(TimeSpan.FromMilliseconds(100), NewThreadScheduler.Default)
				.ConsoleTrace("ObserveOn")
				.Do(_ => waitHandle.Set())
				.Subscribe())
			{
				subject.OnNext(333);
				Assert.IsTrue(waitHandle.WaitOne(1000), "Timeout");
			}

			AssertLogEquals(
				@"	T{t1} Enter  ObserveOn Subscribe(..) -> #1
					T{t1} Enter  |   Subject Subscribe(..) -> #2
					T{t1} Exit   ObserveOn Subscribe(..) -> #1
					T{t1} Enter  Subject (#2) OnNext(333)
					T{t2} Enter  ObserveOn (#1) OnNext(666)
					T{t1} Enter  ObserveOn (#1) Dispose()
					T{t1} Enter  |   Subject (#2) Dispose()
					T{t1} Exit   ObserveOn (#1) Dispose()");
		}

		[Test]
		public void ConsoleTrace_makes_use_of_tostring_parameter_for_string_conversion()
		{
			var obs = new Subject<int>();
			obs
				.ConsoleTrace("the name", i => i.ToString("X")) // -=> the name:'2A'
				.Subscribe(v => { });
			obs.OnNext(42);

			AssertLogEquals(
				@"	T{t} Enter  the name Subscribe(..) -> #1
					T{t} Enter  the name (#1) OnNext(2A)");
		}

		void AssertLogEquals(string template)
		{
			AssertMatchTemplate(template, _trace.ToString());
		}

		static void AssertMatchTemplate(string template, string actual)
		{
			var regex = new Regex("⦃(?<key>[^⦄]+)⦄", RegexOptions.Multiline);
			var regexTemplate = Regex.Replace(template, "^\\s*", "", RegexOptions.Multiline);

			regexTemplate = regexTemplate
				.Replace("\r\n", "\n")
				.Replace("{", "⦃")
				.Replace("}", "⦄")
				.Replace("⦃⦃", "{")
				.Replace("⦄⦄", "}");

			regexTemplate = Regex.Escape(regexTemplate);

			HashSet<string> keys = new HashSet<string>();
			regexTemplate = "^" + regex.Replace(
				regexTemplate,
				match => keys.Add(match.Value) ? match.Result("(?<${key}>.*?)") : match.Result("\\k<${key}>")) + "$";

			Assert.IsTrue(Regex.IsMatch(actual, regexTemplate), "Does not match template " + template);
		}

		[SetUp]
		public void SetUp()
		{
			_originalLogger = ObservableTraceLogger.Current;
			_trace = new StringWriter();
			ObservableTraceLogger.Current =
				ObservableTraceLogger.Create(
					TextWriter.Synchronized(new Tee(TestContext.Out, _trace) { NewLine = "\n" }),
					measure: false);
		}

		[TearDown]
		public void TearDown()
		{
			ObservableTraceLogger.Current.Dispose();
			ObservableTraceLogger.Current = _originalLogger;
		}

		/// <summary>
		/// Text writer that redirects to two writers, get text out in test for diagnostics purposes
		/// </summary>
		class Tee : TextWriter
		{
			readonly TextWriter a;
			readonly TextWriter b;

			public Tee(TextWriter a, TextWriter b)
			{
				this.a = a;
				this.b = b;
			}

			public override IFormatProvider FormatProvider
			{
				get { return CultureInfo.InvariantCulture; }
			}

			public override Encoding Encoding
			{
				get { return a.Encoding; }
			}

			public override void Write(char value)
			{
				a.Write(value);
				b.Write(value);
			}

			public override void Write(char[] buffer, int index, int count)
			{
				a.Write(buffer, index, count);
				b.Write(buffer, index, count);
			}
		}
	}
}