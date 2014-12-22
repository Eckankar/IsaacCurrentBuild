# Isaac Current Build

A simple little thing that extracts the following information from the currently on-going Binding of Isaac: Rebirth game:

* Seed
* Obtained items

Might be useful for making chatbots for people streaming Binding of Isaac: Rebirth, for instance.

# Installation

Download the source code and compile it with Visual Studio, or [download the binary file](https://github.com/Eckankar/IsaacCurrentBuild/releases).

# Usage

Run the program, and it'll output to console the current build information.

Note for if you're testing it out: The program automatically closes itself. (To allow for better integration with bots or whatnot.) If you need it
to stay open, [download this version instead](http://brohr.coq.dk/data/IsaacCurrentBuild.zip).

# Known limitations

* The list of obtained items breaks if the build is rerolled. Sadly the game
  doesn't log that a reroll occurs, so it's not easy to properly update based on
  that.
