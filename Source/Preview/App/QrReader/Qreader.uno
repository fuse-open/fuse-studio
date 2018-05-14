using Uno;
using Uno.Collections;
using Fuse.Scripting;
using Fuse.Reactive;
using Fuse;
using Uno.Compiler.ExportTargetInterop;
using Uno.Threading;

public class Qreader : NativeModule {
	public Qreader () {
		// Add Load function to load image as a texture
		AddMember(new NativePromise<string, string>("scan", (FutureFactory<string>)Scan,null));
	}

	static Future<string> Scan(object[] args)
	{
		//var path = Uno.IO.Path.Combine(Uno.IO.Directory.GetUserDirectory(Uno.IO.UserDirectory.Data), "temp.jpg");
			return QreaderImpl.Scan();
	}

}


[ForeignInclude(Language.Java,
                "android.app.Activity",
                "android.content.Intent",
                "android.net.Uri",
                "android.os.Bundle",
																"com.google.android.gms.common.api.CommonStatusCodes",
																"com.fuse.qreader.BarcodeCaptureActivity",
																"com.google.android.gms.vision.barcode.Barcode")]

[ForeignInclude(Language.ObjC, "QreaderTask.h", "QRCodeReaderViewController.h","QRCodeReader.h")]
[Require("Gradle.Dependency.Compile","com.android.support:support-v4:23.0.1")]
[Require("Gradle.Dependency.Compile","com.google.android.gms:play-services-vision:9.2.1")]
[Require("Gradle.Dependency.Compile","com.android.support:design:23.0.1")]
public class QreaderImpl
{
	static int RC_BARCODE_CAPTURE = 9001;

	static bool InProgress {
		get; set;
	}

	static Promise<string> FutureResult {
		get; set;
	}

	static extern(Android) Java.Object _intentListener;

	public static Future<string> Scan() {
		if (InProgress) {
			return null;
		}
		InProgress = true;
		if defined(Android) {
			 if (_intentListener == null)
				_intentListener = Init();
		}
		FutureResult = new Promise<string>();
		ScannerImpl();
		return FutureResult;
	}

	[Foreign(Language.Java)]
	static extern(Android) Java.Object Init()
	@{
	    com.fuse.Activity.ResultListener l = new com.fuse.Activity.ResultListener() {
	        @Override public boolean onResult(int requestCode, int resultCode, android.content.Intent data) {
	            return @{OnRecieved(int,int,Java.Object):Call(requestCode, resultCode, data)};
	        }
	    };
	    com.fuse.Activity.subscribeToResults(l);
	    return l;
	@}

	[Foreign(Language.Java)]
	static extern(Android) bool OnRecieved(int requestCode, int resultCode, Java.Object data)
	@{

						if (requestCode == @{RC_BARCODE_CAPTURE}&&resultCode == CommonStatusCodes.SUCCESS && data != null) {

										Barcode barcode = ((Intent)data).getParcelableExtra(BarcodeCaptureActivity.BarcodeObject);
										if(barcode.displayValue != ""){
													@{Picked(string):Call(barcode.displayValue)};
										}else{
														@{Cancelled():Call()};
										}
						}
						else {
										@{Cancelled():Call()};
						}

	    return (requestCode == @{RC_BARCODE_CAPTURE});
	@}

	static extern(!Mobile) void ScannerImpl () {
		throw new Fuse.Scripting.Error("Unsupported platform");
	}

	[Require("Gradle.Dependency.Compile","com.android.support:support-v4:23.0.1")]
	[Require("Gradle.Dependency.Compile","com.google.android.gms:play-services-vision:9.2.1")]
	[Require("Gradle.Dependency.Compile","com.android.support:design:23.0.1")]
	[Require("AndroidManifest.ApplicationElement", "<activity android:name=\"com.fuse.qreader.BarcodeCaptureActivity\" android:theme=\"@style/Theme.AppCompat\"></activity>")]
	[Require("AndroidManifest.RootElement", "<uses-feature android:name=\"android.hardware.camera\"/>")]
	[Require("AndroidManifest.RootElement", "<uses-feature android:name=\"android.hardware.camera.autofocus\"/>")]
	[Require("AndroidManifest.RootElement", "<uses-permission android:name=\"android.permission.CAMERA\"/>")]
	[Foreign(Language.Java)]
	static extern(Android) void ScannerImpl ()
	@{
		Activity a = com.fuse.Activity.getRootActivity();
		// Intent intent = new Intent(Intent.ACTION_PICK, android.provider.MediaStore.Images.Media.EXTERNAL_CONTENT_URI);
		Intent intent = new Intent(a, BarcodeCaptureActivity.class);
		a.startActivityForResult(intent, @{RC_BARCODE_CAPTURE});

	@}

	[Require("Entity","QreaderImpl.Cancelled()")]
	[Require("Entity","QreaderImpl.Picked(string)")]
	[Foreign(Language.ObjC)]
	static extern(iOS) void ScannerImpl ()
	@{
			QreaderTask *task = [[QreaderTask alloc] init];

			static QRCodeReaderViewController *vc = nil;
			static dispatch_once_t onceToken;

			UIViewController *uivc = [UIApplication sharedApplication].keyWindow.rootViewController;
			[task setUivc:uivc];

			dispatch_once(&onceToken, ^{
					QRCodeReader *reader = [QRCodeReader readerWithMetadataObjectTypes:@[AVMetadataObjectTypeQRCode]];
					vc                   = [QRCodeReaderViewController readerWithCancelButtonTitle:@"Cancel" codeReader:reader];
					vc.modalPresentationStyle = UIModalPresentationFormSheet;
			});

			vc.delegate = task;

			[uivc
				presentViewController:vc
				animated:YES
				completion:nil];
	@}

	public static void Cancelled () {
		if (InProgress == false) 
			return;
		InProgress = false;
		FutureResult.Reject(new Exception("User cancelled the qr scanner"));
	}

	public static void Picked (string result) {
		if (InProgress == false) 
			return;
		
		InProgress = false;
		FutureResult.Resolve(result);
	}

}
