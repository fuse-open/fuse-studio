# Fusion - Getting Started
_Fusion is a framework for cross-platform UI development, which uses the native UI framework to render its content. It's based on a reactive philosphy, where the UI is updated on demand compared to immediate UI mode frameworks._

## Creating your first view
A view in _Fusion_ is a composition of `IControl`'s that are put together to create a view.

Here is an example showing two labels stacked on each other:

```csharp
Layout.StackFromTop(
	Label.Create("My first text"),
    Label.Create("My second text")
);
```


This will show two labels stacked on top of each other. So in this example, we make two `IControl`'s representing the two labels in the view, and then those are composited by the `Layout.StackFromTop` which in turn creates a new `IControl` consisting of the two `IControl`'s stacked on top of each other.

Here is a new example showing a more advanced example, where the label text is dynamic:
```csharp
var watch = Observable.Interval(TimeSpan.FromSeconds(1))
	.Select(time => time + 1)
	.StartWith(0);

return Layout.StackFromTop(
    Label.Create(watch.Select(time => time.ToString()), Font.SystemDefault(20))
).Center();
```

The label's text subscribes to the `IObservable`, where the source is a timer that ticks every second, starting from 0. The `Font.SystemDefault` method is used to get a system font with a specific size. So the font will differ between Windows and macOS. Also the last line says `.Center()`, which centers the stackpanel inside its parent frame.

So now we have shown that you can have static text and dynamic text, but you can also have dynamic layout:
```csharp
var watch = Observable.Interval(TimeSpan.FromSeconds(1))
	.Select(time => time + 1)
	.StartWith(0);

var shapeBasedOnTime = watch
        .Select(time => (time % 2) == 0) // Check if even number
        .Select(isEven => isEven
            ? Shapes.Rectangle(fill: Brush.SolidColor(Color.FromRgb(0x2255ff)))
            : Shapes.Circle(fill: Brush.SolidColor(Color.FromRgb(0x2255ff))))
        .Switch();

return Layout.StackFromTop(
    Label.Create(watch.Select(time => time.ToString()), Font.SystemDefault(100), TextAlignment.Center),
    shapeBasedOnTime.WithSize(new Size<Points>(100,100))
).Center();
```

So this example uses the `watch` to switch between two shapes, based on if the watch time is even or not. The interesting part here is that a `IObservable<IControl>` can be converted to a `IControl` by doing a `.Switch()` on the type as on line 10. This behaviour, is as you would expect a `Switch()` on a `IObservable<IObservable<OfSomething>>` would do. What that tells us is that `IControl` is what we call an _Observable interface_, in the sense that its backing field data is observable.

## Layout
**TODO: Show some examples of how to do layout, and how to use padding etc.**

## Understanding databindings in Fusion
There is mainly three ways the data is passed between the declaration of the UI and Fusion. As shown below:
* `IObservable` - Is used for read-only bindings between the data and the view. Which means that the view listens for changes to this variable, and where the view isn't allowed to change it.
* `IProperty` - Is used for two-way bindings between the data and the view, for example in an editable textbox.
* `Command` _(Note: Subject to change)_ - Is used for events triggered by the view. For example when a button is clicked, the command is executed. In other words, `Command` suits only as a contract between the decleration and the view, and there are no data flow going on here.

Lets look at an example which shows the use of `IProperty`:
```csharp
var text = Property.Create("Type here");
return Layout.StackFromTop(
    TextBox.Create(text).WithWidth(150),
    Label.Create(text.Skip(1).StartWith(""))
).Center();
```
First we create a property to be used as our backing storage for our user input(`TextBox`). This property is then bound to `TextBox` and `Label`, which in effect cause the `Label` to change in response to `TextBox` changes.

** TODO: Write something about ICommand and IObservable input (mention Observable.Return eg.) **





