# Fatty the Robot AI
<img src="docs\fatty.jpg">
Fatty is a robot with an AI brain including speech synth and recognition, natural language understanding, computer vision and servo motor control. 
Fatty runs on a Windows 10 Desktop, or Raspberry PI 2 with Windows IoT. He was built with the aim at getting Kids involved in building robots at home that can do amazing things.

## What can Fatty Do?
* Intepret spoken words into Intents that can control any part of the system
* Tell jokes, sing songs etc.
* Move and Dance... (work in progress)

## What's special about Fatty's AI Brain?
* Simple but completely modular Messaging based architecture makes it infinitely extensible, great for hacking on and trying new things.
* Uses Rx (Reactive Extensions) for a simple and reliable multi-threaded and asynchronous architecture.
* Messages can triggered by local inputs on the Robot, or sent remotely to the robot from a desktop pc etc. (or maybe even a mobile phone!)

## How it works
The brain has modules that can send and or receive special messages called Intent's.
And intent has a Name, Probability, Priority and a striing dictionary of key/value pairs for meta data about the Intent.
Intents do not need to be registered in any central location and any module can handle and interpret any intent.
Intents can be easily converted to and from a string for sending over the wire or writing / reading from log files etc.
Intents are routed to each module currently via a broadcast pub sub, the plan is in future to use a Neural network to prioritise and schedule routing. This will be an area of continued research and development.

Example:
Say Text=Hello my name is Fatty

This tells the SpeechSynthesis module to read the text "Hello my name is Fatty" out loud


Some modules are only applicable in certain contexts, for example the TextInput module is only relevant when there is a user interface. The MotorControl module is only relevant when on a Raspberry PI with it's GPIO port available.
