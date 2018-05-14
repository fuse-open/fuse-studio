#import "AccelerometerImpl.hh"
#import <Uno/Memory.h>

@implementation AccelerometerImpl

-(id) initWithCallback: (AccelerometerCallback) callback
{
	self = [super init];

	if(self != nil)
	{
		self.callback = callback;
		self.operationQueue = [[NSOperationQueue alloc] init];
		self.motionManager = [[CMMotionManager alloc] init];
	}

	return self;
}

-(void) start
{
	[self.motionManager startAccelerometerUpdatesToQueue: self.operationQueue
		withHandler: ^void (CMAccelerometerData *data, NSError *error) {
			uAutoReleasePool pool;

			if(error != nil) {
				return;
			}

			CMAcceleration accel = [data acceleration];

			self.callback((float)accel.x, (float)accel.y, (float)accel.z);
		}];
}

-(void) stop
{
	[self.motionManager stopAccelerometerUpdates];
}

@end
