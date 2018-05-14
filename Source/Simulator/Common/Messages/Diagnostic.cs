using System;
using System.IO;
using Uno;

namespace Outracks.Simulator.Protocol
{
	public enum DiagnosticType
	{
		Error,
		Warning,
		InternalError,
		Deprecated,
		Unsupported,
		PerformanceWarning
	}

	public sealed class Diagnostic : IBinaryMessage
	{
		public static readonly string MessageType = "Diagnostic";
		public string Type { get { return MessageType; } }

		public DiagnosticType DiagnosticType { get; private set; }

		public string DeviceId { get; private set; }
		public string DeviceName { get; private set; }

		static int _idEnumerator = 0;
		/// <summary>
		/// An int that uniquely this diagnostic in the context of this DeviceId. 
		/// Temporal diagnostics can be dismissed later by sending a DismissDiagnostic message with this ID.
		/// </summary>
		public int DiagnosticId { get; private set; }

		/// <summary>
		/// Appropriate value to display in diagnostic listing
		/// </summary>
		public string Message { get; private set; }

		/// <summary>
		/// All details of this diagnostic
		/// </summary>
		public string Details { get; private set; }

		/// <summary>
		/// Project-releative path to the source file for this diagnostic
		/// If SourceFile is null, LineNumber/ColumnNumber is not used
		/// </summary> 
		public string SourceFile { get; private set; }

		/// <summary>
		/// The line number whithin the SourceFile of the diagnostic. 
		/// 
		/// If SourceFile is null, this property is not used
		/// </summary>
		public int LineNumber { get; private set; }

		/// <summary>
		/// The column number whithin the SourceFile and LineNumber of the diagnostic. 
		/// 
		/// If SourceFile is null, this property is not used
		/// If -1, the whole line is the source.
		/// </summary>
		public int ColumnNumber { get; private set; }


		/// <summary>
		/// 
		/// </summary>
		/// <param name="deviceId"></param>
		/// <param name="deviceName"></param>
		/// <param name="message"></param>
		/// <param name="details"></param>
		/// <param name="sourceFile"></param>
		/// <param name="lineNumber"></param>
		/// <param name="columnNumber"></param>
		/// <param name="diagnosticId">Pass -1 to get an auto-generated Id</param>
		public Diagnostic(string deviceId, string deviceName, string message, string details, string sourceFile, int lineNumber, int columnNumber, int diagnosticId = -1)
		{
			DeviceId = deviceId;
			DeviceName = deviceName;
			DiagnosticId = (diagnosticId == -1 ? _idEnumerator++ : diagnosticId);
			Message = message;
			Details = details ?? "";
			SourceFile = sourceFile ?? "";
			LineNumber = lineNumber;
			ColumnNumber = columnNumber;
		}

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.Write(DeviceId);
			writer.Write(DeviceName);
			writer.Write(Message);
			writer.Write(Details);
			writer.Write(SourceFile);
			writer.Write(LineNumber);
			writer.Write(ColumnNumber);
			writer.Write(DiagnosticId);
		}

		public static Diagnostic ReadDataFrom(BinaryReader reader)
		{
			var deviceId = reader.ReadString();
			var deviceName = reader.ReadString();
			var message = reader.ReadString();
			var details = NullIfEmpty(reader.ReadString());
			var sourceFile = NullIfEmpty(reader.ReadString());
			var lineNumber = reader.ReadInt32();
			var columnNumber = reader.ReadInt32();
			var diagnosticId = reader.ReadInt32();
			return new Diagnostic(deviceId, deviceName, message, details, sourceFile, lineNumber, columnNumber, diagnosticId);
		}

		static string NullIfEmpty(string s)
		{
			if (string.IsNullOrEmpty(s)) return null;
			return s;
		}
	}

	public sealed class DismissDiagnostic : IBinaryMessage
	{
		public static readonly string MessageType = "DismissDiagnostic";
		public string Type { get { return MessageType; } }

		public string DeviceId { get; private set; }
		public string DeviceName { get; private set; }

		/// <summary>
		/// A GUID that identifies this diagnostic to dismiss.
		/// </summary>
		public int DiagnosticId { get; private set; }

		public DismissDiagnostic(string deviceId, string deviceName, int diagnosticId)
		{
			DeviceId = deviceId;
			DeviceName = deviceName;
			DiagnosticId = diagnosticId;
		}

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.Write(DeviceId);
			writer.Write(DeviceName);
			writer.Write(DiagnosticId);
		}

		public static DismissDiagnostic ReadDataFrom(BinaryReader reader)
		{
			var deviceId = reader.ReadString();
			var deviceName = reader.ReadString();
			var diagnosticId = reader.ReadInt32();
			return new DismissDiagnostic(deviceId, deviceName, diagnosticId);
		}
	}
}