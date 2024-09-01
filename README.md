# CrossfireRPG.ServerInterface

CrossfireRPG.ServerInterface is a c# library (implemented as a shared project) for communicating with a Crossfire (RPG) server.

The Crossfire server and clients are available from https://crossfire.real-time.com/


## Organization

The `CrossfireRPG.ServerInterface` namespace is organized as follows:


### CrossfireRPG.ServerInterface.Definitions

Contains various constants, enumerations, and types to ensure the CrossfireRPG.ServerInterface library can commiunicate with the Crossfire server in a consistent manner.

Note that most definition files have a corresponding header file on the server. This is noted in the definition file comments.


### CrossfireRPG.ServerInterface.Protocol

Contains classes for parsing, creating, sending, and managing protocol messages to a Crossfire server. The classes inside the `CrossfireRPG.ServerInterface.Protocol` namespace build upon each other as documented below:

#### Low Level Classes

- `BufferAssembler`: Encodes string and binary data into protocol messages (in the Crossfire server protocol format)
- `BufferTokenizer`: Retrieves string and binary data from a protocol message (using the Crossfire server protocol format)


#### Message Builder

The `MessageBuilder` class is used to build and send protocol messages to a Crossfire server. The `MessageBuilder` uses a `BufferAssembler` to provide an easy to use API for sending protocol messages.

Note that the `MessageBuilder` is implemented as _partial class_ (split over multiple files), with the various protocol messages grouped into subfolders. All the partial files are prefixed with `MessageBuilder_`.


#### Message Parser

The `MessageParser` class is used to parse protocol messages from a Crossfire server. The `MessageParser` utilizes a `BufferTokenizer` for retrieving data from the protocol bytes.

The `MessageParser` is implemented as an _abstract class_, and each protocol message defines an _abstract_ method to handle the parsed protocol message, prefixed with `Handle`.

Note that the `MessageParser` is implemented as _partial class_ (split over multiple files), with the various protocol messages grouped into subfolders. All the partial files are prefixed with `MessageParser_`.


#### Message Handler

The `MessageHandler` class is an implementation of the `MessageParser` class, which provides an _event_ for each protocol message.

Note that the `MessageHandler` is implemented as _partial class_ (split over multiple files), with the various protocol messages grouped into subfolders. All the partial files are prefixed with `MessageHandler_`.



### CrossfireRPG.ServerInterface.Network

Contains classes used to communicate with a Crossfire server over the TCP/IP protocol.

- `SocketConnection`: Provides a TCP/IP interface to a Crossfire server, with _events_ when a connection status changes or data is available.
- `MetaServer`: Queries the global Crossfire MetaServers to provide a list of active, public Crossfire servers, along with some server metadata.


### CrossfireRPG.ServerInterface.Managers

The idea of the _Managers_ (or _DataManagers_) is to combine multiple related `MessageHandler` protocol message events into a single maintainer of thos messages. This _Manager_ then maintains an up-to-date `DataObject` (or list of DataObjects), removing the need to handle protocol messages directly.

Each of the _Managers_ derive from a base `DataManager` class, that
- Provides a consistent way to access managed data (`DataObject`)
- Provides notifications when the managed data is added, removed, changed, or cleared 
- Provides a way to update or clear managed data on a player change or server disconnect
  - Note that the server expects some information to be stored between player sessions, and some information to be cleared between player sessions


## Notes

This library code was split from the DockWindow Client as a first step to open sourcing the client, and therefore may contain dependencies that are not available at the time of this writing. Please be patient.
