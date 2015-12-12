# Fatty the Robot AI
<img src="docs\fatty.jpg">
Fatty is a robot with an AI brain including speech synth and recognition, natural language understanding, computer vision and servo motor control. 
Fatty runs on a Windows 10 Desktop, or Raspberry PI 2 with Windows IoT. He was built with the aim at getting Kids involved in building robots at home that can do amazing things.

## What can Fatty do now?
* Intepret spoken words into Intents that can control any part of the system
* Tell jokes, sing songs etc.

## What will Fatty be able to do soon?
* Move and Dance
* Recognise and track faces
* Play games

## What's special about Fatty's AI Brain?
* Simple but completely modular Messaging based architecture makes it infinitely extensible, great for hacking on and trying new things.
* Uses Rx (Reactive Extensions) for a simple and reliable multi-threaded and asynchronous architecture.
* Messages can triggered by local inputs on the Robot, or sent remotely to the robot from a desktop pc etc. (or maybe even a mobile phone!)

## How it works
The brain has modules that can send and or receive special messages called Intents.
And intent has a Name, Probability, Priority and a string dictionary of key/value pairs for meta data about the Intent.
Intents do not need to be registered in any central location and any module can handle and interpret any intent.
Intents can be easily converted to and from a string for sending over the wire or writing / reading from log files etc.
Intents are routed to each module currently via a broadcast pub/sub model, the plan is in future to use a Neural network to prioritise and schedule routing between modules. This will be an area of continued research and development.

Example:
Say Text=Hello my name is Fatty

This tells the SpeechSynthesis module to read the text "Hello my name is Fatty" out loud


Some modules are only applicable in certain contexts, for example the TextInput module is only relevant when there is a user interface. The MotorControl module is only relevant when on a Raspberry PI with it's GPIO port available.


## How to build your own Robot
While having a physical robot for running this AI is completely optional, it is really cool to have an autonomous robot running around your house. The robot you see above is a variation on the basic [Robot Kit on hackster.io|https://www.hackster.io/windowsiot/robot-kit-6dd474]
