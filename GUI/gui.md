### MVHC Design Pattern
**MVHC** stands for **Model-View-Handler-Controller**. This pattern is an adaptation of the **MVVC** pattern specifically tailored for Unity development.


```
+--------------------------------------------+
|                  Widget                    |
|  (Container for all components below)      |
|                                            |
|  +------------+     +-------------+        |
|  |   Model    |     |   Handler   |        |
|  | (Data)     |<--->| (Data Access|        |
|  |            |     |  & Events)  |        |
|  +------------+     +-------------+        |
|         ^                   |              |
|         |                   v              |
|  +------------+     +-------------+        |
|  | Controller |<--->|    View     |        |
|  | (State &   |     | (Display &  |        |
|  |  Logic)    |     |  Visuals)   |        |
|  +------------+     +-------------+        |
+--------------------------------------------+
```
- Model and Handler communicate bidirectionally to manage data and notify changes.
- Handler passes data/events to Controller.
- Controller transforms data and commands View.
- View handles UI rendering and visual feedback.

All parts live inside the Widget container.


#### Widget
A **Widget** is a combination of components, views, and the data they work with. For example, a widget might consist of the following components:

- **WidgetBooster**
  - **BoosterData**: A data structure describing the booster.
  - **WidgetBoosterBaseHandler**: Handles the appearance and initialization (common for all booster variants).
  - **WidgetBoosterGameplayHandler**
  - **WidgetBoosterShopHandler**
  - **WidgetBoosterBaseController**
  - **WidgetBoosterBaseView**

### Composition

MVHC does not impose strict limitations or interfaces on the design of components. It is more of a convention and organizational pattern. A widget can have:

- **Always one active View**. Variations of the View can exist for different backends (e.g., uGUI, IMGUI, 3D GUI) and behaviors (e.g., different types of animations).
- **Model**: The data representation in the game logic. Changes in the model will affect the Controller and Handler. If data is represented by an interface, maintaining the interface while changing the internal data format does not require changes in the controller and handler.

### Nested Widgets

A widget can contain other widgets within it. Depending on the system design, nested widgets can be independent or encapsulated within the parent widget to varying degrees of independence.

- **Maximum Degree of Embedding**: Nested widgets contain only their views. The parent controller knows about all these views and passes the appropriate data to them.
- **Medium Degree of Embedding**: Nested widgets contain views and controllers. The parent controller works with the child controllers.
- **Low Degree of Embedding**: Nested widgets contain views, controllers, and handlers. The parent controller can access the children either directly or through data changes or event dispatching.

### Model
The data for the widget resides in the game logic (a.k.a. business logic/domain logic). 

- The data does not necessarily have to be designed for use in the pattern. It can be abstract data or a part of the game responsible for widget data. Alternatively, it can be specifically represented as a data structure.
- The only way for a widget to know about data changes or their current status is by using the handler (events or bridges).
- If the data is complex and the widget is intricate, it is advisable to hide the data behind an interface. For example, `IBoosterModel` can read and write booster data values, and the handler interacts through this interface.

### Handler
The data handler:

- Receives notifications about data changes that the widget is interested in or polls such data through bridges. The handler thus decides the degree of widget isolation from the rest of the system. If only events arrive or a callback is provided for data access, low code coupling is achieved. If the handler itself polls subsystems and knows where to access data, coupling is high.
- Data coming into the handler is in the game format. The conversion of game data to GUI data happens inside the Controller. The handler only passes data to the controller.
- A widget can have several handler variations but use only one of them. For example, `WidgetBoosterGameplayHandler` and `WidgetBoosterShopHandler`.
- A widget can have multiple handlers based on notification methods or sources. For example, one handler subscribes and unsubscribes to events when the widget is active, while another continues to poll a parameter even when the widget is disabled (`WidgetAwardShopHandler` & `WidgetAwardShopInactiveHandler`).
- A widget can have separated handlers for data reading and modification. For example, all internal button presses and slider changes from the widget view are sent to `WidgetColorPickerInternalHandler`, which accesses and modifies data. Such changes can, in turn, trigger events that go to another handler responsible for data display.

### Controller
The controller:

- Holds the state (data) for the current widget specialization.
- Converts the passed data from the game representation to the internal format.
- Contains methods within the game domain. For example, `SetPrice(0)`, `SetCounter(0)`, while the View method might be `SetLabelText(0)`. If the goal is to make a more generic widget, methods will be `SetImage`, `SetText`.
- A widget can have multiple specializations. For example:
  - `WidgetBoosterShopController` contains fields like: `Price`, `NextUpgrade`
  - `WidgetBoosterGameplayController` contains fields like: `Cooldown`, `Amount`, `Progression`

### View
The view:

- Represents the display logic only.
- Can contain the visual state of the control (e.g., button pressed, disabled, highlighted). Methods like `PlayAnimation`, `SwitchMode`.
- Can have variations of different views for a single widget. For example, a button can be visualized with IMGUI or uGUI. The button can animate using tweeners, animations, or simply changing frames, which can be implemented with different Views.