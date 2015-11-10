Introduction
============

This node pack wraps the functionality of NetMQ, the native c# implementation of the original c++ zmq. It implements all socket types as nodes and provides uniform send/receive functionality. Context can be set manually for each socket. Everything spreads beautifully, as you would expect it from a vvvv pack in 2015.

For beginners, it allows safe messaging of Raw data between vvvv instances, local and remote and removes problems with TCP (receive the packages the way you sent them) and UDP (reliable streaming and primary handshaking for high-level communication, depending on the socket you choose).
For a in-depth introduction to ZeroMQ (and distributed architecture in general), consult the great [zqm Guide](http://zguide.zeromq.org/page:all).
Note that most examples can be patched along with the guide, just make sure to check the Configuration Pin of Bind in Herr Inspektor correctly. 

PGM has not been tested much, as it needs admin rights for vvvv and some tinkering with windows. However, TCP and Inproc work solid, eg for quick messaging and streaming video data.

Credits
=======

Author
------
Marko Ritter (www.intolight.com)


Nuget
----
* [VVVV-sdk](https://github.com/vvvv/vvvv-sdk), LGPL
* [NetMQ](https://github.com/zeromq/netmq), LGPL 3

License
=======

![LGPL 3.0](https://www.gnu.org/graphics/lgplv3-147x51.png), LGPL 3

