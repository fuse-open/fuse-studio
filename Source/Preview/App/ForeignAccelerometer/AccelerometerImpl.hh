#import <Foundation/Foundation.h>
#import <CoreMotion/CoreMotion.h>

typedef void (^AccelerometerCallback)(float, float, float);

@interface AccelerometerImpl : NSObject

-(id) initWithCallback: (AccelerometerCallback) callback;
-(void) start;
-(void) stop;

@property (nonatomic, copy) AccelerometerCallback callback;
@property (strong) NSOperationQueue *operationQueue;
@property (strong) CMMotionManager *motionManager;

@end